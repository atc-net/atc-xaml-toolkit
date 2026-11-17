namespace Atc.XamlToolkit.XamlStyler.MarkupExtensions.Formatter;

internal sealed class AttributeInfoFormatter
{
    private readonly MarkupExtensionFormatter formatter;
    private readonly IndentService indentService;

    public AttributeInfoFormatter(
        MarkupExtensionFormatter formatter,
        IndentService indentService)
    {
        this.formatter = formatter;
        this.indentService = indentService;
    }

    public string ToMultiLineString(
        AttributeInfo attrInfo,
        string baseIndentationString)
    {
        if (!attrInfo.IsMarkupExtension)
        {
            return $"{attrInfo.Name}=\"{attrInfo.Value}\"";
        }

        var currentIndentationString = $"{baseIndentationString}{string.Empty.PadLeft(attrInfo.Name.Length + 2, ' ')}";
        var lines = formatter.Format(attrInfo.MarkupExtension!).Select(EscapeQuotes).ToList();

        var buffer = new StringBuilder();
        buffer.Append(attrInfo.Name).Append("=\"").Append(lines[0]);

        foreach (var line in lines.Skip(1))
        {
            buffer.AppendLine();
            buffer.Append(indentService.Normalize(currentIndentationString + line));
        }

        buffer.Append('"');
        return buffer.ToString();
    }

    public string ToSingleLineString(
        AttributeInfo attrInfo,
        XamlLanguageOptions xamlLanguageOptions)
    {
        var valuePart = attrInfo.IsMarkupExtension
            ? EscapeQuotes(formatter.FormatSingleLine(attrInfo.MarkupExtension!))
            : attrInfo.Value.ToXmlEncodedString(xamlLanguageOptions.UnescapedAttributeCharacters);

        return $"{attrInfo.Name}=\"{valuePart}\"";
    }

    /// <summary>
    /// The attribute is always written with double quotes, so a double quote inside a markup
    /// extension (possible when the source attribute used single quotes) has to be escaped.
    /// </summary>
    private static string EscapeQuotes(string value) =>
        value.ReplaceOrdinal("\"", "&quot;");
}