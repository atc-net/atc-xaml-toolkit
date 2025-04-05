namespace Atc.XamlToolkit.Data.Converters;

public abstract class ValueConverterBase<TInput, TOutput>
    : IValueConverter
{
    public abstract TOutput Convert(
        TInput value,
        object? parameter,
        CultureInfo culture);

    public abstract TInput ConvertBack(
        TOutput value,
        object? parameter,
        CultureInfo culture);

    object? IValueConverter.Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is not TInput typedValue)
        {
            throw new UnexpectedTypeException(value?.GetType() ?? typeof(object), typeof(TInput));
        }

        return Convert(
            typedValue,
            parameter,
            culture);
    }

    object? IValueConverter.ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is not TOutput typedValue)
        {
            throw new UnexpectedTypeException(value?.GetType() ?? typeof(object), typeof(TInput));
        }

        return ConvertBack(
            typedValue,
            parameter,
            culture);
    }
}