// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.ValueConverters;

public sealed class NullToVisibilityVisibleValueConverter :
    ValueConverterBase<object?, Visibility>,
    Microsoft.UI.Xaml.Data.IValueConverter
{
    public static readonly NullToVisibilityVisibleValueConverter Instance = new();

    public override Visibility Convert(
        object? value,
        object? parameter,
        CultureInfo culture)
        => value is null
            ? Visibility.Visible
            : Visibility.Collapsed;

    public override object? ConvertBack(
        Visibility value,
        object? parameter,
        CultureInfo culture)
        => value == Visibility.Visible
            ? null
            : new object();

    object? Microsoft.UI.Xaml.Data.IValueConverter.Convert(
        object? value,
        Type targetType,
        object? parameter,
        string language)
        => Convert(
            value,
            parameter,
            CultureInfo.CurrentCulture);

    object? Microsoft.UI.Xaml.Data.IValueConverter.ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        string language)
    {
        if (value is not Visibility visibility)
        {
            throw new UnexpectedTypeException(value?.GetType() ?? typeof(object), typeof(Visibility));
        }

        return ConvertBack(
            visibility,
            parameter,
            CultureInfo.CurrentCulture);
    }
}