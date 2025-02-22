namespace Atc.XamlToolkit.WpfSample;

public partial class MainWindow
{
    public MainWindow(
        MainWindowViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}