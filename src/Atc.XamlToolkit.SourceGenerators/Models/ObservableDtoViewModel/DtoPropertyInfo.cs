// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.ObservableDtoViewModel;

internal sealed record DtoPropertyInfo(
    string Name,
    string Type,
    bool IsRecordParameter,
    bool IsReadOnly,
    EquatableArray<string> Attributes,
    EquatableArray<string> DocumentationComments)
{
    /// <summary>
    /// Gets or sets a value indicating whether the setter must compare with
    /// <c>EqualityComparer&lt;T&gt;.Default</c> because the type has no <c>==</c> operator.
    /// </summary>
    public bool UseEqualityComparer { get; set; }
}