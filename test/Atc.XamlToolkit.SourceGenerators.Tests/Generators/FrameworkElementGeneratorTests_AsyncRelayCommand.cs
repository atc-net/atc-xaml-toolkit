namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "OK.")]
public sealed partial class FrameworkElementGeneratorTests
{
    [Fact]
    public void AsyncRelayCommand_NoParameter()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class TestControl : UserControl
            {
                [RelayCommand]
                public Task Save()
                {
                    return Task.CompletedTask;
                }
            }
            """;

        const string expectedCode =
            """
            // <auto-generated>
            #nullable enable
            using Atc.XamlToolkit.Command;

            namespace TestNamespace;

            public partial class TestControl
            {
                private IRelayCommandAsync? saveCommand;

                public IRelayCommandAsync SaveCommand => saveCommand ??= new RelayCommandAsync(Save);
            }

            #nullable disable
            """;

        var generatorResult = RunGenerator<FrameworkElementGenerator>(inputCode);

        AssertGeneratorRunResultAsEqual(expectedCode, generatorResult);
    }
}