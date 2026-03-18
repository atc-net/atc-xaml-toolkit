namespace Atc.XamlToolkit.XamlStyler.DocumentProcessors;

internal sealed class EndElementDocumentProcessor : IDocumentProcessor
{
    private readonly IXamlStylerOptions options;
    private readonly IndentService indentService;

    public EndElementDocumentProcessor(
        IXamlStylerOptions options,
        IndentService indentService)
    {
        this.options = options;
        this.indentService = indentService;
    }

    public void Process(
        XmlReader xmlReader,
        StringBuilder output,
        ElementProcessContext elementProcessContext)
    {
        if (elementProcessContext.Current.IsPreservingSpace)
        {
            output.Append("</").Append(xmlReader.Name).Append('>');
        }
        else if (elementProcessContext.Current.IsSignificantWhiteSpace && !output.IsNewLine())
        {
            output.Append("</").Append(xmlReader.Name).Append('>');
        }
        else if (elementProcessContext.Current.ContentType == ContentTypes.None
            && options.RemoveEndingTagOfEmptyElement)
        {
            output = output.TrimEnd(' ', '\t', '\r', '\n');

            var bracketIndex = output.LastIndexOf('>');
            output.Insert(bracketIndex, '/');

            if (output[bracketIndex - 1] != '\t'
                && output[bracketIndex - 1] != ' '
                && options.SpaceBeforeClosingSlash)
            {
                output.Insert(bracketIndex, ' ');
            }
        }
        else if (elementProcessContext.Current.ContentType == ContentTypes.SingleLineTextOnly
            && !elementProcessContext.Current.IsMultlineStartTag)
        {
            var bracketIndex = output.LastIndexOf('>');
            var text = output.Substring(bracketIndex + 1, output.Length - bracketIndex - 1).Trim();

            output.Length = bracketIndex + 1;
            output.Append(text).Append("</").Append(xmlReader.Name).Append('>');
        }
        else
        {
            var currentIndentString = indentService.GetIndentString(xmlReader.Depth);

            if (!output.IsNewLine())
            {
                output.Append(Environment.NewLine);
            }

            output.Append(currentIndentString).Append("</").Append(xmlReader.Name).Append('>');
        }

        elementProcessContext.Pop();
    }
}