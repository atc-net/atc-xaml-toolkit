namespace Atc.XamlToolkit.Command;

public interface IRelayCommandAsync : IRelayCommand
{
    Task ExecuteAsync(object? parameter);
}