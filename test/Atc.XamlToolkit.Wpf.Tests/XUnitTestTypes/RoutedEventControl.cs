namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

public partial class RoutedEventControl : System.Windows.Controls.UserControl
{
    [Atc.XamlToolkit.Controls.Attributes.RoutedEvent(GenerateRaiseMethod = true)]
    private static readonly System.Windows.RoutedEvent opened;

    [Atc.XamlToolkit.Controls.Attributes.RoutedEvent(
        System.Windows.RoutingStrategy.Direct,
        HandlerType = typeof(System.Windows.RoutedPropertyChangedEventHandler<int>),
        GenerateRaiseMethod = true)]
    private static readonly System.Windows.RoutedEvent valueChanged;

    [Atc.XamlToolkit.Controls.Attributes.RoutedEvent(
        AddOwnerOf = typeof(SharedRoutedEvents),
        HandlerType = typeof(EventHandler<RoutedEventTestEventArgs>),
        GenerateRaiseMethod = true)]
    private static readonly System.Windows.RoutedEvent deviceError;

    public void Open()
        => RaiseOpened();

    public void ChangeValue(
        int oldValue,
        int newValue)
        => RaiseValueChanged(oldValue, newValue);

    public void Fail(string message)
        => RaiseDeviceError(new RoutedEventTestEventArgs(message));
}