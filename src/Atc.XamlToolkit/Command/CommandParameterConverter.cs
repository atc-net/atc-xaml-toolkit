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
        if (parameter is string text)
        {
            return Enum.TryParse(enumType, text, ignoreCase: false, out var result)
                ? result!
                : throw new FormatException($"'{text}' is not a value of {enumType.Name}.");
        }

        try
        {
            return Enum.ToObject(enumType, parameter);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidCastException($"Cannot convert {parameter.GetType().Name} to {enumType.Name}.", ex);
        }
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