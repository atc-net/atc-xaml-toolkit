namespace Atc.XamlToolkit.XamlStyler.MarkupExtensions.Parser;

/// <summary>
/// A literal string value in a markup extension argument.
/// </summary>
public sealed class LiteralValue : Value
{
    /// <summary>Gets the string value.</summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LiteralValue"/> class.
    /// </summary>
    /// <param name="value">The literal string value.</param>
    public LiteralValue(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }
}