namespace Atc.XamlToolkit.Controls.Attributes;

public abstract class DependencyPropertyBaseAttribute : Attribute
{
    public object? DefaultValue { get; set; }

    public string? PropertyChangedCallback { get; set; }

    public string? Category { get; set; }

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

    public override string ToString()
        => $"{nameof(DefaultValue)}: {DefaultValue}, {nameof(PropertyChangedCallback)}: {PropertyChangedCallback}, {nameof(Category)}: {Category}, {nameof(Description)}: {Description}, {nameof(GenerateDocumentation)}: {GenerateDocumentation}";
}