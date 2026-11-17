namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

[Atc.XamlToolkit.Controls.Attributes.UseDependencyPropertyTemplate(
    typeof(TemplatePickerMembers<TemplateDriveInfo?>),
    Exclude = ["Value", "ValueChanged", "Refreshed"])]
public partial class TemplateLabelPicker : System.Windows.Controls.UserControl;