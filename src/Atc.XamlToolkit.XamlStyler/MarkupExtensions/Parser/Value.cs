namespace Atc.XamlToolkit.XamlStyler.MarkupExtensions.Parser;

/// <summary>
/// Base class for values in a markup extension: either a literal or a nested markup extension.
/// </summary>
public abstract class Value
{
    /// <summary>
    /// Converts a string to a <see cref="LiteralValue"/>.
    /// </summary>
    /// <param name="value">The string value.</param>
    public static implicit operator Value(string value)
    {
        return new LiteralValue(value);
    }

    /// <summary>
    /// Creates a <see cref="LiteralValue"/> from a string.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>A new <see cref="LiteralValue"/>.</returns>
    public static Value FromString(string value) => new LiteralValue(value);
}