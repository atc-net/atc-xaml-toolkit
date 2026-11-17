namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

public static partial class SharedRoutedEvents
{
    [Atc.XamlToolkit.Controls.Attributes.RoutedEvent(HandlerType = typeof(EventHandler<RoutedEventTestEventArgs>), GenerateDocumentation = true)]
    private static readonly System.Windows.RoutedEvent deviceError;
}