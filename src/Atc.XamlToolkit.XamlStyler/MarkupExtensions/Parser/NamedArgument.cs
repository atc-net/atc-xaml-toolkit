namespace Atc.XamlToolkit.XamlStyler.MarkupExtensions.Parser;

/// <summary>
/// A named argument in a markup extension (e.g., Path=Name).
/// </summary>
public sealed class NamedArgument : Argument
{
    /// <summary>Gets the argument name.</summary>
    public string Name { get; }

    /// <summary>Gets the argument value.</summary>
    public Value Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedArgument"/> class.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <param name="value">The argument value.</param>
    public NamedArgument(
        string name,
        Value value)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <inheritdoc />
    public override string ToString() => $"{Name}={Value}";
}