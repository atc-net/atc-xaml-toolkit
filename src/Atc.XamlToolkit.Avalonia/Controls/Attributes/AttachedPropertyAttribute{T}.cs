namespace Atc.XamlToolkit.Controls.Attributes;

/// <summary>
/// Marks a class to generate an Avalonia attached property with an explicitly specified type and name.
/// </summary>
/// <typeparam name="T">The type of the attached property.</typeparam>
/// <example>
/// <code>
/// [AttachedProperty&lt;string&gt;("Watermark", DefaultValue = "")]
/// public static partial class WatermarkBehavior
/// {
/// }
/// </code>
/// Generates:
/// <code>
/// public static readonly AttachedProperty&lt;string&gt; WatermarkProperty = ...
/// public static string GetWatermark(AvaloniaObject element) { ... }
/// public static void SetWatermark(AvaloniaObject element, string value) { ... }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class AttachedPropertyAttribute<T> : StyledPropertyBaseAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AttachedPropertyAttribute{T}"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the attached property to generate.</param>
    public AttachedPropertyAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }

    /// <summary>
    /// Gets the name of the attached property.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Gets the type.
    /// </summary>
    public Type Type { get; } = typeof(T);

    /// <inheritdoc/>
    public override string ToString()
        => $"{nameof(PropertyName)}: {PropertyName}, {nameof(Type)}: {Type}, {base.ToString()}";
}