namespace Atc.XamlToolkit.Data.Converters;

public abstract class MultiValueConverterBase<TInput, TOutput>
    : IMultiValueConverter
{
    public abstract TOutput Convert(
        TInput[] values,
        object? parameter,
        CultureInfo culture);

    public abstract object?[]? ConvertBack(
        TOutput value,
        object? parameter,
        CultureInfo culture);

    object? IMultiValueConverter.Convert(
        object?[]? values,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(values);

        try
        {
            var typedValues = new TInput[values.Length];

            for (var i = 0; i < values.Length; i++)
            {
                if (values[i] is not TInput typed)
                {
                    throw new UnexpectedTypeException(values[i]?.GetType() ?? typeof(object), typeof(TInput));
                }

                typedValues[i] = typed;
            }

            return Convert(typedValues, parameter, culture);
        }
        catch (Exception ex)
        {
            throw new UnexpectedTypeException("Failed to convert multi-value inputs.", ex);
        }
    }

    object?[]? IMultiValueConverter.ConvertBack(
        object? value,
        Type[] targetTypes,
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