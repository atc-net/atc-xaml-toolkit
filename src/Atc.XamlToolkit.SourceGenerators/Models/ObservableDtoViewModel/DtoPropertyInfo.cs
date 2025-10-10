// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.ObservableDtoViewModel;

internal sealed class DtoPropertyInfo(
    string name,
    string type)
{
    public string Name { get; } = name;

    public string Type { get; } = type;

    public override string ToString()
        => $"{nameof(Name)}: {Name}, {nameof(Type)}: {Type}";
}