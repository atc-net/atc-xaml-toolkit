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
        var lines = formatter.Format(attrInfo.MarkupExtension!).ToList();

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
            ? formatter.FormatSingleLine(attrInfo.MarkupExtension!)
            : attrInfo.Value.ToXmlEncodedString(xamlLanguageOptions.UnescapedAttributeCharacters);

        return $"{attrInfo.Name}=\"{valuePart}\"";
    }
}