// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.Wpf.Tests.ValueConverters;

public sealed class StringNullOrEmptyToVisibilityVisibleValueConverterTests
{
    private readonly IValueConverter converter = StringNullOrEmptyToVisibilityVisibleValueConverter.Instance;

    [Theory]
    [InlineData(Visibility.Visible, "")]
    [InlineData(Visibility.Collapsed, "test")]
    [InlineData(Visibility.Collapsed, " ")]
    [InlineData(Visibility.Collapsed, "Hello World")]
    public void Convert(Visibility expected, string input)
        => Assert.Equal(
            expected,
            converter.Convert(input, targetType: null, parameter: null, culture: null));

    [Fact]
    public void ConvertBack_ThrowsNotSupportedException()
        => Assert.Throws<NotSupportedException>(
            () => converter.ConvertBack(Visibility.Visible, targetType: null, parameter: null, culture: null));
}