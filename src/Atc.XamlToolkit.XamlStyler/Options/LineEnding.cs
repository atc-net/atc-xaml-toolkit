namespace Atc.XamlToolkit.XamlStyler.Options;

/// <summary>
/// Controls line ending style in formatted output.
/// </summary>
public enum LineEnding
{
    /// <summary>Use the system default line ending.</summary>
    Auto,

    /// <summary>Use LF (\n) line endings.</summary>
    LF,

    /// <summary>Use CRLF (\r\n) line endings.</summary>
    CRLF,
}