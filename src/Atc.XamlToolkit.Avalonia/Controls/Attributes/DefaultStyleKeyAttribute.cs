namespace Atc.XamlToolkit.Controls.Attributes;

/// <summary>
/// Makes a control look up its default style by a type: the control's own type, or the
/// type given to the attribute. The source generator emits the platform's boilerplate.
/// </summary>
/// <remarks>
/// Generates <c>protected override Type StyleKeyOverride =&gt; typeof(...)</c>.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class DefaultStyleKeyAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultStyleKeyAttribute"/> class
    /// that uses the annotated control's own type as the style key.
    /// </summary>
    public DefaultStyleKeyAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultStyleKeyAttribute"/> class
    /// that uses another type as the style key, for example the base control whose style is reused.
    /// </summary>
    /// <param name="styleType">The type to use as the style key.</param>
    public DefaultStyleKeyAttribute(Type styleType)
        => StyleType = styleType;

    /// <summary>
    /// Gets the type to use as the style key; <see langword="null"/> means the annotated control's own type.
    /// </summary>
    public Type? StyleType { get; }
}