namespace Atc.XamlToolkit.Behaviors;

/// <summary>
/// A behavior that manages focus for UI elements in WPF applications.
/// Allows declarative focus control through XAML properties.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Behavior cleanup happens in OnDetaching")]
public class FocusBehavior : Microsoft.Xaml.Behaviors.Behavior<FrameworkElement>
{
    /// <summary>
    /// Dependency property for controlling whether the element should receive focus when attached.
    /// </summary>
    public static readonly DependencyProperty HasInitialFocusProperty = DependencyProperty.Register(
        nameof(HasInitialFocus),
        typeof(bool),
        typeof(FocusBehavior),
        new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>
    /// Dependency property for binding focus state to a ViewModel property.
    /// </summary>
    public static readonly DependencyProperty IsFocusedProperty = DependencyProperty.Register(
        nameof(IsFocused),
        typeof(bool),
        typeof(FocusBehavior),
        new PropertyMetadata(BooleanBoxes.FalseBox, OnIsFocusedChanged));

    /// <summary>
    /// Dependency property for selecting all text when the element receives focus (TextBox only).
    /// </summary>
    public static readonly DependencyProperty SelectAllOnFocusProperty = DependencyProperty.Register(
        nameof(SelectAllOnFocus),
        typeof(bool),
        typeof(FocusBehavior),
        new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>
    /// Dependency property for focus trigger - when changed, sets focus to the element.
    /// </summary>
    public static readonly DependencyProperty FocusTriggerProperty = DependencyProperty.Register(
        nameof(FocusTrigger),
        typeof(object),
        typeof(FocusBehavior),
        new PropertyMetadata(defaultValue: null, OnFocusTriggerChanged));

    private bool isUpdatingFocus;

    /// <summary>
    /// Gets or sets a value indicating whether the element should receive focus when the behavior is attached.
    /// </summary>
    public bool HasInitialFocus
    {
        get => (bool)GetValue(HasInitialFocusProperty);
        set => SetValue(HasInitialFocusProperty, BooleanBoxes.Box(value));
    }

    /// <summary>
    /// Gets or sets a value indicating whether the element is focused.
    /// This property is two-way bindable to track and control focus state.
    /// </summary>
    public bool IsFocused
    {
        get => (bool)GetValue(IsFocusedProperty);
        set => SetValue(IsFocusedProperty, BooleanBoxes.Box(value));
    }

    /// <summary>
    /// Gets or sets a value indicating whether all text should be selected when the element receives focus.
    /// This only applies to TextBox controls.
    /// </summary>
    public bool SelectAllOnFocus
    {
        get => (bool)GetValue(SelectAllOnFocusProperty);
        set => SetValue(SelectAllOnFocusProperty, BooleanBoxes.Box(value));
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
        base.OnAttached();

        if (AssociatedObject is not null)
        {
            AssociatedObject.GotFocus += OnGotFocus;
            AssociatedObject.LostFocus += OnLostFocus;
            AssociatedObject.Loaded += OnLoaded;

            if (AssociatedObject.IsLoaded && HasInitialFocus)
            {
                SetFocus();
            }
        }
    }

    /// <inheritdoc />
    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
        {
            AssociatedObject.GotFocus -= OnGotFocus;
            AssociatedObject.LostFocus -= OnLostFocus;
            AssociatedObject.Loaded -= OnLoaded;
        }

        base.OnDetaching();
    }

    private static void OnIsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FocusBehavior behavior || behavior.isUpdatingFocus || e.NewValue is not bool isFocused || !isFocused)
        {
            return;
        }

        behavior.SetFocus();
    }

    private static void OnFocusTriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FocusBehavior behavior && e.NewValue is not null)
        {
            behavior.SetFocus();
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (HasInitialFocus)
        {
            SetFocus();
        }
    }

    private void OnGotFocus(object sender, RoutedEventArgs e)
    {
        isUpdatingFocus = true;
        IsFocused = true;
        isUpdatingFocus = false;

        if (SelectAllOnFocus && AssociatedObject is System.Windows.Controls.TextBox textBox)
        {
            textBox.SelectAll();
        }
    }

    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
        isUpdatingFocus = true;
        IsFocused = false;
        isUpdatingFocus = false;
    }

    private void SetFocus()
    {
        if (AssociatedObject is not null)
        {
            _ = AssociatedObject.Dispatcher.BeginInvoke(
                DispatcherPriority.Input,
                new Action(() =>
                {
                    AssociatedObject.Focus();
                    Keyboard.Focus(AssociatedObject);
                }));
        }
    }
}