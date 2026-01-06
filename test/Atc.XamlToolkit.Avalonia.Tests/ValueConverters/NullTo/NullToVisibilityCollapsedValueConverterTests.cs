// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.Avalonia.Tests.ValueConverters.NullTo;

public sealed class NullToVisibilityCollapsedValueConverterTests
{
    private readonly IValueConverter converter = NullToVisibilityCollapsedValueConverter.Instance;

    [Theory]
    [InlineData(false, null)]
    [InlineData(true, "")]
    [InlineData(true, "some string")]
    [InlineData(true, 0)]
    [InlineData(true, 42)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Convert(
        bool expected,
        object? input)
        => Assert.Equal(
            expected,
            converter.Convert(input, targetType: null!, parameter: null, culture: null!));

    [Fact]
    public void ConvertBack_False_ReturnsNull()
        => Assert.Null(
            converter.ConvertBack(false, targetType: null!, parameter: null, culture: null!));

    [Fact]
    public void ConvertBack_True_ReturnsNonNull()
        => Assert.NotNull(
            converter.ConvertBack(true, targetType: null!, parameter: null, culture: null!));
}