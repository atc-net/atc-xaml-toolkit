namespace Atc.XamlToolkit.XamlStyler.DocumentProcessors;

/// <summary>
/// Interface for document node processors.
/// </summary>
internal interface IDocumentProcessor
{
    /// <summary>
    /// Processes the current node in the XML reader and appends formatted output.
    /// </summary>
    /// <param name="xmlReader">The XML reader positioned at the node to process.</param>
    /// <param name="output">The output buffer.</param>
    /// <param name="elementProcessContext">The element processing context.</param>
    void Process(
        XmlReader xmlReader,
        StringBuilder output,
        ElementProcessContext elementProcessContext);
}