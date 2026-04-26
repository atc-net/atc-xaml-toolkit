// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.ToGenerate;

/// <summary>
/// Represents a computed property that depends on other properties.
/// </summary>
internal sealed record ComputedPropertyToGenerate(
    string Name,
    EquatableArray<string> DependentPropertyNames);