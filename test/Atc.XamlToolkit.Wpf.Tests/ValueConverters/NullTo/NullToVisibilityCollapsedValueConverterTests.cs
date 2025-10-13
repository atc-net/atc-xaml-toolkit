// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.Wpf.Tests.ValueConverters.NullTo;

public sealed class NullToVisibilityCollapsedValueConverterTests
{
    private readonly IValueConverter converter = NullToVisibilityCollapsedValueConverter.Instance;

    [Theory]
    [InlineData(Visibility.Collapsed, null)]
    [InlineData(Visibility.Visible, "")]
    [InlineData(Visibility.Visible, "some string")]
    [InlineData(Visibility.Visible, 0)]
    [InlineData(Visibility.Visible, 42)]
    [InlineData(Visibility.Visible, false)]
    [InlineData(Visibility.Visible, true)]
    public void Convert(Visibility expected, object? input)
        => Assert.Equal(
            expected,
            converter.Convert(input, targetType: null, parameter: null, culture: null));

    [Fact]
    public void ConvertBack_Collapsed_ReturnsNull()
        => Assert.Null(
            converter.ConvertBack(Visibility.Collapsed, targetType: null, parameter: null, culture: null));

    [Theory]
    [InlineData(Visibility.Visible)]
    [InlineData(Visibility.Hidden)]
    public void ConvertBack_NotCollapsed_ReturnsNonNull(Visibility input)
        => Assert.NotNull(
            converter.ConvertBack(input, targetType: null, parameter: null, culture: null));
}