// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.ObservableDtoViewModel;

internal sealed class DtoMethodParameterInfo(
    string name,
    string type)
{
    public string Name { get; } = name;

    public string Type { get; } = type;

    public override string ToString()
        => $"{nameof(Type)}: {Type}, {nameof(Name)}: {Name}";
}