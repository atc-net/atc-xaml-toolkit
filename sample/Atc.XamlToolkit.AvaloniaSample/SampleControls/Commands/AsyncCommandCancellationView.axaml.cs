namespace Atc.XamlToolkit.AvaloniaSample.SampleControls.Commands;

public partial class AsyncCommandCancellationView : UserControl
{
    public AsyncCommandCancellationView()
    {
        InitializeComponent();
        DataContext = new AsyncCommandCancellationViewModel();
    }
}