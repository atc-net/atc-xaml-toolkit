// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.ObservableDtoViewModel;

internal sealed class DtoMethodInfo(
    string name,
    string returnType,
    List<DtoMethodParameterInfo> parameters)
{
    public string Name { get; } = name;

    public string ReturnType { get; } = returnType;

    public List<DtoMethodParameterInfo> Parameters { get; } = parameters;

    public override string ToString()
        => $"{nameof(Name)}: {Name}, {nameof(ReturnType)}: {ReturnType}, {nameof(Parameters)}: {Parameters.Count}";
}