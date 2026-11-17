namespace Atc.XamlToolkit.XamlStyler.DocumentProcessors;

internal sealed class WhitespaceDocumentProcessor : IDocumentProcessor
{
    public void Process(
        XmlReader xmlReader,
        StringBuilder output,
        ElementProcessContext elementProcessContext)
    {
        var hasNewline = xmlReader.Value.ContainsOrdinal('\n');

        if (elementProcessContext.Current.IsSignificantWhiteSpace && hasNewline)
        {
            elementProcessContext.Current.IsSignificantWhiteSpace = false;
        }

        if (hasNewline && !elementProcessContext.Current.IsPreservingSpace)
        {
            output.Append(xmlReader.Value
                .ReplaceOrdinal(" ", string.Empty)
                .ReplaceOrdinal("\t", string.Empty)
                .ReplaceOrdinal("\r", string.Empty)
                .ReplaceOrdinal("\n", Environment.NewLine));
        }
        else if (!hasNewline
            && !elementProcessContext.Current.IsPreservingSpace
            && !elementProcessContext.Current.IsSignificantWhiteSpace)
        {
            // Spaces between block elements on one line carry no meaning. Writing them would leave
            // trailing spaces once the next element moves to its own line.
        }
        else
        {
            output.Append(xmlReader.Value.ReplaceOrdinal("\n", Environment.NewLine));
        }
    }
}