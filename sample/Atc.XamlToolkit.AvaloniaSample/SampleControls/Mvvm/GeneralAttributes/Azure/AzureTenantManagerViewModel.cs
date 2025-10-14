namespace Atc.XamlToolkit.AvaloniaSample.SampleControls.Mvvm.GeneralAttributes.Azure;

public sealed partial class AzureTenantManagerViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool isSignedIn;

    [ObservableProperty(
        AfterChangedCallback = nameof(UpdateIsSignedIn))]
    private string? signedInUser;

    [ObservableProperty(
        DependentPropertyNames = [nameof(CurrentEnvironmentType)],
        AfterChangedCallback = nameof(SetAuthServiceEnvironmentType))]
    private string selectedEnvironmentKey = nameof(AppEnvironmentType.Local);

    [ObservableProperty]
    private string defaultAzureAccount = string.Empty;

    public AppEnvironmentType CurrentEnvironmentType
        => Enum.Parse<AppEnvironmentType>(selectedEnvironmentKey);

    private bool CanLogin() => !IsSignedIn && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task Login(
        CancellationToken cancellationToken)
    {
        IsBusy = true;

        // Simulate backend call
        await Task
            .Delay(2_000, cancellationToken)
            .ConfigureAwait(false);

        IsBusy = false;
    }

    private bool CanLogout() => IsSignedIn && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanLogout))]
    private async Task Logout(
        CancellationToken cancellationToken)
    {
        IsBusy = true;

        // Simulate backend call
        await Task
            .Delay(500, cancellationToken)
            .ConfigureAwait(false);

        IsBusy = false;
    }

    private static void SetAuthServiceEnvironmentType()
    {
        // Simulate action
    }

    private void UpdateIsSignedIn()
        => IsSignedIn = SignedInUser is not null;
}