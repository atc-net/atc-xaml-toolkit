// ReSharper disable StringLiteralTypo
namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "OK.")]
public sealed partial class FrameworkElementGeneratorTests
{
    [Fact]
    public void RoutedEvent_OnAvalonia_EmitsDiagnostic()
    {
        // The stub Avalonia.AvaloniaProperty type makes GetXamlPlatform() return Avalonia
        // for this in-process test compilation — Roslyn's GetTypeByMetadataName resolves
        // types declared in the compilation as well as referenced assemblies.
        const string inputCode =
            """
            namespace Avalonia
            {
                public class AvaloniaProperty { }
            }

            namespace TestNamespace
            {
                public partial class CustomButton
                {
                    [RoutedEvent(RoutingStrategy.Bubble)]
                    private static readonly RoutedEvent tap;
                }
            }
            """;

        var generatorResult = RunGenerator<FrameworkElementGenerator>(inputCode);

        AssertGeneratorRunResultHasDiagnostics(["AtcXamlToolkit0010"], generatorResult);
    }

    [Fact]
    public void RoutedEvent_OnWinUI_EmitsDiagnostic()
    {
        const string inputCode =
            """
            namespace Microsoft.UI.Xaml
            {
                public class DependencyProperty { }
            }

            namespace TestNamespace
            {
                public partial class CustomButton
                {
                    [RoutedEvent(RoutingStrategy.Bubble)]
                    private static readonly RoutedEvent tap;
                }
            }
            """;

        var generatorResult = RunGenerator<FrameworkElementGenerator>(inputCode);

        AssertGeneratorRunResultHasDiagnostics(["AtcXamlToolkit0010"], generatorResult);
    }

    [Fact]
    public void RoutedEvent_OnWpf_NoDiagnostic()
    {
        // Default test compilation has Atc.XamlToolkit.Wpf loaded, so GetXamlPlatform()
        // returns WPF and the diagnostic is suppressed.
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class CustomButton : Button
            {
                [RoutedEvent(RoutingStrategy.Bubble)]
                private static readonly RoutedEvent tap;
            }
            """;

        var generatorResult = RunGenerator<FrameworkElementGenerator>(inputCode);

        AssertGeneratorRunResultHasDiagnostics([], generatorResult);
    }

    [Fact]
    public void RoutedEvent_OnAvalonia_MultipleFields_EmitsDiagnosticPerField()
    {
        const string inputCode =
            """
            namespace Avalonia
            {
                public class AvaloniaProperty { }
            }

            namespace TestNamespace
            {
                public partial class CustomButton
                {
                    [RoutedEvent(RoutingStrategy.Bubble)]
                    private static readonly RoutedEvent tap;

                    [RoutedEvent(RoutingStrategy.Tunnel)]
                    private static readonly RoutedEvent drag;
                }
            }
            """;

        var generatorResult = RunGenerator<FrameworkElementGenerator>(inputCode);

        AssertGeneratorRunResultHasDiagnostics(
            ["AtcXamlToolkit0010", "AtcXamlToolkit0010"],
            generatorResult);
    }
}