namespace Atc.XamlToolkit.Command;

/// <summary>
/// Converts a raw command parameter (as delivered by a XAML <c>CommandParameter</c>)
/// to the parameter type of a generic command.
/// </summary>
internal static class CommandParameterConverter
{
    /// <summary>
    /// Converts <paramref name="parameter"/> to <typeparamref name="T"/> when it is an
    /// <see cref="IConvertible"/> of another type; otherwise returns it unchanged.
    /// Text is read with the invariant culture, as XAML literals are culture-invariant.
    /// Enum and <see cref="Nullable{T}"/> parameter types are supported.
    /// </summary>
    /// <typeparam name="T">The command parameter type.</typeparam>
    /// <param name="parameter">The raw command parameter.</param>
    /// <returns>The converted parameter.</returns>
    /// <exception cref="InvalidCastException">The conversion is not supported.</exception>
    /// <exception cref="FormatException">The parameter is not in a format recognized by <typeparamref name="T"/>.</exception>
    /// <exception cref="OverflowException">The parameter is out of the range of <typeparamref name="T"/>.</exception>
    public static object? Convert<T>(object? parameter)
    {
        if (parameter is null or T or not IConvertible)
        {
            return parameter;
        }

        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        return targetType.IsEnum
            ? ConvertToEnum(targetType, parameter)
            : System.Convert.ChangeType(parameter, targetType, CultureInfo.InvariantCulture);
    }

    private static object ConvertToEnum(
        Type enumType,
        object parameter)
    {
        object value;

        if (parameter is string text)
        {
            if (!Enum.TryParse(enumType, text, ignoreCase: false, out var parsed))
            {
                throw new FormatException($"'{text}' is not a value of {enumType.Name}.");
            }

            value = parsed!;
        }
        else if (parameter is bool or char)
        {
            // Enum.ToObject accepts these, but they are not numbers a caller means as an enum.
            throw new InvalidCastException($"Cannot convert {parameter.GetType().Name} to {enumType.Name}.");
        }
        else
        {
            try
            {
                value = Enum.ToObject(enumType, parameter);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidCastException($"Cannot convert {parameter.GetType().Name} to {enumType.Name}.", ex);
            }
        }

        // Enum.TryParse and Enum.ToObject take any number, also one the enum does not define.
        return IsValueOf(enumType, value)
            ? value
            : throw new FormatException($"'{parameter}' is not a value of {enumType.Name}.");
    }

    // A defined value, or for a [Flags] enum a combination of defined values. A combination
    // is written as names ("Read, Write"); an undefined number is written as digits.
    private static bool IsValueOf(
        Type enumType,
        object value)
    {
        if (Enum.IsDefined(enumType, value))
        {
            return true;
        }

        if (!enumType.IsDefined(typeof(FlagsAttribute), inherit: false))
        {
            return false;
        }

        var text = value.ToString()!;
        return text.Length > 0 && !char.IsDigit(text[0]) && text[0] != '-';
    }

    /// <summary>
    /// Converts <paramref name="parameter"/> like <see cref="Convert{T}"/>, but reports a
    /// failed conversion through the return value instead of an exception.
    /// </summary>
    /// <typeparam name="T">The command parameter type.</typeparam>
    /// <param name="parameter">The raw command parameter.</param>
    /// <param name="converted">The converted parameter when the method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if the parameter was converted; otherwise <see langword="false"/>.</returns>
    public static bool TryConvert<T>(
        object? parameter,
        out object? converted)
    {
        try
        {
            converted = Convert<T>(parameter);
            return true;
        }
        catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
        {
            converted = null;
            return false;
        }
    }
}