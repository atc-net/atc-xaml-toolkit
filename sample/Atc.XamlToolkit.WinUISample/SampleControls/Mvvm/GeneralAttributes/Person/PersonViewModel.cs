#pragma warning disable CA1507
namespace Atc.XamlToolkit.WinUISample.SampleControls.Mvvm.GeneralAttributes.Person;

public partial class PersonViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))]
    [Required]
    [MinLength(2)]
    private string firstName = "John";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName), nameof(Age))]
    [NotifyPropertyChangedFor(nameof(Email))]
    [NotifyPropertyChangedFor(nameof(TheProperty))]
    private string? lastName = "Doe";

    [ObservableProperty]
    private int age = 27;

    [ObservableProperty]
    private string? email;

    [ObservableProperty("TheProperty", DependentPropertyNames = [nameof(FullName), nameof(Age)])]
    private string? myTestProperty;

    public string FullName => $"{FirstName} {LastName}";
}