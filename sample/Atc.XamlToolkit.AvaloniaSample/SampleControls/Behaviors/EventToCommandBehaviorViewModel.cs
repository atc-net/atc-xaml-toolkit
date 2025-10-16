namespace Atc.XamlToolkit.AvaloniaSample.SampleControls.Behaviors;

public partial class EventToCommandBehaviorViewModel : ViewModelBase
{
    [ObservableProperty]
    private int clickCount;

    [ObservableProperty]
    private string lastSelection = "None";

    [ObservableProperty]
    private int textChangeCount;

    [ObservableProperty]
    private string lastParameter = "None";

    [ObservableProperty]
    private string mouseState = "Outside";

    [ObservableProperty]
    private IBrush borderBackground = Brushes.LightGray;

    public ObservableCollection<string> Items { get; } =
    [
        "Item 1",
        "Item 2",
        "Item 3",
        "Item 4",
        "Item 5"
    ];

    [RelayCommand]
    private void ButtonClick()
    {
        ClickCount++;
    }

    [RelayCommand]
    private void SelectionChanged(SelectionChangedEventArgs args)
    {
        if (args.AddedItems.Count > 0)
        {
            LastSelection = args.AddedItems[0]?.ToString() ?? "Unknown";
        }
        else
        {
            LastSelection = "None";
        }
    }

    [RelayCommand]
    private void TextChanged(object? parameter)
    {
        TextChangeCount++;
        LastParameter = parameter?.ToString() ?? "null";
    }

    [RelayCommand]
    private void MouseEnter()
    {
        MouseState = "Inside";
        BorderBackground = Brushes.LightBlue;
    }

    [RelayCommand]
    private void MouseLeave()
    {
        MouseState = "Outside";
        BorderBackground = Brushes.LightGray;
    }
}