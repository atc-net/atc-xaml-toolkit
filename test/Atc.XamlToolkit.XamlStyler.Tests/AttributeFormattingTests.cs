namespace Atc.XamlToolkit.XamlStyler.Tests;

public sealed class AttributeFormattingTests
{
    private const string WpfNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

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
}