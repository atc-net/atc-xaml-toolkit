namespace Atc.XamlToolkit.Controls.Attributes;

/// <summary>
/// Marks a private field to be transformed into a public Avalonia styled property.
/// The property name is inferred from the field name by removing common prefixes and capitalizing.
/// </summary>
/// <example>
/// <code>
/// [StyledProperty(PropertyChangedCallback = nameof(OnNameChanged))]
/// private string name;
/// </code>
/// Generates:
/// <code>
/// public static readonly StyledProperty&lt;string&gt; NameProperty = ...
/// public string Name { get; set; }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field, Inherited = false)]
public sealed class StyledPropertyAttribute : StyledPropertyBaseAttribute
{
    /// <summary>
    /// Gets or sets the name of the generated property. By default it is the name of the
    /// field with its first letter in upper case. State it when the field cannot have that
    /// name, for example because the class has locals or parameters of the same name.
    /// </summary>
    public string? PropertyName { get; set; }
}