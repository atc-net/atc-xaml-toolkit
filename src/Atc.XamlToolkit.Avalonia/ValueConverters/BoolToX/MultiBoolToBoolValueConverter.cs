// ReSharper disable SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.ValueConverters;

public sealed class MultiBoolToBoolValueConverter :
    MultiValueConverterBase<bool, bool>,
    Avalonia.Data.Converters.IMultiValueConverter
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

    public object? Convert(
        IList<object?> values,
        Type targetType,
        object? parameter,
        CultureInfo culture)
        => ((IMultiValueConverter)this).Convert(
            values.ToArray(),
            targetType,
            parameter,
            culture);

    public override object[] ConvertBack(
        bool value,
        object? parameter,
        CultureInfo culture)
        => throw new NotSupportedException("This is a OneWay converter.");
}