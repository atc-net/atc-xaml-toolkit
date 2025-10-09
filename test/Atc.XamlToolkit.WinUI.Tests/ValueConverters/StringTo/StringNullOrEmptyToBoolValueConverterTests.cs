// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.WinUI.Tests.ValueConverters;

public sealed class StringNullOrEmptyToBoolValueConverterTests
{
    private readonly IValueConverter converter = StringNullOrEmptyToBoolValueConverter.Instance;

    [Theory]
    [InlineData(true, "")]
    [InlineData(false, "test")]
    [InlineData(false, " ")]
    [InlineData(false, "Hello World")]
    public void Convert(bool expected, string input)
        => Assert.Equal(
            expected,
            converter.Convert(input, targetType: null, parameter: null, language: null));

    [Fact]
    public void ConvertBack_ThrowsNotSupportedException()
        => Assert.Throws<NotSupportedException>(
            () => converter.ConvertBack(true, targetType: null, parameter: null, language: null));
}