namespace Atc.XamlToolkit.XamlStyler.Tests.Options;

public sealed class XamlStylerOptionsTests
{
    [Fact]
    public void DefaultOptions_HasExpectedIndentSize()
    {
        // Arrange & Act
        var options = new XamlStylerOptions();

        // Assert
        options.IndentSize.Should().Be(4);
    }

    [Fact]
    public void DefaultOptions_HasExpectedAttributesTolerance()
    {
        // Arrange & Act
        var options = new XamlStylerOptions();

        // Assert
        options.AttributesTolerance.Should().Be(2);
    }

    [Fact]
    public void DefaultOptions_EnableAttributeReordering_IsTrue()
    {
        // Arrange & Act
        var options = new XamlStylerOptions();

        // Assert
        options.EnableAttributeReordering.Should().BeTrue();
    }

    [Fact]
    public void DefaultOptions_RemoveEndingTagOfEmptyElement_IsTrue()
    {
        // Arrange & Act
        var options = new XamlStylerOptions();

        // Assert
        options.RemoveEndingTagOfEmptyElement.Should().BeTrue();
    }

    [Fact]
    public void DefaultOptions_SpaceBeforeClosingSlash_IsTrue()
    {
        // Arrange & Act
        var options = new XamlStylerOptions();

        // Assert
        options.SpaceBeforeClosingSlash.Should().BeTrue();
    }

    [Fact]
    public void DefaultOptions_ReorderVSM_IsLast()
    {
        // Arrange & Act
        var options = new XamlStylerOptions();

        // Assert
        options.ReorderVSM.Should().Be(VisualStateManagerRule.Last);
    }

    [Fact]
    public void DefaultOptions_LineEnding_IsAuto()
    {
        // Arrange & Act
        var options = new XamlStylerOptions();

        // Assert
        options.LineEnding.Should().Be(LineEnding.Auto);
    }

    [Fact]
    public void Clone_ReturnsIndependentCopy()
    {
        // Arrange
        var original = new XamlStylerOptions();
        var clone = original.Clone();

        // Act
        clone.IndentSize = 8;
        clone.AttributesTolerance = 99;

        // Assert
        original.IndentSize.Should().Be(4);
        original.AttributesTolerance.Should().Be(2);
    }

    [Fact]
    public void FromConfigFile_InvalidPath_ReturnsDefaults()
    {
        // Arrange & Act
        var options = XamlStylerOptions.FromConfigFile("/non/existent/path/config.json");

        // Assert - should fall back to defaults
        options.IndentSize.Should().Be(4);
        options.AttributesTolerance.Should().Be(2);
        options.EnableAttributeReordering.Should().BeTrue();
    }

    [Fact]
    public void FindConfigFile_NoConfigExists_ReturnsNull()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act
            var result = XamlStylerOptions.FindConfigFile(tempDir);

            // Assert
            result.Should().BeNull();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}