namespace Atc.XamlToolkit.XamlStyler.Tests;

public sealed class ElementReorderingTests
{
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
    public void ReorderGridChildren_SortsByRowThenColumn()
    {
        var xaml = @"<Grid xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
    <TextBlock Grid.Row=""1"" Grid.Column=""0"" Text=""B"" />
    <TextBlock Grid.Row=""0"" Grid.Column=""1"" Text=""A2"" />
    <TextBlock Grid.Row=""0"" Grid.Column=""0"" Text=""A1"" />
</Grid>";
        var result = StyleWith(xaml, o => o.ReorderGridChildren = true);

        // After reordering: Row=0,Col=0 first, then Row=0,Col=1, then Row=1,Col=0
        var a1Index = result.IndexOf("A1", StringComparison.Ordinal);
        var a2Index = result.IndexOf("A2", StringComparison.Ordinal);
        var bIndex = result.IndexOf("\"B\"", StringComparison.Ordinal);

        a1Index.Should().BeLessThan(a2Index, "Row=0,Col=0 should come before Row=0,Col=1");
        a2Index.Should().BeLessThan(bIndex, "Row=0 elements should come before Row=1");
    }

    [Fact]
    public void ReorderGridChildren_Disabled_PreservesOrder()
    {
        var xaml = @"<Grid xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
    <TextBlock Grid.Row=""1"" Text=""B"" />
    <TextBlock Grid.Row=""0"" Text=""A"" />
</Grid>";
        var result = StyleWith(xaml, o => o.ReorderGridChildren = false);

        var bIndex = result.IndexOf("\"B\"", StringComparison.Ordinal);
        var aIndex = result.IndexOf("\"A\"", StringComparison.Ordinal);
        bIndex.Should().BeLessThan(aIndex, "original order should be preserved");
    }

    [Fact]
    public void ReorderSetters_ByProperty_SortsAlphabetically()
    {
        var xaml = @"<Style xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
    <Setter Property=""Width"" Value=""100"" />
    <Setter Property=""Background"" Value=""Red"" />
    <Setter Property=""Height"" Value=""50"" />
</Style>";
        var result = StyleWith(xaml, o => o.ReorderSetters = ReorderSettersBy.Property);

        var bgIndex = result.IndexOf("Background", StringComparison.Ordinal);
        var hIndex = result.IndexOf("Height", StringComparison.Ordinal);
        var wIndex = result.IndexOf("Width", StringComparison.Ordinal);

        bgIndex.Should().BeLessThan(hIndex, "Background < Height alphabetically");
        hIndex.Should().BeLessThan(wIndex, "Height < Width alphabetically");
    }

    [Fact]
    public void ReorderSetters_None_PreservesOrder()
    {
        var xaml = @"<Style xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
    <Setter Property=""Width"" Value=""100"" />
    <Setter Property=""Background"" Value=""Red"" />
</Style>";
        var result = StyleWith(xaml, o => o.ReorderSetters = ReorderSettersBy.None);

        var wIndex = result.IndexOf("Width", StringComparison.Ordinal);
        var bgIndex = result.IndexOf("Background", StringComparison.Ordinal);
        wIndex.Should().BeLessThan(bgIndex, "original order preserved");
    }
}