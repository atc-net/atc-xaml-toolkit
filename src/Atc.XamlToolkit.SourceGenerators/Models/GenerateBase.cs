// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators;

internal abstract record GenerateBase(
    string NamespaceName,
    string ClassName,
    string? ClassAccessModifier,
    bool IsStatic)
{
    public string GeneratedFileName => $"{NamespaceName}.{ClassName}.g.cs";
}