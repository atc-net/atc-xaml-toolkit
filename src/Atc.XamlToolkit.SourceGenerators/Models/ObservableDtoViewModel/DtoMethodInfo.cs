// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.ObservableDtoViewModel;

internal sealed record DtoMethodInfo(
    string Name,
    string ReturnType,
    EquatableArray<DtoMethodParameterInfo> Parameters);