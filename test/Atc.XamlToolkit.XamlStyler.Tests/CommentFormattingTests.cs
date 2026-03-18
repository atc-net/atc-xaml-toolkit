namespace Atc.XamlToolkit.XamlStyler.Tests;

public sealed class CommentFormattingTests
{
    private const string WpfNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

    [Fact]
    public void SingleLineComment_PaddedCorrectly()
    {
        // Arrange
        var xaml = $"""<Root xmlns="{WpfNamespace}"><!--Test comment--></Root>""";
        var options = new XamlStylerOptions { CommentSpaces = 2 };
        var service = new XamlStylerService(options);

        // Act
        var result = service.StyleDocument(xaml);

        // Assert - Comment should have padding spaces
        result.Should().Contain("<!--  Test comment  -->");
    }

    [Fact]
    public void CommentPadding_CustomValue_Applied()
    {
        // Arrange
        var xaml = $"""<Root xmlns="{WpfNamespace}"><!--Test--></Root>""";
        var options = new XamlStylerOptions { CommentSpaces = 4 };
        var service = new XamlStylerService(options);

        // Act
        var result = service.StyleDocument(xaml);

        // Assert - Comment should have 4 spaces padding
        result.Should().Contain("<!--    Test    -->");
    }
}