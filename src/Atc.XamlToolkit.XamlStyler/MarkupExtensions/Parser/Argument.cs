namespace Atc.XamlToolkit.XamlStyler.MarkupExtensions.Parser;

/// <summary>
/// Base class for markup extension arguments.
/// </summary>
[SuppressMessage("Design", "S1694:An abstract class should have both abstract and concrete methods", Justification = "Serves as a marker base type with ToString override.")]
public abstract class Argument
{
    /// <summary>
    /// Gets the string representation of this argument.
    /// </summary>
    /// <returns>The string representation.</returns>
    public abstract override string ToString();
}