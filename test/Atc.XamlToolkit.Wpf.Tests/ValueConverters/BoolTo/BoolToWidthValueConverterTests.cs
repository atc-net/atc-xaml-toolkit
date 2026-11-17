// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.Wpf.Tests.ValueConverters;

public sealed class BoolToWidthValueConverterTests
{
    private readonly IValueConverter converter = BoolToWidthValueConverter.Instance;

    [Theory]
    [InlineData(0, false, null)]
    [InlineData(double.NaN, true, null)]
    [InlineData(10, true, 10)]
    [InlineData(double.NaN, true, "Auto")]
    public void Convert(
        double expected,
        bool input,
        object? parameter)
        => Assert.Equal(
            expected,
            converter.Convert(input, targetType: null, parameter, culture: null));

    [Theory]
    [InlineData(10.5, "10.5", "en-US")]
    [InlineData(10.5, "10.5", "da-DK")]
    [InlineData(10.5, 10.5, "da-DK")]
    [InlineData(96, "1in", "en-US")]
    [InlineData(96, "1in", "da-DK")]
    [InlineData(144, "1.5in", "da-DK")]
    public void Convert_IsIndependentOfCurrentCulture(
        double expected,
        object parameter,
        string cultureName)
    {
        // Arrange
        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
        try
        {
            // Act
            var actual = converter.Convert(value: true, targetType: null, parameter, culture: null);

            // Assert
            Assert.Equal(expected, actual);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void ConvertBack_Should_Throw_Exception()
    {
        // Act
        var exception = Record.Exception(() => converter.ConvertBack(value: null, targetType: null, parameter: null, culture: null));

        // Assert
        Assert.IsType<NotSupportedException>(exception);
        Assert.Equal("This is a OneWay converter.", exception.Message);
    }
}