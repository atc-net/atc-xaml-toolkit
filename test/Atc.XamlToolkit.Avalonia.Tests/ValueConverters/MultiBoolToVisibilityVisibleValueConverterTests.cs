namespace Atc.XamlToolkit.Avalonia.Tests.ValueConverters;

public sealed class MultiBoolToVisibilityVisibleValueConverterTests
{
    private readonly IMultiValueConverter converter = MultiBoolToVisibilityVisibleValueConverter.Instance;

    private readonly object[] inputSet1 = [true, true, true];
    private readonly object[] inputSet2 = [true, false, true];

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 2)]
    public void Convert(bool expected, int inputSetNumber)
    {
        // Arrange
        var input = inputSetNumber switch
        {
            1 => inputSet1,
            2 => inputSet2,
            _ => [],
        };

        // Atc
        var actual = converter.Convert(input, targetType: null!, parameter: null, culture: null!);

        // Arrange
        Assert.Equal(expected, actual);
    }
}