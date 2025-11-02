namespace Atc.XamlToolkit.WinUISample.SampleControls.Commands;

public sealed partial class AsyncCommandCancellationAndIsBusyView
{
    public AsyncCommandCancellationAndIsBusyView()
    {
        InitializeComponent();
    }

    public AsyncCommandCancellationAndIsBusyViewModel ViewModel => (AsyncCommandCancellationAndIsBusyViewModel)DataContext;
}