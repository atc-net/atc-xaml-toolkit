namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

/// <summary>
/// Defects found by reviewing the documentation samples against the generator.
/// </summary>
public sealed class GeneratorTests_ReviewFindings : GeneratorTestBase
{
    private const string IgnoredOptionDiagnosticId = "AtcXamlToolkit0013";
    private const string MissingPartialDiagnosticId = "AtcXamlToolkit0002";

    private static (string GeneratedCode, IReadOnlyList<string> DiagnosticIds) Generate(
        string inputCode)
    {
        var result = RunGenerator<FrameworkElementGenerator>(inputCode);

        var generatedCode = string.Join(
                Environment.NewLine,
                result.GeneratorResult.GeneratedSources.Select(x => x.SourceText.ToString()))
            .EnsureEnvironmentNewLines();

        return (generatedCode, result.GeneratorResult.Diagnostics.Select(x => x.Id).ToList());
    }

    [Fact]
    public void AttachedProperty_DefaultUpdateSourceTrigger_Wpf_ImportsItsNamespace()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public static partial class DragBehavior
            {
                [AttachedProperty(DefaultValue = false, DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged)]
                private static bool isDraggable;
            }
            """;

        var (generatedCode, _) = Generate(inputCode);

        Assert.Contains("using System.Windows.Data;", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void StyledProperty_OfANullableReferenceType_Avalonia_CompilesWithoutWarnings()
    {
        // StyledProperty<string> does not take the string? value the generated property has.
        const string inputCode =
            """
            using Atc.XamlToolkit.Controls.Attributes;
            using Avalonia;
            using Avalonia.Controls;

            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [StyledProperty]
                private string? title;

                [StyledProperty]
                private string name = string.Empty;

                [StyledProperty]
                private int? count;
            }

            public partial class MyBehavior : AvaloniaObject
            {
                [AttachedProperty]
                private static string? watermark;
            }
            """;

        AssertCompilesWithAllGeneratorsWithoutGlobalUsings(inputCode);

        var generatedCode = string.Join(Environment.NewLine, GetSourcesGeneratedForAvaloniaConsumer(inputCode));

        Assert.Contains("StyledProperty<string?> TitleProperty", generatedCode, StringComparison.Ordinal);
        Assert.Contains("StyledProperty<string> NameProperty", generatedCode, StringComparison.Ordinal);
        Assert.Contains("StyledProperty<int?> CountProperty", generatedCode, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Protected", "Internal")]
    [InlineData("Internal", "Protected")]
    [InlineData("Internal", "Public")]
    [InlineData("Private", "Internal")]
    [InlineData("PrivateProtected", "Protected")]
    public void DependencyProperty_SetterThatIsNotMoreRestrictiveThanTheProperty_Wpf_IsIgnoredWithADiagnostic(
        string accessModifier,
        string setterAccessModifier)
    {
        // C# rejects an accessor modifier that is not more restrictive than the property (CS0273).
        var inputCode =
            $$"""
            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [DependencyProperty(AccessModifier = PropertyAccessModifier.{{accessModifier}}, SetterAccessModifier = PropertyAccessModifier.{{setterAccessModifier}})]
                private double zoom;
            }
            """;

        var (generatedCode, diagnosticIds) = Generate(inputCode);

        Assert.Equal([IgnoredOptionDiagnosticId], diagnosticIds);
        Assert.Contains("        set => SetValue(ZoomProperty, value);", generatedCode, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Internal", "Private", "private set")]
    [InlineData("Protected", "PrivateProtected", "private protected set")]
    [InlineData("ProtectedInternal", "Internal", "internal set")]
    [InlineData("Public", "Internal", "internal set")]
    [InlineData("Internal", "Internal", "set")]
    public void DependencyProperty_SetterThatIsMoreRestrictiveOrTheSame_Wpf_ReportsNoDiagnostic(
        string accessModifier,
        string setterAccessModifier,
        string expectedSetter)
    {
        var inputCode =
            $$"""
            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [DependencyProperty(AccessModifier = PropertyAccessModifier.{{accessModifier}}, SetterAccessModifier = PropertyAccessModifier.{{setterAccessModifier}})]
                private double zoom;
            }
            """;

        var (generatedCode, diagnosticIds) = Generate(inputCode);

        Assert.Empty(diagnosticIds);
        Assert.Contains($"        {expectedSetter} => SetValue(ZoomProperty, value);", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void AttachedProperty_SetMethodLessRestrictiveThanGetMethod_Wpf_IsAllowed()
    {
        // The Get and Set methods of an attached property are separate methods.
        const string inputCode =
            """
            namespace TestNamespace;

            public static partial class ZoomHelper
            {
                [AttachedProperty(AccessModifier = PropertyAccessModifier.Private, SetterAccessModifier = PropertyAccessModifier.Internal)]
                private static double zoom;
            }
            """;

        var (generatedCode, diagnosticIds) = Generate(inputCode);

        Assert.Empty(diagnosticIds);
        Assert.Contains("private static double GetZoom(", generatedCode, StringComparison.Ordinal);
        Assert.Contains("internal static void SetZoom(", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void RoutedEvent_UnboundGenericHandlerThatCannotBeClosed_Wpf_FallsBackWithADiagnostic()
    {
        // 'event RoutedPropertyChangedEventHandler<> Changed' does not compile.
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class CustomButton : Button
            {
                [RoutedEvent(HandlerType = typeof(RoutedPropertyChangedEventHandler<>))]
                private static readonly RoutedEvent changed;
            }
            """;

        var (generatedCode, diagnosticIds) = Generate(inputCode);

        Assert.Equal([IgnoredOptionDiagnosticId], diagnosticIds);
        Assert.DoesNotContain("<>", generatedCode, StringComparison.Ordinal);
        Assert.Contains("public event RoutedEventHandler Changed", generatedCode, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("[DependencyProperty] private int count;")]
    [InlineData("[AttachedProperty] private static int count;")]
    [InlineData("[RoutedEvent] private static readonly RoutedEvent tap;")]
    [InlineData("[TemplatePartField(\"PART_Root\")] private Border root;")]
    public void ClassThatIsNotPartial_WithAFrameworkElementAttributeOnAMember_ReportsADiagnostic(
        string member)
    {
        var inputCode =
            $$"""
            namespace TestNamespace;

            public class MyControl : UserControl
            {
                {{member}}
            }
            """;

        var (generatedCode, diagnosticIds) = Generate(inputCode);

        Assert.Equal([MissingPartialDiagnosticId], diagnosticIds);
        Assert.Empty(generatedCode);
    }

    [Theory]
    [InlineData("[DefaultStyleKey]")]
    [InlineData("[DependencyProperty<int>(\"Count\")]")]
    [InlineData("[UseDependencyPropertyTemplate(typeof(Members))]")]
    public void ClassThatIsNotPartial_WithAFrameworkElementAttributeOnTheClass_ReportsADiagnostic(
        string attribute)
    {
        var inputCode =
            $$"""
            namespace TestNamespace;

            {{attribute}}
            public class MyControl : UserControl
            {
            }
            """;

        var (_, diagnosticIds) = Generate(inputCode);

        Assert.Equal([MissingPartialDiagnosticId], diagnosticIds);
    }

    [Fact]
    public void PropertyTemplate_ThatIsNotPartial_ReportsNoDiagnostic()
    {
        // A template only declares members for other classes and gets no code of its own.
        const string inputCode =
            """
            namespace TestNamespace;

            [DependencyPropertyTemplate]
            internal abstract class Members
            {
                [DependencyProperty] private int count;
            }
            """;

        var (_, diagnosticIds) = Generate(inputCode);

        Assert.Empty(diagnosticIds);
    }

    [Fact]
    public void PartialClass_WithAFrameworkElementAttribute_ReportsNoMissingPartialDiagnostic()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [DependencyProperty] private int count;
            }
            """;

        var (_, diagnosticIds) = Generate(inputCode);

        Assert.Empty(diagnosticIds);
    }
}