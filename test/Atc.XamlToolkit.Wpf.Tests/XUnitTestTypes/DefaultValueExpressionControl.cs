namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

public partial class DefaultValueExpressionControl : System.Windows.Controls.UserControl
{
    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(
        DefaultValueExpression = "CreateFrozenBrush(40)",
        Flags = System.Windows.FrameworkPropertyMetadataOptions.AffectsRender)]
    private System.Windows.Media.Brush? gridBrush;

    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(
        DefaultValueExpression = "System.Windows.Media.Brushes.Black.Color.ToString(System.Globalization.CultureInfo.InvariantCulture)")]
    private string? colorText;

    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(DefaultValueExpression = "System.Array.Empty<object>()")]
    private System.Collections.IEnumerable? items;

    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(DefaultValue = 3, Flags = System.Windows.FrameworkPropertyMetadataOptions.None)]
    private int count;

    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(DefaultValue = 3)]
    private int step;

    private static System.Windows.Media.SolidColorBrush CreateFrozenBrush(
        byte gray)
    {
        var brush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(gray, gray, gray));
        brush.Freeze();
        return brush;
    }
}