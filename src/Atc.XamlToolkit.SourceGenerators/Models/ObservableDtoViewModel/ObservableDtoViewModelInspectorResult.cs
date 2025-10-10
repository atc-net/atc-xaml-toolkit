// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.ObservableDtoViewModel;

internal sealed class ObservableDtoViewModelInspectorResult
{
    public ObservableDtoViewModelInspectorResult(
        string? dtoTypeName,
        List<DtoPropertyInfo>? properties)
    {
        DtoTypeName = dtoTypeName;
        Properties = properties ?? [];
    }

    public string? DtoTypeName { get; }

    public List<DtoPropertyInfo> Properties { get; }

    public bool FoundAnythingToGenerate => !string.IsNullOrEmpty(DtoTypeName) && Properties.Count > 0;

    public override string ToString()
        => $"{nameof(DtoTypeName)}: {DtoTypeName}, {nameof(Properties)}.Count: {Properties.Count}, {nameof(FoundAnythingToGenerate)}: {FoundAnythingToGenerate}";
}