namespace Atc.XamlToolkit.XamlStyler.DocumentProcessors;

internal sealed class CdataDocumentProcessor : IDocumentProcessor
{
    private readonly IndentService indentService;

    public CdataDocumentProcessor(IndentService indentService)
    {
        this.indentService = indentService;
    }

    public void Process(
        XmlReader xmlReader,
        StringBuilder output,
        ElementProcessContext elementProcessContext)
    {
        if (output.IsNewLine())
        {
            elementProcessContext.UpdateParentElementProcessStatus(ContentTypes.MultiLineTextOnly);
            if (!elementProcessContext.Current.IsPreservingSpace)
            {
                output.Append(indentService.GetIndentString(xmlReader.Depth));
            }
        }
        else
        {
            elementProcessContext.UpdateParentElementProcessStatus(ContentTypes.SingleLineTextOnly);
        }

        output.Append("<![CDATA[")
            .Append(xmlReader.Value.ReplaceOrdinal("\n", Environment.NewLine))
            .Append("]]>");
    }
}