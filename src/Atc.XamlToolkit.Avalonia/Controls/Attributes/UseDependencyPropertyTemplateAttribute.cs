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
/// <remarks>
/// The attribute is an instruction to the source generator and is not kept in the compiled
/// class: a control would otherwise carry a reference to the template type, which is
/// usually internal, in its metadata.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
[System.Diagnostics.Conditional("ATC_XAML_TOOLKIT_KEEP_GENERATOR_ATTRIBUTES")]
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

    /// <summary>
    /// Gets or sets the texts that replace the placeholders <c>{0}</c>, <c>{1}</c>, ... in
    /// the XML documentation of the template's fields, for documentation that names what
    /// the control is about: "Gets or sets the selected {0}." with <c>["drive"]</c>.
    /// </summary>
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "An attribute argument has to be an array.")]
    public string[]? DocumentationValues { get; set; }
}