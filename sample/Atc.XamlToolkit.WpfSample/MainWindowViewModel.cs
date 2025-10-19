// ReSharper disable PartialTypeWithSinglePart
namespace Atc.XamlToolkit.WpfSample;

public partial class MainWindowViewModel : MainWindowViewModelBase
{
    [ObservableProperty]
    private List<SampleViewItem> sampleViews = [];

    private SampleViewItem? selectedSampleView;

    [ObservableProperty]
    private object? currentView;

    public SampleViewItem? SelectedSampleView
    {
        get => selectedSampleView;
        set
        {
            if (Set(ref selectedSampleView, value))
            {
                UpdateCurrentView();
            }
        }
    }

    public MainWindowViewModel()
        => InitializeSampleViews();

    private void InitializeSampleViews()
            => SampleViews = BuildSampleViewTree(
                rootNamespace: "SampleControls",
                assembly: typeof(MainWindowViewModel).Assembly);

    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    private static List<SampleViewItem> BuildSampleViewTree(
        string rootNamespace,
        Assembly assembly)
    {
        var anchorMid = "." + rootNamespace + ".";
        var anchorEnd = "." + rootNamespace;

        var views = assembly
            .GetTypes()
            .Where(t =>
                t is { IsClass: true, IsAbstract: false, Namespace: not null } &&
                t.Namespace.Contains(anchorEnd, StringComparison.Ordinal) &&
                t.Name.EndsWith("View", StringComparison.Ordinal))
            .OrderBy(t => t.Namespace, StringComparer.Ordinal)
            .ThenBy(t => t.Name, StringComparer.Ordinal)
            .ToArray();

        var result = new List<SampleViewItem>();
        var nodes = new Dictionary<string, SampleViewItem>(StringComparer.Ordinal);

        foreach (var type in views)
        {
            string remainder;
            var ns = type.Namespace!;
            var idx = ns.IndexOf(anchorMid, StringComparison.Ordinal);
            if (idx >= 0)
            {
                remainder = ns[(idx + anchorMid.Length)..];
            }
            else if (ns.EndsWith(anchorEnd, StringComparison.Ordinal))
            {
                remainder = string.Empty;
            }
            else
            {
                continue;
            }

            var relativeSegments = string.IsNullOrEmpty(remainder)
                ? []
                : remainder.Split('.');

            for (var i = 0; i < relativeSegments.Length; i++)
            {
                var seg = relativeSegments[i];
                var key = string.Join('/', relativeSegments.Take(i + 1));

                if (nodes.TryGetValue(key, out var node))
                {
                    continue;
                }

                var route = $"{rootNamespace}/{key}";
                node = new SampleViewItem(seg, route);
                nodes[key] = node;

                if (i == 0)
                {
                    result.Add(node);
                }
                else
                {
                    var parentKey = string.Join('/', relativeSegments.Take(i));
                    nodes[parentKey].Children.Add(node);
                }
            }

            if (relativeSegments.Length == 0)
            {
                result.Add(new SampleViewItem(type.Name, viewType: type));
            }
            else
            {
                var parentKey = string.Join('/', relativeSegments);
                nodes[parentKey].Children.Add(new SampleViewItem(type.Name, viewType: type));
            }
        }

        return result;
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK")]
    private void UpdateCurrentView()
    {
        if (SelectedSampleView?.ViewType is null)
        {
            CurrentView = null;
            return;
        }

        try
        {
            var newView = Activator.CreateInstance(SelectedSampleView.ViewType);

            // Force clear first to ensure change notification
            CurrentView = null;
            CurrentView = newView;
        }
        catch (Exception)
        {
            CurrentView = null;
        }
    }
}