namespace Atc.XamlToolkit.WinUISample.SampleControls.Mvvm.Validation;

public sealed partial class LoginView : UserControl
{
    public LoginViewModel ViewModel { get; }

    public LoginView()
    {
        ViewModel = new LoginViewModel();
        InitializeComponent();
    }
}