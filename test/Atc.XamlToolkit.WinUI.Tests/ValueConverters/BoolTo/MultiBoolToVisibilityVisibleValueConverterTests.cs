// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.WinUI.Tests.ValueConverters;

public sealed class MultiBoolToVisibilityVisibleValueConverterTests
{
    private readonly MultiBoolToVisibilityVisibleValueConverter converter = MultiBoolToVisibilityVisibleValueConverter.Instance;

    private static readonly bool[] AllTrue = [true, true, true];
    private static readonly bool[] OneFalse = [true, false, true];
    private static readonly bool[] AllFalse = [false, false, false];
    private static readonly bool[] Empty = [];

    [Theory]
    [InlineData(Visibility.Visible, nameof(AllTrue))]
    [InlineData(Visibility.Collapsed, nameof(OneFalse))]
    public void Convert_AND_DefaultOperator(Visibility expected, string inputSetName)
    {
        // Arrange
        var input = GetInput(inputSetName);

        // Act
        var actual = converter.Convert(input, parameter: null, culture: null);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(Visibility.Visible, nameof(AllTrue))]
    [InlineData(Visibility.Visible, nameof(OneFalse))]
    [InlineData(Visibility.Collapsed, nameof(AllFalse))]
    [InlineData(Visibility.Collapsed, nameof(Empty))]
    public void Convert_OR_WithEnumParameter(Visibility expected, string inputSetName)
    {
        // Arrange
        var input = GetInput(inputSetName);

        // Act
        var actual = converter.Convert(
            input,
            parameter: BooleanOperatorType.OR,
            culture: CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Convert_OR_WithStringParameter()
    {
        // Act
        var actual = converter.Convert(
            OneFalse,
            parameter: "or",
            culture: CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(Visibility.Visible, actual);
    }

    [Fact]
    public void ConvertBack_Should_Throw_Exception()
    {
        // Act
        var exception = Record.Exception(() => converter.ConvertBack(value: Visibility.Visible, parameter: null, culture: null));

        // Assert
        Assert.IsType<NotSupportedException>(exception);
        Assert.Equal("This is a OneWay converter.", exception.Message);
    }

    private static bool[] GetInput(string name) => name switch
    {
        nameof(AllTrue) => AllTrue,
        nameof(OneFalse) => OneFalse,
        nameof(AllFalse) => AllFalse,
        nameof(Empty) => Empty,
        _ => throw new ArgumentOutOfRangeException(nameof(name)),
    };
}