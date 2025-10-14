// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.ObservableDtoViewModel;

internal sealed class ObservableDtoViewModelInspectorResult
{
    public ObservableDtoViewModelInspectorResult(
        string? dtoTypeName,
        bool isDtoRecord,
        bool hasCustomToString,
        bool useIsDirty,
        bool enableValidationOnPropertyChanged,
        bool enableValidationOnInit,
        List<DtoPropertyInfo>? properties,
        List<DtoMethodInfo>? methods)
    {
        DtoTypeName = dtoTypeName;
        IsDtoRecord = isDtoRecord;
        HasCustomToString = hasCustomToString;
        UseIsDirty = useIsDirty;
        EnableValidationOnPropertyChanged = enableValidationOnPropertyChanged;
        EnableValidationOnInit = enableValidationOnInit;
        Properties = properties ?? [];
        Methods = methods ?? [];
    }

    public string? DtoTypeName { get; }

    public bool IsDtoRecord { get; }

    public bool HasCustomToString { get; }

    public bool UseIsDirty { get; }

    public bool EnableValidationOnPropertyChanged { get; }

    public bool EnableValidationOnInit { get; }

    public List<DtoPropertyInfo> Properties { get; }

    public List<DtoMethodInfo> Methods { get; }

    public bool FoundAnythingToGenerate => !string.IsNullOrEmpty(DtoTypeName) &&
                                           Properties.Count > 0;

    public override string ToString()
        => $"{nameof(DtoTypeName)}: {DtoTypeName}, {nameof(IsDtoRecord)}: {IsDtoRecord}, {nameof(HasCustomToString)}: {HasCustomToString}, {nameof(UseIsDirty)}: {UseIsDirty}, {nameof(EnableValidationOnPropertyChanged)}: {EnableValidationOnPropertyChanged}, {nameof(EnableValidationOnInit)}: {EnableValidationOnInit}, {nameof(Properties)}.Count: {Properties.Count}, {nameof(Methods)}.Count: {Methods.Count}, {nameof(FoundAnythingToGenerate)}: {FoundAnythingToGenerate}";
}