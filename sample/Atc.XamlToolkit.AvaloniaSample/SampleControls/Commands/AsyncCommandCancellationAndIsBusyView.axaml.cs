namespace Atc.XamlToolkit.AvaloniaSample.SampleControls.Commands;

public partial class AsyncCommandCancellationAndIsBusyView : UserControl
{
    public AsyncCommandCancellationAndIsBusyView()
    {
        InitializeComponent();
        DataContext = new AsyncCommandCancellationAndIsBusyViewModel();
    }
}