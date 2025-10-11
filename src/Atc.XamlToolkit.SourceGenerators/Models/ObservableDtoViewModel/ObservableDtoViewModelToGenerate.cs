// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.ObservableDtoViewModel;

internal sealed class ObservableDtoViewModelToGenerate : GenerateBase
{
    public ObservableDtoViewModelToGenerate(
        string namespaceName,
        string className,
        string accessModifier,
        string dtoTypeName,
        bool isRecord,
        List<DtoPropertyInfo> properties)
        : base(namespaceName, className, accessModifier, isStatic: false)
    {
        DtoTypeName = dtoTypeName;
        IsRecord = isRecord;
        Properties = properties;
    }

    public string DtoTypeName { get; }

    public bool IsRecord { get; }

    public List<DtoPropertyInfo> Properties { get; }
}