namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

/// <summary>
/// Defects in the framework-element generator found in review.
/// </summary>
public sealed class GeneratorTests_FrameworkElementReviewFindings : GeneratorTestBase
{
    private const string IgnoredOptionDiagnosticId = "AtcXamlToolkit0013";

    private static (string GeneratedCode, IReadOnlyList<Diagnostic> Diagnostics) Generate(
        string inputCode)
    {
        var result = RunGenerator<FrameworkElementGenerator>(inputCode);

        var generatedCode = string.Join(
                Environment.NewLine,
                result.GeneratorResult.GeneratedSources.Select(x => x.SourceText.ToString()))
            .EnsureEnvironmentNewLines();

        return (generatedCode, result.GeneratorResult.Diagnostics);
    }

    [Theory]
    [InlineData("[DependencyProperty(GeneratePartialHooks = true)] private MyMode mode;", "defaultValue: default(MyMode)")]
    [InlineData("[DependencyProperty(GenerateCoerceHook = true)] private MyMode mode;", "defaultValue: default(MyMode)")]
    [InlineData("[DependencyProperty(PropertyChangedCallback = nameof(OnChanged))] private MyPoint mode;", "defaultValue: default(MyPoint)")]
    [InlineData("[DependencyProperty(Flags = FrameworkPropertyMetadataOptions.AffectsRender)] private MyPoint mode;", "defaultValue: default(MyPoint)")]
    public void DependencyProperty_ValueTypeWithoutADefaultValue_Wpf_GetsTheDefaultOfTheType(
        string member,
        string expected)
    {
        // 'defaultValue: null' for a value type throws when the property is registered.
        var inputCode =
            $$"""
            namespace TestNamespace;

            public enum MyMode { One, Two }

            public struct MyPoint { public int X; }

            public partial class MyControl : UserControl
            {
                {{member}}
            }
            """;

        var (generatedCode, _) = Generate(inputCode);

        Assert.Contains(expected, generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("defaultValue: null", generatedCode, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("private MyMode? mode;")]
    [InlineData("private MyClass mode;")]
    public void DependencyProperty_NullableOrReferenceTypeWithoutADefaultValue_Wpf_KeepsNull(
        string field)
    {
        var inputCode =
            $$"""
            namespace TestNamespace;

            public enum MyMode { One, Two }

            public class MyClass { }

            public partial class MyControl : UserControl
            {
                [DependencyProperty(GeneratePartialHooks = true)]
                {{field}}
            }
            """;

        var (generatedCode, _) = Generate(inputCode);

        Assert.Contains("defaultValue: null", generatedCode, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("public static partial class XAttach", "[AttachedProperty(AddOwnerOf = typeof(Grid), GeneratePartialHooks = true)] private static int row;")]
    [InlineData("public static partial class XAttach", "[AttachedProperty(AddOwnerOf = typeof(DockPanel), DefaultValue = 1)] private static int row;")]
    [InlineData("public partial class XHelper", "[AttachedProperty(AddOwnerOf = typeof(Grid), PropertyChangedCallback = nameof(OnRowChanged))] private static int row;")]
    public void AddOwnerOf_WithAnOverride_InAClassThatIsNotADependencyObject_Wpf_DropsTheOverrideWithADiagnostic(
        string classDeclaration,
        string member)
    {
        // Metadata can only be overridden for an owner that derives from DependencyObject;
        // WPF throws for any other owner when the class is first used.
        var inputCode =
            $$"""
            namespace TestNamespace;

            {{classDeclaration}}
            {
                {{member}}
            }
            """;

        var (generatedCode, diagnostics) = Generate(inputCode);

        Assert.Equal([IgnoredOptionDiagnosticId], diagnostics.Select(x => x.Id), StringComparer.Ordinal);
        Assert.Contains("RowProperty.AddOwner(", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("Metadata", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("OnRowChanged", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void AddOwnerOf_WithoutAnOverride_InAStaticClass_Wpf_ReportsNoDiagnostic()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public static partial class XAttach
            {
                [AttachedProperty(AddOwnerOf = typeof(Grid))]
                private static int row;
            }
            """;

        var (_, diagnostics) = Generate(inputCode);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void GenerateCoerceHook_WithACoerceValueCallback_Wpf_PassesARejectedValueThrough()
    {
        // A coerce callback rejects a value by returning DependencyProperty.UnsetValue,
        // which cannot be cast to the property type.
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [DependencyProperty(GenerateCoerceHook = true, CoerceValueCallback = nameof(Reject))]
                private double amount;
            }
            """;

        var (generatedCode, _) = Generate(inputCode);

        Assert.Contains(
            """
                private static object? CoerceAmountPropertyCallback(
                    DependencyObject d,
                    object baseValue)
                {
                    var coerced = Reject(d, baseValue);
                    return ReferenceEquals(coerced, DependencyProperty.UnsetValue)
                        ? coerced
                        : ((MyControl)d).CoerceAmount((double)coerced!);
                }
            """.EnsureEnvironmentNewLines(),
            generatedCode,
            StringComparison.Ordinal);
    }

    [Fact]
    public void DependencyProperty_IsReadOnly_InAStaticClass_Wpf_IsIgnoredWithADiagnostic()
    {
        // A static class is not a DependencyObject, so a read-only (non-attached) property
        // cannot be registered for it. [AttachedProperty] is the attribute for a static class.
        const string inputCode =
            """
            namespace TestNamespace;

            public static partial class XHelper
            {
                [DependencyProperty(IsReadOnly = true)]
                private static int count;
            }
            """;

        var (generatedCode, diagnostics) = Generate(inputCode);

        Assert.Equal([IgnoredOptionDiagnosticId], diagnostics.Select(x => x.Id), StringComparer.Ordinal);
        Assert.DoesNotContain("PropertyKey", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void DependencyProperty_Hooks_InAStaticClass_Wpf_AreStaticAndTakeTheTarget()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public static partial class XHelper
            {
                [DependencyProperty(GeneratePartialHooks = true, GenerateCoerceHook = true)]
                private static int count;
            }
            """;

        var (generatedCode, _) = Generate(inputCode);

        Assert.Contains("static partial void OnCountChanged(DependencyObject d, int oldValue, int newValue);", generatedCode, StringComparison.Ordinal);
        Assert.Contains("private static partial int CoerceCount(DependencyObject d, int value);", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("((XHelper)d)", generatedCode, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("[AttachedProperty(ElementParameterName = \"class\")] private static int count;", "GetCount(DependencyObject element)")]
    [InlineData("[AttachedProperty(ElementParameterName = \"value\")] private static int count;", "GetCount(DependencyObject element)")]
    [InlineData("[AttachedProperty(ElementParameterName = \"not valid\")] private static int count;", "GetCount(DependencyObject element)")]
    [InlineData("[AttachedProperty(PropertyName = \"class\")] private static int count;", "GetCount(DependencyObject element)")]
    [InlineData("[AttachedProperty(PropertyName = \"1x\")] private static int count;", "GetCount(DependencyObject element)")]
    [InlineData("[AttachedProperty(BrowsableForTargetType = true)] private static int count;", "GetCount(DependencyObject element)")]
    public void AttachedProperty_OptionWithAValueThatCannotBeUsed_Wpf_IsIgnoredWithADiagnostic(
        string member,
        string expected)
    {
        var inputCode =
            $$"""
            namespace TestNamespace;

            public static partial class XHelper
            {
                {{member}}
            }
            """;

        var (generatedCode, diagnostics) = Generate(inputCode);

        Assert.Equal([IgnoredOptionDiagnosticId], diagnostics.Select(x => x.Id), StringComparer.Ordinal);
        Assert.Contains(expected, generatedCode, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("[DependencyProperty(PropertyKeyAccessModifier = PropertyAccessModifier.Internal)] private int count;")]
    [InlineData("[DependencyProperty(AddOwnerOf = typeof(RangeBase), PropertyKeyAccessModifier = PropertyAccessModifier.Internal)] private double minimum;")]
    [InlineData("[RoutedEvent(RaiseMethodAccessModifier = PropertyAccessModifier.Internal)] private static readonly RoutedEvent tap;")]
    [InlineData("[RoutedEvent(GenerateHandlerMethods = false)] private static readonly RoutedEvent tap;")]
    public void OptionThatNeedsAnotherOption_Wpf_IsReportedWhenItHasNoEffect(
        string member)
    {
        var inputCode =
            $$"""
            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                {{member}}
            }
            """;

        var (_, diagnostics) = Generate(inputCode);

        Assert.Equal([IgnoredOptionDiagnosticId], diagnostics.Select(x => x.Id), StringComparer.Ordinal);
    }

    [Fact]
    public void ProtectedKeyAndRaiseMethod_InASealedClass_Wpf_AreNarrowedToWhatTheyMeanThere()
    {
        // 'protected' in a sealed class is reported by the compiler (CS0628).
        const string inputCode =
            """
            namespace TestNamespace;

            public sealed partial class MyControl : UserControl
            {
                [DependencyProperty(IsReadOnly = true, PropertyKeyAccessModifier = PropertyAccessModifier.Protected)]
                private int count;

                [RoutedEvent(GenerateRaiseMethod = true, RaiseMethodAccessModifier = PropertyAccessModifier.ProtectedInternal)]
                private static readonly RoutedEvent tap;
            }
            """;

        var (generatedCode, diagnostics) = Generate(inputCode);

        Assert.Empty(diagnostics);
        Assert.Contains("private static readonly DependencyPropertyKey CountPropertyKey", generatedCode, StringComparison.Ordinal);
        Assert.Contains("    internal void RaiseTap()", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("protected", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void GenerateCoerceHook_WithACoerceValueCallbackThatMayReturnNull_Wpf_DeclaresANullableResult()
    {
        // The named callback can be declared as returning object?; passing its result on from
        // a method that returns object would be a nullable warning in generated code.
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [DependencyProperty(GenerateCoerceHook = true, CoerceValueCallback = nameof(Reject))]
                private Brush fill;
            }
            """;

        var (generatedCode, _) = Generate(inputCode);

        Assert.Contains("private static object? CoerceFillPropertyCallback(", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void IgnoredOptionDiagnostic_PointsAtTheAttribute()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public static partial class XHelper
            {
                [AttachedProperty(BrowsableForTargetType = true)]
                private static int count;
            }
            """;

        var (_, diagnostics) = Generate(inputCode);

        var location = Assert.Single(diagnostics).Location.GetLineSpan();
        Assert.True(location.IsValid);
        Assert.Equal(4, location.StartLinePosition.Line);
    }

    [Fact]
    public void RoutedEvent_InAGenericControl_Wpf_NamesTheOwnerWithItsTypeParameters()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class GenericEvents<T> : UserControl
            {
                [RoutedEvent]
                private static readonly RoutedEvent tap;
            }
            """;

        var (generatedCode, _) = Generate(inputCode);

        Assert.Contains("ownerType: typeof(GenericEvents<T>));", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void TemplatePartField_WhoseTypeUsesATypeParameterOfTheClass_Wpf_DeclaresThePartWithoutAType()
    {
        // 'typeof(List<T>)' is not allowed as an attribute argument.
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class Ctl<T> : Control
            {
                [TemplatePartField("PART_Items")]
                private System.Collections.Generic.List<T>? items;

                [TemplatePartField("PART_Root")]
                private Border? root;
            }
            """;

        var (generatedCode, _) = Generate(inputCode);

        Assert.Contains("[global::System.Windows.TemplatePart(Name = \"PART_Items\")]", generatedCode, StringComparison.Ordinal);
        Assert.Contains("[global::System.Windows.TemplatePart(Name = \"PART_Root\", Type = typeof(Border))]", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void DependencyProperty_DefaultValueExpressionNull_WinUI_PassesNullToTheMetadata()
    {
        // The WinUI PropertyMetadata has no constructor without arguments.
        const string inputCode =
            """
            namespace Microsoft.UI.Xaml
            {
                public class DependencyObject
                {
                }
            }

            namespace TestNamespace
            {
                public partial class MyControl : Microsoft.UI.Xaml.DependencyObject
                {
                    [DependencyProperty(DefaultValueExpression = "null")]
                    private string title;
                }
            }
            """;

        var (generatedCode, _) = Generate(inputCode);

        Assert.DoesNotContain("new PropertyMetadata()", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void NullArguments_WithResolvedAttributes_Avalonia_DoNotMakeTheGeneratorThrow()
    {
        const string inputCode =
            """
            using Atc.XamlToolkit.Controls.Attributes;
            using Avalonia.Controls;

            namespace TestNamespace;

            [DependencyPropertyTemplate]
            internal abstract class Members
            {
                [StyledProperty]
                private int count;
            }

            [UseDependencyPropertyTemplate(typeof(Members), Exclude = null, DocumentationValues = null)]
            public partial class MyControl : UserControl
            {
                [StyledProperty(PropertyChangedCallback = null, DefaultValueExpression = null, PropertyName = null)]
                private int step;
            }
            """;

        AssertCompilesWithAllGeneratorsWithoutGlobalUsings(inputCode);
    }
}