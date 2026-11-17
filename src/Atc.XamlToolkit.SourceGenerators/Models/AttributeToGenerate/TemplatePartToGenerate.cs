// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.ToGenerate;

/// <summary>
/// A field marked with <c>[TemplatePartField]</c>.
/// </summary>
/// <param name="FieldName">The field to assign.</param>
/// <param name="PartNameExpression">The part name as a C# expression: a string literal, or the constant as written.</param>
/// <param name="PartNameText">The part name itself; null when only the expression is known.</param>
/// <param name="TypeName">The type of the part as written in generated code, without a nullable annotation.</param>
/// <param name="IsRequired">Whether a missing part is an error.</param>
/// <param name="IsFieldNullable">Whether the field is declared nullable.</param>
/// <param name="RequiredNamespaces">The namespaces needed to write <paramref name="TypeName"/> by its simple name.</param>
internal sealed record TemplatePartToGenerate(
    string FieldName,
    string PartNameExpression,
    string? PartNameText,
    string TypeName,
    bool IsRequired,
    bool IsFieldNullable,
    EquatableArray<string> RequiredNamespaces)
{
    /// <summary>
    /// Gets or sets a value indicating whether the type of the part uses a type parameter
    /// of the class, which cannot be written in the <c>typeof</c> of an attribute.
    /// </summary>
    public bool UsesTypeParameter { get; set; }
}