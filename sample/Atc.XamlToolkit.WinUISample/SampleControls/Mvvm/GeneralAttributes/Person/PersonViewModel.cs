#pragma warning disable CA1507
namespace Atc.XamlToolkit.WinUISample.SampleControls.Mvvm.GeneralAttributes.Person;

public partial class PersonViewModel : ViewModelBase
{
    public PersonViewModel()
    {
        InitializeValidation(
            validateOnPropertyChanged: true,
            validateAllPropertiesOnInit: false);
    }

    [ObservableProperty]
    [Required(ErrorMessage = "First name is required")]
    [MinLength(2, ErrorMessage = "First name must be at least 2 characters long")]
    [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    private string? firstName;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Email), nameof(Age))]
    [NotifyPropertyChangedFor(nameof(TheProperty))]
    [Required(ErrorMessage = "Last name is required")]
    [MinLength(2, ErrorMessage = "Last name must be at least 2 characters long")]
    [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    private string? lastName;

    [ObservableProperty]
    [Required(ErrorMessage = "Age is required")]
    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
    private int? age;

    [ObservableProperty]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    private string? email;

    [ObservableProperty("TheProperty", DependentPropertyNames = [nameof(FullName), nameof(Age)])]
    [MaxLength(100, ErrorMessage = "The property cannot exceed 100 characters")]
    private string? myTestProperty;

    [ComputedProperty]
    public string FullName => $"{FirstName} {LastName}";

    // Error message properties for UI binding
    public string FirstNameError
        => GetErrors(nameof(FirstName)).Cast<string>().FirstOrDefault() ?? string.Empty;

    public string LastNameError
        => GetErrors(nameof(LastName)).Cast<string>().FirstOrDefault() ?? string.Empty;

    public string AgeError
        => GetErrors(nameof(Age)).Cast<string>().FirstOrDefault() ?? string.Empty;

    public string EmailError
        => GetErrors(nameof(Email)).Cast<string>().FirstOrDefault() ?? string.Empty;

    public string ThePropertyError
        => GetErrors(nameof(TheProperty)).Cast<string>().FirstOrDefault() ?? string.Empty;
}