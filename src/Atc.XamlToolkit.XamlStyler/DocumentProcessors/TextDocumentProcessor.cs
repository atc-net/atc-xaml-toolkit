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

            // Text that directly follows an inline element (e.g. "<Run />world") must stay attached to it:
            // a line break here would be rendered as a space.
            var isAttachedToInlineElement = elementProcessContext.Current.IsSignificantWhiteSpace
                && xmlEncodedContent.Length > 0
                && !char.IsWhiteSpace(xmlEncodedContent[0])
                && output.Length > 0
                && !output.IsNewLine();

            foreach (var line in textLines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.Length > 0)
                {
                    if (isAttachedToInlineElement)
                    {
                        output.Append(trimmedLine);
                        isAttachedToInlineElement = false;
                    }
                    else
                    {
                        output.Append(Environment.NewLine).Append(currentIndentString).Append(trimmedLine);
                    }
                }
            }

            // Remember that whitespace followed the text, so an inline element written next keeps it.
            elementProcessContext.Current.TextTrailingWhitespaceIndex =
                xmlEncodedContent.Length > 0 && char.IsWhiteSpace(xmlEncodedContent[xmlEncodedContent.Length - 1])
                    ? output.Length
                    : -1;
        }

        if (xmlEncodedContent.ContainsOrdinal('\n'))
        {
            elementProcessContext.UpdateParentElementProcessStatus(ContentTypes.MultiLineTextOnly);
        }
    }
}