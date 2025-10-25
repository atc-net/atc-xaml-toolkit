namespace Atc.XamlToolkit.WpfSample.SampleControls.FrameworkElements.RoutedEventComponents;

/// <summary>
/// Custom numeric up/down control demonstrating routed events for value changes.
/// </summary>
public partial class NumericUpDown : Control
{
    static NumericUpDown()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumericUpDown),
            new FrameworkPropertyMetadata(typeof(NumericUpDown)));
    }

    public NumericUpDown()
    {
        Value = 0;
    }

    /// <summary>
    /// Routed event raised when the value is incremented.
    /// </summary>
    [RoutedEvent(HandlerType = typeof(ValueChangedRoutedEventHandler))]
    private static readonly RoutedEvent valueIncremented;

    /// <summary>
    /// Routed event raised when the value is decremented.
    /// </summary>
    [RoutedEvent(HandlerType = typeof(ValueChangedRoutedEventHandler))]
    private static readonly RoutedEvent valueDecremented;

    /// <summary>
    /// Dependency property for the current value.
    /// </summary>
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(int),
        typeof(NumericUpDown),
        new FrameworkPropertyMetadata(
            defaultValue: 0,
            propertyChangedCallback: OnValueChanged));

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NumericUpDown control)
        {
            return;
        }

        var oldValue = (int)e.OldValue;
        var newValue = (int)e.NewValue;

        if (newValue > oldValue)
        {
            var args = new ValueChangedRoutedEventArgs(
                ValueIncrementedEvent,
                OldValue: oldValue,
                NewValue: newValue,
                Delta: newValue - oldValue);
            control.RaiseEvent(args);
        }
        else if (newValue < oldValue)
        {
            var args = new ValueChangedRoutedEventArgs(
                ValueDecrementedEvent,
                OldValue: oldValue,
                NewValue: newValue,
                Delta: oldValue - newValue);
            control.RaiseEvent(args);
        }
    }

    public void Increment()
    {
        Value++;
    }

    public void Decrement()
    {
        Value--;
    }
}

/// <summary>
/// Custom event handler delegate for value change events.
/// </summary>
public delegate void ValueChangedRoutedEventHandler(object sender, ValueChangedRoutedEventArgs e);

/// <summary>
/// Custom routed event args containing value change details.
/// </summary>
public sealed class ValueChangedRoutedEventArgs(
    RoutedEvent routedEvent,
    int OldValue,
    int NewValue,
    int Delta)
    : RoutedEventArgs(routedEvent)
{
    public int OldValue { get; } = OldValue;

    public int NewValue { get; } = NewValue;

    public int Delta { get; } = Delta;
}
