namespace Atc.XamlToolkit.XamlStyler.Tests;

public sealed class GoldenFileTests
{
    private static readonly string TestDataDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");

    private static string ReadInput(string fileName) =>
        File.ReadAllText(Path.Combine(TestDataDir, "Input", fileName));

    private static string StyleWithDefaults(string xaml)
    {
        var service = new XamlStylerService(new XamlStylerOptions());
        return service.StyleDocument(xaml);
    }

    [Fact]
    public void WpfWindow_FormattingIsIdempotent()
    {
        var input = ReadInput("WpfWindow.xaml");
        var firstPass = StyleWithDefaults(input);
        var secondPass = StyleWithDefaults(firstPass);

        secondPass.Should().Be(firstPass, "formatting should be idempotent");
    }

    [Fact]
    public void WpfWindow_AttributesAreReordered()
    {
        var input = ReadInput("WpfWindow.xaml");
        var result = StyleWithDefaults(input);

        // xmlns should come before Title (based on default ordering rules)
        var xmlnsIndex = result.IndexOf("xmlns=", StringComparison.Ordinal);
        var titleIndex = result.IndexOf("Title=", StringComparison.Ordinal);
        xmlnsIndex.Should().BeLessThan(titleIndex, "xmlns should be ordered before Title");
    }

    [Fact]
    public void WpfWindow_ExtraWhitespaceRemoved()
    {
        var input = ReadInput("WpfWindow.xaml");
        var result = StyleWithDefaults(input);

        // The input has extra spaces between attributes (e.g. 'Width="120"    Height="40"').
        // After formatting, those multi-space gaps should no longer appear.
        result.Should().NotContain("\"    ", "extra whitespace between attributes should be removed");
        result.Should().NotContain("\"   ", "triple-space gaps between attributes should be removed");
    }

    [Fact]
    public void WpfWindow_BindingsPreserved()
    {
        var input = ReadInput("WpfWindow.xaml");
        var result = StyleWithDefaults(input);

        result.Should().Contain("{Binding ClickCommand}");
        result.Should().Contain("{Binding UserInput");
        result.Should().Contain("{Binding IsEnabled}");
    }

    [Fact]
    public void ResourceDictionary_FormattingIsIdempotent()
    {
        var xaml = @"<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
    <Style TargetType=""TextBlock"">
        <Setter Property=""FontSize"" Value=""24"" />
        <Setter Property=""FontWeight"" Value=""Bold"" />
        <Setter Property=""Foreground"" Value=""DarkBlue"" />
    </Style>
    <SolidColorBrush Color=""#FF0078D7"" />
</ResourceDictionary>";
        var firstPass = StyleWithDefaults(xaml);
        var secondPass = StyleWithDefaults(firstPass);
        secondPass.Should().Be(firstPass, "formatting should be idempotent");
    }
}