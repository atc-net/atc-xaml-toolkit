// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.ToGenerate;

/// <summary>
/// Represents a computed property that depends on other properties.
/// </summary>
/// <param name="name">The name of the computed property.</param>
/// <param name="dependentPropertyNames">The names of properties this computed property depends on.</param>
internal sealed class ComputedPropertyToGenerate(
    string name,
    ICollection<string> dependentPropertyNames)
{
    public string Name { get; } = name;

    public ICollection<string> DependentPropertyNames { get; } = dependentPropertyNames;

    public override string ToString()
        => $"{nameof(Name)}: {Name}, {nameof(DependentPropertyNames)}.Count: {DependentPropertyNames.Count}";
}