namespace Atc.XamlToolkit.XamlStyler.Tests;

public sealed class AttributeReorderingTests
{
    private const string WpfNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

    [Fact]
    public void Reordering_Enabled_SortsAttributes()
    {
        // Arrange - xmlns should come before custom attributes after reordering
        var xaml = $"""<Root Width="100" xmlns="{WpfNamespace}"><TextBlock Text="Hi" /></Root>""";
        var options = new XamlStylerOptions
        {
            EnableAttributeReordering = true,
            AttributesTolerance = 10,
        };
        var service = new XamlStylerService(options);

        // Act
        var result = service.StyleDocument(xaml);

        // Assert - xmlns should appear before Width after reordering
        var xmlnsIndex = result.IndexOf("xmlns=", StringComparison.Ordinal);
        var widthIndex = result.IndexOf("Width=", StringComparison.Ordinal);
        xmlnsIndex.Should().BeLessThan(widthIndex);
    }

    [Fact]
    public void Reordering_Disabled_PreservesOriginalOrder()
    {
        // Arrange - Width placed before xmlns intentionally
        var xaml = $"""<Root Width="100" xmlns="{WpfNamespace}"><TextBlock Text="Hi" /></Root>""";
        var options = new XamlStylerOptions
        {
            EnableAttributeReordering = false,
            AttributesTolerance = 10,
        };
        var service = new XamlStylerService(options);

        // Act
        var result = service.StyleDocument(xaml);

        // Assert - Width should still appear before xmlns
        var widthIndex = result.IndexOf("Width=", StringComparison.Ordinal);
        var xmlnsIndex = result.IndexOf("xmlns=", StringComparison.Ordinal);
        widthIndex.Should().BeLessThan(xmlnsIndex);
    }
}