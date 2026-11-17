namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

/// <summary>
/// Gaps found while converting the projects of a consumer library.
/// </summary>
public sealed class GeneratorTests_ConversionFindings : GeneratorTestBase
{
    private static (string GeneratedCode, IReadOnlyList<string> DiagnosticIds) Generate<T>(
        string inputCode)
        where T : IIncrementalGenerator, new()
    {
        var result = RunGenerator<T>(inputCode);

        var generatedCode = string.Join(
                Environment.NewLine,
                result.GeneratorResult.GeneratedSources.Select(x => x.SourceText.ToString()))
            .EnsureEnvironmentNewLines();

        return (generatedCode, result.GeneratorResult.Diagnostics.Select(x => x.Id).ToList());
    }

    [Theory]
    [InlineData("Internal", "internal")]
    [InlineData("Protected", "protected")]
    public void DependencyProperty_PropertyKeyAccessModifier_Wpf_IsTheAccessibilityOfTheKey(
        string enumValue,
        string modifier)
    {
        var inputCode =
            $$"""
            namespace TestNamespace;

            public partial class WindowCommands : UserControl
            {
                [DependencyProperty(IsReadOnly = true, PropertyKeyAccessModifier = PropertyAccessModifier.{{enumValue}})]
                private Window parentWindow;
            }
            """;

        var (generatedCode, _) = Generate<FrameworkElementGenerator>(inputCode);

        Assert.Contains($"    {modifier} static readonly DependencyPropertyKey ParentWindowPropertyKey = DependencyProperty.RegisterReadOnly(", generatedCode, StringComparison.Ordinal);
        Assert.Contains("        private set => SetValue(ParentWindowPropertyKey, value);", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void AttachedProperty_PropertyKeyAccessModifier_Wpf_IsTheAccessibilityOfTheKey()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public static partial class DataGridFilter
            {
                [AttachedProperty(IsReadOnly = true, PropertyKeyAccessModifier = PropertyAccessModifier.Internal)]
                private static bool isColumnFilterActive;

                [AttachedProperty(IsReadOnly = true)]
                private static bool isFiltered;
            }
            """;

        var (generatedCode, _) = Generate<FrameworkElementGenerator>(inputCode);

        Assert.Contains("internal static readonly DependencyPropertyKey IsColumnFilterActivePropertyKey", generatedCode, StringComparison.Ordinal);
        Assert.Contains("private static readonly DependencyPropertyKey IsFilteredPropertyKey", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void AttachedProperty_WithoutAccessors_Wpf_PutsTheFieldDocumentationOnThePropertyIdentifier()
    {
        // With no accessors to carry it, the documentation written on the field would be lost.
        const string inputCode =
            """
            namespace TestNamespace;

            public static partial class DataGridFilter
            {
                /// <summary>
                /// Identifies the IsEnabled attached property, which turns the
                /// column filters of a grid on.
                /// </summary>
                [AttachedProperty(GenerateClrAccessors = false)]
                private static bool isEnabled;

                /// <summary>
                /// Gets or sets the search text.
                /// </summary>
                [AttachedProperty]
                private static string searchText;
            }
            """;

        var (generatedCode, _) = Generate<FrameworkElementGenerator>(inputCode);

        Assert.Contains(
            """
                /// <summary>
                /// Identifies the IsEnabled attached property, which turns the
                /// column filters of a grid on.
                /// </summary>
                public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            """.EnsureEnvironmentNewLines(),
            generatedCode,
            StringComparison.Ordinal);
        Assert.Contains(
            """
                /// <summary>
                /// Identifies the SearchText attached property.
                /// </summary>
                public static readonly DependencyProperty SearchTextProperty = DependencyProperty.RegisterAttached(
            """.EnsureEnvironmentNewLines(),
            generatedCode,
            StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("public partial class Item")]
    [InlineData("public partial class Item : System.ComponentModel.INotifyPropertyChanged")]
    [InlineData("public partial class Item : SomeOtherBase")]
    public void ObservableProperty_InAClassThatCannotRaiseTheChange_ReportsADiagnostic(
        string classDeclaration)
    {
        // The generator needs RaisePropertyChanged from ObservableObject or [INotifyPropertyChanged].
        // Without it nothing is generated, and nothing said why.
        var inputCode =
            $$"""
            namespace TestNamespace;

            {{classDeclaration}}
            {
                [ObservableProperty]
                private bool isChecked;
            }
            """;

        var (generatedCode, diagnosticIds) = Generate<ViewModelGenerator>(inputCode);

        Assert.Equal(["AtcXamlToolkit0014"], diagnosticIds);
        Assert.Empty(generatedCode);
    }

    [Theory]
    [InlineData("public partial class Item : ViewModelBase")]
    [InlineData("public partial class Item : ObservableObject")]
    [InlineData("[INotifyPropertyChanged] public partial class Item")]
    [InlineData("[ObservableDtoViewModel(typeof(Dto))] public partial class Item : ViewModelBase")]
    public void ObservableProperty_InAClassThatCanRaiseTheChange_ReportsNoDiagnostic(
        string classDeclaration)
    {
        var inputCode =
            $$"""
            namespace TestNamespace;

            public class Dto
            {
                public string Name { get; set; }
            }

            {{classDeclaration}}
            {
                [ObservableProperty]
                private bool isChecked;
            }
            """;

        var (_, diagnosticIds) = Generate<ViewModelGenerator>(inputCode);

        Assert.DoesNotContain("AtcXamlToolkit0014", diagnosticIds, StringComparer.Ordinal);
    }

    [Fact]
    public void PropertyTemplate_DocumentationValues_FillThePlaceholdersInTheTemplateDocumentation()
    {
        const string inputCode =
            """
            using Atc.XamlToolkit.Controls.Attributes;
            using Avalonia.Controls;

            namespace TestNamespace;

            [DependencyPropertyTemplate]
            internal abstract class PickerMembers
            {
                /// <summary>
                /// Gets or sets the selected {0}; <see langword="null"/> when no {0} is
                /// selected. Use <see cref="System.Collections.Generic.List{T}"/> for {1}.
                /// </summary>
                [StyledProperty]
                private string? value;

                /// <summary>
                /// Gets or sets the placeholder text.
                /// </summary>
                [StyledProperty(DefaultValue = "")]
                private string watermarkText;
            }

            [UseDependencyPropertyTemplate(typeof(PickerMembers), DocumentationValues = ["drive", "drives"])]
            public partial class DrivePicker : UserControl
            {
            }

            [UseDependencyPropertyTemplate(typeof(PickerMembers), DocumentationValues = ["printer", "printers"])]
            public partial class PrinterPicker : UserControl
            {
            }
            """;

        AssertCompilesWithAllGeneratorsWithoutGlobalUsings(inputCode);

        var sources = GetSourcesGeneratedForAvaloniaConsumer(inputCode);
        var drivePicker = sources.Single(x => x.Contains("partial class DrivePicker", StringComparison.Ordinal)).EnsureEnvironmentNewLines();
        var printerPicker = sources.Single(x => x.Contains("partial class PrinterPicker", StringComparison.Ordinal));

        Assert.Contains(
            """
                /// <summary>
                /// Gets or sets the selected drive; <see langword="null"/> when no drive is
                /// selected. Use <see cref="System.Collections.Generic.List{T}"/> for drives.
                /// </summary>
            """.EnsureEnvironmentNewLines(),
            drivePicker,
            StringComparison.Ordinal);
        Assert.Contains("/// Gets or sets the selected printer;", printerPicker, StringComparison.Ordinal);
        Assert.Contains("/// Gets or sets the placeholder text.", printerPicker, StringComparison.Ordinal);
        Assert.DoesNotContain("{0}", drivePicker + printerPicker, StringComparison.Ordinal);
    }
}