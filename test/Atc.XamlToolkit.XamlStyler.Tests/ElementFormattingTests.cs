namespace Atc.XamlToolkit.XamlStyler.Tests;

public sealed class ElementFormattingTests
{
    private static string WrapXaml(string inner) =>
        $"<Root xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">{inner}</Root>";

    private static string StyleWith(
        string xaml,
        Action<XamlStylerOptions>? configure = null)
    {
        var options = new XamlStylerOptions();
        configure?.Invoke(options);
        var service = new XamlStylerService(options);
        return service.StyleDocument(xaml);
    }

    [Fact]
    public void EmptyElement_SelfClosing_WhenOptionEnabled()
    {
        var xaml = WrapXaml("\n    <Button></Button>\n");
        var result = StyleWith(xaml, o => o.RemoveEndingTagOfEmptyElement = true);
        result.Should().Contain("<Button />");
        result.Should().NotContain("</Button>");
    }

    [Fact]
    public void EmptyElement_KeepsEndTag_WhenOptionDisabled()
    {
        var xaml = WrapXaml("\n    <Button></Button>\n");
        var result = StyleWith(xaml, o => o.RemoveEndingTagOfEmptyElement = false);
        result.Should().Contain("</Button>");
    }

    [Fact]
    public void PutEndingBracketOnNewLine_MovesClosingBracket()
    {
        // Element with many attributes so it becomes multiline
        var xaml = WrapXaml("\n    <Button Name=\"A\" Width=\"100\" Height=\"50\" Margin=\"5\" />\n");
        var result = StyleWith(xaml, o =>
        {
            o.AttributesTolerance = 1;
            o.PutEndingBracketOnNewLine = true;
        });

        // The closing /> or > should be on its own line
        // Check that /> appears after a newline + indentation
        var lines = result.Split('\n').Select(l => l.TrimEnd('\r')).ToArray();
        var closingLine = lines.FirstOrDefault(l => l.Trim() == "/>");
        closingLine.Should().NotBeNull("the closing bracket should be on its own line");
    }

    [Fact]
    public void RootElementLineBreakRule_Always_BreaksAttributes()
    {
        var xaml = "<Root xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" Title=\"Test\" />";
        var result = StyleWith(xaml, o =>
        {
            o.RootElementLineBreakRule = LineBreakRule.Always;
            o.AttributesTolerance = 10; // high tolerance, but Always overrides
        });

        // With Always rule, attributes should be on separate lines even within tolerance
        result.Should().Contain(Environment.NewLine);
        var lines = result.Split('\n').Select(l => l.TrimEnd('\r')).Where(l => l.Trim().Length > 0).ToArray();
        lines.Length.Should().BeGreaterThan(1);
    }

    [Fact]
    public void RootElementLineBreakRule_Never_KeepsOneLine()
    {
        var xaml = "<Root xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" Title=\"Test\" Width=\"100\" Height=\"50\" />";
        var result = StyleWith(xaml, o =>
        {
            o.RootElementLineBreakRule = LineBreakRule.Never;
            o.AttributesTolerance = 1; // low tolerance, but Never overrides
        });

        // All attributes should stay on one line
        var lines = result.Split('\n').Select(l => l.TrimEnd('\r')).Where(l => l.Trim().Length > 0).ToArray();
        lines.Length.Should().Be(1);
    }

    [Fact]
    public void TextContent_WithGreaterThan_IsPreserved()
    {
        var xaml = WrapXaml("\n    <TextBlock>File > Open</TextBlock>\n");

        var result = StyleWith(xaml);

        result.Should().Contain("<TextBlock>File > Open</TextBlock>");
        StyleWith(result).Should().Be(result, "formatting should be idempotent");
    }

    [Fact]
    public void InlineContent_KeepsSpaceBetweenTextAndInlineElement()
    {
        var xaml = WrapXaml("\n    <TextBlock>Hello <Run Text=\"x\" /> world</TextBlock>\n");

        var result = StyleWith(xaml);

        var expected = string.Join(
            Environment.NewLine,
            "<Root xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">",
            "    <TextBlock>",
            "        Hello <Run Text=\"x\" />",
            "        world</TextBlock>",
            "</Root>");
        result.Should().Be(expected);
        StyleWith(result).Should().Be(result, "formatting should be idempotent");
    }

    [Fact]
    public void InlineContent_DoesNotAddSpaceBetweenInlineElementAndText()
    {
        var xaml = WrapXaml("\n    <TextBlock>Hello<Run Text=\"x\" />world <Bold>big</Bold>.</TextBlock>\n");

        var result = StyleWith(xaml);

        var expected = string.Join(
            Environment.NewLine,
            "<Root xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">",
            "    <TextBlock>",
            "        Hello<Run Text=\"x\" />world <Bold>big</Bold>.</TextBlock>",
            "</Root>");
        result.Should().Be(expected);
        StyleWith(result).Should().Be(result, "formatting should be idempotent");
    }

    [Fact]
    public void InlineContent_OnSeparateLines_KeepsWhitespaceBetweenTextAndInlineElement()
    {
        var xaml = WrapXaml("\n    <TextBlock>\n        Hello\n        <Run Text=\"x\" />\n        world\n    </TextBlock>\n");

        var result = StyleWith(xaml);

        var expected = string.Join(
            Environment.NewLine,
            "<Root xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">",
            "    <TextBlock>",
            "        Hello <Run Text=\"x\" />",
            "        world</TextBlock>",
            "</Root>");
        result.Should().Be(expected);
        StyleWith(result).Should().Be(result, "formatting should be idempotent");
    }
}