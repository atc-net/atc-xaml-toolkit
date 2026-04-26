// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.ObservableDtoViewModel;

internal sealed record DtoPropertyInfo(
    string Name,
    string Type,
    bool IsRecordParameter,
    bool IsReadOnly,
    EquatableArray<string> Attributes,
    EquatableArray<string> DocumentationComments);