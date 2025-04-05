namespace Atc.XamlToolkit.Avalonia.Tests.ValueConverters;

public sealed class BoolToVisibilityVisibleValueConverterTests
{
    private readonly IValueConverter converter = BoolToVisibilityVisibleValueConverter.Instance;

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void Convert(bool expected, bool input)
        => Assert.Equal(
            expected,
            converter.Convert(input, targetType: null!, parameter: null, culture: null!));

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void ConvertBack(bool expected, bool input)
        => Assert.Equal(
            expected,
            converter.ConvertBack(input, targetType: null!, parameter: null, culture: null!));
}