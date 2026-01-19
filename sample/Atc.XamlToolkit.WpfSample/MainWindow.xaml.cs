namespace Atc.XamlToolkit.WpfSample;

public partial class MainWindow
{
    private readonly MainWindowViewModel viewModel;

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();

        this.viewModel = viewModel;
        DataContext = viewModel;

        Loaded += OnLoaded;
        Closing += OnClosing;
        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;
    }

    private void OnLoaded(
        object sender,
        RoutedEventArgs e)
        => viewModel.OnLoaded(this, e);

    private void OnClosing(
        object? sender,
        CancelEventArgs e)
        => viewModel.OnClosing(this, e);

    private void OnKeyDown(
        object sender,
        KeyEventArgs e)
        => viewModel.OnKeyDown(this, e);

    private void OnKeyUp(
        object sender,
        KeyEventArgs e)
        => viewModel.OnKeyUp(this, e);

    private void SampleTreeViewOnSelectedItemChanged(
        object sender,
        RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is SampleViewItem item)
        {
            viewModel.SelectedSampleView = item;
        }
    }
}