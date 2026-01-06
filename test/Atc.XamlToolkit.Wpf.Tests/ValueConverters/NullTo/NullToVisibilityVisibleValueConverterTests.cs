// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.Wpf.Tests.ValueConverters.NullTo;

public sealed class NullToVisibilityVisibleValueConverterTests
{
    private readonly IValueConverter converter = NullToVisibilityVisibleValueConverter.Instance;

    [Theory]
    [InlineData(Visibility.Visible, null)]
    [InlineData(Visibility.Collapsed, "")]
    [InlineData(Visibility.Collapsed, "some string")]
    [InlineData(Visibility.Collapsed, 0)]
    [InlineData(Visibility.Collapsed, 42)]
    [InlineData(Visibility.Collapsed, false)]
    [InlineData(Visibility.Collapsed, true)]
    public void Convert(
        Visibility expected,
        object? input)
        => Assert.Equal(
            expected,
            converter.Convert(input, targetType: null, parameter: null, culture: null));

    [Fact]
    public void ConvertBack_Visible_ReturnsNull()
        => Assert.Null(
            converter.ConvertBack(Visibility.Visible, targetType: null, parameter: null, culture: null));

    [Theory]
    [InlineData(Visibility.Collapsed)]
    [InlineData(Visibility.Hidden)]
    public void ConvertBack_NotVisible_ReturnsNonNull(Visibility input)
        => Assert.NotNull(
            converter.ConvertBack(input, targetType: null, parameter: null, culture: null));
}