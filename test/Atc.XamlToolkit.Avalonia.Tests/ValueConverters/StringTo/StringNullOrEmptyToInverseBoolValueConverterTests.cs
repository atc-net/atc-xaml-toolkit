// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.Avalonia.Tests.ValueConverters;

public sealed class StringNullOrEmptyToInverseBoolValueConverterTests
{
    private readonly IValueConverter converter = StringNullOrEmptyToInverseBoolValueConverter.Instance;

    [Theory]
    [InlineData(false, "")]
    [InlineData(true, "test")]
    [InlineData(true, " ")]
    [InlineData(true, "Hello World")]
    public void Convert(bool expected, string input)
        => Assert.Equal(
            expected,
            converter.Convert(input, targetType: null!, parameter: null, culture: null!));

    [Fact]
    public void ConvertBack_ThrowsNotSupportedException()
        => Assert.Throws<NotSupportedException>(
            () => converter.ConvertBack(true, targetType: null!, parameter: null, culture: null!));
}