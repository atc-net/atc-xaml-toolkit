#pragma warning disable CA1507
namespace Atc.XamlToolkit.AvaloniaSample.SampleControls;

public partial class PersonViewModel : ViewModelBase
{
    [ObservableProperty(DependentCommands = [nameof(SaveCommand)])]
    [NotifyPropertyChangedFor(nameof(FullName))]
    [Required]
    [MinLength(2)]
    private string firstName = "John";

    [ObservableProperty(DependentCommands = [nameof(SaveCommand)])]
    [NotifyPropertyChangedFor(nameof(FullName), nameof(Age))]
    [NotifyPropertyChangedFor(nameof(Email))]
    [NotifyPropertyChangedFor(nameof(TheProperty))]
    private string? lastName = "Doe";

    [ObservableProperty(DependentCommands = [nameof(SaveCommand)])]
    private int age = 27;

    [ObservableProperty(DependentCommands = [nameof(SaveCommand)])]
    private string? email;

    [ObservableProperty("TheProperty", DependentProperties = [nameof(FullName), nameof(Age)])]
    private string? myTestProperty;

    public string FullName => $"{FirstName} {LastName}";
}