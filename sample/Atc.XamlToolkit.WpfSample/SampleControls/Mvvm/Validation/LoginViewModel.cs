namespace Atc.XamlToolkit.WpfSample.SampleControls.Mvvm.Validation;

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
/// The <c>SubmitCommand.CanExecute</c> reads <see cref="ObservableValidator.HasErrors"/>. Each field
/// declares <c>[NotifyCanExecuteChangedFor(nameof(SubmitCommand))]</c> so the command
/// re-evaluates after each property change — by which point the
/// <c>validateOnPropertyChanged</c> handler has already updated the error state.
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
    }

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
}