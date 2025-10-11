// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.WinUI.Tests.ValueConverters;

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
    public void Convert(Visibility expected, object? input)
        => Assert.Equal(
            expected,
            converter.Convert(input, targetType: null, parameter: null, language: null));

    [Fact]
    public void ConvertBack_Visible_ReturnsNull()
        => Assert.Null(
            converter.ConvertBack(Visibility.Visible, targetType: null, parameter: null, language: null));

    [Fact]
    public void ConvertBack_Collapsed_ReturnsNonNull()
        => Assert.NotNull(
            converter.ConvertBack(Visibility.Collapsed, targetType: null, parameter: null, language: null));
}