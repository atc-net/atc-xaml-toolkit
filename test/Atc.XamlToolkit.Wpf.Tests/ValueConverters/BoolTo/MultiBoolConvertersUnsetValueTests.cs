// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.Wpf.Tests.ValueConverters;

/// <summary>
/// WPF passes <see cref="System.Windows.DependencyProperty.UnsetValue"/> for every child
/// binding of a MultiBinding that has not produced a value yet (for example before the
/// DataContext is set) and does not catch exceptions thrown by a converter.
/// </summary>
public sealed class MultiBoolConvertersUnsetValueTests
{
    [Fact]
    public void MultiBoolToBool_Convert_ReturnsUnsetValue_WhenAnInputIsUnset()
    {
        System.Windows.Data.IMultiValueConverter sut = new MultiBoolToBoolValueConverter();

        var actual = sut.Convert(
            [true, System.Windows.DependencyProperty.UnsetValue],
            typeof(bool),
            parameter: null,
            CultureInfo.InvariantCulture);

        Assert.Same(System.Windows.DependencyProperty.UnsetValue, actual);
    }

    [Fact]
    public void MultiBoolToVisibilityVisible_Convert_ReturnsUnsetValue_WhenAnInputIsUnset()
    {
        System.Windows.Data.IMultiValueConverter sut = new MultiBoolToVisibilityVisibleValueConverter();

        var actual = sut.Convert(
            [System.Windows.DependencyProperty.UnsetValue, true],
            typeof(System.Windows.Visibility),
            parameter: null,
            CultureInfo.InvariantCulture);

        Assert.Same(System.Windows.DependencyProperty.UnsetValue, actual);
    }
}