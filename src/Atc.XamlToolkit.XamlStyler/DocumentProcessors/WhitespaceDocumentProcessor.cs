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
        else
        {
            output.Append(xmlReader.Value.ReplaceOrdinal("\n", Environment.NewLine));
        }
    }
}