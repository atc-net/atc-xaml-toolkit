namespace Atc.XamlToolkit.Tests.Metadata;

public sealed class PropertyDisplayAttributeTests
{
    [Fact]
    public void DefaultConstructor_ShouldHaveNullDisplayNameAndDefaults()
    {
        // Act
        var sut = new PropertyDisplayAttribute();

        // Assert
        Assert.Null(sut.DisplayName);
        Assert.Null(sut.GroupName);
        Assert.Equal(int.MaxValue, sut.Order);
        Assert.Null(sut.Description);
    }

    [Fact]
    public void Constructor_WithDisplayName_ShouldSetDisplayName()
    {
        // Act
        var sut = new PropertyDisplayAttribute("My Property");

        // Assert
        Assert.Equal("My Property", sut.DisplayName);
        Assert.Null(sut.GroupName);
        Assert.Equal(int.MaxValue, sut.Order);
    }

    [Fact]
    public void Constructor_WithDisplayNameAndGroupName_ShouldSetBoth()
    {
        // Act
        var sut = new PropertyDisplayAttribute("My Property", "Appearance");

        // Assert
        Assert.Equal("My Property", sut.DisplayName);
        Assert.Equal("Appearance", sut.GroupName);
    }

    [Fact]
    public void Constructor_WithAllParameters_ShouldSetAll()
    {
        // Act
        var sut = new PropertyDisplayAttribute("My Property", "Appearance", 5);

        // Assert
        Assert.Equal("My Property", sut.DisplayName);
        Assert.Equal("Appearance", sut.GroupName);
        Assert.Equal(5, sut.Order);
    }

    [Fact]
    public void Description_ShouldBeSettable()
    {
        // Act
        var sut = new PropertyDisplayAttribute("Name")
        {
            Description = "A tooltip",
        };

        // Assert
        Assert.Equal("A tooltip", sut.Description);
    }

    [Fact]
    public void ToString_ShouldContainAllProperties()
    {
        // Arrange
        var sut = new PropertyDisplayAttribute("Name", "Group", 3)
        {
            Description = "Desc",
        };

        // Act
        var result = sut.ToString();

        // Assert
        Assert.Contains("Name", result, StringComparison.Ordinal);
        Assert.Contains("Group", result, StringComparison.Ordinal);
        Assert.Contains("3", result, StringComparison.Ordinal);
        Assert.Contains("Desc", result, StringComparison.Ordinal);
    }
}