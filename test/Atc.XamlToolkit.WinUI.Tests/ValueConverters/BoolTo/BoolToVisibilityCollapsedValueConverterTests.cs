// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.WinUI.Tests.ValueConverters;

public sealed class BoolToVisibilityCollapsedValueConverterTests
{
    private readonly IValueConverter converter = BoolToVisibilityCollapsedValueConverter.Instance;

    [Theory]
    [InlineData(Visibility.Collapsed, true)]
    [InlineData(Visibility.Visible, false)]
    public void Convert(
        Visibility expected,
        bool input)
        => Assert.Equal(
            expected,
            converter.Convert(input, targetType: null, parameter: null, language: null));

    [Theory]
    [InlineData(true, Visibility.Collapsed)]
    [InlineData(false, Visibility.Visible)]
    public void ConvertBack(
        bool expected,
        Visibility input)
        => Assert.Equal(
            expected,
            converter.ConvertBack(input, targetType: null, parameter: null, language: null));
}