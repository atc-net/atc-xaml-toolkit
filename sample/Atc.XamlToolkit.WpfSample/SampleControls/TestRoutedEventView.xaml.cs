// ReSharper disable InconsistentNaming
namespace Atc.XamlToolkit.WpfSample.SampleControls;

[SuppressMessage("", "SA1311", Justification = "OK.")]
public partial class TestRoutedEventView
{
    public TestRoutedEventView()
    {
        InitializeComponent();
    }

    [RoutedEvent]
    private static readonly RoutedEvent tap1;

    [RoutedEvent(RoutingStrategy.Tunnel)]
    private static readonly RoutedEvent tap2;

    [RoutedEvent(HandlerType = typeof(NumericBoxChangedRoutedEventHandler))]
    private static readonly RoutedEvent valueIncremented;
}

[SuppressMessage("", "CA1003", Justification = "OK.")]
[SuppressMessage("", "CA1711", Justification = "OK.")]
public delegate void NumericBoxChangedRoutedEventHandler(object sender, NumericBoxChangedRoutedEventArgs args);

[SuppressMessage("", "SA1402", Justification = "OK.")]
public sealed class NumericBoxChangedRoutedEventArgs(
    RoutedEvent routedEvent,
    double interval)
    : RoutedEventArgs(routedEvent)
{
    public double Interval { get; set; } = interval;
}