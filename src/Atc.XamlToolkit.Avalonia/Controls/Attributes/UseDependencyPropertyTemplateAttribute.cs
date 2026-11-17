namespace Atc.XamlToolkit.Controls.Attributes;

/// <summary>
/// Applies a property template to a control: the source generator emits, in the annotated
/// class, the members it would emit if the attributed fields of the template were declared
/// in the class itself.
/// </summary>
/// <param name="templateType">
/// The template: a class marked with <see cref="DependencyPropertyTemplateAttribute"/>,
/// with its type arguments when it is generic, e.g. <c>typeof(PickerMembers&lt;DriveInfo&gt;)</c>.
/// </param>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class UseDependencyPropertyTemplateAttribute(Type templateType) : Attribute
{
    /// <summary>
    /// Gets the template that is applied.
    /// </summary>
    public Type TemplateType { get; } = templateType;

    /// <summary>
    /// Gets or sets the members of the template that are left out, by the name of the
    /// generated property or event, e.g. <c>"ShowRefreshButton"</c>.
    /// </summary>
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "An attribute argument has to be an array.")]
    public string[]? Exclude { get; set; }
}