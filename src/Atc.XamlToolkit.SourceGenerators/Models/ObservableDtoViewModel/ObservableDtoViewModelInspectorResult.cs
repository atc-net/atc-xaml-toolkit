// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.ObservableDtoViewModel;

internal sealed class ObservableDtoViewModelInspectorResult
{
    public ObservableDtoViewModelInspectorResult(
        string? dtoTypeName,
        bool isRecord,
        List<DtoPropertyInfo>? properties)
    {
        DtoTypeName = dtoTypeName;
        IsRecord = isRecord;
        Properties = properties ?? [];
    }

    public string? DtoTypeName { get; }

    public bool IsRecord { get; }

    public List<DtoPropertyInfo> Properties { get; }

    public bool FoundAnythingToGenerate => !string.IsNullOrEmpty(DtoTypeName) && Properties.Count > 0;

    public override string ToString()
        => $"{nameof(DtoTypeName)}: {DtoTypeName}, {nameof(IsRecord)}: {IsRecord}, {nameof(Properties)}.Count: {Properties.Count}, {nameof(FoundAnythingToGenerate)}: {FoundAnythingToGenerate}";
}