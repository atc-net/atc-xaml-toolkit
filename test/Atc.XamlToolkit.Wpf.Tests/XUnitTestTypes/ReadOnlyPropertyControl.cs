namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

public partial class ReadOnlyPropertyControl : System.Windows.Controls.UserControl
{
    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(IsReadOnly = true)]
    private string? summaryText;

    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(IsReadOnly = true, DefaultValue = 3, ValidateValueCallback = nameof(IsNotNegative))]
    private int selectedCount;

    public void Select(
        int count,
        string summary)
    {
        SelectedCount = count;
        SummaryText = summary;
    }

    private static bool IsNotNegative(object value)
        => value is int and >= 0;
}