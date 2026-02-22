namespace Atc.XamlToolkit.Tests.Metadata;

public sealed class PropertyEditorHintAttributeTests
{
    [Theory]
    [InlineData(EditorHint.Default)]
    [InlineData(EditorHint.Slider)]
    [InlineData(EditorHint.ColorPicker)]
    [InlineData(EditorHint.FilePath)]
    [InlineData(EditorHint.DirectoryPath)]
    [InlineData(EditorHint.MultiLineText)]
    [InlineData(EditorHint.Password)]
    [InlineData(EditorHint.ReadOnly)]
    public void Constructor_ShouldSetHint(EditorHint hint)
    {
        // Act
        var sut = new PropertyEditorHintAttribute(hint);

        // Assert
        Assert.Equal(hint, sut.Hint);
    }

    [Fact]
    public void ToString_ShouldContainHintValue()
    {
        // Arrange
        var sut = new PropertyEditorHintAttribute(EditorHint.Slider);

        // Act
        var result = sut.ToString();

        // Assert
        Assert.Contains("Hint", result, StringComparison.Ordinal);
        Assert.Contains("Slider", result, StringComparison.Ordinal);
    }
}