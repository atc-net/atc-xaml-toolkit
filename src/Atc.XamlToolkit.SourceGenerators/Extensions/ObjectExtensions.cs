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
        string type,
        XamlPlatform xamlPlatform)
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
                // Avalonia doesn't use BooleanBoxes, only WPF/WinUI do
                if (xamlPlatform != XamlPlatform.Avalonia)
                {
                    strDefaultValue = strDefaultValue.ToLowerInvariant() switch
                    {
                        "true" => "BooleanBoxes.TrueBox",
                        "false" => "BooleanBoxes.FalseBox",
                        _ => strDefaultValue,
                    };
                }
                else
                {
                    strDefaultValue = strDefaultValue.ToLowerInvariant();
                }

                break;
            case "decimal":
                if (AllowedDecimalConst.Any(x => x.Equals(strDefaultValue, StringComparison.Ordinal)))
                {
                    strDefaultValue = "decimal." + strDefaultValue;
                }
                else if (!strDefaultValue.Contains("decimal."))
                {
                    strDefaultValue = strDefaultValue.Length == 0
                        ? SimpleTypeFactory.CreateDefaultValueAsStrForType(type, xamlPlatform)
                        : strDefaultValue.Replace(',', '.');

                    if (strDefaultValue is not null &&
                        !strDefaultValue.EndsWith("m", StringComparison.Ordinal))
                    {
                        strDefaultValue += "m";
                    }
                }

                break;
            case "double":
                if (AllowedDoubleFloatConst.Any(x => x.Equals(strDefaultValue, StringComparison.Ordinal)))
                {
                    strDefaultValue = "double." + strDefaultValue;
                }
                else if (!strDefaultValue.Contains("double."))
                {
                    if (strDefaultValue == double.MinValue.ToString(Thread.CurrentThread.CurrentCulture))
                    {
                        strDefaultValue = "double.MinValue";
                    }
                    else if (strDefaultValue == double.MaxValue.ToString(Thread.CurrentThread.CurrentCulture))
                    {
                        strDefaultValue = "double.MaxValue";
                    }
                    else
                    {
                        strDefaultValue = strDefaultValue.Length == 0
                            ? SimpleTypeFactory.CreateDefaultValueAsStrForType(type, xamlPlatform)
                            : strDefaultValue.Replace(',', '.');

                        if (strDefaultValue is not null &&
                            !strDefaultValue.EndsWith("d", StringComparison.Ordinal))
                        {
                            strDefaultValue += "d";
                        }
                    }
                }

                break;
            case "float":
                if (AllowedDoubleFloatConst.Any(x => x.Equals(strDefaultValue, StringComparison.Ordinal)))
                {
                    strDefaultValue = "float." + strDefaultValue;
                }
                else if (!strDefaultValue.Contains("float."))
                {
                    if (strDefaultValue == float.MinValue.ToString(Thread.CurrentThread.CurrentCulture))
                    {
                        strDefaultValue = "float.MinValue";
                    }
                    else if (strDefaultValue == float.MaxValue.ToString(Thread.CurrentThread.CurrentCulture))
                    {
                        strDefaultValue = "float.MaxValue";
                    }
                    else
                    {
                        strDefaultValue = strDefaultValue.Length == 0
                            ? SimpleTypeFactory.CreateDefaultValueAsStrForType(type, xamlPlatform)
                            : strDefaultValue.Replace(',', '.');

                        if (strDefaultValue is not null &&
                            !strDefaultValue.EndsWith("f", StringComparison.Ordinal))
                        {
                            strDefaultValue += "f";
                        }
                    }
                }

                break;
            case "uint":
                if (AllowedDoubleFloatConst.Any(x => x.Equals(strDefaultValue, StringComparison.Ordinal)))
                {
                    strDefaultValue = "uint." + strDefaultValue;
                }
                else if (!strDefaultValue.Contains("uint."))
                {
                    if (strDefaultValue == uint.MinValue.ToString(Thread.CurrentThread.CurrentCulture))
                    {
                        strDefaultValue = "uint.MinValue";
                    }
                    else if (strDefaultValue == uint.MaxValue.ToString(Thread.CurrentThread.CurrentCulture))
                    {
                        strDefaultValue = "uint.MaxValue";
                    }
                    else
                    {
                        if (strDefaultValue.Length == 0)
                        {
                            strDefaultValue = SimpleTypeFactory.CreateDefaultValueAsStrForType(type, xamlPlatform);
                        }
                        else if (!strDefaultValue.EndsWith("U", StringComparison.Ordinal))
                        {
                            strDefaultValue += "U";
                        }
                    }
                }

                break;
            case "ulong":
                if (AllowedDoubleFloatConst.Any(x => x.Equals(strDefaultValue, StringComparison.Ordinal)))
                {
                    strDefaultValue = "ulong." + strDefaultValue;
                }
                else if (!strDefaultValue.Contains("ulong."))
                {
                    if (strDefaultValue == ulong.MinValue.ToString(Thread.CurrentThread.CurrentCulture))
                    {
                        strDefaultValue = "ulong.MinValue";
                    }
                    else if (strDefaultValue == ulong.MaxValue.ToString(Thread.CurrentThread.CurrentCulture))
                    {
                        strDefaultValue = "ulong.MaxValue";
                    }
                    else
                    {
                        if (strDefaultValue.Length == 0)
                        {
                            strDefaultValue = SimpleTypeFactory.CreateDefaultValueAsStrForType(type, xamlPlatform);
                        }
                        else if (!strDefaultValue.EndsWith("UL", StringComparison.Ordinal))
                        {
                            strDefaultValue += "UL";
                        }
                    }
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

                break;
            case "string":
                strDefaultValue = strDefaultValue.Length == 0
                    ? "string.Empty"
                    : $"\"{strDefaultValue}\"";
                break;

            case "byte":
                if (strDefaultValue.Length == 0)
                {
                    strDefaultValue = SimpleTypeFactory.CreateDefaultValueAsStrForType(type, xamlPlatform);
                }
                else if (!strDefaultValue.StartsWith("(byte)", StringComparison.Ordinal))
                {
                    strDefaultValue = "(byte)" + strDefaultValue;
                }

                break;
            case "sbyte":
                if (strDefaultValue.Length == 0)
                {
                    strDefaultValue = SimpleTypeFactory.CreateDefaultValueAsStrForType(type, xamlPlatform);
                }
                else if (!strDefaultValue.StartsWith("(sbyte)", StringComparison.Ordinal))
                {
                    strDefaultValue = "(sbyte)" + strDefaultValue;
                }

                break;
            case "short":
                if (strDefaultValue.Length == 0)
                {
                    strDefaultValue = SimpleTypeFactory.CreateDefaultValueAsStrForType(type, xamlPlatform);
                }
                else if (!strDefaultValue.StartsWith("(short)", StringComparison.Ordinal))
                {
                    strDefaultValue = "(short)" + strDefaultValue;
                }

                break;
            case "ushort":
                if (strDefaultValue.Length == 0)
                {
                    strDefaultValue = SimpleTypeFactory.CreateDefaultValueAsStrForType(type, xamlPlatform);
                }
                else if (!strDefaultValue.StartsWith("(ushort)", StringComparison.Ordinal))
                {
                    strDefaultValue = "(ushort)" + strDefaultValue;
                }

                break;

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
                if (xamlPlatform == XamlPlatform.Wpf)
                {
                    if (!strDefaultValue.Contains("Brushes."))
                    {
                        strDefaultValue = strDefaultValue.Length == 0
                            ? "Brushes.DeepPink"
                            : $"Brushes.{strDefaultValue}";
                    }
                }
                else
                {
                    if (!strDefaultValue.Contains("new SolidColorBrush(Colors."))
                    {
                        strDefaultValue = strDefaultValue.Length == 0
                            ? "new SolidColorBrush(Colors.DeepPink)"
                            : $"new SolidColorBrush(Colors.{strDefaultValue})";
                    }
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