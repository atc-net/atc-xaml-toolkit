namespace Atc.XamlToolkit.XamlStyler.MarkupExtensions.Formatter;

internal abstract class MarkupExtensionFormatterBase
{
    private readonly MarkupExtensionFormatter markupExtensionFormatter;

    protected MarkupExtensionFormatterBase(
        MarkupExtensionFormatter markupExtensionFormatter)
    {
        this.markupExtensionFormatter = markupExtensionFormatter;
    }

    public IEnumerable<string> FormatArguments(
        MarkupExtension markupExtension,
        bool isNested = false) =>
        markupExtension.Arguments.Any()
            ? Format($"{{{markupExtension.TypeName} ", FormatArguments(markupExtension.Arguments, isNested), "}")
            : new[] { $"{{{markupExtension.TypeName}}}" };

    protected abstract IEnumerable<string> FormatArguments(
        IList<Argument> arguments,
        bool isNested = false);

    protected IEnumerable<string> FormatArgument(
        Argument argument,
        bool isNested = false)
    {
        if (argument is NamedArgument namedArgument)
        {
            return FormatNamedArgument(namedArgument);
        }

        if (argument is PositionalArgument positionalArgument)
        {
            if (positionalArgument.Value is MarkupExtension extension)
            {
                return markupExtensionFormatter.Format(extension, isNested);
            }

            return FormatPositionalArgument(positionalArgument);
        }

        throw new ArgumentException($"Unhandled argument type {argument.GetType().FullName}", nameof(argument));
    }

    private static IEnumerable<string> Format(
        string prefix,
        IEnumerable<string> lines,
        string? suffix = null)
    {
        var list = new List<string>();
        var lineList = lines.ToList();

        var queued = prefix + lineList[0];
        foreach (var line in lineList.Skip(1))
        {
            list.Add(queued);
            queued = new string(' ', prefix.Length) + line;
        }

        list.Add(queued + suffix);
        return list;
    }

    private IEnumerable<string> FormatNamedArgument(NamedArgument namedArgument)
        => Format($"{namedArgument.Name}=", FormatValue(namedArgument.Value));

    private IEnumerable<string> FormatPositionalArgument(
        PositionalArgument positionalArgument) =>
        FormatValue(positionalArgument.Value);

    private IEnumerable<string> FormatValue(Value value)
    {
        if (value is LiteralValue literalValue)
        {
            return new[] { literalValue.Value };
        }

        if (value is MarkupExtension ext)
        {
            return markupExtensionFormatter.Format(ext, isNested: true);
        }

        throw new ArgumentException($"Unhandled value type {value.GetType().FullName}", nameof(value));
    }
}