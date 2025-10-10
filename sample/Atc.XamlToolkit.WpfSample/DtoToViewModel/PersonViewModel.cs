namespace Atc.XamlToolkit.WpfSample.DtoToViewModel;

[ObservableDtoViewModel(typeof(Person))]
public partial class PersonViewModel : ViewModelBase
{
    public PersonViewModel()
    {
        FirstName = "John";
        LastName = "Doe";
        Age = 21;
    }
}