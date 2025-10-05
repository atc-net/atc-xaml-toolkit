// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.Avalonia.Tests.ValueConverters;

public sealed class ToLowerValueConverterTests
{
    private readonly IValueConverter converter = ToLowerValueConverter.Instance;

    [Theory]
    [InlineData("", "")]
    [InlineData("hello", "HELLO")]
    [InlineData("hello", "Hello")]
    [InlineData("hello", "hello")]
    [InlineData("hello world", "HELLO WORLD")]
    [InlineData("123", "123")]
    public void Convert(string expected, string input)
        => Assert.Equal(
            expected,
            converter.Convert(input, targetType: null!, parameter: null, culture: null!));

    [Fact]
    public void Convert_WithCulture()
    {
        var culture = new CultureInfo("tr-TR"); // Turkish culture for I/i conversion
        var result = converter.Convert("ISTANBUL", targetType: null!, parameter: null, culture: culture);
        Assert.Equal("ıstanbul", result); // Turkish I lowercases to dotless ı
    }

    [Fact]
    public void ConvertBack_ThrowsNotSupportedException()
        => Assert.Throws<NotSupportedException>(
            () => converter.ConvertBack("test", targetType: null!, parameter: null, culture: null!));
}