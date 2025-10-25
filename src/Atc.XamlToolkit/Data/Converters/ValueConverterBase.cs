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
        // Handle null values for nullable reference types
        if (value is null)
        {
            // Check if TInput can accept null (nullable reference type or nullable value type)
            if (default(TInput) is null || Nullable.GetUnderlyingType(typeof(TInput)) is not null)
            {
                return Convert(
                    default!,
                    parameter,
                    culture);
            }

            throw new UnexpectedTypeException(typeof(object), typeof(TInput));
        }

        if (value is not TInput typedValue)
        {
            throw new UnexpectedTypeException(value.GetType(), typeof(TInput));
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
        // Handle null values for nullable reference types
        if (value is null)
        {
            // Check if TOutput can accept null (nullable reference type or nullable value type)
            if (default(TOutput) is null || Nullable.GetUnderlyingType(typeof(TOutput)) is not null)
            {
                return ConvertBack(
                    default!,
                    parameter,
                    culture);
            }

            throw new UnexpectedTypeException(typeof(object), typeof(TOutput));
        }

        if (value is not TOutput typedValue)
        {
            throw new UnexpectedTypeException(value.GetType(), typeof(TOutput));
        }

        return ConvertBack(
            typedValue,
            parameter,
            culture);
    }
}