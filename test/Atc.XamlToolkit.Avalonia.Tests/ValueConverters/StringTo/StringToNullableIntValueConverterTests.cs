// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.Avalonia.Tests.ValueConverters;

public sealed class StringToNullableIntValueConverterTests
{
    private readonly IValueConverter converter = StringToNullableIntValueConverter.Instance;

    [Theory]
    [InlineData("", null)]
    [InlineData("123", 123)]
    [InlineData("-456", -456)]
    [InlineData("0", 0)]
    public void Convert(
        string expected,
        int? input)
        => Assert.Equal(
            expected,
            converter.Convert(input, targetType: null, parameter: null, culture: null));

    [Theory]
    [InlineData(null, null)]
    [InlineData(null, "   ")]
    [InlineData(123, "123")]
    [InlineData(-456, "-456")]
    [InlineData(0, "0")]
    public void ConvertBack(
        int? expected,
        string? input)
        => Assert.Equal(
            expected,
            converter.ConvertBack(input, targetType: null, parameter: null, culture: null));
}