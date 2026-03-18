namespace Atc.XamlToolkit.XamlStyler.DocumentProcessors;

internal sealed class TextDocumentProcessor : IDocumentProcessor
{
    private readonly IndentService indentService;

    public TextDocumentProcessor(IndentService indentService)
    {
        this.indentService = indentService;
    }

    public void Process(
        XmlReader xmlReader,
        StringBuilder output,
        ElementProcessContext elementProcessContext)
    {
        elementProcessContext.UpdateParentElementProcessStatus(ContentTypes.SingleLineTextOnly);

        var xmlEncodedContent = xmlReader.Value.ToXmlEncodedString(ignoreCarrier: true);
        if (elementProcessContext.Current.IsPreservingSpace)
        {
            output.Append(xmlEncodedContent.ReplaceOrdinal("\n", Environment.NewLine));
        }
        else
        {
            var currentIndentString = indentService.GetIndentString(xmlReader.Depth);
            var textLines = xmlEncodedContent.Trim()
                .Split('\n')
                .Where(s => s.Trim().Length > 0)
                .ToList();

            foreach (var line in textLines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.Length > 0)
                {
                    output.Append(Environment.NewLine).Append(currentIndentString).Append(trimmedLine);
                }
            }
        }

        if (xmlEncodedContent.ContainsOrdinal('\n'))
        {
            elementProcessContext.UpdateParentElementProcessStatus(ContentTypes.MultiLineTextOnly);
        }
    }
}