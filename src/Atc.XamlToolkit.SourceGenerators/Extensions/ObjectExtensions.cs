namespace Atc.XamlToolkit.SourceGenerators.Extensions;

internal static class ObjectExtensions
{
    private static readonly string[] AllowedIntLongConst = [
        "MaxValue",
        "MinValue",
    ];

    private static readonly string[] AllowedDecimalConst = [
        "MaxValue",
        "MinValue",
        "MinusOne",
        "One",
        "Zero",
    ];

    private static readonly string[] AllowedDoubleFloatConst = [
        "MaxValue",
        "MinValue",
        "NaN",
        "NegativeInfinity",
        "NegativeZero",
        "E",
        "Epsilon",
        "Pi",
        "PositiveInfinity",
        "Tau",
    ];

    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    public static string? TransformDefaultValueIfNeeded(
        this object? defaultValue,
        string type)
    {
        if (defaultValue is null)
        {
            return null;
        }

        var strDefaultValue = defaultValue
            .ToString()?
            .EnsureNoNameof() ?? string.Empty;

        if (type.Contains("<"))
        {
            var sa = strDefaultValue.Split(';');
            if (type.Contains("List"))
            {
                var listElements = string.Join(", ", sa.Select(x => $"\"{x.Trim()}\""));
                return $"new List<string> {{ {listElements} }}";
            }
        }

        var parts = type.Split('.');
        var lastPart = parts[parts.Length - 1].RemoveNullableSuffix();

        switch (lastPart)
        {
            case "bool":
                strDefaultValue = strDefaultValue.ToLowerInvariant() switch
                {
                    "true" => "BooleanBoxes.TrueBox",
                    "false" => "BooleanBoxes.FalseBox",
                    _ => strDefaultValue,
                };
                break;
            case "decimal":
                if (AllowedDecimalConst.Any(x => x.Equals(strDefaultValue, StringComparison.Ordinal)))
                {
                    strDefaultValue = "decimal." + strDefaultValue;
                }
                else if (!strDefaultValue.Contains("decimal."))
                {
                    strDefaultValue = strDefaultValue.Length == 0
                        ? SimpleTypeFactory.CreateDefaultValueAsStrForType(type)
                        : strDefaultValue.Replace(',', '.') + "m";
                }

                break;
            case "double":
                if (AllowedDoubleFloatConst.Any(x => x.Equals(strDefaultValue, StringComparison.Ordinal)))
                {
                    strDefaultValue = "double." + strDefaultValue;
                }
                else if (!strDefaultValue.Contains("double."))
                {
                    strDefaultValue = strDefaultValue.Length == 0
                        ? SimpleTypeFactory.CreateDefaultValueAsStrForType(type)
                        : strDefaultValue.Replace(',', '.') + "d";
                }

                break;
            case "float":
                if (AllowedDoubleFloatConst.Any(x => x.Equals(strDefaultValue, StringComparison.Ordinal)))
                {
                    strDefaultValue = "float." + strDefaultValue;
                }
                else if (!strDefaultValue.Contains("float."))
                {
                    strDefaultValue = strDefaultValue.Length == 0
                        ? SimpleTypeFactory.CreateDefaultValueAsStrForType(type)
                        : strDefaultValue.Replace(',', '.') + "f";
                }

                break;
            case "int" or "long":
                if (strDefaultValue.Length == 0)
                {
                    strDefaultValue = "0";
                }
                else if (AllowedIntLongConst.Any(x => x.Equals(strDefaultValue, StringComparison.Ordinal)))
                {
                    strDefaultValue = lastPart + "." + strDefaultValue;
                }
                else if (!strDefaultValue.Contains("int.") &&
                         !strDefaultValue.Contains("long."))
                {
                    // Skip
                }

                break;
            case "string":
            {
                strDefaultValue = strDefaultValue.Length == 0
                    ? "string.Empty"
                    : $"\"{strDefaultValue}\"";
                break;
            }

            case "Color":
            {
                if (!strDefaultValue.Contains("Colors."))
                {
                    strDefaultValue = strDefaultValue.Length == 0
                        ? "Colors.DeepPink"
                        : $"Colors.{strDefaultValue}";
                }

                break;
            }

            case "Brush":
            case "SolidColorBrush":
            {
                if (!strDefaultValue.Contains("Brushes."))
                {
                    strDefaultValue = strDefaultValue.Length == 0
                        ? "Brushes.DeepPink"
                        : $"Brushes.{strDefaultValue}";
                }

                break;
            }

            case "FontFamily":
            {
                strDefaultValue = strDefaultValue.Length == 0
                    ? "new FontFamily(\"Arial\")"
                    : $"new FontFamily(\"{strDefaultValue}\")";
                break;
            }
        }

        return strDefaultValue;
    }

    public static string? EnsureNoNameof(
        this object? value)
    {
        if (value is null ||
            !value.ToString().StartsWith("nameof", StringComparison.Ordinal))
        {
            return value?.ToString();
        }

        return value
            .ToString()
            .Replace("nameof(", string.Empty)
            .Replace(")", string.Empty);
    }

    public static string? EnsureNameof(
        this object? value)
    {
        if (value is null ||
            value.ToString().Contains("nameof"))
        {
            return value?.ToString();
        }

        return $"nameof({value})";
    }
}