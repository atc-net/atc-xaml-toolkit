// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.Wpf.Tests.ValueConverters;

public sealed class MultiBoolToVisibilityVisibleValueConverterTests
{
    private readonly IMultiValueConverter converter = MultiBoolToVisibilityVisibleValueConverter.Instance;

    private static readonly object[] AllTrue = [true, true, true];
    private static readonly object[] OneFalse = [true, false, true];
    private static readonly object[] AllFalse = [false, false, false];
    private static readonly object[] Empty = [];

    [Theory]
    [InlineData(Visibility.Visible, nameof(AllTrue))]
    [InlineData(Visibility.Collapsed, nameof(OneFalse))]
    public void Convert_AND_DefaultOperator(Visibility expected, string inputSetName)
    {
        // Arrange
        var input = GetInput(inputSetName);

        // Atc
        var actual = converter.Convert(input, targetType: null, parameter: null, culture: null);

        // Arrange
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
            targetType: null!,
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
            targetType: null!,
            parameter: "or",
            culture: CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(Visibility.Visible, actual!);
    }

    [Fact]
    public void ConvertBack_Should_Throw_Exception()
    {
        // Act
        var exception = Record.Exception(() => converter.ConvertBack(value: null, targetTypes: null, parameter: null, culture: null));

        // Assert
        Assert.IsType<NotSupportedException>(exception);
        Assert.Equal("This is a OneWay converter.", exception.Message);
    }

    private static object[] GetInput(string name) => name switch
    {
        nameof(AllTrue) => AllTrue,
        nameof(OneFalse) => OneFalse,
        nameof(AllFalse) => AllFalse,
        nameof(Empty) => Empty,
        _ => throw new ArgumentOutOfRangeException(nameof(name)),
    };
}