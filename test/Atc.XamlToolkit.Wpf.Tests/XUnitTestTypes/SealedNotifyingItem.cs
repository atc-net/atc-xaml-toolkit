namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

/// <summary>
/// A sealed class that gets its change notification from the generator.
/// </summary>
[Atc.XamlToolkit.Mvvm.INotifyPropertyChanged]
public sealed partial class SealedNotifyingItem
{
    [Atc.XamlToolkit.Mvvm.ObservableProperty]
    private bool isChecked;

    private string label = string.Empty;

    public string Label
    {
        get => label;
        set => Set(ref label, value);
    }
}