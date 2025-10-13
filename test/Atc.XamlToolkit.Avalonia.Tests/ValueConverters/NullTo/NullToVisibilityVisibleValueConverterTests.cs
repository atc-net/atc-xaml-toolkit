// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.Avalonia.Tests.ValueConverters.NullTo;

public sealed class NullToVisibilityVisibleValueConverterTests
{
    private readonly IValueConverter converter = NullToVisibilityVisibleValueConverter.Instance;

    [Theory]
    [InlineData(true, null)]
    [InlineData(false, "")]
    [InlineData(false, "some string")]
    [InlineData(false, 0)]
    [InlineData(false, 42)]
    [InlineData(false, false)]
    [InlineData(false, true)]
    public void Convert(bool expected, object? input)
        => Assert.Equal(
            expected,
            converter.Convert(input, targetType: null!, parameter: null, culture: null!));

    [Fact]
    public void ConvertBack_True_ReturnsNull()
        => Assert.Null(
            converter.ConvertBack(true, targetType: null!, parameter: null, culture: null!));

    [Fact]
    public void ConvertBack_False_ReturnsNonNull()
        => Assert.NotNull(
            converter.ConvertBack(false, targetType: null!, parameter: null, culture: null!));
}