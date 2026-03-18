namespace Atc.XamlToolkit.XamlStyler.DocumentProcessors;

internal sealed class CommentDocumentProcessor : IDocumentProcessor
{
    private readonly IXamlStylerOptions options;
    private readonly IndentService indentService;

    public CommentDocumentProcessor(
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
        elementProcessContext.UpdateParentElementProcessStatus(ContentTypes.Mixed);

        var currentIndentString = indentService.GetIndentString(xmlReader.Depth);
        var content = xmlReader.Value;

        if (output.Length > 0 && !output.IsNewLine())
        {
            output.Append(Environment.NewLine);
        }

        if (content.ContainsOrdinal('<') && content.ContainsOrdinal('>'))
        {
            output.Append(currentIndentString).Append("<!--");

            if (content.ContainsOrdinal('\n'))
            {
                output.Append(string.Join(
                    Environment.NewLine,
                    content.GetLines().Select(l => l.TrimEnd(' '))));

                if (content.TrimEnd(' ').EndsWith("\n", StringComparison.Ordinal))
                {
                    output.Append(currentIndentString);
                }
            }
            else
            {
                output.Append(content);
            }

            output.Append("-->");
        }
        else if (content.ContainsOrdinal("#region")
            || content.ContainsOrdinal("#endregion"))
        {
            output.Append(currentIndentString).Append("<!--").Append(content.Trim()).Append("-->");
        }
        else if (content.ContainsOrdinal('\n'))
        {
            output.Append(currentIndentString).Append("<!--");

            var contentIndentString = indentService.GetIndentString(xmlReader.Depth + 1);
            foreach (var line in content.Trim().GetLines())
            {
                output.Append(Environment.NewLine).Append(contentIndentString).Append(line.Trim());
            }

            output.Append(Environment.NewLine).Append(currentIndentString).Append("-->");
        }
        else
        {
            output
                .Append(currentIndentString)
                .Append("<!--")
                .Append(' ', options.CommentSpaces)
                .Append(content.Trim())
                .Append(' ', options.CommentSpaces)
                .Append("-->");
        }
    }
}