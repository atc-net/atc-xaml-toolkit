namespace Atc.XamlToolkit.WinUISample.SampleControls.Commands;

public sealed partial class CounterView : UserControl
{
    public CounterViewModel ViewModel { get; }

    public CounterView()
    {
        ViewModel = new CounterViewModel();
        InitializeComponent();
    }
}