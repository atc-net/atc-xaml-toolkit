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
        List<DtoMethodInfo>? methods,
        List<ObservablePropertyToGenerate>? customProperties,
        List<RelayCommandToGenerate>? customCommands)
    {
        DtoTypeName = dtoTypeName;
        IsDtoRecord = isDtoRecord;
        HasCustomToString = hasCustomToString;
        UseIsDirty = useIsDirty;
        EnableValidationOnPropertyChanged = enableValidationOnPropertyChanged;
        EnableValidationOnInit = enableValidationOnInit;
        Properties = properties ?? [];
        Methods = methods ?? [];
        CustomProperties = customProperties ?? [];
        CustomCommands = customCommands ?? [];
    }

    public string? DtoTypeName { get; }

    public bool IsDtoRecord { get; }

    public bool HasCustomToString { get; }

    public bool UseIsDirty { get; }

    public bool EnableValidationOnPropertyChanged { get; }

    public bool EnableValidationOnInit { get; }

    public List<DtoPropertyInfo> Properties { get; }

    public List<DtoMethodInfo> Methods { get; }

    public List<ObservablePropertyToGenerate> CustomProperties { get; }

    public List<RelayCommandToGenerate> CustomCommands { get; }

    public bool FoundAnythingToGenerate => !string.IsNullOrEmpty(DtoTypeName) &&
                                           Properties.Count > 0;

    public override string ToString()
        => $"{nameof(DtoTypeName)}: {DtoTypeName}, {nameof(IsDtoRecord)}: {IsDtoRecord}, {nameof(HasCustomToString)}: {HasCustomToString}, {nameof(UseIsDirty)}: {UseIsDirty}, {nameof(EnableValidationOnPropertyChanged)}: {EnableValidationOnPropertyChanged}, {nameof(EnableValidationOnInit)}: {EnableValidationOnInit}, {nameof(Properties)}.Count: {Properties.Count}, {nameof(Methods)}.Count: {Methods.Count}, {nameof(CustomProperties)}.Count: {CustomProperties.Count}, {nameof(CustomCommands)}.Count: {CustomCommands.Count}, {nameof(FoundAnythingToGenerate)}: {FoundAnythingToGenerate}";
}