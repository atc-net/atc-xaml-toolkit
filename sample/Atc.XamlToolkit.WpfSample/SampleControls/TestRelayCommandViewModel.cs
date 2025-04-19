namespace Atc.XamlToolkit.WpfSample.SampleControls;

public sealed partial class TestRelayCommandViewModel : ViewModelBase
{
    public static bool CanExecuteProperty => true;

    public static bool CanExecuteMethod() => true;

    public static bool CanExecuteMethodParameterString(string val) => true;

    [RelayCommand]
    public void RelayCommandNoParameter()
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty))]
    public void RelayCommandNoParameterCanExecuteProperty()
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethod))]
    public void RelayCommandNoParameterCanExecuteMethod()
    {
        throw new NotSupportedException();
    }

    [RelayCommand]
    public void RelayCommandNoParameterCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty))]
    public void RelayCommandNoParameterCanExecutePropertyCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethod))]
    public void RelayCommandNoParameterCanExecuteMethodCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty))]
    public void RelayCommandParameterStringCanExecutePropertyCancellationToken(string val, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethodParameterString))]
    public void RelayCommandParameterStringCanExecuteMethodCancellationToken(string val, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(ExecuteOnBackgroundThread = true)]
    public void RelayCommandExecuteOnBackgroundThreadNoParameter()
    {
        throw new NotSupportedException();
    }

    [RelayCommand(ExecuteOnBackgroundThread = true)]
    public void RelayCommandExecuteOnBackgroundThreadNoParameterCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty), ExecuteOnBackgroundThread = true)]
    public void RelayCommandExecuteOnBackgroundThreadNoParameterCanExecuteProperty()
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethod), ExecuteOnBackgroundThread = true)]
    public void RelayCommandExecuteOnBackgroundThreadNoParameterCanExecuteMethod()
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty), ExecuteOnBackgroundThread = true)]
    public void RelayCommandExecuteOnBackgroundThreadNoParameterCanExecutePropertyCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethod), ExecuteOnBackgroundThread = true)]
    public void RelayCommandExecuteOnBackgroundThreadNoParameterCanExecuteMethodCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty), ExecuteOnBackgroundThread = true)]
    public void RelayCommandExecuteOnBackgroundThreadParameterStringCanExecutePropertyCancellationToken(string val, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethodParameterString), ExecuteOnBackgroundThread = true)]
    public void RelayCommandExecuteOnBackgroundThreadParameterStringCanExecuteMethodCancellationToken(string val, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }
}