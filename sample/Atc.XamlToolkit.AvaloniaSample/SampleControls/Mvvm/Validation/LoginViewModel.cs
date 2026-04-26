namespace Atc.XamlToolkit.AvaloniaSample.SampleControls.Mvvm.Validation;

/// <summary>
/// Demonstrates the canonical ViewModelBase + <c>InitializeValidation()</c> pattern.
/// </summary>
/// <remarks>
/// <para>
/// This is the direct MVVM-validation path — distinct from <c>[ObservableDtoViewModel]</c>
/// which wraps an existing DTO. The view-model declares its own properties via
/// <c>[ObservableProperty]</c>, places <c>System.ComponentModel.DataAnnotations</c> attributes
/// on the camelCase backing fields, and calls <c>InitializeValidation(validateOnPropertyChanged: true)</c>
/// in its constructor so every keystroke re-runs validation.
/// </para>
/// <para>
/// Avalonia honours <c>INotifyDataErrorInfo</c> natively — TextBoxes bound to a property
/// with <c>DataValidationErrors</c> show the error inline. The view-model also exposes
/// per-field <c>*Error</c> properties so the view can show a friendlier first-error message.
/// </para>
/// </remarks>
public partial class LoginViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    [Required(ErrorMessage = "Username is required")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
    private string username = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    private string email = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    private string password = string.Empty;

    [ObservableProperty]
    private string? lastSubmittedUsername;

    public LoginViewModel()
    {
        InitializeValidation(validateOnPropertyChanged: true);
        PropertyChanged += OnPropertyChangedToError;
    }

    public string UsernameError
        => GetErrors(nameof(Username)).Cast<string>().FirstOrDefault() ?? string.Empty;

    public string EmailError
        => GetErrors(nameof(Email)).Cast<string>().FirstOrDefault() ?? string.Empty;

    public string PasswordError
        => GetErrors(nameof(Password)).Cast<string>().FirstOrDefault() ?? string.Empty;

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private void Submit()
    {
        LastSubmittedUsername = Username;
    }

    [RelayCommand]
    private void Reset()
    {
        Username = string.Empty;
        Email = string.Empty;
        Password = string.Empty;
        LastSubmittedUsername = null;
    }

    private bool CanSubmit()
        => !HasErrors
           && !string.IsNullOrEmpty(Username)
           && !string.IsNullOrEmpty(Email)
           && !string.IsNullOrEmpty(Password);

    private void OnPropertyChangedToError(
        object? sender,
        PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Username):
                OnPropertyChanged(nameof(UsernameError));
                break;
            case nameof(Email):
                OnPropertyChanged(nameof(EmailError));
                break;
            case nameof(Password):
                OnPropertyChanged(nameof(PasswordError));
                break;
        }
    }
}