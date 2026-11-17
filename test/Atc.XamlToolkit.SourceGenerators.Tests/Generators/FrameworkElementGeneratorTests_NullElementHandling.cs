namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "OK.")]
public sealed partial class FrameworkElementGeneratorTests
{
    [Fact]
    public void AttachedProperty_NullElementHandling_Default_Wpf_SetIgnoresANullElement()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public static partial class ItemHelper
            {
                [AttachedProperty]
                private static double spacing;
            }
            """;

        var generatedCode = GenerateFrameworkElementCode(inputCode);

        Assert.Contains(
            """
                public static double GetSpacing(DependencyObject element)
                    => (double)element.GetValue(SpacingProperty);
            """.EnsureEnvironmentNewLines(),
            generatedCode,
            StringComparison.Ordinal);
        Assert.Contains(
            """
                public static void SetSpacing(DependencyObject element, double value)
                    => element?.SetValue(SpacingProperty, value);
            """.EnsureEnvironmentNewLines(),
            generatedCode,
            StringComparison.Ordinal);
    }

    [Fact]
    public void AttachedProperty_NullElementHandling_ThrowArgumentNullException_Wpf()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public static partial class ItemHelper
            {
                [AttachedProperty(NullElementHandling = NullElementHandling.ThrowArgumentNullException)]
                private static double spacing;

                [AttachedProperty(NullElementHandling = NullElementHandling.ThrowArgumentNullException)]
                private static bool isActive;
            }
            """;

        var generatedCode = GenerateFrameworkElementCode(inputCode);

        Assert.Contains(
            """
                public static double GetSpacing(DependencyObject element)
                {
                    global::System.ArgumentNullException.ThrowIfNull(element);
                    return (double)element.GetValue(SpacingProperty);
                }
            """.EnsureEnvironmentNewLines(),
            generatedCode,
            StringComparison.Ordinal);
        Assert.Contains(
            """
                public static void SetSpacing(DependencyObject element, double value)
                {
                    global::System.ArgumentNullException.ThrowIfNull(element);
                    element.SetValue(SpacingProperty, value);
                }
            """.EnsureEnvironmentNewLines(),
            generatedCode,
            StringComparison.Ordinal);
        Assert.Contains("element.SetValue(IsActiveProperty, Atc.XamlToolkit.BooleanBoxes.Box(value));", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("element?.", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void AttachedProperty_NullElementHandling_None_Wpf_SetDoesNotCheck()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public static partial class ItemHelper
            {
                [AttachedProperty(NullElementHandling = NullElementHandling.None)]
                private static double spacing;
            }
            """;

        var generatedCode = GenerateFrameworkElementCode(inputCode);

        Assert.Contains(
            """
                public static void SetSpacing(DependencyObject element, double value)
                    => element.SetValue(SpacingProperty, value);
            """.EnsureEnvironmentNewLines(),
            generatedCode,
            StringComparison.Ordinal);
        Assert.Contains("=> (double)element.GetValue(SpacingProperty);", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("ThrowIfNull", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("element?.", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void AttachedProperty_NullElementHandling_ThrowArgumentNullException_Wpf_ReadOnly_WritesThroughTheKey()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public static partial class ItemHelper
            {
                [AttachedProperty(IsReadOnly = true, NullElementHandling = NullElementHandling.ThrowArgumentNullException)]
                private static double spacing;
            }
            """;

        var generatedCode = GenerateFrameworkElementCode(inputCode);

        Assert.Contains(
            """
                private static void SetSpacing(DependencyObject element, double value)
                {
                    global::System.ArgumentNullException.ThrowIfNull(element);
                    element.SetValue(SpacingPropertyKey, value);
                }
            """.EnsureEnvironmentNewLines(),
            generatedCode,
            StringComparison.Ordinal);
    }
}