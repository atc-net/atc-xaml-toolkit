namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "OK.")]
public sealed partial class FrameworkElementGeneratorTests
{
    [Fact]
    public void DependencyProperty_DefaultValueExpression_Wpf_BrushType_IsEmittedUnchanged()
    {
        // DefaultValue = "CreateFrozenBrush(40)" on a Brush property becomes Brushes.CreateFrozenBrush(40).
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class ZoomGridOverlay : UserControl
            {
                [DependencyProperty(DefaultValueExpression = "CreateFrozenBrush(40)")]
                private Brush gridBrush;
            }
            """;

        var generatedCode = GenerateFrameworkElementCode(inputCode);

        Assert.Contains("new PropertyMetadata(defaultValue: CreateFrozenBrush(40)));", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("Brushes.", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void DependencyProperty_DefaultValueExpression_Wpf_StringType_IsNotQuoted()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class ColorPicker : UserControl
            {
                [DependencyProperty(DefaultValueExpression = "Brushes.Black.Color.ToString(CultureInfo.InvariantCulture)")]
                private string colorText;
            }
            """;

        var generatedCode = GenerateFrameworkElementCode(inputCode);

        Assert.Contains("defaultValue: Brushes.Black.Color.ToString(CultureInfo.InvariantCulture)));", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void DependencyProperty_DefaultValueExpression_Wpf_WinsOverDefaultValue()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [DependencyProperty(DefaultValue = 1, DefaultValueExpression = "DefaultCount")]
                private int count;
            }
            """;

        var generatedCode = GenerateFrameworkElementCode(inputCode);

        Assert.Contains("defaultValue: DefaultCount));", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void DependencyProperty_DefaultValueExpression_Wpf_WithFlagsAndCallback()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class ZoomGridOverlay : UserControl
            {
                [DependencyProperty(
                    DefaultValueExpression = "CreateFrozenBrush(40)",
                    Flags = FrameworkPropertyMetadataOptions.AffectsRender,
                    PropertyChangedCallback = nameof(OnPenPropertyChanged))]
                private Brush gridBrush;
            }
            """;

        var generatedCode = GenerateFrameworkElementCode(inputCode);

        Assert.Contains("new FrameworkPropertyMetadata(", generatedCode, StringComparison.Ordinal);
        Assert.Contains("defaultValue: CreateFrozenBrush(40),", generatedCode, StringComparison.Ordinal);
        Assert.Contains("propertyChangedCallback: OnPenPropertyChanged,", generatedCode, StringComparison.Ordinal);
        Assert.Contains("flags: FrameworkPropertyMetadataOptions.AffectsRender));", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void AttachedProperty_DefaultValueExpression_Wpf_IsEmittedUnchanged()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public static partial class ItemHelper
            {
                [AttachedProperty(DefaultValueExpression = "new Thickness(1, 2, 3, 4)")]
                private static Thickness itemMargin;
            }
            """;

        var generatedCode = GenerateFrameworkElementCode(inputCode);

        Assert.Contains("defaultValue: new Thickness(1, 2, 3, 4)));", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void DependencyProperty_AddOwnerOf_Wpf_WithDefaultValueExpression_PassesTheDefault()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class TokenBox : UserControl
            {
                [DependencyProperty(AddOwnerOf = typeof(ComboBox), DefaultValueExpression = "SystemParameters.PrimaryScreenHeight / 3")]
                private double maxDropDownHeight;
            }
            """;

        var generatedCode = GenerateFrameworkElementCode(inputCode);

        Assert.Contains("new FrameworkPropertyMetadata(defaultValue: SystemParameters.PrimaryScreenHeight / 3));", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void DependencyProperty_FlagsNone_Wpf_ForcesFrameworkPropertyMetadata()
    {
        // Without flags the generator picks PropertyMetadata. An explicit None is the way to
        // ask for FrameworkPropertyMetadata, e.g. for a class that overrides the metadata.
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [DependencyProperty(DefaultValue = 3, Flags = FrameworkPropertyMetadataOptions.None)]
                private int count;

                [DependencyProperty(DefaultValue = 3)]
                private int step;
            }
            """;

        var generatedCode = GenerateFrameworkElementCode(inputCode);

        Assert.Contains(
            """
                    typeof(MyControl),
                    new FrameworkPropertyMetadata(
                        defaultValue: 3,
                        flags: FrameworkPropertyMetadataOptions.None));
            """.EnsureEnvironmentNewLines(),
            generatedCode,
            StringComparison.Ordinal);
        Assert.Contains("new PropertyMetadata(defaultValue: 3));", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void DefaultValueExpression_WithResolvedAttributes_Avalonia_GeneratesCodeThatCompiles()
    {
        const string inputCode =
            """
            using Atc.XamlToolkit.Controls.Attributes;
            using Avalonia.Controls;

            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [StyledProperty(DefaultValueExpression = "CreateTitle(\"a\", 2)")]
                private string title;

                [StyledProperty(DefaultValueExpression = "System.TimeSpan.FromMilliseconds(300)")]
                private System.TimeSpan delay;

                private static string CreateTitle(string prefix, int number) => prefix + number;
            }
            """;

        AssertCompilesWithAllGeneratorsWithoutGlobalUsings(inputCode);

        var generatedCode = string.Join(
            Environment.NewLine,
            GetSourcesGeneratedForAvaloniaConsumer(inputCode));

        Assert.Contains("defaultValue: CreateTitle(\"a\", 2));", generatedCode, StringComparison.Ordinal);
        Assert.Contains("defaultValue: System.TimeSpan.FromMilliseconds(300));", generatedCode, StringComparison.Ordinal);
    }
}