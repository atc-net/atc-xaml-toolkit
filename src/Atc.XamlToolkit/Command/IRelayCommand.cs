// ReSharper disable UnusedMemberInSuper.Global
namespace Atc.XamlToolkit.Command;

public interface IRelayCommand : ICommand
{
    [SuppressMessage("Design", "CA1030:Use events where appropriate", Justification = "OK.")]
    void RaiseCanExecuteChanged();
}