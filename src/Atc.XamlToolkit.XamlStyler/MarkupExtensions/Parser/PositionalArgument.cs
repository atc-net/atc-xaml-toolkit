namespace Atc.XamlToolkit.XamlStyler.MarkupExtensions.Parser;

/// <summary>
/// A positional (unnamed) argument in a markup extension.
/// </summary>
public sealed class PositionalArgument : Argument
{
    /// <summary>Gets the argument value.</summary>
    public Value Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PositionalArgument"/> class.
    /// </summary>
    /// <param name="value">The argument value.</param>
    public PositionalArgument(Value value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString() ?? string.Empty;
}