// ReSharper disable RedundantAttributeUsageProperty
// ReSharper disable UnusedMember.Global
namespace Atc.XamlToolkit.Controls.Attributes;

[SuppressMessage("Performance", "CA1813:Avoid unsealed attributes", Justification = "OK - inherits for AttachedPropertyAttribute")]
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class DependencyPropertyAttribute : DependencyPropertyBaseAttribute
{
    /// <summary>
    /// Gets or sets the name of the generated property. By default it is the name of the
    /// field with its first letter in upper case. State it when the field cannot have that
    /// name, for example because the class has locals or parameters of the same name.
    /// </summary>
    public string? PropertyName { get; set; }
}