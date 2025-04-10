namespace Atc.XamlToolkit.SourceGenerators.Extensions;

internal static class ObjectExtensions
{
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

        if (!type.IsSimpleType() && !type.IsSimpleUiType())
        {
            return strDefaultValue;
        }

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
        var lastPart = parts[parts.Length - 1];

        switch (lastPart)
        {
            case "bool":
                strDefaultValue = strDefaultValue switch
                {
                    "true" => "BooleanBoxes.TrueBox",
                    "True" => "BooleanBoxes.TrueBox",
                    "false" => "BooleanBoxes.FalseBox",
                    "False" => "BooleanBoxes.FalseBox",
                    _ => strDefaultValue,
                };
                break;
            case "decimal":
                strDefaultValue = strDefaultValue.Length == 0
                    ? SimpleTypeFactory.CreateDefaultValueAsStrForType(type)
                    : strDefaultValue.Replace(',', '.') + "m";
                break;
            case "double":
                strDefaultValue = strDefaultValue.Length == 0
                    ? SimpleTypeFactory.CreateDefaultValueAsStrForType(type)
                    : strDefaultValue.Replace(',', '.') + "d";
                break;
            case "float":
                strDefaultValue = strDefaultValue.Length == 0
                    ? SimpleTypeFactory.CreateDefaultValueAsStrForType(type)
                    : strDefaultValue.Replace(',', '.') + "f";
                break;
            case "int" or "long":
                if (strDefaultValue.Length == 0)
                {
                    strDefaultValue = "0";
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
                strDefaultValue = strDefaultValue.Length == 0
                    ? "Colors.DeepPink"
                    : $"Colors.{strDefaultValue}";
                break;
            }

            case "Brush":
            {
                strDefaultValue = strDefaultValue.Length == 0
                    ? "Brushes.DeepPink"
                    : $"Brushes.{strDefaultValue}";
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