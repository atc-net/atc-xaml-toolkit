// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.Avalonia.Tests.ValueConverters;

/// <summary>
/// Avalonia passes <see cref="global::Avalonia.AvaloniaProperty.UnsetValue"/> for every child
/// binding of a MultiBinding that has not produced a value yet.
/// </summary>
public sealed class MultiBoolConvertersUnsetValueTests
{
    [Fact]
    public void MultiBoolToBool_Convert_ReturnsUnsetValue_WhenAnInputIsUnset()
    {
        global::Avalonia.Data.Converters.IMultiValueConverter sut = new MultiBoolToBoolValueConverter();

        var actual = sut.Convert(
            [true, global::Avalonia.AvaloniaProperty.UnsetValue],
            typeof(bool),
            parameter: null,
            CultureInfo.InvariantCulture);

        Assert.Same(global::Avalonia.AvaloniaProperty.UnsetValue, actual);
    }

    [Fact]
    public void MultiBoolToVisibilityVisible_Convert_ReturnsUnsetValue_WhenAnInputIsUnset()
    {
        global::Avalonia.Data.Converters.IMultiValueConverter sut = new MultiBoolToVisibilityVisibleValueConverter();

        var actual = sut.Convert(
            [global::Avalonia.AvaloniaProperty.UnsetValue, true],
            typeof(bool),
            parameter: null,
            CultureInfo.InvariantCulture);

        Assert.Same(global::Avalonia.AvaloniaProperty.UnsetValue, actual);
    }
}