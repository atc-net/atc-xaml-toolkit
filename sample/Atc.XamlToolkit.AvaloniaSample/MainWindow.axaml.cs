namespace Atc.XamlToolkit.AvaloniaSample;

public partial class MainWindow : Window
{
    private MainWindowViewModel GetViewModel() => (MainWindowViewModel)DataContext!;

    public MainWindow()
    {
        InitializeComponent();

        DataContext = new MainWindowViewModel();
    }

    public MainWindow(MainWindowViewModel viewModel)
        : this()
    {
        DataContext = viewModel;

        Loaded += OnLoaded;
        Closing += OnClosing;
        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;
    }

    private void OnLoaded(
        object? sender,
        RoutedEventArgs e)
        => GetViewModel().OnLoaded(this, e);

    private void OnClosing(
        object? sender,
        CancelEventArgs e)
        => GetViewModel().OnClosing(this, e);

    private void OnKeyDown(
        object? sender,
        KeyEventArgs e)
        => GetViewModel().OnKeyDown(this, e);

    private void OnKeyUp(
        object? sender,
        KeyEventArgs e)
        => GetViewModel().OnKeyUp(this, e);
}