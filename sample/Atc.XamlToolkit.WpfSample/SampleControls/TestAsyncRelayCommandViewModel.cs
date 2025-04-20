namespace Atc.XamlToolkit.WpfSample.SampleControls;

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

    [RelayCommand(CanExecute = nameof(CanExecuteProperty))]
    public Task AsyncRelayCommandNoParameterCanExecuteProperty()
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethod))]
    public Task AsyncRelayCommandNoParameterCanExecuteMethod()
    {
        throw new NotSupportedException();
    }

    [RelayCommand]
    public Task AsyncRelayCommandNoParameterCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty))]
    public Task AsyncRelayCommandNoParameterCanExecutePropertyCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethod))]
    public Task AsyncRelayCommandNoParameterCanExecuteMethodCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty))]
    public Task AsyncRelayCommandParameterStringCanExecutePropertyCancellationToken(string val, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethodParameterString))]
    public Task AsyncRelayCommandParameterStringCanExecuteMethodCancellationToken(string val, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(ExecuteOnBackgroundThread = true)]
    public Task AsyncRelayCommandExecuteOnBackgroundThreadNoParameter()
    {
        throw new NotSupportedException();
    }

    [RelayCommand(ExecuteOnBackgroundThread = true)]
    public Task AsyncRelayCommandExecuteOnBackgroundThreadNoParameterCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty), ExecuteOnBackgroundThread = true)]
    public Task AsyncRelayCommandExecuteOnBackgroundThreadNoParameterCanExecutePropertyCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethod), ExecuteOnBackgroundThread = true)]
    public Task AsyncRelayCommandExecuteOnBackgroundThreadNoParameterCanExecuteMethodCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty), ExecuteOnBackgroundThread = true)]
    public Task AsyncRelayCommandExecuteOnBackgroundThreadParameterStringCanExecutePropertyCancellationToken(string val, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethodParameterString), ExecuteOnBackgroundThread = true)]
    public Task AsyncRelayCommandExecuteOnBackgroundThreadParameterStringCanExecuteMethodCancellationToken(string val, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }
}