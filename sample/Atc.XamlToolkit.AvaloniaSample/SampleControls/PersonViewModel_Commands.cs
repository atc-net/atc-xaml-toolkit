// ReSharper disable ConvertIfStatementToReturnStatement
namespace Atc.XamlToolkit.AvaloniaSample.SampleControls;

public partial class PersonViewModel
{
    [RelayCommand]
    public Task ShowData()
    {
        var sb = new StringBuilder();
        sb.Append("FirstName: ");
        sb.AppendLine(FirstName);
        sb.Append("LastName: ");
        sb.AppendLine(LastName);
        sb.Append("Age: ");
        sb.AppendLine(Age.ToString(GlobalizationConstants.EnglishCultureInfo));
        sb.Append("Email: ");
        sb.AppendLine(Email);
        sb.Append("TheProperty: ");
        sb.AppendLine(TheProperty);

        var box = MessageBoxManager.GetMessageBoxStandard(
            "Show-Data",
            sb.ToString());

        return box.ShowAsync();
    }

    [RelayCommand(CanExecute = nameof(CanSaveHandler))]
    public Task SaveHandler()
    {
        var box = MessageBoxManager.GetMessageBoxStandard(
            "Save-Data",
            "Hello from SaveHandler method");

        return box.ShowAsync();
    }

    public bool CanSaveHandler()
    {
        if (string.IsNullOrWhiteSpace(FirstName))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(LastName))
        {
            return false;
        }

        if (Age is < 18 or > 120)
        {
            return false;
        }

        if (Email is not null && !Email.IsEmailAddress())
        {
            return false;
        }

        return true;
    }
}