// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.ObservableDtoViewModel;

internal sealed class ObservableDtoViewModelToGenerate : GenerateBase
{
    public ObservableDtoViewModelToGenerate(
        string namespaceName,
        string className,
        string accessModifier,
        string dtoTypeName,
        List<DtoPropertyInfo> properties)
        : base(namespaceName, className, accessModifier, isStatic: false)
    {
        DtoTypeName = dtoTypeName;
        Properties = properties;
    }

    public string DtoTypeName { get; }

    public List<DtoPropertyInfo> Properties { get; }
}