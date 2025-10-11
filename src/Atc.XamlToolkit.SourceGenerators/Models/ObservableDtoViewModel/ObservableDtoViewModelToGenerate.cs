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
        List<DtoPropertyInfo> properties)
        : base(namespaceName, className, accessModifier, isStatic: false)
    {
        DtoTypeName = dtoTypeName;
        IsDtoRecord = isDtoRecord;
        HasCustomToString = hasCustomToString;
        Properties = properties;
    }

    public string DtoTypeName { get; }

    public bool IsDtoRecord { get; }

    public bool HasCustomToString { get; }

    public List<DtoPropertyInfo> Properties { get; }
}