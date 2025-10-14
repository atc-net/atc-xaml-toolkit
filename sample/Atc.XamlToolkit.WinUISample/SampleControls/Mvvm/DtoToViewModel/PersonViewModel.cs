namespace Atc.XamlToolkit.WinUISample.SampleControls.Mvvm.DtoToViewModel;

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
        PropertyChanged += OnPropertyChangedToError;
    }

    // Error message properties for UI binding
    public string FirstNameError => GetErrors(nameof(FirstName)).Cast<string>().FirstOrDefault() ?? string.Empty;

    public string LastNameError => GetErrors(nameof(LastName)).Cast<string>().FirstOrDefault() ?? string.Empty;

    public string AgeError => GetErrors(nameof(Age)).Cast<string>().FirstOrDefault() ?? string.Empty;

    private void OnPropertyChangedToError(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(FirstName):
                OnPropertyChanged(nameof(FirstNameError));
                break;
            case nameof(LastName):
                OnPropertyChanged(nameof(LastNameError));
                break;
            case nameof(Age):
                OnPropertyChanged(nameof(AgeError));
                break;
        }
    }
}