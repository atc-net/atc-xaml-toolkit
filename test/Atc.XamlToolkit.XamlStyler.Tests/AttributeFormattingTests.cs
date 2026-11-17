namespace Atc.XamlToolkit.XamlStyler.Tests;

public sealed class AttributeFormattingTests
{
    private const string WpfNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

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
    public void SingleAttribute_StaysOnSameLine()
    {
        // Arrange
        var xaml = $"""<Root xmlns="{WpfNamespace}"><Button Content="OK" /></Root>""";
        var options = new XamlStylerOptions { AttributesTolerance = 2 };
        var service = new XamlStylerService(options);

        // Act
        var result = service.StyleDocument(xaml);

        // Assert - With tolerance=2, a single attribute should stay on the same line
        result.Should().Contain("<Button Content=\"OK\"");
    }

    [Fact]
    public void AttributesExceedTolerance_BreakToMultipleLines()
    {
        // Arrange
        var xaml = $"""<Root xmlns="{WpfNamespace}"><Button Content="OK" Width="100" Height="50" Margin="10" /></Root>""";
        var options = new XamlStylerOptions { AttributesTolerance = 1 };
        var service = new XamlStylerService(options);

        // Act
        var result = service.StyleDocument(xaml);

        // Assert - Attributes exceeding tolerance should break to multiple lines
        var lines = result.Split('\n');
        var buttonLines = lines.Where(l =>
            l.Contains("Content", StringComparison.Ordinal) ||
            l.Contains("Width", StringComparison.Ordinal) ||
            l.Contains("Height", StringComparison.Ordinal) ||
            l.Contains("Margin", StringComparison.Ordinal)).ToArray();
        buttonLines.Length.Should().BeGreaterThan(1);
    }

    [Fact]
    public void KeepFirstAttributeOnSameLine_KeepsFirstAttribute()
    {
        // Arrange
        var xaml = $"""<Root xmlns="{WpfNamespace}"><Button Content="OK" Width="100" Height="50" /></Root>""";
        var options = new XamlStylerOptions
        {
            AttributesTolerance = 1,
            KeepFirstAttributeOnSameLine = true,
        };
        var service = new XamlStylerService(options);

        // Act
        var result = service.StyleDocument(xaml);

        // Assert - First attribute (after reordering) should be on the same line as <Button
        // Attributes get reordered: Width, Height come before Content per default rules
        result.Should().Contain("<Button Width=\"100\"");
    }

    [Fact]
    public void SpaceBeforeClosingSlash_AddsSpace()
    {
        // Arrange
        var xaml = $"""<Root xmlns="{WpfNamespace}"><Button Content="OK" /></Root>""";
        var options = new XamlStylerOptions { SpaceBeforeClosingSlash = true, AttributesTolerance = 10 };
        var service = new XamlStylerService(options);

        // Act
        var result = service.StyleDocument(xaml);

        // Assert
        result.Should().Contain(" />");
    }

    [Fact]
    public void SpaceBeforeClosingSlash_Disabled_NoSpace()
    {
        // Arrange
        var xaml = $"""<Root xmlns="{WpfNamespace}"><Button Content="OK" /></Root>""";
        var options = new XamlStylerOptions { SpaceBeforeClosingSlash = false, AttributesTolerance = 10 };
        var service = new XamlStylerService(options);

        // Act
        var result = service.StyleDocument(xaml);

        // Assert
        result.Should().Contain("/>");
        result.Should().NotContain(" />");
    }

    [Fact]
    public void RemoveEndingTagOfEmptyElement_ConvertsToSelfClosing()
    {
        // Arrange
        var xaml = $"""<Root xmlns="{WpfNamespace}"><Button></Button></Root>""";
        var options = new XamlStylerOptions { RemoveEndingTagOfEmptyElement = true };
        var service = new XamlStylerService(options);

        // Act
        var result = service.StyleDocument(xaml);

        // Assert - Empty element should be converted to self-closing
        result.Should().NotContain("</Button>");
        result.Should().Contain("<Button");
        result.Should().Contain("/>");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MarkupExtension_WithDoubleQuotedValueContainingComma_IsNotSplitOnTheNextRun(
        bool formatMarkupExtension)
    {
        var xaml = "<TextBlock xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" Text='{Binding Name, StringFormat=\"a,b\"}' />";

        var firstPass = StyleWith(xaml, o => o.FormatMarkupExtension = formatMarkupExtension);
        var secondPass = StyleWith(firstPass, o => o.FormatMarkupExtension = formatMarkupExtension);

        firstPass.Should().Contain("StringFormat=&quot;a,b&quot;}");
        secondPass.Should().Be(firstPass, "a quoted value is one argument, also when the quotes are written as entities");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MarkupExtension_WithDoubleQuotedValue_IsWrittenAsValidXml(
        bool formatMarkupExtension)
    {
        var xaml = "<TextBlock xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" Text='{Binding Name, StringFormat=\"Hi {0}\"}' />";

        var result = StyleWith(xaml, o => o.FormatMarkupExtension = formatMarkupExtension);

        result.Should().Be("<TextBlock xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" Text=\"{Binding Name, StringFormat=&quot;Hi {0}&quot;}\" />");
        System.Xml.Linq.XDocument.Parse(result).Root!.Attribute("Text")!.Value
            .Should().Be("{Binding Name, StringFormat=\"Hi {0}\"}");
        StyleWith(result, o => o.FormatMarkupExtension = formatMarkupExtension)
            .Should().Be(result, "formatting should be idempotent");
    }

    [Fact]
    public void MarkupExtension_WithDoubleQuotedValue_OnMultipleLines_IsWrittenAsValidXml()
    {
        var xaml = "<TextBlock xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" Width=\"1\" Height=\"2\" Text='{MultiBinding Converter={StaticResource C}, StringFormat=\"Hi {0}\"}' />";

        var result = StyleWith(xaml);

        result.Should().Contain("StringFormat=&quot;Hi {0}&quot;}\"");
        var parse = () => System.Xml.Linq.XDocument.Parse(result);
        parse.Should().NotThrow();
        StyleWith(result).Should().Be(result, "formatting should be idempotent");
    }

    [Fact]
    public void XmlnsDeclaration_WithSpaceBeforeEquals_IsWrittenWithoutPlaceholder()
    {
        var xaml = "<Grid xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x =\"http://schemas.microsoft.com/winfx/2006/xaml\" x:Name=\"Root\" />";

        var result = StyleWith(xaml, o => o.AttributesTolerance = 5);

        result.Should().Be("<Grid xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" x:Name=\"Root\" />");
    }

    [Fact]
    public void AttributeValue_MentioningXmlnsPrefix_IsNotMistakenForADeclaration()
    {
        var xaml = "<Button xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" Width=\"100\" ToolTip=\"Add xmlns:local\" Height=\"100\" />";

        var result = StyleWith(xaml, o => o.AttributesTolerance = 5);

        result.Should().Be("<Button xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" Width=\"100\" Height=\"100\" ToolTip=\"Add xmlns:local\" />");
    }

    [Fact]
    public void MaxAttributeCharactersPerLine_CountsXmlnsDeclarationsByTheirWrittenLength()
    {
        // 'xmlns:a="urn:a" xmlns:b="urn:b"' is 31 characters, which fits within 32.
        var xaml = "<Grid xmlns:a=\"urn:a\" xmlns:b=\"urn:b\" A=\"1\" />";

        var result = StyleWith(xaml, o =>
        {
            o.MaxAttributesPerLine = 0;
            o.MaxAttributeCharactersPerLine = 32;
        });

        var expected = string.Join(
            Environment.NewLine,
            "<Grid",
            "    xmlns:a=\"urn:a\" xmlns:b=\"urn:b\"",
            "    A=\"1\" />");
        result.Should().Be(expected);
    }
}