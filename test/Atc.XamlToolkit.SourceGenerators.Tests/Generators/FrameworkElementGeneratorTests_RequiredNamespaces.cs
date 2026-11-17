namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "OK.")]
public sealed partial class FrameworkElementGeneratorTests
{
    [Fact]
    public void StyledProperty_OfAWellKnownFrameworkType_ImportsItsNamespace()
    {
        // IEnumerable, DateTime, IDisposable and the like are "special types" to the compiler
        // but have no C# keyword, so the generated file needs their namespace like any other.
        const string inputCode =
            """
            using Atc.XamlToolkit.Controls.Attributes;
            using Avalonia.Controls;

            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [StyledProperty]
                private System.Collections.IEnumerable? items;

                [StyledProperty]
                private System.Collections.Generic.IList<int>? numbers;

                [StyledProperty]
                private System.DateTime created;

                [StyledProperty]
                private System.IDisposable? subscription;

                [StyledProperty]
                private int? count;

                [StyledProperty]
                private string? title;
            }
            """;

        AssertCompilesWithAllGeneratorsWithoutGlobalUsings(inputCode);

        var generatedCode = string.Join(
            Environment.NewLine,
            GetSourcesGeneratedForAvaloniaConsumer(inputCode));

        Assert.Contains("using System;", generatedCode, StringComparison.Ordinal);
        Assert.Contains("using System.Collections;", generatedCode, StringComparison.Ordinal);
        Assert.Contains("using System.Collections.Generic;", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void StyledProperty_OfKeywordTypesOnly_ImportsNoSystemNamespace()
    {
        const string inputCode =
            """
            using Atc.XamlToolkit.Controls.Attributes;
            using Avalonia.Controls;

            namespace TestNamespace;

            public partial class MyControl : UserControl
            {
                [StyledProperty]
                private int? count;

                [StyledProperty]
                private string? title;

                [StyledProperty]
                private object? tagValue;

                [StyledProperty]
                private double[]? values;
            }
            """;

        AssertCompilesWithAllGeneratorsWithoutGlobalUsings(inputCode);

        var generatedCode = string.Join(
            Environment.NewLine,
            GetSourcesGeneratedForAvaloniaConsumer(inputCode));

        Assert.DoesNotContain("using System", generatedCode, StringComparison.Ordinal);
    }
}