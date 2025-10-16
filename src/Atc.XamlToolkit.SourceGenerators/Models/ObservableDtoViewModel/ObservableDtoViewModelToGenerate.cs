// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.ObservableDtoViewModel;

internal sealed class ObservableDtoViewModelToGenerate : GenerateBase
{
    public ObservableDtoViewModelToGenerate(
        string namespaceName,
        string className,
        string accessModifier,
        string dtoTypeName,
        bool isDtoRecord,
        bool hasCustomToString,
        bool useIsDirty,
        bool enableValidationOnPropertyChanged,
        bool enableValidationOnInit,
        List<DtoPropertyInfo> properties,
        List<DtoMethodInfo> methods,
        List<ObservablePropertyToGenerate> customProperties,
        List<RelayCommandToGenerate> customCommands)
        : base(namespaceName, className, accessModifier, isStatic: false)
    {
        DtoTypeName = dtoTypeName;
        IsDtoRecord = isDtoRecord;
        HasCustomToString = hasCustomToString;
        UseIsDirty = useIsDirty;
        EnableValidationOnPropertyChanged = enableValidationOnPropertyChanged;
        EnableValidationOnInit = enableValidationOnInit;
        Properties = properties;
        Methods = methods;
        CustomProperties = customProperties;
        CustomCommands = customCommands;
    }

    public string DtoTypeName { get; }

    public bool IsDtoRecord { get; }

    public bool HasCustomToString { get; }

    public bool UseIsDirty { get; }

    public bool EnableValidationOnPropertyChanged { get; }

    public bool EnableValidationOnInit { get; }

    public List<DtoPropertyInfo> Properties { get; }

    public List<DtoMethodInfo> Methods { get; }

    public List<ObservablePropertyToGenerate> CustomProperties { get; }

    public List<RelayCommandToGenerate> CustomCommands { get; }
}