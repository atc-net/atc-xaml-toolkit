namespace Atc.XamlToolkit.WinUISample.SampleControls.Mvvm.GeneralAttributes.Person;

public sealed partial class PersonView : UserControl
{
    public PersonViewModel ViewModel { get; }

    public PersonView()
    {
        ViewModel = new PersonViewModel();
        InitializeComponent();
    }
}