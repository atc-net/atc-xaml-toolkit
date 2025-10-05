// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.Wpf.Tests.ValueConverters;

public sealed class ToUpperValueConverterTests
{
    private readonly IValueConverter converter = ToUpperValueConverter.Instance;

    [Theory]
    [InlineData("", "")]
    [InlineData("HELLO", "hello")]
    [InlineData("HELLO", "Hello")]
    [InlineData("HELLO", "HELLO")]
    [InlineData("HELLO WORLD", "hello world")]
    [InlineData("123", "123")]
    public void Convert(string expected, string input)
        => Assert.Equal(
            expected,
            converter.Convert(input, targetType: null, parameter: null, culture: null));

    [Fact]
    public void Convert_WithCulture()
    {
        var culture = new CultureInfo("tr-TR"); // Turkish culture for I/i conversion
        var result = converter.Convert("istanbul", targetType: null, parameter: null, culture: culture);
        Assert.Equal("İSTANBUL", result);
    }

    [Fact]
    public void ConvertBack_ThrowsNotSupportedException()
        => Assert.Throws<NotSupportedException>(
            () => converter.ConvertBack("TEST", targetType: null, parameter: null, culture: null));
}