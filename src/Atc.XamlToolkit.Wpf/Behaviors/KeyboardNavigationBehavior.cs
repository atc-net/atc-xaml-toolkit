namespace Atc.XamlToolkit.Behaviors;

/// <summary>
/// A behavior that enables custom keyboard navigation for UI elements in WPF applications.
/// Supports arrow keys, Enter, Escape, and Tab navigation with customizable commands.
/// </summary>
public class KeyboardNavigationBehavior : Microsoft.Xaml.Behaviors.Behavior<FrameworkElement>
{
    /// <summary>
    /// Dependency property for the command to execute on Up arrow key.
    /// </summary>
    public static readonly DependencyProperty UpCommandProperty = DependencyProperty.Register(
        nameof(UpCommand),
        typeof(ICommand),
        typeof(KeyboardNavigationBehavior),
        new PropertyMetadata(defaultValue: null));

    /// <summary>
    /// Dependency property for the command to execute on Down arrow key.
    /// </summary>
    public static readonly DependencyProperty DownCommandProperty = DependencyProperty.Register(
        nameof(DownCommand),
        typeof(ICommand),
        typeof(KeyboardNavigationBehavior),
        new PropertyMetadata(defaultValue: null));

    /// <summary>
    /// Dependency property for the command to execute on Left arrow key.
    /// </summary>
    public static readonly DependencyProperty LeftCommandProperty = DependencyProperty.Register(
        nameof(LeftCommand),
        typeof(ICommand),
        typeof(KeyboardNavigationBehavior),
        new PropertyMetadata(defaultValue: null));

    /// <summary>
    /// Dependency property for the command to execute on Right arrow key.
    /// </summary>
    public static readonly DependencyProperty RightCommandProperty = DependencyProperty.Register(
        nameof(RightCommand),
        typeof(ICommand),
        typeof(KeyboardNavigationBehavior),
        new PropertyMetadata(defaultValue: null));

    /// <summary>
    /// Dependency property for the command to execute on Enter key.
    /// </summary>
    public static readonly DependencyProperty EnterCommandProperty = DependencyProperty.Register(
        nameof(EnterCommand),
        typeof(ICommand),
        typeof(KeyboardNavigationBehavior),
        new PropertyMetadata(defaultValue: null));

    /// <summary>
    /// Dependency property for the command to execute on Escape key.
    /// </summary>
    public static readonly DependencyProperty EscapeCommandProperty = DependencyProperty.Register(
        nameof(EscapeCommand),
        typeof(ICommand),
        typeof(KeyboardNavigationBehavior),
        new PropertyMetadata(defaultValue: null));

    /// <summary>
    /// Dependency property for the command to execute on Tab key.
    /// </summary>
    public static readonly DependencyProperty TabCommandProperty = DependencyProperty.Register(
        nameof(TabCommand),
        typeof(ICommand),
        typeof(KeyboardNavigationBehavior),
        new PropertyMetadata(defaultValue: null));

    /// <summary>
    /// Dependency property to enable/disable the behavior.
    /// </summary>
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
        nameof(IsEnabled),
        typeof(bool),
        typeof(KeyboardNavigationBehavior),
        new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Gets or sets the command to execute when the Up arrow key is pressed.
    /// </summary>
    public ICommand? UpCommand
    {
        get => (ICommand?)GetValue(UpCommandProperty);
        set => SetValue(UpCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the Down arrow key is pressed.
    /// </summary>
    public ICommand? DownCommand
    {
        get => (ICommand?)GetValue(DownCommandProperty);
        set => SetValue(DownCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the Left arrow key is pressed.
    /// </summary>
    public ICommand? LeftCommand
    {
        get => (ICommand?)GetValue(LeftCommandProperty);
        set => SetValue(LeftCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the Right arrow key is pressed.
    /// </summary>
    public ICommand? RightCommand
    {
        get => (ICommand?)GetValue(RightCommandProperty);
        set => SetValue(RightCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the Enter key is pressed.
    /// </summary>
    public ICommand? EnterCommand
    {
        get => (ICommand?)GetValue(EnterCommandProperty);
        set => SetValue(EnterCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the Escape key is pressed.
    /// </summary>
    public ICommand? EscapeCommand
    {
        get => (ICommand?)GetValue(EscapeCommandProperty);
        set => SetValue(EscapeCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the Tab key is pressed.
    /// </summary>
    public ICommand? TabCommand
    {
        get => (ICommand?)GetValue(TabCommandProperty);
        set => SetValue(TabCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the behavior is enabled.
    /// </summary>
    public bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, BooleanBoxes.Box(value));
    }

    /// <inheritdoc />
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is not null)
        {
            AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
            AssociatedObject.Loaded += OnLoaded;
            AssociatedObject.GotFocus += OnGotFocus;
        }
    }

    private void OnLoaded(
        object sender,
        RoutedEventArgs e)
    {
        // Set focus to the element when it's loaded so keyboard events work immediately
        if (AssociatedObject is not null && AssociatedObject.Focusable)
        {
            AssociatedObject.Focus();
        }
    }

    private void OnGotFocus(
        object sender,
        RoutedEventArgs e)
    {
        // Ensure keyboard focus is set when the element gets focus
        if (AssociatedObject is not null)
        {
            Keyboard.Focus(AssociatedObject);
        }
    }

    /// <inheritdoc />
    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
        {
            AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
            AssociatedObject.Loaded -= OnLoaded;
            AssociatedObject.GotFocus -= OnGotFocus;
        }

        base.OnDetaching();
    }

    private void OnPreviewKeyDown(
        object sender,
        KeyEventArgs e)
    {
        if (!IsEnabled)
        {
            return;
        }

        var command = e.Key switch
        {
            Key.Up => UpCommand,
            Key.Down => DownCommand,
            Key.Left => LeftCommand,
            Key.Right => RightCommand,
            Key.Enter => EnterCommand,
            Key.Escape => EscapeCommand,
            Key.Tab => TabCommand,
            _ => null,
        };

        if (command?.CanExecute(e) == true)
        {
            command.Execute(e);
            e.Handled = true;
        }
    }
}