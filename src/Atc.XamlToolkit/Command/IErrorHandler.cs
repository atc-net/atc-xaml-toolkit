namespace Atc.XamlToolkit.Command;

public interface IErrorHandler
{
    void HandleError(Exception ex);
}