namespace Atc.XamlToolkit.Behaviors;

/// <summary>
/// A behavior that manages focus for UI elements in Avalonia applications.
/// Allows declarative focus control through XAML properties.
/// </summary>
[SuppressMessage("", "MA0015:The expression does not match a parameter", Justification = "OK")]
public class FocusBehavior : Avalonia.Xaml.Interactivity.Behavior<Control>
{
    /// <summary>
    /// Styled property for controlling whether the element should receive focus when attached.
    /// </summary>
    public static readonly StyledProperty<bool> HasInitialFocusProperty =
        AvaloniaProperty.Register<FocusBehavior, bool>(nameof(HasInitialFocus));

    /// <summary>
    /// Styled property for binding focus state to a ViewModel property.
    /// </summary>
    public static readonly StyledProperty<bool> IsFocusedProperty =
        AvaloniaProperty.Register<FocusBehavior, bool>(nameof(IsFocused));

    /// <summary>
    /// Styled property for selecting all text when the element receives focus (TextBox only).
    /// </summary>
    public static readonly StyledProperty<bool> SelectAllOnFocusProperty =
        AvaloniaProperty.Register<FocusBehavior, bool>(nameof(SelectAllOnFocus));

    /// <summary>
    /// Styled property for focus trigger - when changed, sets focus to the element.
    /// </summary>
    public static readonly StyledProperty<object?> FocusTriggerProperty =
        AvaloniaProperty.Register<FocusBehavior, object?>(nameof(FocusTrigger));

    private object? lastFocusTrigger;
    private bool isUpdatingFocus;

    /// <summary>
    /// Gets or sets a value indicating whether the element should receive focus when the behavior is attached.
    /// </summary>
    public bool HasInitialFocus
    {
        get => GetValue(HasInitialFocusProperty);
        set => SetValue(HasInitialFocusProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the element is focused.
    /// This property is two-way bindable to track and control focus state.
    /// </summary>
    public bool IsFocused
    {
        get => GetValue(IsFocusedProperty);
        set => SetValue(IsFocusedProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether all text should be selected when the element receives focus.
    /// This only applies to TextBox controls.
    /// </summary>
    public bool SelectAllOnFocus
    {
        get => GetValue(SelectAllOnFocusProperty);
        set => SetValue(SelectAllOnFocusProperty, value);
    }

    /// <summary>
    /// Gets or sets a trigger value. When this property changes, the element will receive focus.
    /// Useful for triggering focus from a ViewModel.
    /// </summary>
    public object? FocusTrigger
    {
        get => GetValue(FocusTriggerProperty);
        set => SetValue(FocusTriggerProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttached()
    {
        ArgumentNullException.ThrowIfNull(AssociatedObject);

        base.OnAttached();

        // Use AddHandler to ensure we receive events even if marked as handled
        AssociatedObject.AddHandler(InputElement.GotFocusEvent, OnGotFocus, handledEventsToo: true);
        AssociatedObject.AddHandler(InputElement.LostFocusEvent, OnLostFocus, handledEventsToo: true);
        AssociatedObject.AttachedToVisualTree += OnAttachedToVisualTree;

        // Listen to the behavior's own property changes to respond to binding updates
        this.PropertyChanged += OnBehaviorPropertyChanged;

        // Check if already attached to visual tree
        if (AssociatedObject.Parent is not null && HasInitialFocus)
        {
            SetFocus();
        }
    }

    /// <inheritdoc />
    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
        {
            AssociatedObject.RemoveHandler(InputElement.GotFocusEvent, OnGotFocus);
            AssociatedObject.RemoveHandler(InputElement.LostFocusEvent, OnLostFocus);
            AssociatedObject.AttachedToVisualTree -= OnAttachedToVisualTree;
        }

        this.PropertyChanged -= OnBehaviorPropertyChanged;

        base.OnDetaching();
    }

    private void OnBehaviorPropertyChanged(
        object? sender,
        AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == IsFocusedProperty)
        {
            // When IsFocused changes to true (from ViewModel), set focus
            // Skip if we're updating the property ourselves to avoid infinite loops
            if (e.NewValue is true &&
                !isUpdatingFocus &&
                AssociatedObject is not null &&
                !AssociatedObject.IsFocused)
            {
                SetFocus();
            }
        }
        else if (e.Property == FocusTriggerProperty)
        {
            var newTrigger = e.NewValue;
            if (newTrigger is not null && !Equals(newTrigger, lastFocusTrigger))
            {
                lastFocusTrigger = newTrigger;
                SetFocus();
            }
        }
    }

    private void OnAttachedToVisualTree(
        object? sender,
        EventArgs e)
    {
        if (HasInitialFocus)
        {
            SetFocus();
        }
    }

    private void OnGotFocus(
        object? sender,
        GotFocusEventArgs e)
    {
        isUpdatingFocus = true;
        IsFocused = true;
        isUpdatingFocus = false;

        if (SelectAllOnFocus && AssociatedObject is TextBox textBox)
        {
            // Use Dispatcher to ensure SelectAll happens after all focus events are processed
            _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(
                () => textBox.SelectAll(),
                Avalonia.Threading.DispatcherPriority.Input);
        }
    }

    private void OnLostFocus(
        object? sender,
        Avalonia.Interactivity.RoutedEventArgs e)
    {
        isUpdatingFocus = true;
        IsFocused = false;
        isUpdatingFocus = false;
    }

    private void SetFocus()
    {
        if (AssociatedObject is null)
        {
            return;
        }

        // Ensure the control is focusable
        if (!AssociatedObject.Focusable)
        {
            AssociatedObject.Focusable = true;
        }

        // Try setting KeyboardNavigation.IsTabStop to ensure it's reachable
        if (!KeyboardNavigation.GetIsTabStop(AssociatedObject))
        {
            KeyboardNavigation.SetIsTabStop(AssociatedObject, value: true);
        }

        // Use InvokeAsync to ensure focus is set after the current operation completes
        _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(
            () =>
            {
                // Set focus using keyboard navigation to trigger proper focus visuals
                var focused = AssociatedObject.Focus(NavigationMethod.Directional);

                if (focused && AssociatedObject is TextBox textBox && !SelectAllOnFocus)
                {
                    // For TextBox, position caret at the end (only if not using SelectAllOnFocus)
                    var length = textBox.Text?.Length ?? 0;
                    textBox.CaretIndex = length;
                    textBox.SelectionStart = length;
                    textBox.SelectionEnd = length;
                }
            },
            Avalonia.Threading.DispatcherPriority.Input);
    }
}