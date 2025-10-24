// ReSharper disable RedundantAttributeUsageProperty
// ReSharper disable UnusedMember.Global
namespace Atc.XamlToolkit.Controls.Attributes;

[SuppressMessage("Performance", "CA1813:Avoid unsealed attributes", Justification = "OK - inherits for AttachedPropertyAttribute")]
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class DependencyPropertyAttribute : DependencyPropertyBaseAttribute;