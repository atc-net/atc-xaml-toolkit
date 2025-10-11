namespace Atc.XamlToolkit.WpfSample.SourceGeneratorTesting;

public sealed partial class TestAsyncRelayCommandViewModel : ViewModelBase
{
    public static bool CanExecuteProperty => true;

    public static bool CanExecuteMethod() => true;

    public static bool CanExecuteMethodParameterString(string val) => true;

    [RelayCommand]
    public Task AsyncRelayCommandNoParameter()
    {
        throw new NotSupportedException();
    }

    [RelayCommand(AutoSetIsBusy = true)]
    public Task AsyncRelayCommandAutoSetIsBusyNoParameter()
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty))]
    public Task AsyncRelayCommandNoParameterCanExecuteProperty()
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty), AutoSetIsBusy = true)]
    public Task AsyncRelayCommandNoParameterCanExecutePropertyAutoSetIsBusy()
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethod))]
    public Task AsyncRelayCommandNoParameterCanExecuteMethod()
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethod), AutoSetIsBusy = true)]
    public Task AsyncRelayCommandNoParameterCanExecuteMethodAutoSetIsBusy()
    {
        throw new NotSupportedException();
    }

    [RelayCommand]
    public Task AsyncRelayCommandNoParameterCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(AutoSetIsBusy = true)]
    public Task AsyncRelayCommandNoParameterAutoSetIsBusyCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty))]
    public Task AsyncRelayCommandNoParameterCanExecutePropertyCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty), AutoSetIsBusy = true)]
    public Task AsyncRelayCommandNoParameterCanExecutePropertyAutoSetIsBusyCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethod))]
    public Task AsyncRelayCommandNoParameterCanExecuteMethodCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethod), AutoSetIsBusy = true)]
    public Task AsyncRelayCommandNoParameterCanExecuteMethodAutoSetIsBusyCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty))]
    public Task AsyncRelayCommandParameterStringCanExecutePropertyCancellationToken(string val, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty), AutoSetIsBusy = true)]
    public Task AsyncRelayCommandParameterStringCanExecutePropertyAutoSetIsBusyCancellationToken(string val, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethodParameterString))]
    public Task AsyncRelayCommandParameterStringCanExecuteMethodCancellationToken(string val, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethodParameterString), AutoSetIsBusy = true)]
    public Task AsyncRelayCommandParameterStringCanExecuteMethodAutoSetIsBusyCancellationToken(string val, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(ExecuteOnBackgroundThread = true)]
    public Task AsyncRelayCommandExecuteOnBackgroundThreadNoParameter()
    {
        throw new NotSupportedException();
    }

    [RelayCommand(ExecuteOnBackgroundThread = true, AutoSetIsBusy = true)]
    public Task AsyncRelayCommandExecuteOnBackgroundThreadAutoSetIsBusyNoParameter()
    {
        throw new NotSupportedException();
    }

    [RelayCommand(ExecuteOnBackgroundThread = true)]
    public Task AsyncRelayCommandExecuteOnBackgroundThreadNoParameterCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(ExecuteOnBackgroundThread = true, AutoSetIsBusy = true)]
    public Task AsyncRelayCommandExecuteOnBackgroundThreadAutoSetIsBusyNoParameterCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty), ExecuteOnBackgroundThread = true)]
    public Task AsyncRelayCommandExecuteOnBackgroundThreadNoParameterCanExecutePropertyCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty), ExecuteOnBackgroundThread = true, AutoSetIsBusy = true)]
    public Task AsyncRelayCommandExecuteOnBackgroundThreadAutoSetIsBusyNoParameterCanExecutePropertyCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethod), ExecuteOnBackgroundThread = true)]
    public Task AsyncRelayCommandExecuteOnBackgroundThreadNoParameterCanExecuteMethodCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethod), ExecuteOnBackgroundThread = true, AutoSetIsBusy = true)]
    public Task AsyncRelayCommandExecuteOnBackgroundThreadAutoSetIsBusyNoParameterCanExecuteMethodCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty), ExecuteOnBackgroundThread = true)]
    public Task AsyncRelayCommandExecuteOnBackgroundThreadParameterStringCanExecutePropertyCancellationToken(string val, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty), ExecuteOnBackgroundThread = true, AutoSetIsBusy = true)]
    public Task AsyncRelayCommandExecuteOnBackgroundThreadAutoSetIsBusyParameterStringCanExecutePropertyCancellationToken(string val, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethodParameterString), ExecuteOnBackgroundThread = true)]
    public Task AsyncRelayCommandExecuteOnBackgroundThreadParameterStringCanExecuteMethodCancellationToken(string val, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethodParameterString), ExecuteOnBackgroundThread = true, AutoSetIsBusy = true)]
    public Task AsyncRelayCommandExecuteOnBackgroundThreadAutoSetIsBusyParameterStringCanExecuteMethodCancellationToken(string val, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }
}