namespace Atc.XamlToolkit.XamlStyler.DocumentProcessors;

internal sealed class XmlDeclarationDocumentProcessor : IDocumentProcessor
{
    public void Process(
        XmlReader xmlReader,
        StringBuilder output,
        ElementProcessContext elementProcessContext)
    {
        output.Append("<?xml ").Append(xmlReader.Value.Trim()).Append(" ?>");
    }
}