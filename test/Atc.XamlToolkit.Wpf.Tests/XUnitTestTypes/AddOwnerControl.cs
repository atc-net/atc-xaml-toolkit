namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

public partial class AddOwnerControl : System.Windows.Controls.UserControl
{
    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(AddOwnerOf = typeof(System.Windows.Controls.ComboBox))]
    private double maxDropDownHeight;

    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(AddOwnerOf = typeof(System.Windows.Controls.TextBox), GeneratePartialHooks = true)]
    private System.Windows.TextAlignment textAlignment;

    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(
        AddOwnerOf = typeof(System.Windows.Controls.Primitives.TextBoxBase),
        DefaultValue = true,
        Flags = System.Windows.FrameworkPropertyMetadataOptions.Inherits,
        PropertyChangedCallback = nameof(OnIsReadOnlyPropertyChanged))]
    private bool isReadOnly;

    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(AddOwnerOf = typeof(System.Windows.Controls.Primitives.RangeBase), DefaultValue = 5d)]
    private double maximum;

    public System.Collections.ObjectModel.Collection<string> Changes { get; } = [];

    partial void OnTextAlignmentChanged(
        System.Windows.TextAlignment oldValue,
        System.Windows.TextAlignment newValue)
        => Changes.Add($"TextAlignment:{oldValue}->{newValue}");

    private static void OnIsReadOnlyPropertyChanged(
        System.Windows.DependencyObject d,
        System.Windows.DependencyPropertyChangedEventArgs e)
        => ((AddOwnerControl)d).Changes.Add($"IsReadOnly:{e.OldValue}->{e.NewValue}");
}