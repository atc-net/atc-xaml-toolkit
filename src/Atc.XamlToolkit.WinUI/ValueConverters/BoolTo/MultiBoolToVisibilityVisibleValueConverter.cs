// ReSharper disable SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.ValueConverters;

public sealed class MultiBoolToVisibilityVisibleValueConverter :
    MultiValueConverterBase<bool, Visibility>
{
    public static readonly MultiBoolToVisibilityVisibleValueConverter Instance = new();

    public BooleanOperatorType DefaultOperator { get; set; } = BooleanOperatorType.AND;

    public override Visibility Convert(
        bool[] values,
        object? parameter,
        CultureInfo culture)
    {
        var operatorType = BooleanOperatorTypeResolver.Resolve(
            parameter,
            DefaultOperator);

        return operatorType switch
        {
            BooleanOperatorType.AND => values.All(b => b)
                ? Visibility.Visible
                : Visibility.Collapsed,
            BooleanOperatorType.OR => values.Any(b => b)
                ? Visibility.Visible
                : Visibility.Collapsed,
            _ => throw new SwitchCaseDefaultException(nameof(operatorType)),
        };
    }

    public override object?[] ConvertBack(
        Visibility value,
        object? parameter,
        CultureInfo culture)
        => throw new NotSupportedException("This is a OneWay converter.");
}