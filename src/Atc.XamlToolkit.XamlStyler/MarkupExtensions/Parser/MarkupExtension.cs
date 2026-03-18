namespace Atc.XamlToolkit.XamlStyler.MarkupExtensions.Parser;

/// <summary>
/// Represents a parsed XAML markup extension (e.g., {Binding Path=Name}).
/// </summary>
public sealed class MarkupExtension : Value
{
    /// <summary>Gets the type name of the markup extension.</summary>
    public string TypeName { get; }

    /// <summary>Gets the list of arguments.</summary>
    public IList<Argument> Arguments { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkupExtension"/> class.
    /// </summary>
    /// <param name="typeName">The markup extension type name.</param>
    /// <param name="arguments">The arguments.</param>
    public MarkupExtension(
        string typeName,
        params Argument[] arguments)
    {
        TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
        Arguments = new List<Argument>(arguments ?? throw new ArgumentNullException(nameof(arguments)));
    }
}