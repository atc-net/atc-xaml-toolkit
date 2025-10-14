namespace Atc.XamlToolkit.AvaloniaSample.SampleControls.Mvvm.DtoToViewModel;

[ObservableDtoViewModel(
    typeof(Person),
    EnableValidationOnPropertyChanged = true,
    EnableValidationOnInit = false,
    UseIsDirty = true)]
public partial class PersonViewModel : ViewModelBase
{
    public PersonViewModel()
        : this(
            new Person
            {
                FirstName = "John",
                LastName = "Doe",
                Age = 21,
            })
    {
    }
}