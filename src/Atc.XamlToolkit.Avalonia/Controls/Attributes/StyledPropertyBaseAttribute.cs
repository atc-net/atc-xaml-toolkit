namespace Atc.XamlToolkit.Controls.Attributes;

/// <summary>
/// Base attribute for Avalonia styled property attributes.
/// Simplified version without WPF-specific features (coercion, validation, flags, etc.).
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, Inherited = false)]
public abstract class StyledPropertyBaseAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the default value for the property.
    /// </summary>
    public object? DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets the name of the property changed callback method.
    /// </summary>
    public string? PropertyChangedCallback { get; set; }

    /// <summary>
    /// Gets or sets the category for design-time tools.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets the description for design-time tools.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to emit a default
    /// <c>/// &lt;summary&gt;</c> XML comment on the generated CLR property.
    /// When the property has no field-level XML doc, the generator emits
    /// <c>Gets or sets the {Name}.</c> for instance properties, or
    /// <c>Gets the {Name} attached property value.</c> /
    /// <c>Sets the {Name} attached property value.</c> for static accessors.
    /// Field-level XML doc comments always win over the default summary.
    /// Defaults to <c>false</c>; can be flipped per-assembly via
    /// <c>[assembly: GenerateDocumentationDefault]</c>.
    /// </summary>
    public bool GenerateDocumentation { get; set; }
}