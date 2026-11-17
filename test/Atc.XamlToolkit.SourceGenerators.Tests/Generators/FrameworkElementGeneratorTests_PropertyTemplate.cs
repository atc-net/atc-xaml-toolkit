namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "OK.")]
public sealed partial class FrameworkElementGeneratorTests
{
    private const string PropertyTemplateUsings =
        """
        using Atc.XamlToolkit.Controls.Attributes;
        using Avalonia.Controls;

        namespace TestNamespace;

        """;

    private static string GeneratedForClass(
        string inputCode,
        string className)
        => GetSourcesGeneratedForAvaloniaConsumer(inputCode)
            .Single(x => x.Contains($"partial class {className}", StringComparison.Ordinal))
            .EnsureEnvironmentNewLines();

    [Fact]
    public void PropertyTemplate_IsAppliedToEveryClassThatUsesIt()
    {
        const string inputCode = PropertyTemplateUsings +
            """
            [DependencyPropertyTemplate]
            internal abstract class PickerMembers
            {
                /// <summary>
                /// Gets or sets the text shown when nothing is selected.
                /// </summary>
                [StyledProperty(DefaultValue = "")]
                private string watermarkText;

                [StyledProperty(DefaultValue = true)]
                private bool showRefreshButton;
            }

            [UseDependencyPropertyTemplate(typeof(PickerMembers))]
            public partial class DrivePicker : UserControl
            {
            }

            [UseDependencyPropertyTemplate(typeof(PickerMembers))]
            public partial class PrinterPicker : UserControl
            {
            }
            """;

        AssertCompilesWithAllGeneratorsWithoutGlobalUsings(inputCode);

        var drivePicker = GeneratedForClass(inputCode, "DrivePicker");
        var printerPicker = GeneratedForClass(inputCode, "PrinterPicker");

        Assert.Contains("public static readonly StyledProperty<string> WatermarkTextProperty = Avalonia.AvaloniaProperty.Register<DrivePicker, string>(", drivePicker, StringComparison.Ordinal);
        Assert.Contains("public static readonly StyledProperty<bool> ShowRefreshButtonProperty = Avalonia.AvaloniaProperty.Register<DrivePicker, bool>(", drivePicker, StringComparison.Ordinal);
        Assert.Contains("/// Gets or sets the text shown when nothing is selected.", drivePicker, StringComparison.Ordinal);
        Assert.Contains("public string WatermarkText", drivePicker, StringComparison.Ordinal);

        Assert.Contains("WatermarkTextProperty = Avalonia.AvaloniaProperty.Register<PrinterPicker, string>(", printerPicker, StringComparison.Ordinal);
        Assert.Contains("ShowRefreshButtonProperty = Avalonia.AvaloniaProperty.Register<PrinterPicker, bool>(", printerPicker, StringComparison.Ordinal);
    }

    [Fact]
    public void PropertyTemplate_GeneratesTheSameCodeAsFieldsDeclaredInTheClass()
    {
        const string withTemplate = PropertyTemplateUsings +
            """
            [DependencyPropertyTemplate]
            internal abstract class PickerMembers
            {
                [StyledProperty(DefaultValue = "", GeneratePartialHooks = true)]
                private string watermarkText;

                [StyledProperty(DefaultValue = true, SetterAccessModifier = Atc.XamlToolkit.PropertyAccessModifier.Private)]
                private bool showRefreshButton;
            }

            [UseDependencyPropertyTemplate(typeof(PickerMembers))]
            public partial class DrivePicker : UserControl
            {
            }
            """;

        const string withoutTemplate = PropertyTemplateUsings +
            """
            public partial class DrivePicker : UserControl
            {
                [StyledProperty(DefaultValue = "", GeneratePartialHooks = true)]
                private string watermarkText;

                [StyledProperty(DefaultValue = true, SetterAccessModifier = Atc.XamlToolkit.PropertyAccessModifier.Private)]
                private bool showRefreshButton;
            }
            """;

        Assert.Equal(
            GeneratedForClass(withoutTemplate, "DrivePicker"),
            GeneratedForClass(withTemplate, "DrivePicker"));
    }

    [Fact]
    public void PropertyTemplate_Generic_SubstitutesTheTypeArgument()
    {
        const string inputCode = PropertyTemplateUsings +
            """
            public sealed class DiskDriveInfo
            {
            }

            [DependencyPropertyTemplate]
            internal abstract class PickerMembers<TInfo>
            {
                [StyledProperty]
                private TInfo value;

                [StyledProperty]
                private System.Collections.Generic.IList<TInfo> items;
            }

            [UseDependencyPropertyTemplate(typeof(PickerMembers<DiskDriveInfo?>))]
            public partial class DrivePicker : UserControl
            {
            }

            [UseDependencyPropertyTemplate(typeof(PickerMembers<int>))]
            public partial class NumberPicker : UserControl
            {
            }
            """;

        AssertCompilesWithAllGeneratorsWithoutGlobalUsings(inputCode);

        var drivePicker = GeneratedForClass(inputCode, "DrivePicker");
        var numberPicker = GeneratedForClass(inputCode, "NumberPicker");

        Assert.Contains("public DiskDriveInfo? Value", drivePicker, StringComparison.Ordinal);
        Assert.Contains("public IList<DiskDriveInfo?> Items", drivePicker, StringComparison.Ordinal);
        Assert.Contains("public int Value", numberPicker, StringComparison.Ordinal);
        Assert.Contains("public IList<int> Items", numberPicker, StringComparison.Ordinal);
        Assert.DoesNotContain("TInfo", drivePicker + numberPicker, StringComparison.Ordinal);
    }

    [Fact]
    public void PropertyTemplate_Exclude_LeavesOutTheNamedMembers()
    {
        const string inputCode = PropertyTemplateUsings +
            """
            [DependencyPropertyTemplate]
            internal abstract class PickerMembers
            {
                [StyledProperty(DefaultValue = "")]
                private string watermarkText;

                [StyledProperty(DefaultValue = true)]
                private bool showRefreshButton;

                [StyledProperty]
                private int _maxItems;
            }

            [UseDependencyPropertyTemplate(typeof(PickerMembers), Exclude = ["ShowRefreshButton", "MaxItems"])]
            public partial class LabelDrivePicker : UserControl
            {
            }
            """;

        AssertCompilesWithAllGeneratorsWithoutGlobalUsings(inputCode);

        var generatedCode = GeneratedForClass(inputCode, "LabelDrivePicker");

        Assert.Contains("WatermarkTextProperty", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("ShowRefreshButton", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("MaxItems", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void PropertyTemplate_IsCombinedWithTheMembersOfTheClassAndWithOtherTemplates()
    {
        const string inputCode = PropertyTemplateUsings +
            """
            [DependencyPropertyTemplate]
            internal abstract class PickerMembers
            {
                [StyledProperty(DefaultValue = "")]
                private string watermarkText;
            }

            [DependencyPropertyTemplate]
            internal abstract class RefreshMembers
            {
                [StyledProperty(DefaultValue = true)]
                private bool showRefreshButton;
            }

            [UseDependencyPropertyTemplate(typeof(PickerMembers))]
            [UseDependencyPropertyTemplate(typeof(RefreshMembers))]
            public partial class DrivePicker : UserControl
            {
                [StyledProperty]
                private int driveCount;
            }
            """;

        AssertCompilesWithAllGeneratorsWithoutGlobalUsings(inputCode);

        var generatedCode = GeneratedForClass(inputCode, "DrivePicker");

        Assert.Contains("WatermarkTextProperty", generatedCode, StringComparison.Ordinal);
        Assert.Contains("ShowRefreshButtonProperty", generatedCode, StringComparison.Ordinal);
        Assert.Contains("DriveCountProperty", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void PropertyTemplate_TheTemplateClassItself_GetsNoGeneratedCode()
    {
        // A partial template would otherwise get registrations of its own, in a class that is
        // not a control.
        const string inputCode = PropertyTemplateUsings +
            """
            [DependencyPropertyTemplate]
            internal abstract partial class PickerMembers
            {
                [StyledProperty(DefaultValue = "")]
                private string watermarkText;
            }

            [UseDependencyPropertyTemplate(typeof(PickerMembers))]
            public partial class DrivePicker : UserControl
            {
            }
            """;

        AssertCompilesWithAllGeneratorsWithoutGlobalUsings(inputCode);

        Assert.DoesNotContain(
            GetSourcesGeneratedForAvaloniaConsumer(inputCode),
            x => x.Contains("partial class PickerMembers", StringComparison.Ordinal));
    }

    [Fact]
    public void PropertyTemplate_ATypeThatIsNotMarkedAsATemplate_ReportsADiagnostic()
    {
        const string inputCode = PropertyTemplateUsings +
            """
            internal abstract class NotATemplate
            {
                [StyledProperty(DefaultValue = "")]
                private string watermarkText;
            }

            [UseDependencyPropertyTemplate(typeof(NotATemplate))]
            public partial class DrivePicker : UserControl
            {
            }
            """;

        Assert.Contains("AtcXamlToolkit0012", GetGeneratorDiagnosticIdsForAvaloniaConsumer(inputCode), StringComparer.Ordinal);
        Assert.DoesNotContain(
            GetSourcesGeneratedForAvaloniaConsumer(inputCode),
            x => x.Contains("WatermarkTextProperty", StringComparison.Ordinal));
    }

    [Fact]
    public void PropertyTemplate_AValidTemplate_ReportsNoDiagnostic()
    {
        const string inputCode = PropertyTemplateUsings +
            """
            [DependencyPropertyTemplate]
            internal abstract class PickerMembers
            {
                [StyledProperty(DefaultValue = "")]
                private string watermarkText;
            }

            [UseDependencyPropertyTemplate(typeof(PickerMembers))]
            public partial class DrivePicker : UserControl
            {
            }
            """;

        Assert.DoesNotContain("AtcXamlToolkit0012", GetGeneratorDiagnosticIdsForAvaloniaConsumer(inputCode), StringComparer.Ordinal);
    }
}