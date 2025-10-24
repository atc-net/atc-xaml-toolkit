namespace Atc.XamlToolkit.WpfSample.SampleControls.FrameworkElements.DependencyPropertyComponents;

/// <summary>
/// Example custom control using class-level [DependencyProperty&lt;T&gt;] attributes.
/// Demonstrates explicit property declarations with types and default values.
/// </summary>
[DependencyProperty<string>("Text", DefaultValue = "")]
[DependencyProperty<string>("TextColor", DefaultValue = "Black")]
[DependencyProperty<string>("Icon", DefaultValue = "●")]
[DependencyProperty<string>("IconColor", DefaultValue = "Gray")]
[DependencyProperty<double>("FontSize", DefaultValue = 14.0)]
public partial class StyledLabelControl
{
    public StyledLabelControl()
    {
        InitializeComponent();
    }
}