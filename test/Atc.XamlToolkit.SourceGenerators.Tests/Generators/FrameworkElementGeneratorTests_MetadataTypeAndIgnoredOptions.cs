namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "OK.")]
public sealed partial class FrameworkElementGeneratorTests
{
    private const string IgnoredOptionDiagnosticId = "AtcXamlToolkit0013";

    private static (string GeneratedCode, IReadOnlyList<string> DiagnosticIds) GenerateWithDiagnostics(
        string inputCode)
    {
        var result = RunGenerator<FrameworkElementGenerator>(inputCode);

        var generatedCode = string.Join(
            Environment.NewLine,
            result.GeneratorResult.GeneratedSources.Select(x => x.SourceText.ToString()))
            .EnsureEnvironmentNewLines();

        return (generatedCode, result.GeneratorResult.Diagnostics.Select(x => x.Id).ToList());
    }

    [Theory]
    [InlineData("DefaultValue = 3", "new UIPropertyMetadata(defaultValue: 3));")]
    [InlineData("PropertyChangedCallback = nameof(OnCountChanged)", "new UIPropertyMetadata(defaultValue: 0, propertyChangedCallback: OnCountChanged));")]
    [InlineData("CoerceValueCallback = nameof(CoerceCount)", "new UIPropertyMetadata(defaultValue: 0, propertyChangedCallback: null, coerceValueCallback: CoerceCount));")]
    [InlineData("DefaultValue = 3, PropertyChangedCallback = nameof(OnCountChanged), CoerceValueCallback = nameof(CoerceCount)", "new UIPropertyMetadata(defaultValue: 3, propertyChangedCallback: OnCountChanged, coerceValueCallback: CoerceCount));")]
    [InlineData("IsAnimationProhibited = true", "new UIPropertyMetadata(defaultValue: 0, propertyChangedCallback: null, coerceValueCallback: null, isAnimationProhibited: true));")]
    public void DependencyProperty_MetadataType_UIPropertyMetadata_Wpf(
        string options,
        string expectedMetadata)
    {
        var inputCode =
            $$"""
            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [DependencyProperty(MetadataType = DependencyPropertyMetadataType.UIPropertyMetadata, {{options}})]
                private int count;
            }
            """;

        var (generatedCode, diagnosticIds) = GenerateWithDiagnostics(inputCode);

        Assert.Contains(expectedMetadata, generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("new PropertyMetadata(", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("FrameworkPropertyMetadata", generatedCode, StringComparison.Ordinal);
        Assert.Empty(diagnosticIds);
    }

    [Fact]
    public void DependencyProperty_MetadataType_UIPropertyMetadata_Wpf_ReferenceTypeWithoutOptions_HasNoArguments()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [DependencyProperty(MetadataType = DependencyPropertyMetadataType.UIPropertyMetadata)]
                private Brush headerBrush;

                [DependencyProperty(MetadataType = DependencyPropertyMetadataType.UIPropertyMetadata, PropertyChangedCallback = nameof(OnTextChanged))]
                private Brush textBrush;
            }
            """;

        var (generatedCode, _) = GenerateWithDiagnostics(inputCode);

        Assert.Contains("new UIPropertyMetadata());", generatedCode, StringComparison.Ordinal);
        Assert.Contains("new UIPropertyMetadata(propertyChangedCallback: OnTextChanged));", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void DependencyProperty_MetadataType_UIPropertyMetadata_Wpf_WithValidateValueCallback()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [DependencyProperty(MetadataType = DependencyPropertyMetadataType.UIPropertyMetadata, DefaultValue = 3, ValidateValueCallback = nameof(IsValid))]
                private int count;
            }
            """;

        var (generatedCode, _) = GenerateWithDiagnostics(inputCode);

        Assert.Contains("new UIPropertyMetadata(defaultValue: 3),", generatedCode, StringComparison.Ordinal);
        Assert.Contains("validateValueCallback: IsValid);", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void AttachedProperty_MetadataType_UIPropertyMetadata_Wpf()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public static partial class PanelHelper
            {
                [AttachedProperty(MetadataType = DependencyPropertyMetadataType.UIPropertyMetadata, DefaultValue = 0d, PropertyChangedCallback = nameof(OnSpacingChanged))]
                private static double spacing;
            }
            """;

        var (generatedCode, _) = GenerateWithDiagnostics(inputCode);

        Assert.Contains("SpacingProperty = DependencyProperty.RegisterAttached(", generatedCode, StringComparison.Ordinal);
        Assert.Contains("new UIPropertyMetadata(defaultValue: 0d, propertyChangedCallback: OnSpacingChanged));", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void DependencyProperty_MetadataType_PropertyMetadata_Wpf()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [DependencyProperty(MetadataType = DependencyPropertyMetadataType.PropertyMetadata, DefaultValue = 3, PropertyChangedCallback = nameof(OnCountChanged))]
                private int count;
            }
            """;

        var (generatedCode, _) = GenerateWithDiagnostics(inputCode);

        Assert.Contains("new PropertyMetadata(defaultValue: 3, propertyChangedCallback: OnCountChanged));", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void DependencyProperty_MetadataType_FrameworkPropertyMetadata_Wpf_WithoutFlags()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [DependencyProperty(MetadataType = DependencyPropertyMetadataType.FrameworkPropertyMetadata, DefaultValue = 3)]
                private int count;
            }
            """;

        var (generatedCode, diagnosticIds) = GenerateWithDiagnostics(inputCode);

        Assert.Contains("new FrameworkPropertyMetadata(", generatedCode, StringComparison.Ordinal);
        Assert.Contains("defaultValue: 3,", generatedCode, StringComparison.Ordinal);
        Assert.Contains("flags: FrameworkPropertyMetadataOptions.None));", generatedCode, StringComparison.Ordinal);
        Assert.Empty(diagnosticIds);
    }

    [Theory]
    [InlineData("MetadataType = DependencyPropertyMetadataType.UIPropertyMetadata, Flags = FrameworkPropertyMetadataOptions.AffectsRender")]
    [InlineData("MetadataType = DependencyPropertyMetadataType.UIPropertyMetadata, DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged")]
    [InlineData("MetadataType = DependencyPropertyMetadataType.PropertyMetadata, Flags = FrameworkPropertyMetadataOptions.AffectsRender")]
    [InlineData("MetadataType = DependencyPropertyMetadataType.PropertyMetadata, IsAnimationProhibited = true")]
    public void DependencyProperty_MetadataType_ThatCannotCarryAnOption_Wpf_IsIgnoredWithADiagnostic(
        string options)
    {
        var inputCode =
            $$"""
            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [DependencyProperty({{options}})]
                private int count;
            }
            """;

        var (generatedCode, diagnosticIds) = GenerateWithDiagnostics(inputCode);

        Assert.Equal([IgnoredOptionDiagnosticId], diagnosticIds);
        Assert.Contains("new FrameworkPropertyMetadata(", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("UIPropertyMetadata", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void DependencyProperty_AddOwnerOf_Wpf_WithUIPropertyMetadata()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class TokenBox : UserControl
            {
                [DependencyProperty(AddOwnerOf = typeof(ComboBox), MetadataType = DependencyPropertyMetadataType.UIPropertyMetadata, PropertyChangedCallback = nameof(OnChanged))]
                private double maxDropDownHeight;
            }
            """;

        var (generatedCode, _) = GenerateWithDiagnostics(inputCode);

        Assert.Contains("new UIPropertyMetadata(propertyChangedCallback: OnChanged));", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void DependencyProperty_AddOwnerOf_Wpf_FlagsWithoutDefaultValue_TakesTheDefaultOfTheSharedProperty()
    {
        // The metadata constructors that take flags also take a default value. Passing the
        // default of the property type would replace the default of the shared property.
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class NumericBox : UserControl
            {
                [DependencyProperty(AddOwnerOf = typeof(TextBoxBase), Flags = FrameworkPropertyMetadataOptions.Inherits)]
                private bool isReadOnly;
            }
            """;

        var (generatedCode, _) = GenerateWithDiagnostics(inputCode);

        Assert.Contains("defaultValue: TextBoxBase.IsReadOnlyProperty.GetMetadata(typeof(NumericBox)).DefaultValue,", generatedCode, StringComparison.Ordinal);
        Assert.Contains("flags: FrameworkPropertyMetadataOptions.Inherits));", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("FalseBox", generatedCode, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("AddOwnerOf = typeof(ComboBox), ValidateValueCallback = nameof(IsValid)")]
    [InlineData("AddOwnerOf = typeof(ComboBox), IsReadOnly = true")]
    public void DependencyProperty_AddOwnerOf_Wpf_WithAnOptionThatDoesNotApply_ReportsADiagnostic(
        string options)
    {
        var inputCode =
            $$"""
            namespace TestNamespace;

            public partial class TokenBox : UserControl
            {
                [DependencyProperty({{options}})]
                private double maxDropDownHeight;
            }
            """;

        var (generatedCode, diagnosticIds) = GenerateWithDiagnostics(inputCode);

        Assert.Equal([IgnoredOptionDiagnosticId], diagnosticIds);
        Assert.Contains("ComboBox.MaxDropDownHeightProperty.AddOwner(", generatedCode, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("public partial class CustomButton : Button", "HandlerType = typeof(CustomChangedEventHandler), GenerateRaiseMethod = true")]
    [InlineData("public static partial class CustomEvents", "GenerateRaiseMethod = true")]
    public void RoutedEvent_GenerateRaiseMethod_ThatCannotBeGenerated_ReportsADiagnostic(
        string classDeclaration,
        string options)
    {
        var inputCode =
            $$"""
            namespace TestNamespace;

            {{classDeclaration}}
            {
                [RoutedEvent({{options}})]
                private static readonly RoutedEvent changed;
            }
            """;

        var (generatedCode, diagnosticIds) = GenerateWithDiagnostics(inputCode);

        Assert.Equal([IgnoredOptionDiagnosticId], diagnosticIds);
        Assert.Contains("ChangedEvent", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("RaiseChanged", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void RoutedEvent_GenerateRaiseMethod_ThatCanBeGenerated_ReportsNoDiagnostic()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class CustomButton : Button
            {
                [RoutedEvent(HandlerType = typeof(System.Windows.RoutedPropertyChangedEventHandler<int>), GenerateRaiseMethod = true)]
                private static readonly RoutedEvent changed;
            }
            """;

        var (generatedCode, diagnosticIds) = GenerateWithDiagnostics(inputCode);

        Assert.Empty(diagnosticIds);
        Assert.Contains("protected void RaiseChanged(int oldValue, int newValue)", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void DependencyProperty_IsReadOnly_OnAReadonlyField_Wpf_GeneratesNoSetter()
    {
        // The class writes through the key itself.
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [DependencyProperty(IsReadOnly = true)]
                private readonly string summaryText;
            }
            """;

        var (generatedCode, _) = GenerateWithDiagnostics(inputCode);

        Assert.Contains("private static readonly DependencyPropertyKey SummaryTextPropertyKey = DependencyProperty.RegisterReadOnly(", generatedCode, StringComparison.Ordinal);
        Assert.Contains("get => (string)GetValue(SummaryTextProperty);", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("set =>", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void AttachedProperty_IsReadOnly_OnAReadonlyField_Wpf_GeneratesNoSetMethod()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public static partial class DataGridFilter
            {
                [AttachedProperty(IsReadOnly = true)]
                private static readonly bool isFiltered;
            }
            """;

        var (generatedCode, _) = GenerateWithDiagnostics(inputCode);

        Assert.Contains("IsFilteredPropertyKey = DependencyProperty.RegisterAttachedReadOnly(", generatedCode, StringComparison.Ordinal);
        Assert.Contains("public static bool GetIsFiltered(DependencyObject element)", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("SetIsFiltered", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void AttachedProperty_BrowsableForTargetType_Wpf_AddsTheAttributeToTheGetMethod()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public static partial class CheckBoxHelper
            {
                [AttachedProperty(TargetType = typeof(CheckBox), BrowsableForTargetType = true)]
                private static double checkSize;

                [AttachedProperty(TargetType = typeof(CheckBox))]
                private static double checkMargin;
            }
            """;

        var (generatedCode, _) = GenerateWithDiagnostics(inputCode);

        Assert.Contains(
            """
                [AttachedPropertyBrowsableForType(typeof(CheckBox))]
                public static double GetCheckSize(CheckBox element)
            """.EnsureEnvironmentNewLines(),
            generatedCode,
            StringComparison.Ordinal);
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(generatedCode, "AttachedPropertyBrowsableForType", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(1)));
    }
}