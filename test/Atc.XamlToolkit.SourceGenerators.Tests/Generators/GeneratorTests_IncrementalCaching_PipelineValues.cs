namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

/// <summary>
/// Pins that the values flowing through the generator pipelines are equatable,
/// so an unrelated edit does not re-run the outputs.
/// </summary>
public sealed class GeneratorTests_IncrementalCaching_PipelineValues : GeneratorTestBase
{
    private const string SourceWithDiagnostics =
        """
        namespace Test;

        public partial class MyViewModel : ObservableObject
        {
            [ObservableProperty]
            public string Title;

            [ObservableProperty]
            [NotifyPropertyChangedFor("Missing")]
            [NotifyCanExecuteChangedFor("MissingCommand")]
            [NotifyDataErrorInfo]
            private string firstName = string.Empty;

            [ComputedProperty]
            public string Constant => "x";
        }

        public class NotPartialViewModel : ObservableObject
        {
            [ObservableProperty]
            private string name = string.Empty;
        }
        """;

    [Fact]
    public void INotifyPropertyChangedTarget_OnUnchangedInput_IsCached()
    {
        const string source =
            """
            namespace Test;

            [INotifyPropertyChanged]
            public partial class Item
            {
                [ObservableProperty]
                private int count;
            }
            """;

        var result = RunGeneratorIncremental<ViewModelGenerator>(source);

        AssertTrackedStepIsCached(result, "ViewModelGenerator.INotifyPropertyChangedTarget");
    }

    [Theory]
    [InlineData("ViewModelGenerator.MissingPartialDiagnostic")]
    [InlineData("ViewModelGenerator.ObservablePropertyFieldDiagnostics")]
    [InlineData("ViewModelGenerator.NotifyPropertyChangedForDiagnostics")]
    [InlineData("ViewModelGenerator.NotifyCanExecuteChangedForDiagnostics")]
    [InlineData("ViewModelGenerator.ComputedPropertyDiagnostics")]
    [InlineData("ViewModelGenerator.NotifyDataErrorInfoDiagnostics")]
    public void ViewModelDiagnostics_AfterUnrelatedEdit_AreCached(
        string trackingName)
    {
        var result = RunGeneratorIncrementalAfterUnrelatedEdit<ViewModelGenerator>(SourceWithDiagnostics);

        AssertTrackedStepIsCached(result, trackingName);
    }

    [Fact]
    public void RoutedEventOnNonWpfDiagnostic_AfterUnrelatedEdit_IsCached()
    {
        // An Avalonia type in the compilation selects a platform without routed events.
        const string source =
            """
            namespace Avalonia
            {
                public class AvaloniaObject { }
            }

            namespace Test
            {
                public partial class MyControl
                {
                    [RoutedEvent]
                    private static readonly RoutedEvent tap;
                }
            }
            """;

        var result = RunGeneratorIncrementalAfterUnrelatedEdit<FrameworkElementGenerator>(source);

        AssertTrackedStepIsCached(result, "FrameworkElementGenerator.RoutedEventLocations");
    }
}