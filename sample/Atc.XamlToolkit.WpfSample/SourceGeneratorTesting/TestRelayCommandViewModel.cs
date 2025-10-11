namespace Atc.XamlToolkit.WpfSample.SourceGeneratorTesting;

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

    [RelayCommand(AutoSetIsBusy = true)]
    public void RelayCommandAutoSetIsBusyNoParameter()
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty))]
    public void RelayCommandCanExecutePropertyNoParameter()
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty), AutoSetIsBusy = true)]
    public void RelayCommandCanExecutePropertyAutoSetIsBusyNoParameter()
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethod))]
    public void RelayCommandCanExecuteMethodNoParameter()
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethod), AutoSetIsBusy = true)]
    public void RelayCommandCanExecuteMethodAutoSetIsBusyNoParameter()
    {
        throw new NotSupportedException();
    }

    [RelayCommand]
    public void RelayCommandNoParameterCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(AutoSetIsBusy = true)]
    public void RelayCommandAutoSetIsBusyNoParameterCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty))]
    public void RelayCommandCanExecutePropertyNoParameterCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty), AutoSetIsBusy = true)]
    public void RelayCommandCanExecutePropertyAutoSetIsBusyNoParameterCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethod))]
    public void RelayCommandCanExecuteMethodNoParameterCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethod), AutoSetIsBusy = true)]
    public void RelayCommandCanExecuteMethodAutoSetIsBusyNoParameterCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty))]
    public void RelayCommandCanExecutePropertyParameterStringCancellationToken(string val, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty), AutoSetIsBusy = true)]
    public void RelayCommandCanExecutePropertyAutoSetIsBusyParameterStringCancellationToken(string val, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethodParameterString))]
    public void RelayCommandCanExecuteMethodParameterStringCancellationToken(string val, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethodParameterString), AutoSetIsBusy = true)]
    public void RelayCommandCanExecuteMethodAutoSetIsBusyParameterStringCancellationToken(string val, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(ExecuteOnBackgroundThread = true)]
    public void RelayCommandExecuteOnBackgroundThreadNoParameter()
    {
        throw new NotSupportedException();
    }

    [RelayCommand(ExecuteOnBackgroundThread = true, AutoSetIsBusy = true)]
    public void RelayCommandExecuteOnBackgroundThreadAutoSetIsBusyNoParameter()
    {
        throw new NotSupportedException();
    }

    [RelayCommand(ExecuteOnBackgroundThread = true)]
    public void RelayCommandExecuteOnBackgroundThreadNoParameterCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(ExecuteOnBackgroundThread = true, AutoSetIsBusy = true)]
    public void RelayCommandExecuteOnBackgroundThreadAutoSetIsBusyNoParameterCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty), ExecuteOnBackgroundThread = true)]
    public void RelayCommandExecuteOnBackgroundThreadCanExecutePropertyNoParameter()
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty), ExecuteOnBackgroundThread = true, AutoSetIsBusy = true)]
    public void RelayCommandExecuteOnBackgroundThreadAutoSetIsBusyCanExecutePropertyNoParameter()
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethod), ExecuteOnBackgroundThread = true)]
    public void RelayCommandExecuteOnBackgroundThreadCanExecuteMethodNoParameter()
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethod), ExecuteOnBackgroundThread = true, AutoSetIsBusy = true)]
    public void RelayCommandExecuteOnBackgroundThreadAutoSetIsBusyCanExecuteMethodNoParameter()
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty), ExecuteOnBackgroundThread = true)]
    public void RelayCommandExecuteOnBackgroundThreadCanExecutePropertyNoParameterCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty), ExecuteOnBackgroundThread = true, AutoSetIsBusy = true)]
    public void RelayCommandExecuteOnBackgroundThreadAutoSetIsBusyCanExecutePropertyNoParameterCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethod), ExecuteOnBackgroundThread = true)]
    public void RelayCommandExecuteOnBackgroundThreadCanExecuteMethodNoParameterCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethod), ExecuteOnBackgroundThread = true, AutoSetIsBusy = true)]
    public void RelayCommandExecuteOnBackgroundThreadAutoSetIsBusyCanExecuteMethodNoParameterCancellationToken(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty), ExecuteOnBackgroundThread = true)]
    public void RelayCommandExecuteOnBackgroundThreadCanExecutePropertyParameterStringCancellationToken(string val, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProperty), ExecuteOnBackgroundThread = true, AutoSetIsBusy = true)]
    public void RelayCommandExecuteOnBackgroundThreadAutoSetIsBusyCanExecutePropertyParameterStringCancellationToken(string val, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethodParameterString), ExecuteOnBackgroundThread = true)]
    public void RelayCommandExecuteOnBackgroundThreadCanExecuteMethodParameterStringCancellationToken(string val, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMethodParameterString), ExecuteOnBackgroundThread = true, AutoSetIsBusy = true)]
    public void RelayCommandExecuteOnBackgroundThreadAutoSetIsBusyCanExecuteMethodParameterStringCancellationToken(string val, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }
}