namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

public partial class PartialHooksControl : System.Windows.Controls.UserControl
{
    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(GeneratePartialHooks = true)]
    private int count;

    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(GeneratePartialHooks = true, PropertyChangedCallback = nameof(OnTitleChangedStatic))]
    private string? title;

    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(GeneratePartialHooks = true)]
    private bool isActive;

    public System.Collections.ObjectModel.Collection<string> Calls { get; } = [];

    partial void OnCountChanged(
        int oldValue,
        int newValue)
        => Calls.Add($"Count {oldValue}->{newValue}");

    partial void OnTitleChanged(
        string? oldValue,
        string? newValue)
        => Calls.Add($"Title {oldValue ?? "null"}->{newValue ?? "null"}");

    private static void OnTitleChangedStatic(
        System.Windows.DependencyObject d,
        System.Windows.DependencyPropertyChangedEventArgs e)
        => ((PartialHooksControl)d).Calls.Add("Title static callback");
}