// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.ToGenerate;

internal sealed record ObservablePropertyToGenerate(
    string Name,
    string Type,
    string BackingFieldName,
    bool IsReadOnly)
{
    public EquatableArray<string> PropertyNamesToInvalidate { get; set; } = EquatableArray<string>.Empty;

    public EquatableArray<string> CommandNamesToInvalidate { get; set; } = EquatableArray<string>.Empty;

    public string? BeforeChangedCallback { get; set; }

    public string? AfterChangedCallback { get; set; }

    public bool BroadcastOnChange { get; set; }

    public bool UseIsDirty { get; set; }

    public bool IsRequired { get; set; }

    public bool GeneratePartialHooks { get; set; }

    public bool GenerateDocumentation { get; set; }

    public bool ValidatesOnChange { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the setter must compare with
    /// <c>EqualityComparer&lt;T&gt;.Default</c> because the type has no <c>==</c> operator.
    /// </summary>
    public bool UseEqualityComparer { get; set; }

    public EquatableArray<string> CustomAttributes { get; set; } = EquatableArray<string>.Empty;

    /// <summary>
    /// Gets or sets the namespaces the generated file must import for the copied attributes.
    /// </summary>
    public EquatableArray<string> RequiredNamespaces { get; set; } = EquatableArray<string>.Empty;

    public EquatableArray<string> DocumentationComments { get; set; } = EquatableArray<string>.Empty;
}