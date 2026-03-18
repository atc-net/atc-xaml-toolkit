namespace Atc.XamlToolkit.XamlStyler.DocumentProcessors;

internal sealed class SignificantWhitespaceDocumentProcessor : IDocumentProcessor
{
    public void Process(
        XmlReader xmlReader,
        StringBuilder output,
        ElementProcessContext elementProcessContext)
    {
        output.Append(xmlReader.Value.ReplaceOrdinal("\n", Environment.NewLine));
    }
}