namespace Atc.XamlToolkit.XamlStyler.DocumentProcessors;

internal sealed class ProcessInstructionDocumentProcessor : IDocumentProcessor
{
    private readonly IndentService indentService;

    public ProcessInstructionDocumentProcessor(IndentService indentService)
    {
        this.indentService = indentService;
    }

    public void Process(
        XmlReader xmlReader,
        StringBuilder output,
        ElementProcessContext elementProcessContext)
    {
        elementProcessContext.UpdateParentElementProcessStatus(ContentTypes.Mixed);

        var currentIndentString = indentService.GetIndentString(xmlReader.Depth);

        if (!output.IsNewLine())
        {
            output.Append(Environment.NewLine);
        }

        output.Append(currentIndentString)
            .Append("<?")
            .Append(xmlReader.Name)
            .Append(' ')
            .Append(xmlReader.Value)
            .Append("?>");
    }
}