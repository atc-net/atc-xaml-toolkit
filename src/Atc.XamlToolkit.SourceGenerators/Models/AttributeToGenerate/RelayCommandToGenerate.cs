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
    bool GenerateDocumentation)
{
    public EquatableArray<string> DocumentationComments { get; set; } = EquatableArray<string>.Empty;

    /// <summary>
    /// Gets or sets the name of the user's method when it returns a <c>ValueTask</c>.
    /// <see cref="MethodName"/> is then the name of a generated adapter method that returns
    /// a <c>Task</c>, because no command constructor accepts a ValueTask-returning method.
    /// </summary>
    public string? ValueTaskMethodName { get; set; }
}