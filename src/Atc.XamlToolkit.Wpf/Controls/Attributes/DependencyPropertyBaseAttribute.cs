namespace Atc.XamlToolkit.Controls.Attributes;

public abstract class DependencyPropertyBaseAttribute : Attribute
{
    public object? DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets the default value as C# source text that is emitted unchanged, for a
    /// default that is not a constant: a method call, a static member or a constructor,
    /// e.g. <c>"CreateFrozenBrush(40)"</c> or <c>"new Thickness(1)"</c>. Unlike
    /// <see cref="DefaultValue"/> the text is never quoted or rewritten to fit the property
    /// type. When both are set, the expression is used. The value is created once and
    /// shared by all instances, so it should be immutable (a frozen brush, an empty array).
    /// </summary>
    public string? DefaultValueExpression { get; set; }

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
    /// Gets or sets a value indicating whether a typed coerce method is generated:
    /// <c>private partial T Coerce{Name}(T value)</c>, or for an attached property
    /// <c>private static partial T Coerce{Name}(DependencyObject d, T value)</c>.
    /// The generator registers the static <c>CoerceValueCallback</c> and calls the method,
    /// which the class must implement (a method that returns a value cannot be left out).
    /// Can be combined with <see cref="CoerceValueCallback"/>, which then runs first.
    /// </summary>
    public bool GenerateCoerceHook { get; set; }

    /// <summary>
    /// Gets or sets the type whose existing dependency property of the same name is shared,
    /// instead of registering a new one. The generator emits
    /// <c>{Type}.{Name}Property.AddOwner(typeof(Owner))</c> and the CLR property. Metadata is
    /// only passed when a default value, a callback or another metadata option is set, and
    /// is then a <c>FrameworkPropertyMetadata</c>. <see cref="ValidateValueCallback"/> does
    /// not apply, because validation belongs to the registration of the shared property.
    /// </summary>
    public Type? AddOwnerOf { get; set; }

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

    /// <summary>
    /// Gets or sets the accessibility of the generated members: the property identifier
    /// field and the CLR property, or the <c>Get{Name}</c> / <c>Set{Name}</c> methods of an
    /// attached property. The default is <see cref="PropertyAccessModifier.Public"/>.
    /// <see cref="SetterAccessModifier"/> can restrict the setter further.
    /// </summary>
    public PropertyAccessModifier AccessModifier { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the CLR property, or the <c>Get{Name}</c> /
    /// <c>Set{Name}</c> methods of an attached property, are generated. Set it to
    /// <c>false</c> to generate only the property identifier field, for a property that is
    /// read and written with <c>GetValue</c> / <c>SetValue</c>. The default is <c>true</c>.
    /// </summary>
    public bool GenerateClrAccessors { get; set; } = true;

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