// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.ObservableDtoViewModel;

internal sealed record ObservableDtoViewModelToGenerate(
    string NamespaceName,
    string ClassName,
    string? ClassAccessModifier,
    string DtoTypeName,
    bool IsDtoRecord,
    bool HasCustomToString,
    bool UseIsDirty,
    bool EnableValidationOnPropertyChanged,
    bool EnableValidationOnInit,
    EquatableArray<DtoPropertyInfo> Properties,
    EquatableArray<DtoMethodInfo> Methods,
    EquatableArray<ObservablePropertyToGenerate> CustomProperties,
    EquatableArray<RelayCommandToGenerate> CustomCommands,
    EquatableArray<ComputedPropertyToGenerate> ComputedProperties,
    XamlPlatform XamlPlatform)
    : GenerateBase(NamespaceName, ClassName, ClassAccessModifier, IsStatic: false);