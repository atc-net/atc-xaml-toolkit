// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.ToGenerate;

internal sealed record RelayCommandToGenerate(
    string CommandName,
    string MethodName,
    EquatableArray<string> ParameterTypes,
    EquatableArray<string> ParameterNames,
    EquatableArray<string> ParameterValues,
    string? CanExecuteName,
    bool InvertCanExecute,
    bool UsePropertyForCanExecute,
    bool IsAsync,
    bool UseTask,
    bool ExecuteOnBackgroundThread,
    bool AutoSetIsBusy,
    bool SupportsCancellation,
    bool GenerateDocumentation);