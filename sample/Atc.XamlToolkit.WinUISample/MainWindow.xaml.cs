// ReSharper disable RedundantExtendsListEntry
namespace Atc.XamlToolkit.WinUISample;

public sealed partial class MainWindow : Window
{
    private MainWindowViewModel GetViewModel() => (MainWindowViewModel)((FrameworkElement)Content).DataContext!;

    public MainWindow()
    {
        InitializeComponent();

        ((FrameworkElement)Content).DataContext = new MainWindowViewModel();

        var content = (FrameworkElement)Content;

        content.Loaded += OnLoaded;
        Closed += OnClosed;
        content.PreviewKeyDown += OnPreviewKeyDown;
        content.KeyDown += OnKeyDown;
        content.KeyUp += OnKeyUp;
    }

    public MainWindow(
        MainWindowViewModel viewModel)
        : this() =>
        ((FrameworkElement)Content).DataContext = viewModel;

    private void OnLoaded(
        object sender,
        RoutedEventArgs e)
    {
        // Pass 'this' (the Window) instead of sender
        GetViewModel().OnLoaded(this, e);

        // Ensure the content can receive keyboard input
        if (Content is FrameworkElement content)
        {
            content.Focus(FocusState.Programmatic);
        }
    }

    private void OnClosed(
        object sender,
        WindowEventArgs e)
        => GetViewModel().OnClosing(sender, new CancelEventArgs());

    private void OnPreviewKeyDown(
        object sender,
        KeyRoutedEventArgs e)
    {
        // Handle F11 in preview to ensure it's caught before other controls
        if (e.Key == Windows.System.VirtualKey.F11)
        {
            // Pass 'this' (the Window) instead of sender
            GetViewModel().OnKeyDown(this, e);
        }
    }

    private void OnKeyDown(
        object sender,
        KeyRoutedEventArgs e)
        => GetViewModel().OnKeyDown(this, e);

    private void OnKeyUp(
        object sender,
        KeyRoutedEventArgs e)
        => GetViewModel().OnKeyUp(sender, e);
}