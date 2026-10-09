namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

/// <summary>
/// One generated dependency property per <see cref="FrameworkPropertyMetadata"/> argument combination.
/// The test project fails to build if the generator emits a call that matches no constructor.
/// </summary>
[DependencyProperty<int>(
    "FlagsAndIsAnimationProhibited",
    Flags = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
    IsAnimationProhibited = true)]

[DependencyProperty<decimal>(
    "FlagsAndIsAnimationProhibitedAndUpdateSourceTrigger",
    DefaultValue = 0,
    Flags = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
    IsAnimationProhibited = true,
    DefaultUpdateSourceTrigger = UpdateSourceTrigger.LostFocus)]

[DependencyProperty<string>(
    "FlagsAndUpdateSourceTrigger",
    Flags = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
    DefaultUpdateSourceTrigger = UpdateSourceTrigger.LostFocus)]

[DependencyProperty<double>(
    "FlagsAndCoerceValueCallback",
    Flags = FrameworkPropertyMetadataOptions.AffectsMeasure,
    CoerceValueCallback = nameof(CoerceValue))]

[DependencyProperty<string>(
    "PropertyChangedCallbackAndUpdateSourceTrigger",
    PropertyChangedCallback = nameof(OnValueChanged),
    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged)]

[DependencyProperty<int>(
    "IsAnimationProhibitedAndUpdateSourceTrigger",
    IsAnimationProhibited = true,
    DefaultUpdateSourceTrigger = UpdateSourceTrigger.LostFocus)]

[DependencyProperty<double>(
    "IsAnimationProhibitedOnly",
    IsAnimationProhibited = true)]

[DependencyProperty<double>(
    "CoerceValueCallbackOnly",
    CoerceValueCallback = nameof(CoerceValue))]
public sealed partial class FrameworkPropertyMetadataTestControl : FrameworkElement
{
    private static object CoerceValue(
        DependencyObject d,
        object baseValue)
        => baseValue;

    private static void OnValueChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        // Only needs to exist so the generated metadata has a change callback.
    }
}