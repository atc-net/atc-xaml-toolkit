// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.WinUI.Tests.ValueConverters;

public sealed class ToLowerValueConverterTests
{
    private readonly IValueConverter converter = ToLowerValueConverter.Instance;

    [Theory]
    [InlineData("hello world", "HELLO WORLD")]
    [InlineData("hello world", "Hello World")]
    [InlineData("hello world", "hello world")]
    [InlineData("123", "123")]
    [InlineData("", "")]
    public void Convert(
        string expected,
        string input)
        => Assert.Equal(
            expected,
            converter.Convert(input, targetType: null, parameter: null, language: null));

    [Fact]
    public void ConvertBack_ThrowsNotSupportedException()
        => Assert.Throws<NotSupportedException>(
            () => converter.ConvertBack("test", targetType: null, parameter: null, language: null));
}