// ReSharper disable RedundantExtendsListEntry
namespace Atc.XamlToolkit.WinUISample;

public sealed partial class MainWindow : Window
{
    private MainWindowViewModel GetViewModel() => (MainWindowViewModel)((FrameworkElement)Content).DataContext!;

    public MainWindowViewModel ViewModel { get; }

    public MainWindow()
    {
        ViewModel = new MainWindowViewModel();

        InitializeComponent();

        ((FrameworkElement)Content).DataContext = ViewModel;

        var content = (FrameworkElement)Content;

        content.Loaded += OnLoaded;
        Closed += OnClosed;
        content.PreviewKeyDown += OnPreviewKeyDown;
        content.KeyDown += OnKeyDown;
        content.KeyUp += OnKeyUp;

        PopulateTreeView();
    }

    public MainWindow(
        MainWindowViewModel viewModel)
        : this()
    {
        ViewModel = viewModel;
        ((FrameworkElement)Content).DataContext = viewModel;
    }

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

    private void SampleTreeViewOnSelectionChanged(
        TreeView sender,
        TreeViewSelectionChangedEventArgs args)
    {
        if (sender.SelectedNode is not { Content: string } node)
        {
            return;
        }

        // Find the SampleViewItem by traversing the tree
        var item = FindSampleViewItemByNode(node);
        if (item is null)
        {
            return;
        }

        ViewModel.SelectedSampleView = item;

        // Update UI elements manually
        TitleTextBlock.Text = item.Name;
        CurrentViewTextBlock.Text = ViewModel.CurrentView?.ToString() ?? string.Empty;
    }

    private void PopulateTreeView()
    {
        SampleTreeView.RootNodes.Clear();

        foreach (var item in ViewModel.SampleViews)
        {
            var node = CreateTreeViewNode(item);
            SampleTreeView.RootNodes.Add(node);
        }
    }

    private static TreeViewNode CreateTreeViewNode(
        SampleViewItem item)
    {
        var node = new TreeViewNode
        {
            Content = item.Name,
            IsExpanded = true,
        };

        foreach (var child in item.Children)
        {
            node.Children.Add(CreateTreeViewNode(child));
        }

        return node;
    }

    private SampleViewItem? FindSampleViewItemByNode(
        TreeViewNode node)
    {
        // Build path from root to this node
        var path = new List<string>();
        var current = node;
        while (current is not null)
        {
            if (current.Content is string name)
            {
                path.Insert(0, name);
            }

            current = current.Parent;
        }

        // Navigate through SampleViews to find the item
        SampleViewItem? result = null;
        var items = ViewModel.SampleViews.AsEnumerable();

        foreach (var segment in path)
        {
            var found = items.FirstOrDefault(x => x.Name == segment);
            if (found is null)
            {
                return null;
            }

            result = found;
            items = found.Children;
        }

        return result;
    }
}