namespace Atc.XamlToolkit.Command;

public interface IRelayCommandAsync : IRelayCommand, IDisposable
{
    bool IsExecuting { get; }

    Task ExecuteAsync(object? parameter);

    void Cancel();
}