namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

[Atc.XamlToolkit.Controls.Attributes.UseDependencyPropertyTemplate(typeof(TemplatePickerMembers<int>))]
public partial class TemplateNumberPicker : System.Windows.Controls.UserControl
{
    partial void OnValueChanged(
        int oldValue,
        int newValue)
        => RaiseValueChanged(oldValue, newValue);
}