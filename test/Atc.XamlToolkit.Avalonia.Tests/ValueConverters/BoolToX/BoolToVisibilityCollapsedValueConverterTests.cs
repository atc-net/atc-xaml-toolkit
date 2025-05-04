// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.Avalonia.Tests.ValueConverters;

public sealed class BoolToVisibilityCollapsedValueConverterTests
{
    private readonly IValueConverter converter = BoolToVisibilityCollapsedValueConverter.Instance;

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void Convert(bool expected, bool input)
        => Assert.Equal(
            expected,
            converter.Convert(input, targetType: null!, parameter: null, culture: null!));

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void ConvertBack(bool expected, bool input)
        => Assert.Equal(
            expected,
            converter.ConvertBack(input, targetType: null!, parameter: null, culture: null!));
}