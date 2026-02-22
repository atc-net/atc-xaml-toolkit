namespace Atc.XamlToolkit.Metadata;

/// <summary>
/// Specifies whether a property should be visible in dynamic property editors.
/// Use <c>[PropertyBrowsable(false)]</c> to explicitly hide a property that would otherwise be included.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class PropertyBrowsableAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyBrowsableAttribute"/> class.
    /// </summary>
    /// <param name="browsable">Whether the property is browsable.</param>
    public PropertyBrowsableAttribute(bool browsable)
    {
        Browsable = browsable;
    }

    /// <summary>
    /// Gets a value indicating whether the property is browsable in dynamic property editors.
    /// </summary>
    public bool Browsable { get; }

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(Browsable)}: {Browsable}";
}