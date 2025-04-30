// ReSharper disable SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
namespace Atc.XamlToolkit.ValueConverters;

public sealed class MultiBoolToVisibilityVisibleValueConverter :
    MultiValueConverterBase<bool, bool>,
    Avalonia.Data.Converters.IMultiValueConverter
{
    public static readonly MultiBoolToVisibilityVisibleValueConverter Instance = new();

    public BooleanOperatorType DefaultOperator { get; set; } = BooleanOperatorType.AND;

    public override bool Convert(
        bool[] values,
        object? parameter,
        CultureInfo culture)
    {
        var operatorType = ResolveOperator(parameter);

        return operatorType switch
        {
            BooleanOperatorType.AND => values.All(b => b),
            BooleanOperatorType.OR => values.Any(b => b),
            _ => throw new SwitchCaseDefaultException(nameof(operatorType)),
        };
    }

    public override object?[] ConvertBack(
        bool value,
        object? parameter,
        CultureInfo culture)
        => throw new NotSupportedException("This is a OneWay converter.");

    object? Avalonia.Data.Converters.IMultiValueConverter.Convert(
        IList<object?> values,
        Type targetType,
        object? parameter,
        CultureInfo culture)
        => ((IMultiValueConverter)this).Convert(
            values.ToArray(),
            targetType,
            parameter,
            culture);

    private BooleanOperatorType ResolveOperator(
        object? parameter)
    {
        if (parameter is null)
        {
            return DefaultOperator;
        }

        return parameter switch
        {
            BooleanOperatorType operatorType => operatorType is BooleanOperatorType.AND or BooleanOperatorType.OR
                ? operatorType
                : DefaultOperator,
            string s when Enum.TryParse<BooleanOperatorType>(s, ignoreCase: true, out var parsedOperatorType) &&
                          parsedOperatorType is BooleanOperatorType.AND or BooleanOperatorType.OR => parsedOperatorType,
            _ => DefaultOperator,
        };
    }
}