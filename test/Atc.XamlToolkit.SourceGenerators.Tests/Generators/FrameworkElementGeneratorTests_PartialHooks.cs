namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "OK.")]
public sealed partial class FrameworkElementGeneratorTests
{
    [Fact]
    public void DependencyProperty_GeneratePartialHooks_Wpf_GeneratesCallbackAndTypedHook()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [DependencyProperty(GeneratePartialHooks = true)]
                private int count;
            }
            """;

        var generatedCode = Assert.Single(RunGenerator<FrameworkElementGenerator>(inputCode).GeneratorResult.GeneratedSources).SourceText.ToString();

        Assert.Contains("propertyChangedCallback: OnCountPropertyChangedCallback", generatedCode, StringComparison.Ordinal);
        Assert.Contains(
            """
                private static void OnCountPropertyChangedCallback(
                    DependencyObject d,
                    DependencyPropertyChangedEventArgs e)
                    => ((MyControl)d).OnCountChanged((int)e.OldValue, (int)e.NewValue);

                partial void OnCountChanged(int oldValue, int newValue);
            """.EnsureEnvironmentNewLines(),
            generatedCode.EnsureEnvironmentNewLines(),
            StringComparison.Ordinal);
    }

    [Fact]
    public void DependencyProperty_GeneratePartialHooks_Wpf_WithOwnCallback_CallsBoth()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [DependencyProperty(GeneratePartialHooks = true, PropertyChangedCallback = nameof(OnAnyChanged))]
                private int count;

                private static void OnAnyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
                {
                }
            }
            """;

        var generatedCode = Assert.Single(RunGenerator<FrameworkElementGenerator>(inputCode).GeneratorResult.GeneratedSources).SourceText.ToString();

        Assert.Contains("propertyChangedCallback: OnCountPropertyChangedCallback", generatedCode, StringComparison.Ordinal);
        Assert.Contains(
            """
                private static void OnCountPropertyChangedCallback(
                    DependencyObject d,
                    DependencyPropertyChangedEventArgs e)
                {
                    OnAnyChanged(d, e);
                    ((MyControl)d).OnCountChanged((int)e.OldValue, (int)e.NewValue);
                }

                partial void OnCountChanged(int oldValue, int newValue);
            """.EnsureEnvironmentNewLines(),
            generatedCode.EnsureEnvironmentNewLines(),
            StringComparison.Ordinal);
    }

    [Fact]
    public void DependencyProperty_WithoutGeneratePartialHooks_GeneratesNoHook()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [DependencyProperty(PropertyChangedCallback = nameof(OnAnyChanged))]
                private int count;
            }
            """;

        var generatedCode = Assert.Single(RunGenerator<FrameworkElementGenerator>(inputCode).GeneratorResult.GeneratedSources).SourceText.ToString();

        Assert.Contains("propertyChangedCallback: OnAnyChanged", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("partial void", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("PropertyChangedCallback(", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void AttachedProperty_GeneratePartialHooks_Wpf_GeneratesStaticHookWithTheTargetObject()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public static partial class DragBehavior
            {
                [AttachedProperty(GeneratePartialHooks = true)]
                private static bool isDraggable;
            }
            """;

        var generatedCode = Assert.Single(RunGenerator<FrameworkElementGenerator>(inputCode).GeneratorResult.GeneratedSources).SourceText.ToString();

        Assert.Contains("propertyChangedCallback: OnIsDraggablePropertyChangedCallback", generatedCode, StringComparison.Ordinal);
        Assert.Contains(
            """
                private static void OnIsDraggablePropertyChangedCallback(
                    DependencyObject d,
                    DependencyPropertyChangedEventArgs e)
                    => OnIsDraggableChanged(d, (bool)e.OldValue, (bool)e.NewValue);

                static partial void OnIsDraggableChanged(DependencyObject d, bool oldValue, bool newValue);
            """.EnsureEnvironmentNewLines(),
            generatedCode.EnsureEnvironmentNewLines(),
            StringComparison.Ordinal);
    }

    [Fact]
    public void StyledAndAttachedProperty_GeneratePartialHooks_Avalonia_GeneratesCodeThatCompiles()
    {
        const string inputCode =
            """
            using Atc.XamlToolkit.Controls.Attributes;
            using Avalonia;
            using Avalonia.Controls;

            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [StyledProperty(GeneratePartialHooks = true)]
                private int count;

                public int LastOldValue { get; private set; }

                public int LastNewValue { get; private set; }

                partial void OnCountChanged(int oldValue, int newValue)
                {
                    LastOldValue = oldValue;
                    LastNewValue = newValue;
                }
            }

            [AttachedProperty<bool>("IsDraggable", GeneratePartialHooks = true)]
            public partial class DragBehavior : AvaloniaObject
            {
                public static AvaloniaObject? LastTarget { get; private set; }

                static partial void OnIsDraggableChanged(AvaloniaObject d, bool oldValue, bool newValue)
                    => LastTarget = d;
            }
            """;

        AssertCompilesWithAllGeneratorsWithoutGlobalUsings(inputCode);

        var generatedCode = string.Join(
            Environment.NewLine,
            GetSourcesGeneratedForAvaloniaConsumer(inputCode));

        Assert.Contains("=> sender.OnCountChanged(e.GetOldValue<int>(), e.GetNewValue<int>());", generatedCode, StringComparison.Ordinal);
        Assert.Contains("partial void OnCountChanged(int oldValue, int newValue);", generatedCode, StringComparison.Ordinal);
        Assert.Contains("=> OnIsDraggableChanged(d, e.GetOldValue<bool>(), e.GetNewValue<bool>());", generatedCode, StringComparison.Ordinal);
        Assert.Contains("static partial void OnIsDraggableChanged(AvaloniaObject d, bool oldValue, bool newValue);", generatedCode, StringComparison.Ordinal);
    }
}