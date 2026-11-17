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
    /// Gets or sets the default value as C# source text that is emitted unchanged, for a
    /// default that is not a constant: a method call, a static member or a constructor,
    /// e.g. <c>"CreateFrozenBrush(40)"</c> or <c>"new Thickness(1)"</c>. Unlike
    /// <see cref="DefaultValue"/> the text is never quoted or rewritten to fit the property
    /// type. When both are set, the expression is used. The value is created once and
    /// shared by all instances, so it should be immutable (a frozen brush, an empty array).
    /// </summary>
    public string? DefaultValueExpression { get; set; }

    /// <summary>
    /// Gets or sets the name of the property changed callback method.
    /// </summary>
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