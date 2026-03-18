namespace Atc.XamlToolkit.XamlStyler.Tests;

public sealed class LineEndingTests
{
    private const string WpfNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

    private const string TestXaml = $"""
        <Root xmlns="{WpfNamespace}">
            <Button Content="OK" />
        </Root>
        """;

    [Fact]
    public void LineEnding_LF_ProducesUnixEndings()
    {
        // Arrange
        var options = new XamlStylerOptions { LineEnding = LineEnding.LF };
        var service = new XamlStylerService(options);

        // Act
        var result = service.StyleDocument(TestXaml);

        // Assert - No \r should be present in LF mode
        result.Should().NotContain("\r");
        result.Should().Contain("\n");
    }

    [Fact]
    public void LineEnding_CRLF_ProducesWindowsEndings()
    {
        // Arrange
        var options = new XamlStylerOptions { LineEnding = LineEnding.CRLF };
        var service = new XamlStylerService(options);

        // Act
        var result = service.StyleDocument(TestXaml);

        // Assert - All \n should be preceded by \r
        var lfOnly = result.Replace("\r\n", string.Empty, StringComparison.Ordinal);
        lfOnly.Should().NotContain("\n");
    }

    [Fact]
    public void LineEnding_Auto_PreservesDefault()
    {
        // Arrange
        var options = new XamlStylerOptions { LineEnding = LineEnding.Auto };
        var service = new XamlStylerService(options);

        // Act
        var result = service.StyleDocument(TestXaml);

        // Assert - Should produce some output without error
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("<Root");
    }
}