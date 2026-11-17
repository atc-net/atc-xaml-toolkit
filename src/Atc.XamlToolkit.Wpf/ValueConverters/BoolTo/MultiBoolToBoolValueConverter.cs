// ReSharper disable SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.ValueConverters;

[ValueConversion(typeof(bool[]), typeof(bool))]
public sealed class MultiBoolToBoolValueConverter :
    MultiValueConverterBase<bool, bool>,
    System.Windows.Data.IMultiValueConverter
{
    public static readonly MultiBoolToBoolValueConverter Instance = new();

    public BooleanOperatorType DefaultOperator { get; set; } = BooleanOperatorType.AND;

    public override bool Convert(
        bool[] values,
        object? parameter,
        CultureInfo culture)
    {
        var operatorType = BooleanOperatorTypeResolver.Resolve(
            parameter,
            DefaultOperator);

        return operatorType switch
        {
            BooleanOperatorType.AND => values.All(b => b),
            BooleanOperatorType.OR => values.Any(b => b),
            _ => throw new SwitchCaseDefaultException(nameof(operatorType)),
        };
    }

    public override object[] ConvertBack(
        bool value,
        object? parameter,
        CultureInfo culture)
        => throw new NotSupportedException("This is a OneWay converter.");

    object? System.Windows.Data.IMultiValueConverter.Convert(
        object[]? values,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        // WPF passes UnsetValue for a child binding that has not produced a value yet,
        // for example before the DataContext is set. Leave the target unset as well.
        if (values is not null &&
            Array.Exists(values, x => x == DependencyProperty.UnsetValue))
        {
            return DependencyProperty.UnsetValue;
        }

        return ((Data.Converters.IMultiValueConverter)this).Convert(
            values,
            targetType,
            parameter,
            culture);
    }

    object?[] System.Windows.Data.IMultiValueConverter.ConvertBack(
        object? value,
        Type[] targetTypes,
        object? parameter,
        CultureInfo culture)
        => throw new NotSupportedException("This is a OneWay converter.");
}