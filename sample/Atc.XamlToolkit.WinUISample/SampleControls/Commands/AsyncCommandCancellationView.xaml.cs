namespace Atc.XamlToolkit.WinUISample.SampleControls.Commands;

public sealed partial class AsyncCommandCancellationView
{
    public AsyncCommandCancellationView()
    {
        InitializeComponent();
        ViewModel = new AsyncCommandCancellationViewModel();
    }

    public AsyncCommandCancellationViewModel ViewModel { get; }
}