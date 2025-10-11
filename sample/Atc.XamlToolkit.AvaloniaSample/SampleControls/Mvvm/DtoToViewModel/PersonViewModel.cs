namespace Atc.XamlToolkit.AvaloniaSample.SampleControls.Mvvm.DtoToViewModel;

[ObservableDtoViewModel(typeof(Person))]
public partial class PersonViewModel : ViewModelBase
{
    public PersonViewModel()
    {
        dto = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 21,
        };
    }
}