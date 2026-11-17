namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

[Atc.XamlToolkit.Controls.Attributes.UseDependencyPropertyTemplate(typeof(TemplatePickerMembers<TemplateDriveInfo?>))]
public partial class TemplateDrivePicker : System.Windows.Controls.UserControl
{
    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty]
    private int driveCount;

    public void Refresh()
        => RaiseRefreshed();

    partial void OnValueChanged(
        TemplateDriveInfo? oldValue,
        TemplateDriveInfo? newValue)
        => RaiseValueChanged(oldValue, newValue);
}