namespace Atc.XamlToolkit.Command;

public interface IRelayCommandAsync<in T> : IRelayCommand, IDisposable
{
    bool IsExecuting { get; }

    Task ExecuteAsync(T parameter);

    void Cancel();
}