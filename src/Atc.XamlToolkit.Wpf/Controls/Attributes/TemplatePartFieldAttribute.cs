namespace Atc.XamlToolkit.Controls.Attributes;

/// <summary>
/// Marks a field that holds a named part of the control's template. The source generator
/// declares the part on the class with the platform's <c>[TemplatePart]</c> attribute and
/// generates a <c>ResolveTemplateParts</c> method that assigns every marked field.
/// </summary>
/// <remarks>
/// Call the generated <c>ResolveTemplateParts()</c> from <c>OnApplyTemplate()</c>, after
/// <c>base.OnApplyTemplate()</c>. It looks every part up with <c>GetTemplateChild</c>.
/// </remarks>
/// <param name="name">The name of the part in the template, for example <c>PART_CloseButton</c>.</param>
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class TemplatePartFieldAttribute(string name) : Attribute
{
    /// <summary>
    /// Gets the name of the part in the template.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets a value indicating whether the template must contain the part. When true, the
    /// generated method throws an <see cref="InvalidOperationException"/> if it is missing.
    /// </summary>
    public bool IsRequired { get; init; }

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(Name)}: {Name}, {nameof(IsRequired)}: {IsRequired}";
}