// ReSharper disable SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
// ReSharper disable CheckNamespace
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
}