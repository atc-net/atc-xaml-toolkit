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
    /// Gets or sets a value indicating whether the property is a read-only dependency
    /// property. It is then registered with <c>RegisterReadOnly</c> (or
    /// <c>RegisterAttachedReadOnly</c>): the generator emits a private
    /// <c>{Name}PropertyKey</c>, the public <c>{Name}Property</c> taken from the key, and a
    /// setter that writes through the key. The setter is <c>private</c> unless
    /// <see cref="SetterAccessModifier"/> says otherwise. Nothing outside the class can set
    /// the property - not XAML, a style or a binding either.
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Gets or sets the accessibility of the generated CLR property's setter. The default is
    /// <see cref="PropertyAccessModifier.Public"/>. This only restricts the C# setter: the
    /// property is still an ordinary, writable property to XAML and to <c>SetValue</c>.
    /// </summary>
    public PropertyAccessModifier SetterAccessModifier { get; set; }

    public string? CoerceValueCallback { get; set; }

    public string? ValidateValueCallback { get; set; }

    public FrameworkPropertyMetadataOptions Flags { get; set; }

    public UpdateSourceTrigger DefaultUpdateSourceTrigger { get; set; }

    public bool IsAnimationProhibited { get; set; }

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
        => $"{nameof(DefaultValue)}: {DefaultValue}, {nameof(PropertyChangedCallback)}: {PropertyChangedCallback}, {nameof(CoerceValueCallback)}: {CoerceValueCallback}, {nameof(ValidateValueCallback)}: {ValidateValueCallback}, {nameof(Flags)}: {Flags}, {nameof(DefaultUpdateSourceTrigger)}: {DefaultUpdateSourceTrigger}, {nameof(IsAnimationProhibited)}: {IsAnimationProhibited}, {nameof(Category)}: {Category}, {nameof(Description)}: {Description}, {nameof(GenerateDocumentation)}: {GenerateDocumentation}";
}