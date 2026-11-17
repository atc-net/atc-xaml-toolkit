namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

/// <summary>
/// Pins that an edit to one class only regenerates the file of that class.
/// </summary>
public sealed class GeneratorTests_IncrementalCaching_PerClassOutput : GeneratorTestBase
{
    [Fact]
    public void ViewModelGenerator_EditOfOneClass_OnlyRegeneratesThatClass()
    {
        const string first =
            """
            namespace Test;

            public partial class FirstViewModel : ViewModelBase
            {
                [ObservableProperty]
                private string name = string.Empty;
            }
            """;

        const string second =
            """
            namespace Test;

            public partial class SecondViewModel : ViewModelBase
            {
                [ObservableProperty]
                private string title = string.Empty;
            }
            """;

        var result = RunGeneratorIncrementalAfterEdit<ViewModelGenerator>(
            [first, second],
            editedIndex: 1,
            second.Replace("title", "subtitle", StringComparison.Ordinal));

        AssertOutputOfClass(result, "FirstViewModel", isRegenerated: false);
        AssertOutputOfClass(result, "SecondViewModel", isRegenerated: true);
    }

    [Fact]
    public void ObservableDtoViewModelGenerator_EditOfOneClass_OnlyRegeneratesThatClass()
    {
        const string first =
            """
            namespace Test;

            public class Person
            {
                public string Name { get; set; }
            }

            [ObservableDtoViewModel(typeof(Person))]
            public partial class FirstViewModel : ViewModelBase
            {
            }
            """;

        const string second =
            """
            namespace Test;

            public class Order
            {
                public string Title { get; set; }
            }

            [ObservableDtoViewModel(typeof(Order))]
            public partial class SecondViewModel : ViewModelBase
            {
            }
            """;

        var result = RunGeneratorIncrementalAfterEdit<ObservableDtoViewModelGenerator>(
            [first, second],
            editedIndex: 1,
            second.Replace("Title", "Subtitle", StringComparison.Ordinal));

        AssertOutputOfClass(result, "FirstViewModel", isRegenerated: false);
        AssertOutputOfClass(result, "SecondViewModel", isRegenerated: true);
    }

    [Fact]
    public void FrameworkElementGenerator_EditOfOneClass_OnlyRegeneratesThatClass()
    {
        const string first =
            """
            namespace Test;

            public partial class FirstControl : UserControl
            {
                [DependencyProperty]
                private int size;
            }
            """;

        const string second =
            """
            namespace Test;

            public partial class SecondControl : UserControl
            {
                [DependencyProperty]
                private int depth;
            }
            """;

        var result = RunGeneratorIncrementalAfterEdit<FrameworkElementGenerator>(
            [first, second],
            editedIndex: 1,
            second.Replace("depth", "height", StringComparison.Ordinal));

        AssertOutputOfClass(result, "FirstControl", isRegenerated: false);
        AssertOutputOfClass(result, "SecondControl", isRegenerated: true);
    }

    // Finds the source output step that is fed with the model of the given class.
    private static void AssertOutputOfClass(
        GeneratorRunResult result,
        string className,
        bool isRegenerated)
    {
        var outputSteps = result.TrackedOutputSteps
            .SelectMany(x => x.Value)
            .Where(step => step.Inputs.Any(input =>
                input.Source.Outputs[input.OutputIndex].Value is GenerateBase model &&
                model.ClassName == className))
            .ToList();

        if (outputSteps.Count != 1)
        {
            Assert.Fail($"Expected one source output for the model of '{className}', but found {outputSteps.Count}: the files are not generated per class.");
        }

        var wasRegenerated = outputSteps[0].Outputs.Any(x => x.Reason
            is IncrementalStepRunReason.New
            or IncrementalStepRunReason.Modified);

        Assert.Equal(isRegenerated, wasRegenerated);
    }
}