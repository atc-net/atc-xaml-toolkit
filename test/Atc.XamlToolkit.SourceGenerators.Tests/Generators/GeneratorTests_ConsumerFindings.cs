namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

/// <summary>
/// Problems found while converting a consumer library to the generators.
/// </summary>
public sealed class GeneratorTests_ConsumerFindings : GeneratorTestBase
{
    private static string Generate<T>(string inputCode)
        where T : IIncrementalGenerator, new()
        => string.Join(
                Environment.NewLine,
                RunGenerator<T>(inputCode).GeneratorResult.GeneratedSources.Select(x => x.SourceText.ToString()))
            .EnsureEnvironmentNewLines();

    [Fact]
    public void DependencyProperty_IsReadOnly_WithAnExplicitPublicSetter_Wpf_KeepsTheSetterPublic()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class NiceThumb : UserControl
            {
                [DependencyProperty(IsReadOnly = true, SetterAccessModifier = PropertyAccessModifier.Public)]
                private bool isDragging;

                [DependencyProperty(IsReadOnly = true)]
                private bool isMoving;
            }
            """;

        var generatedCode = Generate<FrameworkElementGenerator>(inputCode);

        Assert.Contains("        set => SetValue(IsDraggingPropertyKey, Atc.XamlToolkit.BooleanBoxes.Box(value));", generatedCode, StringComparison.Ordinal);
        Assert.Contains("        private set => SetValue(IsMovingPropertyKey, Atc.XamlToolkit.BooleanBoxes.Box(value));", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void AttachedProperty_IsReadOnly_WithAnExplicitPublicSetter_Wpf_KeepsTheSetMethodPublic()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public static partial class ThumbHelper
            {
                [AttachedProperty(IsReadOnly = true, SetterAccessModifier = PropertyAccessModifier.Public)]
                private static bool isDragging;
            }
            """;

        var generatedCode = Generate<FrameworkElementGenerator>(inputCode);

        Assert.Contains("public static void SetIsDragging(DependencyObject element, bool value)", generatedCode, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("[DependencyProperty(PropertyName = \"State\")]", "private int stateValue;")]
    [InlineData("[DependencyProperty(PropertyName = \"State\")]", "private int _state;")]
    public void DependencyProperty_PropertyName_Wpf_NamesThePropertyIndependentlyOfTheField(
        string attribute,
        string field)
    {
        var inputCode =
            $$"""
            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                {{attribute}}
                {{field}}
            }
            """;

        var generatedCode = Generate<FrameworkElementGenerator>(inputCode);

        Assert.Contains("public static readonly DependencyProperty StateProperty = DependencyProperty.Register(", generatedCode, StringComparison.Ordinal);
        Assert.Contains("nameof(State),", generatedCode, StringComparison.Ordinal);
        Assert.Contains("public int State", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("StateValue", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void AttachedProperty_PropertyName_Wpf_NamesThePropertyAndItsAccessors()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public static partial class DataGridFilter
            {
                /// <summary>
                /// The filter state of the grid.
                /// </summary>
                [AttachedProperty(PropertyName = "State", GeneratePartialHooks = true)]
                private static FilterState? stateField;
            }
            """;

        var generatedCode = Generate<FrameworkElementGenerator>(inputCode);

        Assert.Contains("StateProperty = DependencyProperty.RegisterAttached(", generatedCode, StringComparison.Ordinal);
        Assert.Contains("\"State\",", generatedCode, StringComparison.Ordinal);
        Assert.Contains("public static FilterState? GetState(DependencyObject element)", generatedCode, StringComparison.Ordinal);
        Assert.Contains("public static void SetState(DependencyObject element, FilterState? value)", generatedCode, StringComparison.Ordinal);
        Assert.Contains("static partial void OnStateChanged(DependencyObject d, FilterState? oldValue, FilterState? newValue);", generatedCode, StringComparison.Ordinal);
        Assert.Contains("/// The filter state of the grid.", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("StateField", generatedCode, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not valid")]
    public void DependencyProperty_PropertyName_Wpf_ThatIsNotAnIdentifier_FallsBackToTheFieldName(
        string name)
    {
        var inputCode =
            $$"""
            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [DependencyProperty(PropertyName = "{{name}}")]
                private int count;
            }
            """;

        Assert.Contains("public int Count", Generate<FrameworkElementGenerator>(inputCode), StringComparison.Ordinal);
    }

    [Fact]
    public void PropertyName_WithResolvedAttributes_Avalonia_GeneratesCodeThatCompiles()
    {
        const string inputCode =
            """
            using Atc.XamlToolkit.Controls.Attributes;
            using Avalonia;
            using Avalonia.Controls;

            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [StyledProperty(PropertyName = "State")]
                private int stateValue;

                public int Next()
                {
                    var state = State + 1;
                    State = state;
                    return state;
                }
            }

            public partial class FilterBehavior : AvaloniaObject
            {
                [AttachedProperty(PropertyName = "Hits")]
                private static int hitCount;
            }
            """;

        AssertCompilesWithAllGeneratorsWithoutGlobalUsings(inputCode);

        var generatedCode = string.Join(Environment.NewLine, GetSourcesGeneratedForAvaloniaConsumer(inputCode));

        Assert.Contains("public int State", generatedCode, StringComparison.Ordinal);
        Assert.Contains("public static int GetHits(AvaloniaObject element)", generatedCode, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Private", "private")]
    [InlineData("Internal", "internal")]
    [InlineData("Public", "public")]
    [InlineData("ProtectedInternal", "protected internal")]
    public void RoutedEvent_RaiseMethodAccessModifier_Wpf(
        string enumValue,
        string modifier)
    {
        var inputCode =
            $$"""
            namespace TestNamespace;

            public partial class CustomButton : Button
            {
                [RoutedEvent(GenerateRaiseMethod = true, RaiseMethodAccessModifier = PropertyAccessModifier.{{enumValue}})]
                private static readonly RoutedEvent tap;
            }
            """;

        var generatedCode = Generate<FrameworkElementGenerator>(inputCode);

        Assert.Contains($"    {modifier} void RaiseTap()", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void RoutedEvent_WithoutRaiseMethodAccessModifier_Wpf_IsProtected()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class CustomButton : Button
            {
                [RoutedEvent(GenerateRaiseMethod = true)]
                private static readonly RoutedEvent tap;
            }
            """;

        Assert.Contains("    protected void RaiseTap()", Generate<FrameworkElementGenerator>(inputCode), StringComparison.Ordinal);
    }

    [Fact]
    public void INotifyPropertyChanged_OnASealedClass_GeneratesPrivateMembers()
    {
        // 'protected' in a sealed class is CS0628, which fails a build with warnings as errors.
        const string inputCode =
            """
            namespace TestNamespace;

            [INotifyPropertyChanged]
            public sealed partial class CheckListItem
            {
            }
            """;

        var generatedCode = Generate<ViewModelGenerator>(inputCode);

        Assert.Contains("public event PropertyChangedEventHandler? PropertyChanged;", generatedCode, StringComparison.Ordinal);
        Assert.Contains("    private void RaisePropertyChanged(", generatedCode, StringComparison.Ordinal);
        Assert.Contains("    private void OnPropertyChanged(", generatedCode, StringComparison.Ordinal);
        Assert.Contains("    private bool Set<T>(", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("protected", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void INotifyPropertyChanged_OnAClassThatIsNotSealed_KeepsProtectedMembers()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            [INotifyPropertyChanged]
            public partial class CheckListItem
            {
            }
            """;

        var generatedCode = Generate<ViewModelGenerator>(inputCode);

        Assert.Contains("    protected void RaisePropertyChanged(", generatedCode, StringComparison.Ordinal);
        Assert.Contains("    protected bool Set<T>(", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("///", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void INotifyPropertyChanged_WithGenerateDocumentationDefault_DocumentsTheGeneratedMembers()
    {
        // Without summaries a project that reports CS1591 cannot use the attribute on a public class.
        const string inputCode =
            """
            [assembly: GenerateDocumentationDefault]

            namespace TestNamespace;

            [INotifyPropertyChanged]
            public partial class CheckListItem
            {
            }
            """;

        var generatedCode = Generate<ViewModelGenerator>(inputCode);

        Assert.Contains(
            """
                /// <summary>
                /// Occurs when a property value changes.
                /// </summary>
                public event PropertyChangedEventHandler? PropertyChanged;
            """.EnsureEnvironmentNewLines(),
            generatedCode,
            StringComparison.Ordinal);
        Assert.Equal(4, System.Text.RegularExpressions.Regex.Matches(generatedCode, "/// <summary>", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(1)).Count);
    }

    [Fact]
    public void INotifyPropertyChanged_OnASealedClass_WithObservableProperty_CompilesWithoutWarnings()
    {
        const string inputCode =
            """
            using Atc.XamlToolkit.Mvvm;

            [assembly: GenerateDocumentationDefault]

            namespace TestNamespace;

            /// <summary>
            /// An item in a check list.
            /// </summary>
            [INotifyPropertyChanged]
            public sealed partial class CheckListItem
            {
                [ObservableProperty]
                private bool isChecked;
            }
            """;

        AssertCompilesWithAllGeneratorsWithoutGlobalUsings(inputCode);
    }
}