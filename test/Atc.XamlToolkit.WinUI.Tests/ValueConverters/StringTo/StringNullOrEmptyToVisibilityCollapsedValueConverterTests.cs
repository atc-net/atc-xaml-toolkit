// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.WinUI.Tests.ValueConverters;

public sealed class StringNullOrEmptyToVisibilityCollapsedValueConverterTests
{
    private readonly IValueConverter converter = StringNullOrEmptyToVisibilityCollapsedValueConverter.Instance;

    [Theory]
    [InlineData(Visibility.Collapsed, "")]
    [InlineData(Visibility.Visible, "test")]
    [InlineData(Visibility.Visible, " ")]
    [InlineData(Visibility.Visible, "Hello World")]
    public void Convert(Visibility expected, string input)
        => Assert.Equal(
            expected,
            converter.Convert(input, targetType: null, parameter: null, language: null));

    [Fact]
    public void ConvertBack_ThrowsNotSupportedException()
        => Assert.Throws<NotSupportedException>(
            () => converter.ConvertBack(Visibility.Visible, targetType: null, parameter: null, language: null));
}