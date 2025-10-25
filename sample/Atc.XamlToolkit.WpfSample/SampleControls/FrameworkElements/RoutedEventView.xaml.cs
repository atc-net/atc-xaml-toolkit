namespace Atc.XamlToolkit.WpfSample.SampleControls.FrameworkElements;

/// <summary>
/// Interaction logic for RoutedEventView - demonstrates source-generated routed events.
/// </summary>
public partial class RoutedEventView : UserControl
{
    public RoutedEventView()
    {
        InitializeComponent();
    }

    // Example 1: Simple routed event handlers
    private void OnItemClickedAtBorder(object sender, RoutedEventArgs e)
    {
        EventLogSimple.Text = $"[{DateTime.Now:HH:mm:ss.fff}] ItemClicked event bubbled up to Border container!\n" +
                              $"Source: {e.Source?.GetType().Name}\n" +
                              $"Handled by: Border";
    }

    private void OnItemActionPerformed(object sender, RoutedEventComponents.ItemActionRoutedEventArgs e)
    {
        EventLogSimple.Text += $"\n\n[{DateTime.Now:HH:mm:ss.fff}] ItemActionPerformed event raised!\n" +
                               $"Action Type: {e.ActionType}\n" +
                               $"Timestamp: {e.Timestamp:HH:mm:ss.fff}\n" +
                               $"Source: {e.Source?.GetType().Name}";
    }

    // Example 2: Custom event handler type
    private void OnValueIncremented(object sender, RoutedEventComponents.ValueChangedRoutedEventArgs e)
    {
        ValueDisplay.Text = e.NewValue.ToString(GlobalizationConstants.EnglishCultureInfo);
        EventLogNumeric.Text = $"[{DateTime.Now:HH:mm:ss.fff}] ValueIncremented Event\n" +
                               $"Old Value: {e.OldValue}\n" +
                               $"New Value: {e.NewValue}\n" +
                               $"Delta: +{e.Delta}\n" +
                               $"Source: {e.Source?.GetType().Name}";
    }

    private void OnValueDecremented(object sender, RoutedEventComponents.ValueChangedRoutedEventArgs e)
    {
        ValueDisplay.Text = e.NewValue.ToString(GlobalizationConstants.EnglishCultureInfo);
        EventLogNumeric.Text = $"[{DateTime.Now:HH:mm:ss.fff}] ValueDecremented Event\n" +
                               $"Old Value: {e.OldValue}\n" +
                               $"New Value: {e.NewValue}\n" +
                               $"Delta: -{e.Delta}\n" +
                               $"Source: {e.Source?.GetType().Name}";
    }

    private void IncrementButton_Click(object sender, RoutedEventArgs e)
    {
        NumericControl.Increment();
    }

    private void DecrementButton_Click(object sender, RoutedEventArgs e)
    {
        NumericControl.Decrement();
    }

    // Example 3: Tunneling event handlers
    private void OnPreviewItemClicked_Outer(object sender, RoutedEventArgs e)
    {
        EventLogTunnel.Text = $"[{DateTime.Now:HH:mm:ss.fff}] Step 1: PreviewItemClicked at OUTER Border (Tunneling down)\n" +
                              $"Source: {e.Source?.GetType().Name}\n" +
                              $"Current Handler: Outer Border";
    }

    private void OnPreviewItemClicked_Inner(object sender, RoutedEventArgs e)
    {
        EventLogTunnel.Text += $"\n\n[{DateTime.Now:HH:mm:ss.fff}] Step 2: PreviewItemClicked at INNER Border (Tunneling down)\n" +
                               $"Source: {e.Source?.GetType().Name}\n" +
                               $"Current Handler: Inner Border\n\n" +
                               $"Next: Event reaches the button (source element)";
    }
}