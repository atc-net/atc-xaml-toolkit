namespace Atc.XamlToolkit.XamlStyler.MarkupExtensions.Formatter;

internal sealed class MarkupExtensionFormatter
{
    private readonly IList<string> singleLineTypes;
    private readonly MarkupExtensionFormatterBase singleLineFormatter;
    private readonly MarkupExtensionFormatterBase multiLineFormatter;

    public MarkupExtensionFormatter(IList<string> singleLineTypes)
    {
        this.singleLineTypes = singleLineTypes;
        singleLineFormatter = new SingleLineMarkupExtensionFormatter(this);
        multiLineFormatter = new MultiLineMarkupExtensionFormatter(this);
    }

    public IEnumerable<string> Format(
        MarkupExtension markupExtension,
        bool isNested = false)
    {
        var formatter = (isNested || singleLineTypes.Contains(markupExtension.TypeName, StringComparer.Ordinal))
            ? singleLineFormatter
            : multiLineFormatter;
        return formatter.FormatArguments(markupExtension, isNested);
    }

    public string FormatSingleLine(MarkupExtension markupExtension) =>
        singleLineFormatter.FormatArguments(markupExtension).Single();
}