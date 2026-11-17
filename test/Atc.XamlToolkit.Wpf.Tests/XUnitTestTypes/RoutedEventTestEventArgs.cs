namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

public sealed class RoutedEventTestEventArgs(string message) : System.Windows.RoutedEventArgs
{
    public string Message { get; } = message;
}