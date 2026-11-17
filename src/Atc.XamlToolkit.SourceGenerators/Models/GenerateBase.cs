// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators;

internal abstract record GenerateBase(
    string NamespaceName,
    string ClassName,
    string? ClassAccessModifier,
    bool IsStatic)
{
    /// <summary>
    /// Gets or sets the type parameter list of the class, e.g. <c>&lt;TKey, TValue&gt;</c>;
    /// empty for a non-generic class.
    /// </summary>
    public string TypeParameterList { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the declarations of the types the class is nested in, outermost first,
    /// e.g. <c>partial class Outer&lt;T&gt;</c>; empty for a top-level class.
    /// </summary>
    public EquatableArray<string> ContainingTypeDeclarations { get; set; } = EquatableArray<string>.Empty;

    /// <summary>
    /// Gets or sets the unique file name stem of the class: namespace, containing types and
    /// class name, with the arity of generic types. Null falls back to namespace and class name.
    /// </summary>
    public string? HintName { get; set; }

    /// <summary>
    /// Gets or sets the attributes to put on the generated part of the class, each as a
    /// complete attribute list, e.g. <c>[TemplatePart(Name = "PART_X", Type = typeof(Border))]</c>.
    /// </summary>
    public EquatableArray<string> ClassAttributes { get; set; } = EquatableArray<string>.Empty;

    public string GeneratedFileName
        => $"{HintName ?? $"{NamespaceName}.{ClassName}"}.g.cs";
}