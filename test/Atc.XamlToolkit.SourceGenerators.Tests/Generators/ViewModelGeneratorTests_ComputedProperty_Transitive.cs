namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

/// <summary>
/// Tests that a [ComputedProperty] that depends on another [ComputedProperty]
/// is notified when the underlying observable property changes.
/// </summary>
public sealed class ViewModelGeneratorTests_ComputedProperty_Transitive : GeneratorTestBase
{
    [Fact]
    public void ComputedProperty_DependingOnComputedProperty_IsNotified()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class TestViewModel : ViewModelBase
            {
                [ObservableProperty]
                private string firstName = string.Empty;

                [ObservableProperty]
                private string lastName = string.Empty;

                [ObservableProperty]
                private int age;

                [ComputedProperty]
                public string FullName => $"{FirstName} {LastName}";

                [ComputedProperty]
                public string Greeting => $"Hello {FullName}";

                [ComputedProperty]
                public string Banner => $"*** {Greeting} ({Age}) ***";
            }
            """;

        AssertCompilesWithAllGenerators(inputCode);

        var generatedCode = RunGenerator<ViewModelGenerator>(inputCode)
            .GeneratorResult
            .GeneratedSources[0]
            .SourceText
            .ToString();

        // FirstName and LastName each notify FullName, Greeting and Banner; Age only notifies Banner.
        Assert.Equal(2, CountOccurrences(generatedCode, "RaisePropertyChanged(nameof(FullName));"));
        Assert.Equal(2, CountOccurrences(generatedCode, "RaisePropertyChanged(nameof(Greeting));"));
        Assert.Equal(3, CountOccurrences(generatedCode, "RaisePropertyChanged(nameof(Banner));"));
    }

    [Fact]
    public void ComputedProperty_WithCircularDependency_DoesNotHang()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class TestViewModel : ViewModelBase
            {
                [ObservableProperty]
                private string name = string.Empty;

                [ComputedProperty]
                public string First => Name + Second;

                [ComputedProperty]
                public string Second => Name + First;
            }
            """;

        var generatedCode = RunGenerator<ViewModelGenerator>(inputCode)
            .GeneratorResult
            .GeneratedSources[0]
            .SourceText
            .ToString();

        Assert.Equal(1, CountOccurrences(generatedCode, "RaisePropertyChanged(nameof(First));"));
        Assert.Equal(1, CountOccurrences(generatedCode, "RaisePropertyChanged(nameof(Second));"));
    }

    [Fact]
    public void ObservableDtoViewModel_ComputedProperty_DependingOnComputedProperty_IsNotified()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public class Person
            {
                public string Name { get; set; } = string.Empty;
            }

            [ObservableDtoViewModel(typeof(Person))]
            public partial class PersonViewModel : ViewModelBase
            {
                [ComputedProperty]
                public string Display => Name.ToUpperInvariant();

                [ComputedProperty]
                public string Title => $"Mr. {Display}";
            }
            """;

        AssertCompilesWithAllGenerators(inputCode);

        var generatedCode = RunGenerator<ObservableDtoViewModelGenerator>(inputCode)
            .GeneratorResult
            .GeneratedSources[0]
            .SourceText
            .ToString();

        Assert.Equal(1, CountOccurrences(generatedCode, "RaisePropertyChanged(nameof(Display));"));
        Assert.Equal(1, CountOccurrences(generatedCode, "RaisePropertyChanged(nameof(Title));"));
    }

    private static int CountOccurrences(
        string text,
        string value)
        => text.Split([value], StringSplitOptions.None).Length - 1;
}