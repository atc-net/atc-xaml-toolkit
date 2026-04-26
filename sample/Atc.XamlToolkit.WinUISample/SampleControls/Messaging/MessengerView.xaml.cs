namespace Atc.XamlToolkit.WinUISample.SampleControls.Messaging;

public sealed partial class MessengerView : UserControl
{
    public MessengerViewModel ViewModel { get; }

    public MessengerView()
    {
        ViewModel = new MessengerViewModel();
        InitializeComponent();
    }
}