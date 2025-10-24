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
}