namespace Atc.XamlToolkit.WpfSample;

public partial class MainWindow
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }

    private void SampleTreeViewOnSelectedItemChanged(
        object sender,
        RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is SampleViewItem item &&
            DataContext is MainWindowViewModel viewModel)
        {
            viewModel.SelectedSampleView = item;
        }
    }
}