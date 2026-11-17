namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

/// <summary>
/// Tests that SupportsCancellation, ExecuteOnBackgroundThread and AutoSetIsBusy
/// keep their meaning when they are combined.
/// </summary>
public sealed class ViewModelGeneratorTests_RelayCommand_FlagCombinations : GeneratorTestBase
{
    [Theory]
    [InlineData("")]
    [InlineData("string path, ")]
    [InlineData("string path, int index, ")]
    public void AutoSetIsBusy_SupportsCancellation_ExecuteOnBackgroundThread_PassesTheCommandsToken(
        string parameters)
    {
        var generatedCode = Generate(
            "AutoSetIsBusy = true, SupportsCancellation = true, ExecuteOnBackgroundThread = true",
            $"private Task Load({parameters}CancellationToken cancellationToken) => Task.CompletedTask;");

        Assert.DoesNotContain("CancellationToken.None", generatedCode, StringComparison.Ordinal);
        Assert.Contains("cancellationToken), cancellationToken)", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".Run(() => Load(", generatedCode, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("string path, ")]
    [InlineData("string path, int index, ")]
    public void SupportsCancellation_ExecuteOnBackgroundThread_RunsOnABackgroundThreadWithTheCommandsToken(
        string parameters)
    {
        var generatedCode = Generate(
            "SupportsCancellation = true, ExecuteOnBackgroundThread = true",
            $"private Task Load({parameters}CancellationToken cancellationToken) => Task.CompletedTask;");

        Assert.DoesNotContain("CancellationToken.None", generatedCode, StringComparison.Ordinal);
        Assert.Contains("Task.Run(() => Load(", generatedCode, StringComparison.Ordinal);
        Assert.Contains("cancellationToken), cancellationToken)", generatedCode, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("string path")]
    [InlineData("string path, int index")]
    public void SupportsCancellation_ExecuteOnBackgroundThread_MethodWithoutToken_RunsOnABackgroundThread(
        string parameters)
    {
        var generatedCode = Generate(
            "SupportsCancellation = true, ExecuteOnBackgroundThread = true",
            $"private Task Load({parameters}) => Task.CompletedTask;");

        Assert.Contains("Task.Run(() => Load(", generatedCode, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("string path")]
    [InlineData("string path, int index")]
    public void AutoSetIsBusy_ExecuteOnBackgroundThread_SynchronousMethod_KeepsIsBusyUntilTheWorkIsDone(
        string parameters)
    {
        var generatedCode = Generate(
            "AutoSetIsBusy = true, ExecuteOnBackgroundThread = true",
            $"private void Crunch({parameters}) {{ }}");

        // A fire-and-forget task would reset IsBusy while the work is still running.
        Assert.DoesNotContain("_ = Task", generatedCode, StringComparison.Ordinal);
        Assert.Contains("await Task", generatedCode, StringComparison.Ordinal);
    }

    private static string Generate(
        string attributeArguments,
        string method)
    {
        var inputCode =
            $$"""
              namespace TestNamespace;

              public partial class TestViewModel : ViewModelBase
              {
                  [RelayCommand({{attributeArguments}})]
                  {{method}}
              }
              """;

        // The generated code must also compile.
        AssertCompilesWithAllGenerators(inputCode);

        return RunGenerator<ViewModelGenerator>(inputCode)
            .GeneratorResult
            .GeneratedSources[0]
            .SourceText
            .ToString();
    }
}