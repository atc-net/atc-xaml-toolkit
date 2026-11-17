namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

public partial class ReviewFindingsControl : System.Windows.Controls.UserControl
{
    [Atc.XamlToolkit.Controls.Attributes.RoutedEvent(HandlerType = typeof(System.Windows.RoutedPropertyChangedEventHandler<(int A, string B)>))]
    private static readonly System.Windows.RoutedEvent pairChanged;

    [Atc.XamlToolkit.Controls.Attributes.RoutedEvent(HandlerType = null!, AddOwnerOf = null)]
    private static readonly System.Windows.RoutedEvent plain;

    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(GeneratePartialHooks = true)]
    private ReviewFindingsMode mode;

    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(GenerateCoerceHook = true)]
    private System.Windows.Input.Key key;

    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(
        GenerateCoerceHook = true,
        CoerceValueCallback = nameof(RejectNegative),
        PropertyChangedCallback = null,
        ValidateValueCallback = null)]
    private double amount;

    public int ModeChanges { get; private set; }

    private static object RejectNegative(
        System.Windows.DependencyObject d,
        object baseValue)
        => d is ReviewFindingsControl && (double)baseValue < 0
            ? System.Windows.DependencyProperty.UnsetValue
            : baseValue;

    partial void OnModeChanged(
        ReviewFindingsMode oldValue,
        ReviewFindingsMode newValue)
        => ModeChanges++;

    private partial System.Windows.Input.Key CoerceKey(
        System.Windows.Input.Key value)
        => value;

    private partial double CoerceAmount(double value)
        => value > 100 ? 100 : value;
}