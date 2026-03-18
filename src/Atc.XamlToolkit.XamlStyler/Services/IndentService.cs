namespace Atc.XamlToolkit.XamlStyler.Services;

internal sealed class IndentService
{
    private readonly bool indentWithTabs;
    private readonly int indentSize;
    private readonly AttributeIndentationStyle attributeIndentationStyle;

    public IndentService(IXamlStylerOptions options)
    {
        indentWithTabs = options.IndentWithTabs ?? false;
        indentSize = options.IndentSize;
        attributeIndentationStyle = options.AttributeIndentationStyle;
    }

    public string GetIndentString(int depth)
    {
        if (depth < 0)
        {
            depth = 0;
        }

        return indentWithTabs
            ? new string('\t', depth)
            : new string(' ', depth * indentSize);
    }

    public string GetIndentString(
        int depth,
        int additionalSpaces)
    {
        if (depth < 0)
        {
            depth = 0;
        }

        if (additionalSpaces < 0)
        {
            additionalSpaces = 0;
        }

        if (indentWithTabs)
        {
            return attributeIndentationStyle switch
            {
                AttributeIndentationStyle.Mixed => new string('\t', depth + (additionalSpaces / indentSize)) +
                                                    new string(' ', additionalSpaces % indentSize),
                AttributeIndentationStyle.Spaces => new string('\t', depth) +
                                                     new string(' ', additionalSpaces),
                _ => throw new NotImplementedException($"Unhandled {nameof(AttributeIndentationStyle)}: {attributeIndentationStyle}"),
            };
        }

        return new string(' ', (depth * indentSize) + additionalSpaces);
    }

    /// <summary>
    /// Replace blocks of "indentSize" consecutive spaces with a tab at the beginning of the line.
    /// </summary>
    /// <param name="line">The line to normalize.</param>
    /// <returns>The normalized line.</returns>
    public string Normalize(string line)
    {
        if (!indentWithTabs || attributeIndentationStyle != AttributeIndentationStyle.Mixed)
        {
            return line;
        }

        var runningSpaces = 0;
        var position = 0;
        while (position < line.Length)
        {
            switch (line[position])
            {
                case ' ':
                    runningSpaces++;
                    if (runningSpaces == indentSize)
                    {
#pragma warning disable CA1845 // Use span-based string.Concat - not available on netstandard2.0
                        line = line.Substring(0, position + 1 - runningSpaces) + '\t' + line.Substring(position + 1);
#pragma warning restore CA1845
                        position -= runningSpaces - 1;
                        runningSpaces = 0;
                    }

                    break;

                case '\t':
                    if (runningSpaces != 0)
                    {
                        return line;
                    }

                    break;

                default:
                    return line;
            }

            position++;
        }

        return line;
    }
}