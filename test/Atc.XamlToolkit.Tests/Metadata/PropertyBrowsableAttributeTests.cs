namespace Atc.XamlToolkit.Tests.Metadata;

public sealed class PropertyBrowsableAttributeTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Constructor_ShouldSetBrowsable(bool browsable)
    {
        // Act
        var sut = new PropertyBrowsableAttribute(browsable);

        // Assert
        Assert.Equal(browsable, sut.Browsable);
    }

    [Fact]
    public void ToString_ShouldContainBrowsableValue()
    {
        // Arrange
        var sut = new PropertyBrowsableAttribute(true);

        // Act
        var result = sut.ToString();

        // Assert
        Assert.Contains("Browsable", result, StringComparison.Ordinal);
        Assert.Contains("True", result, StringComparison.Ordinal);
    }
}