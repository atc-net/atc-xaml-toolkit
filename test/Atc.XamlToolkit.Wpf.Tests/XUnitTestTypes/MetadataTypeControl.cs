namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

public partial class MetadataTypeControl : System.Windows.Controls.UserControl
{
    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(IsReadOnly = true)]
    private readonly string? summaryText;

    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(
        MetadataType = Atc.XamlToolkit.Controls.Attributes.DependencyPropertyMetadataType.UIPropertyMetadata,
        DefaultValue = 3,
        GeneratePartialHooks = true)]
    private int uiCount;

    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(
        MetadataType = Atc.XamlToolkit.Controls.Attributes.DependencyPropertyMetadataType.UIPropertyMetadata,
        IsAnimationProhibited = true)]
    private double uiFixed;

    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(MetadataType = Atc.XamlToolkit.Controls.Attributes.DependencyPropertyMetadataType.UIPropertyMetadata)]
    private System.Windows.Media.Brush? uiBrush;

    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(
        MetadataType = Atc.XamlToolkit.Controls.Attributes.DependencyPropertyMetadataType.PropertyMetadata,
        DefaultValue = 4,
        GenerateCoerceHook = true)]
    private int plainCount;

    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(
        MetadataType = Atc.XamlToolkit.Controls.Attributes.DependencyPropertyMetadataType.FrameworkPropertyMetadata,
        DefaultValue = 5)]
    private int frameworkCount;

    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(DefaultValue = 6)]
    private int autoCount;

    public int UiCountChanges { get; private set; }

    public void Summarize(string text)
        => SetValue(SummaryTextPropertyKey, text);

    partial void OnUiCountChanged(
        int oldValue,
        int newValue)
        => UiCountChanges++;

    private partial int CoercePlainCount(int value)
        => value < 0 ? 0 : value;
}