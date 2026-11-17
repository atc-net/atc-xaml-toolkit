namespace Atc.XamlToolkit.XamlStyler.Model;

internal sealed class ElementProcessStatus
{
    public ElementProcessStatus? Parent { get; set; }

    public string? Name { get; set; }

    public ContentTypes ContentType { get; set; }

    public bool IsMultlineStartTag { get; set; }

    public bool IsPreservingSpace { get; set; }

    public bool IsSignificantWhiteSpace { get; set; }

    /// <summary>
    /// Gets or sets the index in the output of the '&gt;' that closes the start tag, or -1 when not recorded.
    /// </summary>
    public int StartTagEndIndex { get; set; } = -1;

    /// <summary>
    /// Gets or sets the output length right after a text node that ended with whitespace, or -1.
    /// An inline element written at exactly this position must be separated from the text by a space.
    /// </summary>
    public int TextTrailingWhitespaceIndex { get; set; } = -1;
}