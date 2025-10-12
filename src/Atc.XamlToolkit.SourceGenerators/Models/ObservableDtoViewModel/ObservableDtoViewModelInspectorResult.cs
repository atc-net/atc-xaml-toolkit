// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.ObservableDtoViewModel;

internal sealed class ObservableDtoViewModelInspectorResult
{
    public ObservableDtoViewModelInspectorResult(
        string? dtoTypeName,
        bool isDtoRecord,
        bool hasCustomToString,
        bool useIsDirty,
        List<DtoPropertyInfo>? properties,
        List<DtoMethodInfo>? methods)
    {
        DtoTypeName = dtoTypeName;
        IsDtoRecord = isDtoRecord;
        HasCustomToString = hasCustomToString;
        UseIsDirty = useIsDirty;
        Properties = properties ?? [];
        Methods = methods ?? [];
    }

    public string? DtoTypeName { get; }

    public bool IsDtoRecord { get; }

    public bool HasCustomToString { get; }

    public bool UseIsDirty { get; }

    public List<DtoPropertyInfo> Properties { get; }

    public List<DtoMethodInfo> Methods { get; }

    public bool FoundAnythingToGenerate => !string.IsNullOrEmpty(DtoTypeName) &&
                                           Properties.Count > 0;

    public override string ToString()
        => $"{nameof(DtoTypeName)}: {DtoTypeName}, {nameof(IsDtoRecord)}: {IsDtoRecord}, {nameof(HasCustomToString)}: {HasCustomToString}, {nameof(UseIsDirty)}: {UseIsDirty}, {nameof(Properties)}.Count: {Properties.Count}, {nameof(Methods)}.Count: {Methods.Count}, {nameof(FoundAnythingToGenerate)}: {FoundAnythingToGenerate}";
}