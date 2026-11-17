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

        var typedValues = new TInput[values.Length];

        for (var i = 0; i < values.Length; i++)
        {
            typedValues[i] = values[i] switch
            {
                TInput typed => typed,

                // A null input is passed on when TInput can hold null (reference type or Nullable<T>).
                null when default(TInput) is null => default!,
                null => throw new UnexpectedTypeException(typeof(object), typeof(TInput)),
                var other => throw new UnexpectedTypeException(other.GetType(), typeof(TInput)),
            };
        }

        // Exceptions from the converter itself are not caught: they are not type errors.
        return Convert(typedValues, parameter, culture);
    }

    object?[]? IMultiValueConverter.ConvertBack(
        object? value,
        Type[] targetTypes,
        object? parameter,
        CultureInfo culture)
    {
        var typedValue = value switch
        {
            TOutput typed => typed,

            // A null value is passed on when TOutput can hold null (reference type or Nullable<T>).
            null when default(TOutput) is null => default!,
            null => throw new UnexpectedTypeException(typeof(object), typeof(TOutput)),
            _ => throw new UnexpectedTypeException(value.GetType(), typeof(TOutput)),
        };

        return ConvertBack(
            typedValue,
            parameter,
            culture);
    }
}