namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

/// <summary>
/// Tests that members declared on a base class are known to the
/// [NotifyPropertyChangedFor] / [NotifyCanExecuteChangedFor] diagnostics.
/// </summary>
public sealed class ViewModelGeneratorTests_InheritedMemberDiagnostics : GeneratorTestBase
{
    [Fact]
    public void NotifyAttributes_ReferringToInheritedMembers_ReportNoDiagnostics()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public abstract partial class BaseViewModel : ViewModelBase
            {
                [ObservableProperty]
                private string subtitle = string.Empty;

                public string Title => "Title";

                [RelayCommand]
                private void Refresh()
                {
                }

                public IRelayCommand ResetCommand { get; } = null!;
            }

            public partial class TestViewModel : BaseViewModel
            {
                [ObservableProperty]
                [NotifyPropertyChangedFor(nameof(Title))]
                [NotifyPropertyChangedFor(nameof(Subtitle))]
                [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
                [NotifyCanExecuteChangedFor(nameof(ResetCommand))]
                private string name = string.Empty;
            }
            """;

        var generatorResult = RunGenerator<ViewModelGenerator>(inputCode);

        Assert.DoesNotContain(generatorResult.Diagnostics, x => x.Id is "AtcXamlToolkit0005" or "AtcXamlToolkit0006");
    }

    [Fact]
    public void NotifyAttributes_ReferringToUnknownMembers_StillReportDiagnostics()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public abstract partial class BaseViewModel : ViewModelBase
            {
                public string Title => "Title";
            }

            public partial class TestViewModel : BaseViewModel
            {
                [ObservableProperty]
                [NotifyPropertyChangedFor("Missing")]
                [NotifyCanExecuteChangedFor("MissingCommand")]
                private string name = string.Empty;
            }
            """;

        var generatorResult = RunGenerator<ViewModelGenerator>(inputCode);

        Assert.Contains(generatorResult.Diagnostics, x => x.Id == "AtcXamlToolkit0005");
        Assert.Contains(generatorResult.Diagnostics, x => x.Id == "AtcXamlToolkit0006");
    }
}