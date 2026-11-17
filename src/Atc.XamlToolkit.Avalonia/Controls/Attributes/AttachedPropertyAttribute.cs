namespace Atc.XamlToolkit.Controls.Attributes;

/// <summary>
/// Marks a private static field to be transformed into an Avalonia attached property.
/// The property name is inferred from the field name by removing common prefixes and capitalizing.
/// </summary>
/// <example>
/// <code>
/// public static partial class MyBehavior
/// {
///     [AttachedProperty]
///     private static bool isEnabled;
/// }
/// </code>
/// Generates:
/// <code>
/// public static readonly AttachedProperty&lt;bool&gt; IsEnabledProperty = ...
/// public static bool GetIsEnabled(AvaloniaObject element) { ... }
/// public static void SetIsEnabled(AvaloniaObject element, bool value) { ... }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field, Inherited = false)]
public sealed class AttachedPropertyAttribute : StyledPropertyBaseAttribute
{
    /// <summary>
    /// Gets or sets the name of the generated property. By default it is the name of the
    /// field with its first letter in upper case. State it when the field cannot have that
    /// name, for example because the class has locals or parameters of the same name.
    /// </summary>
    public string? PropertyName { get; set; }
}