// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.WinUI.Tests.ValueConverters;

public sealed class BoolToVisibilityVisibleValueConverterTests
{
    private readonly IValueConverter converter = BoolToVisibilityVisibleValueConverter.Instance;

    [Theory]
    [InlineData(Visibility.Visible, true)]
    [InlineData(Visibility.Collapsed, false)]
    public void Convert(
        Visibility expected,
        bool input)
        => Assert.Equal(
            expected,
            converter.Convert(input, targetType: null, parameter: null, language: null));

    [Theory]
    [InlineData(true, Visibility.Visible)]
    [InlineData(false, Visibility.Collapsed)]
    public void ConvertBack(
        bool expected,
        Visibility input)
        => Assert.Equal(
            expected,
            converter.ConvertBack(input, targetType: null, parameter: null, language: null));
}