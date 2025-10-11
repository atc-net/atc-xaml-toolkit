namespace Atc.XamlToolkit.WinUISample.SampleControls.Mvvm.GeneralAttributes.Azure;

public sealed partial class AzureTenantManagerView : UserControl
{
    public AzureTenantManagerViewModel ViewModel { get; }

    public AzureTenantManagerView()
    {
        ViewModel = new AzureTenantManagerViewModel();
        InitializeComponent();
    }
}