namespace Atc.XamlToolkit.Tests.Metadata;

public sealed class PropertyRangeAttributeTests
{
    [Fact]
    public void Constructor_WithDoubleRange_ShouldSetMinMax()
    {
        // Act
        var sut = new PropertyRangeAttribute(0.5, 10.0);

        // Assert
        Assert.Equal(0.5, sut.Minimum);
        Assert.Equal(10.0, sut.Maximum);
        Assert.Null(sut.Step);
    }

    [Fact]
    public void Constructor_WithDoubleRangeAndStep_ShouldSetAll()
    {
        // Act
        var sut = new PropertyRangeAttribute(0.0, 1.0, 0.1);

        // Assert
        Assert.Equal(0.0, sut.Minimum);
        Assert.Equal(1.0, sut.Maximum);
        Assert.Equal(0.1, sut.Step);
    }

    [Fact]
    public void Constructor_WithIntRange_ShouldSetMinMax()
    {
        // Act
        var sut = new PropertyRangeAttribute(0, 100);

        // Assert
        Assert.Equal(0.0, sut.Minimum);
        Assert.Equal(100.0, sut.Maximum);
        Assert.Null(sut.Step);
    }

    [Fact]
    public void Constructor_WithIntRangeAndStep_ShouldSetAll()
    {
        // Act
        var sut = new PropertyRangeAttribute(0, 100, 5);

        // Assert
        Assert.Equal(0.0, sut.Minimum);
        Assert.Equal(100.0, sut.Maximum);
        Assert.Equal(5.0, sut.Step);
    }

    [Fact]
    public void ToString_ShouldContainValues()
    {
        // Arrange
        var sut = new PropertyRangeAttribute(1.0, 10.0, 0.5);

        // Act
        var result = sut.ToString();

        // Assert
        Assert.Contains("Minimum", result, StringComparison.Ordinal);
        Assert.Contains("Maximum", result, StringComparison.Ordinal);
        Assert.Contains("Step", result, StringComparison.Ordinal);
    }
}