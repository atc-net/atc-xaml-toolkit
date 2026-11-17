namespace Atc.XamlToolkit.Controls.Attributes;

public abstract class DependencyPropertyBaseAttribute : Attribute
{
    public object? DefaultValue { get; set; }

    public string? PropertyChangedCallback { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a typed partial method is generated that is
    /// called when the property value changes: <c>partial void On{Name}Changed(T oldValue, T newValue)</c>,
    /// or for an attached property <c>static partial void On{Name}Changed(target, T oldValue, T newValue)</c>.
    /// Implement the method in the class to react to the change; when it is not implemented,
    /// the call is removed by the compiler. Can be combined with <see cref="PropertyChangedCallback"/>,
    /// which then runs first.
    /// </summary>
    public bool GeneratePartialHooks { get; set; }

    /// <summary>
    /// Gets or sets the accessibility of the generated CLR property's setter. The default is
    /// <see cref="PropertyAccessModifier.Public"/>. This only restricts the C# setter: the
    /// property is still an ordinary, writable property to XAML and to <c>SetValue</c>.
    /// </summary>
    public PropertyAccessModifier SetterAccessModifier { get; set; }

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