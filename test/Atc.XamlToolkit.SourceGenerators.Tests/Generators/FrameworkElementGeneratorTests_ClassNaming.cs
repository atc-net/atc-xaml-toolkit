namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "OK.")]
public sealed partial class FrameworkElementGeneratorTests
{
    [Fact]
    public void AttachedProperty_OnField_InAStaticClassWithAnyName_IsGenerated()
    {
        // The class name has none of the suffixes Attach / Behavior / Helper.
        const string inputCode =
            """
            namespace TestNamespace;

            public static partial class DataGridFilter
            {
                [AttachedProperty]
                private static bool isEnabled;
            }
            """;

        var generatedCode = Assert.Single(RunGenerator<FrameworkElementGenerator>(inputCode).GeneratorResult.GeneratedSources).SourceText.ToString();

        Assert.Contains("public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(", generatedCode, StringComparison.Ordinal);
        Assert.Contains("public static bool GetIsEnabled(", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void AttachedProperty_OnClass_InAStaticClassWithAnyName_IsGenerated()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            [AttachedProperty<bool>("IsOpen")]
            public static partial class FlyoutBase
            {
            }
            """;

        var generatedCode = Assert.Single(RunGenerator<FrameworkElementGenerator>(inputCode).GeneratorResult.GeneratedSources).SourceText.ToString();

        Assert.Contains("public static readonly DependencyProperty IsOpenProperty = DependencyProperty.RegisterAttached(", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void DependencyProperty_InAClassWithAnyNameAndAnUnknownBaseClass_IsGenerated()
    {
        // The base class is not one the generator knows by name, for example a control
        // from another library.
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class StatusBadge : ThirdPartyControl
            {
                [DependencyProperty]
                private bool isActive;
            }
            """;

        var generatedCode = Assert.Single(RunGenerator<FrameworkElementGenerator>(inputCode).GeneratorResult.GeneratedSources).SourceText.ToString();

        Assert.Contains("public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void RelayCommand_InAViewModelWithAnyName_IsStillLeftToTheViewModelGenerator()
    {
        // Recognising a class by its framework-element attributes must not make the
        // framework-element generator pick up plain view models.
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class OrderViewModel : ViewModelBase
            {
                [RelayCommand]
                private void Save()
                {
                }
            }
            """;

        AssertGeneratorRunResultIsEmpty(RunGenerator<FrameworkElementGenerator>(inputCode));
    }
}