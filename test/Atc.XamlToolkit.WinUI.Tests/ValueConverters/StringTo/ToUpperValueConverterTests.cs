// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.WinUI.Tests.ValueConverters;

public sealed class ToUpperValueConverterTests
{
    private readonly IValueConverter converter = ToUpperValueConverter.Instance;

    [Theory]
    [InlineData("HELLO WORLD", "hello world")]
    [InlineData("HELLO WORLD", "Hello World")]
    [InlineData("HELLO WORLD", "HELLO WORLD")]
    [InlineData("123", "123")]
    [InlineData("", "")]
    public void Convert(string expected, string input)
        => Assert.Equal(
            expected,
            converter.Convert(input, targetType: null, parameter: null, language: null));

    [Fact]
    public void ConvertBack_ThrowsNotSupportedException()
        => Assert.Throws<NotSupportedException>(
            () => converter.ConvertBack("test", targetType: null, parameter: null, language: null));
}