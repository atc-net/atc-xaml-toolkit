// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.ObservableDtoViewModel;

internal sealed class ObservableDtoViewModelInspectorResult
{
    public ObservableDtoViewModelInspectorResult(
        string? dtoTypeName,
        bool isDtoRecord,
        bool hasCustomToString,
        List<DtoPropertyInfo>? properties)
    {
        DtoTypeName = dtoTypeName;
        IsDtoRecord = isDtoRecord;
        HasCustomToString = hasCustomToString;
        Properties = properties ?? [];
    }

    public string? DtoTypeName { get; }

    public bool IsDtoRecord { get; }

    public bool HasCustomToString { get; }

    public List<DtoPropertyInfo> Properties { get; }

    public bool FoundAnythingToGenerate => !string.IsNullOrEmpty(DtoTypeName) &&
                                           Properties.Count > 0;

    public override string ToString()
        => $"{nameof(DtoTypeName)}: {DtoTypeName}, {nameof(IsDtoRecord)}: {IsDtoRecord}, {nameof(HasCustomToString)}: {HasCustomToString}, {nameof(Properties)}.Count: {Properties.Count}, {nameof(FoundAnythingToGenerate)}: {FoundAnythingToGenerate}";
}