namespace Atc.XamlToolkit.Controls.Attributes;

/// <summary>
/// Marks a class to generate an Avalonia styled property with an explicitly specified type and name.
/// Multiple attributes can be applied to generate multiple properties.
/// </summary>
/// <typeparam name="T">The type of the property.</typeparam>
/// <example>
/// <code>
/// [StyledProperty&lt;string&gt;("Title", DefaultValue = "")]
/// [StyledProperty&lt;int&gt;("Count", DefaultValue = 0)]
/// public partial class MyControl : UserControl
/// {
/// }
/// </code>
/// Generates:
/// <code>
/// public static readonly StyledProperty&lt;string&gt; TitleProperty = ...
/// public string Title { get; set; }
/// public static readonly StyledProperty&lt;int&gt; CountProperty = ...
/// public int Count { get; set; }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class StyledPropertyAttribute<T> : StyledPropertyBaseAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StyledPropertyAttribute{T}"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the property to generate.</param>
    public StyledPropertyAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }

    /// <summary>
    /// Gets the name of the property.
    /// </summary>
    public string PropertyName { get; }
}