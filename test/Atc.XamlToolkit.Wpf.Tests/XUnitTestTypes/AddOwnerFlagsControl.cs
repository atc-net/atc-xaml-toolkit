namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

public partial class AddOwnerFlagsControl : System.Windows.Controls.UserControl
{
    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(
        AddOwnerOf = typeof(System.Windows.Controls.ComboBox),
        Flags = System.Windows.FrameworkPropertyMetadataOptions.AffectsMeasure)]
    private double maxDropDownHeight;
}