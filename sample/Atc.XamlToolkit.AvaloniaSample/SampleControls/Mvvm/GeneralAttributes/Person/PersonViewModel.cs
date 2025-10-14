#pragma warning disable CA1507
namespace Atc.XamlToolkit.AvaloniaSample.SampleControls.Mvvm.GeneralAttributes.Person;

public partial class PersonViewModel : ViewModelBase
{
    [ObservableProperty(DependentCommandNames = [nameof(SaveCommand)])]
    [NotifyPropertyChangedFor(nameof(FullName))]
    [Required]
    [MinLength(2)]
    private string firstName = "John";

    [ObservableProperty(DependentCommandNames = [nameof(SaveCommand)])]
    [NotifyPropertyChangedFor(nameof(FullName), nameof(Age))]
    [NotifyPropertyChangedFor(nameof(Email))]
    [NotifyPropertyChangedFor(nameof(TheProperty))]
    private string? lastName = "Doe";

    [ObservableProperty(DependentCommandNames = [nameof(SaveCommand)])]
    private int age = 27;

    [ObservableProperty(DependentCommandNames = [nameof(SaveCommand)])]
    private string? email;

    [ObservableProperty("TheProperty", DependentPropertyNames = [nameof(FullName), nameof(Age)])]
    private string? myTestProperty;

    public string FullName => $"{FirstName} {LastName}";
}