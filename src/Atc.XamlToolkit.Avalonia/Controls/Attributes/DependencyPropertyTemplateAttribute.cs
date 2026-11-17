namespace Atc.XamlToolkit.Controls.Attributes;

/// <summary>
/// Marks a class as a property template: a set of fields with <c>[StyledProperty]</c>,
/// <c>[AttachedProperty]</c> that is declared once and applied to several
/// controls with <see cref="UseDependencyPropertyTemplateAttribute"/>.
/// </summary>
/// <remarks>
/// The class is never instantiated and gets no generated code of its own; make it
/// <c>abstract</c>. It can be generic: the type arguments given where the template is used
/// replace the type parameters in the field types. The template has to be declared in the
/// same project as the controls that use it.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class DependencyPropertyTemplateAttribute : Attribute;