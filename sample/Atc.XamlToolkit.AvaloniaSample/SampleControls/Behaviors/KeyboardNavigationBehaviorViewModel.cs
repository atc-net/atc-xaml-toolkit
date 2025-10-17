namespace Atc.XamlToolkit.AvaloniaSample.SampleControls.Behaviors;

public partial class KeyboardNavigationBehaviorViewModel : ViewModelBase
{
    [ObservableProperty]
    private string navigationLog = string.Empty;

    [ObservableProperty]
    private int currentRow;

    [ObservableProperty]
    private int currentColumn;

    [RelayCommand]
    private void NavigateUp()
    {
        if (CurrentRow > 0)
        {
            CurrentRow--;
        }

        AddLog("Up");
    }

    [RelayCommand]
    private void NavigateDown()
    {
        if (CurrentRow < 2)
        {
            CurrentRow++;
        }

        AddLog("Down");
    }

    [RelayCommand]
    private void NavigateLeft()
    {
        if (CurrentColumn > 0)
        {
            CurrentColumn--;
        }

        AddLog("Left");
    }

    [RelayCommand]
    private void NavigateRight()
    {
        if (CurrentColumn < 2)
        {
            CurrentColumn++;
        }

        AddLog("Right");
    }

    [RelayCommand]
    private void HandleEnter()
    {
        AddLog($"Enter pressed at ({CurrentRow}, {CurrentColumn})");
    }

    [RelayCommand]
    private void HandleEscape()
    {
        CurrentRow = 0;
        CurrentColumn = 0;
        AddLog("Escape - Reset to (0, 0)");
    }

    private void AddLog(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss", GlobalizationConstants.EnglishCultureInfo);
        NavigationLog = $"[{timestamp}] {message}\n{NavigationLog}";

        // Keep only last 10 lines
        var lines = NavigationLog.Split('\n');
        if (lines.Length > 10)
        {
            NavigationLog = string.Join("\n", lines.Take(10));
        }
    }
}