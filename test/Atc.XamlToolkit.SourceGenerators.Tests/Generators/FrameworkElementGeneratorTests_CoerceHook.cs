namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "OK.")]
public sealed partial class FrameworkElementGeneratorTests
{
    [Fact]
    public void DependencyProperty_GenerateCoerceHook_Wpf_GeneratesCallbackAndTypedHook()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [DependencyProperty(GenerateCoerceHook = true)]
                private double maximum;

                private partial double CoerceMaximum(double value)
                    => value < 0 ? 0 : value;
            }
            """;

        var generatedCode = Assert.Single(RunGenerator<FrameworkElementGenerator>(inputCode).GeneratorResult.GeneratedSources).SourceText.ToString();

        Assert.Contains("coerceValueCallback: CoerceMaximumPropertyCallback", generatedCode, StringComparison.Ordinal);
        Assert.Contains(
            """
                private static object CoerceMaximumPropertyCallback(
                    DependencyObject d,
                    object baseValue)
                    => ((MyControl)d).CoerceMaximum((double)baseValue);

                private partial double CoerceMaximum(double value);
            """.EnsureEnvironmentNewLines(),
            generatedCode.EnsureEnvironmentNewLines(),
            StringComparison.Ordinal);
    }

    [Fact]
    public void DependencyProperty_GenerateCoerceHook_Wpf_WithOwnCallback_RunsTheCallbackFirst()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [DependencyProperty(GenerateCoerceHook = true, CoerceValueCallback = nameof(CoerceAny))]
                private double maximum;
            }
            """;

        var generatedCode = Assert.Single(RunGenerator<FrameworkElementGenerator>(inputCode).GeneratorResult.GeneratedSources).SourceText.ToString();

        Assert.Contains("coerceValueCallback: CoerceMaximumPropertyCallback", generatedCode, StringComparison.Ordinal);
        Assert.Contains("=> ((MyControl)d).CoerceMaximum((double)CoerceAny(d, baseValue));", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void DependencyProperty_GenerateCoerceHook_Wpf_NullableReferenceType_PassesNullThrough()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [DependencyProperty(GenerateCoerceHook = true)]
                private string? title;
            }
            """;

        var generatedCode = Assert.Single(RunGenerator<FrameworkElementGenerator>(inputCode).GeneratorResult.GeneratedSources).SourceText.ToString();

        Assert.Contains("private static object? CoerceTitlePropertyCallback(", generatedCode, StringComparison.Ordinal);
        Assert.Contains("object? baseValue)", generatedCode, StringComparison.Ordinal);
        Assert.Contains("=> ((MyControl)d).CoerceTitle((string?)baseValue);", generatedCode, StringComparison.Ordinal);
        Assert.Contains("private partial string? CoerceTitle(string? value);", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void AttachedProperty_GenerateCoerceHook_Wpf_GeneratesStaticHookWithTheTargetObject()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public static partial class ZoomBehavior
            {
                [AttachedProperty(GenerateCoerceHook = true)]
                private static double zoomFactor;
            }
            """;

        var generatedCode = Assert.Single(RunGenerator<FrameworkElementGenerator>(inputCode).GeneratorResult.GeneratedSources).SourceText.ToString();

        Assert.Contains("coerceValueCallback: CoerceZoomFactorPropertyCallback", generatedCode, StringComparison.Ordinal);
        Assert.Contains("=> CoerceZoomFactor(d, (double)baseValue);", generatedCode, StringComparison.Ordinal);
        Assert.Contains("private static partial double CoerceZoomFactor(DependencyObject d, double value);", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void DependencyProperty_GeneratePartialHooksAlone_GeneratesNoCoerceHook()
    {
        // A coerce hook returns a value, so it has to be implemented. It is therefore not
        // part of GeneratePartialHooks, whose hooks may be left unimplemented.
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [DependencyProperty(GeneratePartialHooks = true)]
                private double maximum;
            }
            """;

        var generatedCode = Assert.Single(RunGenerator<FrameworkElementGenerator>(inputCode).GeneratorResult.GeneratedSources).SourceText.ToString();

        Assert.DoesNotContain("coerceValueCallback", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("CoerceMaximum", generatedCode, StringComparison.Ordinal);
    }
}