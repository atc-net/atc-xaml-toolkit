namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

/// <summary>
/// The <c>SetterAccessModifier</c> option of <c>[ObservableProperty]</c>,
/// <c>[DependencyProperty]</c> and <c>[StyledProperty]</c>.
/// </summary>
public sealed class GeneratorTests_SetterAccessModifier : GeneratorTestBase
{
    [Theory]
    [InlineData("PropertyAccessModifier.Internal", "internal set")]
    [InlineData("PropertyAccessModifier.Protected", "protected set")]
    [InlineData("PropertyAccessModifier.ProtectedInternal", "protected internal set")]
    [InlineData("PropertyAccessModifier.PrivateProtected", "private protected set")]
    [InlineData("PropertyAccessModifier.Private", "private set")]
    [InlineData("Atc.XamlToolkit.PropertyAccessModifier.Private", "private set")]
    public void ObservableProperty_SetterAccessModifier_IsAppliedToTheSetter(
        string accessModifier,
        string expectedSetter)
    {
        var inputCode =
            $$"""
            namespace TestNamespace;

            public partial class TestViewModel : ViewModelBase
            {
                [ObservableProperty(SetterAccessModifier = {{accessModifier}})]
                private string name;
            }
            """;

        var generatedCode = Assert.Single(RunGenerator<ViewModelGenerator>(inputCode).GeneratorResult.GeneratedSources).SourceText.ToString();

        Assert.Contains(
            $$"""
                public string Name
                {
                    get => name;
                    {{expectedSetter}}
                    {
            """.EnsureEnvironmentNewLines(),
            generatedCode.EnsureEnvironmentNewLines(),
            StringComparison.Ordinal);
    }

    [Fact]
    public void ObservableProperty_SetterAccessModifier_IsNotMistakenForADependentPropertyName()
    {
        // Named arguments the generator does not know are treated as names of properties
        // to notify; the new option must not end up there.
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class TestViewModel : ViewModelBase
            {
                [ObservableProperty(SetterAccessModifier = PropertyAccessModifier.Private)]
                private string name;
            }
            """;

        var generatedCode = Assert.Single(RunGenerator<ViewModelGenerator>(inputCode).GeneratorResult.GeneratedSources).SourceText.ToString();

        Assert.DoesNotContain("PropertyAccessModifier", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("RaisePropertyChanged(nameof(Private))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void ObservableProperty_WithoutSetterAccessModifier_HasAPublicSetter()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class TestViewModel : ViewModelBase
            {
                [ObservableProperty]
                private string name;
            }
            """;

        var generatedCode = Assert.Single(RunGenerator<ViewModelGenerator>(inputCode).GeneratorResult.GeneratedSources).SourceText.ToString();

        Assert.Contains(
            """
                    get => name;
                    set
                    {
            """.EnsureEnvironmentNewLines(),
            generatedCode.EnsureEnvironmentNewLines(),
            StringComparison.Ordinal);
    }

    [Fact]
    public void DependencyProperty_SetterAccessModifier_Wpf_IsAppliedToTheSetter()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [DependencyProperty(SetterAccessModifier = PropertyAccessModifier.Private)]
                private string selectedStateMessage;
            }
            """;

        var generatedCode = Assert.Single(RunGenerator<FrameworkElementGenerator>(inputCode).GeneratorResult.GeneratedSources).SourceText.ToString();

        Assert.Contains("public static readonly DependencyProperty SelectedStateMessageProperty = DependencyProperty.Register(", generatedCode, StringComparison.Ordinal);
        Assert.Contains("private set => SetValue(SelectedStateMessageProperty, value);", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void AttachedProperty_SetterAccessModifier_Wpf_IsAppliedToTheSetMethod()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public static partial class DragBehavior
            {
                [AttachedProperty(SetterAccessModifier = PropertyAccessModifier.Internal)]
                private static bool isDraggable;
            }
            """;

        var generatedCode = Assert.Single(RunGenerator<FrameworkElementGenerator>(inputCode).GeneratorResult.GeneratedSources).SourceText.ToString();

        Assert.Contains("public static bool GetIsDraggable(DependencyObject element)", generatedCode, StringComparison.Ordinal);
        Assert.Contains("internal static void SetIsDraggable(DependencyObject element, bool value)", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("PropertyKey", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void ObservableDtoViewModel_CustomProperty_SetterAccessModifier_IsApplied()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public class Order
            {
                public string Number { get; set; }
            }

            [ObservableDtoViewModel(typeof(Order))]
            public partial class OrderViewModel : ViewModelBase
            {
                [ObservableProperty(SetterAccessModifier = PropertyAccessModifier.Internal)]
                private bool isExpanded;
            }
            """;

        var generatedCode = Assert.Single(RunGenerator<ObservableDtoViewModelGenerator>(inputCode).GeneratorResult.GeneratedSources).SourceText.ToString();

        Assert.Contains("internal set", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void SetterAccessModifier_WithResolvedAttributes_GeneratesCodeThatCompiles()
    {
        // In a real project the attribute and the enum are resolved, so the generator reads
        // the enum value and not its text. The consumer compilation targets Avalonia.
        const string inputCode =
            """
            using Atc.XamlToolkit;
            using Atc.XamlToolkit.Controls.Attributes;
            using Atc.XamlToolkit.Mvvm;
            using Avalonia.Controls;

            namespace TestNamespace;

            public partial class DeviceInfo : ObservableObject
            {
                [ObservableProperty(SetterAccessModifier = PropertyAccessModifier.Internal)]
                private string name = string.Empty;

                internal void Rename(string newName) => Name = newName;
            }

            public partial class MyControl : UserControl
            {
                [StyledProperty(SetterAccessModifier = PropertyAccessModifier.Private)]
                private int total;

                public void Increase() => Total++;
            }
            """;

        AssertCompilesWithAllGeneratorsWithoutGlobalUsings(inputCode);

        var generatedCode = string.Join(
            Environment.NewLine,
            GetSourcesGeneratedForAvaloniaConsumer(inputCode));

        Assert.Contains("internal set", generatedCode, StringComparison.Ordinal);
        Assert.Contains("private set => SetValue(TotalProperty, value);", generatedCode, StringComparison.Ordinal);
    }
}