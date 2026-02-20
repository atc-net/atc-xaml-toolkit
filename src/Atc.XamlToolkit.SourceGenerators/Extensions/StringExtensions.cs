// ReSharper disable InvertIf
// ReSharper disable ConvertIfStatementToNullCoalescingExpression
namespace Atc.XamlToolkit.SourceGenerators.Extensions;

[SuppressMessage("Design", "CA1308:Teplace the call to 'ToLowerInvariant' with 'ToUpperInvariant'", Justification = "OK.")]
public static class StringExtensions
{
    private static readonly Regex NameofRegex = new(
        @"^nameof\((?<name>\w+)\)$",
        RegexOptions.Compiled | RegexOptions.ExplicitCapture,
        TimeSpan.FromMilliseconds(100));

    private static readonly Dictionary<string, string> TypeAliases = new(StringComparer.Ordinal)
    {
        { nameof(Boolean),  "bool" },
        { nameof(Byte),     "byte" },
        { nameof(SByte),    "sbyte" },
        { nameof(Char),     "char" },
        { nameof(Decimal),  "decimal" },
        { nameof(Double),   "double" },
        { nameof(Single),   "float" },
        { nameof(Int32),    "int" },
        { nameof(UInt32),   "uint" },
        { nameof(Int16),    "short" },
        { nameof(UInt16),   "ushort" },
        { nameof(Int64),    "long" },
        { nameof(UInt64),   "ulong" },
        { nameof(IntPtr),   "nint" },
        { nameof(UIntPtr),  "nuint" },
        { nameof(Object),   "object" },
        { nameof(String),   "string" },
    };

    private static readonly HashSet<string> KnownValueTypes = new(StringComparer.Ordinal)
    {
        // Primitive structs
        "bool", "byte", "sbyte", "char", "decimal", "double", "float", "int", "uint",
        "short", "ushort", "long", "ulong", "nint", "nuint",

        // System structs
        "Guid", "DateTime", "DateTimeOffset", "TimeSpan",

        // WPF / WinUI core structs
        "Color", "Point", "Rect", "Size", "Thickness", "Vector",
        "CornerRadius", "Matrix", "Matrix3D", "Point3D", "Vector3D", "Quaternion",
        "Rect3D", "Int32Rect", "Duration", "GridLength",
        "FontStretch", "FontStyle", "FontWeight",

        // Common enums (layout / visibility / flow)
        "HorizontalAlignment", "VerticalAlignment", "FlowDirection", "Orientation",
        "TextAlignment", "TextWrapping", "Dock", "Visibility", "ScrollBarVisibility",
        "ResizeMode", "WindowState",

        // Media / drawing enums
        "Stretch", "AlignmentX", "AlignmentY", "BitmapScalingMode",
        "PenLineCap", "PenLineJoin", "FillRule", "SweepDirection",

        // Custom enums
        "ImageLocation",
    };

    public static string EnsureNameofContent(this string value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (!value.StartsWith("nameof(", StringComparison.Ordinal) ||
            !value.EndsWith(")", StringComparison.Ordinal))
        {
            return $"nameof({value})";
        }

        if (value.StartsWith("nameof(nameof(", StringComparison.Ordinal))
        {
            value = value
                .Replace("nameof(nameof(", "nameof(")
                .Replace("))", ")");
        }

        return value;
    }

    public static string EnsureTypeofContent(this string value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (!value.StartsWith("typeof(", StringComparison.Ordinal) ||
            !value.EndsWith(")", StringComparison.Ordinal))
        {
            return $"typeof({value})";
        }

        if (value.StartsWith("typeof(typeof(", StringComparison.Ordinal))
        {
            value = value
                .Replace("typeof(typeof(", "typeof(")
                .Replace("))", ")");
        }

        return value;
    }

    public static string EnsureFirstCharacterToUpper(this string value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (value.Length == 0 || char.IsUpper(value[0]))
        {
            return value;
        }

        var chars = value.ToCharArray();
        chars[0] = char.ToUpperInvariant(chars[0]);
        return new string(chars);
    }

    public static string EnsureFirstCharacterToLower(this string value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (value.Length == 0 || char.IsLower(value[0]))
        {
            return value;
        }

        var chars = value.ToCharArray();
        chars[0] = char.ToLowerInvariant(chars[0]);
        return new string(chars);
    }

    public static string EnsureCSharpAliasIfNeeded(this string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return typeName;
        }

        // Detect a nullable suffix that applies to the whole type
        var hasNullableSuffix = typeName[typeName.Length - 1] == '?';
        if (hasNullableSuffix)
        {
            typeName = typeName.Substring(0, typeName.Length - 1); // drop '?'
        }

        // Parse / simplify the *core* type name
        var simplified = EnsureCSharpAliasIfNeededParse(typeName);

        // Re‑attach the suffix if it was present
        return hasNullableSuffix
            ? simplified + "?"
            : simplified;
    }

    public static string ExtractInnerContent(this string value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        var start = value.IndexOf('(');
        if (start == -1)
        {
            return value;
        }

        var end = value.LastIndexOf(')');
        return end == -1
            ? value
            : value.Substring(start + 1, end - start - 1);
    }

    public static IDictionary<string, string?> ExtractAttributeConstructorParameters(
        this string value)
    {
        var result = new Dictionary<string, string?>(StringComparer.Ordinal);

        if (string.IsNullOrEmpty(value))
        {
            return result;
        }

        var content = RemoveOuterBrackets(value);
        var parameters = ExtractParameterString(content);

        if (string.IsNullOrEmpty(parameters))
        {
            return result;
        }

        var arguments = SplitArguments(parameters);

        ProcessArguments(arguments, result);

        return result;
    }

    public static string RemoveTaskDotRun(this string value)
    {
        if (string.IsNullOrEmpty(value) ||
            !value.StartsWith("Task.Run(", StringComparison.Ordinal))
        {
            return value;
        }

        value = value.Replace("Task.Run(", string.Empty);
        value = value.Substring(0, value.Length - 1);
        return value;
    }

    public static string RemoveNullableSuffix(this string value)
    {
        if (string.IsNullOrEmpty(value) ||
            !value.EndsWith("?", StringComparison.Ordinal))
        {
            return value;
        }

        return value.Substring(0, value.Length - 1);
    }

    public static string RemovePrefixFromField(this string fieldName)
    {
        if (fieldName is null)
        {
            throw new ArgumentNullException(nameof(fieldName));
        }

        if (fieldName.StartsWith("m_", StringComparison.Ordinal))
        {
            fieldName = fieldName.Substring(2);
        }
        else if (fieldName.StartsWith("_", StringComparison.Ordinal))
        {
            fieldName = fieldName.Substring(1);
        }

        return fieldName;
    }

    public static bool IsKnownValueType(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        if (value.Contains("<"))
        {
            value = value
                .Substring(value.IndexOf('<') + 1)
                .Replace(">", string.Empty);
        }

        return KnownValueTypes.Contains(value.RemoveNullableSuffix());
    }

    public static string TrimNullableForTypeOf(this string typeName)
    {
        if (string.IsNullOrEmpty(typeName) ||
            !typeName.EndsWith("?", StringComparison.Ordinal))
        {
            return typeName;
        }

        var core = typeName.Substring(0, typeName.Length - 1); // chop '?'

        // Keep the '?' for known value types (they need Nullable<T> in typeof)
        // For unknown types, strip the '?' since they are likely reference types
        // where '?' is just a nullability annotation, not Nullable<T>.
        // Note: We intentionally avoid runtime reflection (Type.GetType / AppDomain scanning)
        // because source generators run inside the compiler process where loaded assemblies
        // don't correspond to the target compilation's type system.
        return KnownValueTypes.Contains(core)
            ? typeName
            : core;
    }

    private static string RemoveOuterBrackets(string value)
    {
        if (value.StartsWith("[", StringComparison.Ordinal) &&
            value.EndsWith("]", StringComparison.Ordinal))
        {
            return value.Substring(1, value.Length - 2);
        }

        return value;
    }

    private static string EnsureCSharpAliasIfNeededParse(string typeName)
    {
        // Nullable<T>
        if (typeName.StartsWith("System.Nullable<", StringComparison.Ordinal) ||
            typeName.StartsWith("Nullable<", StringComparison.Ordinal))
        {
            var start = typeName.IndexOf('<') + 1;
            var inner = typeName.Substring(start, typeName.Length - start - 1); // trim '<' … '>'
            return EnsureCSharpAliasIfNeededParse(inner) + "?";
        }

        // Generic type
        var open = typeName.IndexOf('<');
        if (open > 0)
        {
            var outer = EnsureCSharpAliasIfNeededSimplifyToken(typeName.Substring(0, open));

            var argsStart = open + 1;
            var argList = typeName.Substring(argsStart, typeName.Length - argsStart - 1); // drop '>'
            var args = SplitArguments(argList).Select(EnsureCSharpAliasIfNeededParse);

            return outer + "<" + string.Join(", ", args) + ">";
        }

        // Plain (non‑generic) identifier
        return EnsureCSharpAliasIfNeededSimplifyToken(typeName);
    }

    private static string EnsureCSharpAliasIfNeededSimplifyToken(string token)
    {
        var lastDot = token.LastIndexOf('.');

        var core = lastDot >= 0
            ? token.Substring(lastDot + 1)
            : token;

        return TypeAliases.TryGetValue(core, out var alias)
            ? alias
            : core;
    }

    private static string ExtractParameterString(string value)
    {
        var startIndex = value.IndexOf('(');
        if (startIndex < 0)
        {
            return string.Empty;
        }

        var count = 0;
        var endIndex = -1;
        for (var i = startIndex; i < value.Length; i++)
        {
            if (value[i] == '(')
            {
                count++;
            }
            else if (value[i] == ')')
            {
                count--;
                if (count != 0)
                {
                    continue;
                }

                endIndex = i;
                break;
            }
        }

        return endIndex < 0
            ? string.Empty
            : value.Substring(startIndex + 1, endIndex - startIndex - 1);
    }

    private static void ProcessArguments(
        IEnumerable<string> arguments,
        Dictionary<string, string?> result)
    {
        var unnamedIndex = 0;
        foreach (var arg in arguments)
        {
            var trimmedArg = arg.Trim();
            var eqIndex = trimmedArg.IndexOf('=');
            if (eqIndex > 0)
            {
                var key = trimmedArg
                    .Substring(0, eqIndex)
                    .Trim();

                var value = trimmedArg
                    .Substring(eqIndex + 1)
                    .Trim();

                value = StripQuotes(value);

                if (value.StartsWith("[", StringComparison.Ordinal) &&
                    value.EndsWith("]", StringComparison.Ordinal))
                {
                    value = value
                        .Substring(1, value.Length - 2)
                        .Trim();
                }

                result[key] = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
            else
            {
                var key = unnamedIndex == 0 ? "Name" : unnamedIndex.ToString(CultureInfo.InvariantCulture);
                var value = StripQuotes(trimmedArg);

                var match = NameofRegex.Match(value);
                if (match.Success)
                {
                    value = match.Groups["name"].Value;
                }

                result[key] = string.IsNullOrEmpty(value)
                    ? string.Empty
                    : value;

                unnamedIndex++;
            }
        }
    }

    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    private static List<string> SplitArguments(string value)
    {
        var args = new List<string>();
        var currentArg = new StringBuilder();
        var bracketCount = 0;
        var inQuotes = false;
        var quoteChar = '\0';

        foreach (var c in value)
        {
            if (inQuotes)
            {
                currentArg.Append(c);
                if (c == quoteChar)
                {
                    inQuotes = false;
                }

                continue;
            }

            if (c is '"' or '\'')
            {
                inQuotes = true;
                quoteChar = c;
                currentArg.Append(c);
                continue;
            }

            switch (c)
            {
                case '[':
                    bracketCount++;
                    break;
                case ']':
                    bracketCount--;
                    break;
            }

            if (c == ',' && bracketCount == 0 && !inQuotes)
            {
                args.Add(
                    currentArg
                        .ToString()
                        .Trim());
                currentArg.Clear();
            }
            else
            {
                currentArg.Append(c);
            }
        }

        if (currentArg.Length > 0)
        {
            args.Add(
                currentArg
                    .ToString()
                    .Trim());
        }

        return args;
    }

    private static string StripQuotes(string value)
    {
        if ((value.StartsWith("\"", StringComparison.Ordinal) && value.EndsWith("\"", StringComparison.Ordinal)) ||
            (value.StartsWith("'", StringComparison.Ordinal) && value.EndsWith("'", StringComparison.Ordinal)))
        {
            return value.Substring(1, value.Length - 2);
        }

        return value;
    }
}