namespace Atc.XamlToolkit.Controls.Attributes;

/// <summary>
/// Makes a control look up its default style by a type: the control's own type, or the
/// type given to the attribute. The source generator emits the platform's boilerplate.
/// </summary>
/// <remarks>
/// Generates a static constructor that calls <c>DefaultStyleKeyProperty.OverrideMetadata(...)</c>.
/// A class can only have one static constructor, so when the class already has one, a
/// <c>private static void OverrideDefaultStyleKey()</c> method is generated instead; call it
/// from that constructor (diagnostic AtcXamlToolkit0011 is reported if it is not called).
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