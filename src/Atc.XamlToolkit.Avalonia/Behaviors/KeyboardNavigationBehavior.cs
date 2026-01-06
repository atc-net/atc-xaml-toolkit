// ReSharper disable InvertIf
namespace Atc.XamlToolkit.Behaviors;

/// <summary>
/// A behavior that enables custom keyboard navigation for UI elements in Avalonia applications.
/// Supports arrow keys, Enter, Escape, and Tab navigation with customizable commands.
/// </summary>
[SuppressMessage("", "MA0015:The expression does not match a parameter", Justification = "OK")]
public class KeyboardNavigationBehavior : Avalonia.Xaml.Interactivity.Behavior<Control>
{
    /// <summary>
    /// Styled property for the command to execute on Up arrow key.
    /// </summary>
    public static readonly StyledProperty<ICommand?> UpCommandProperty =
        AvaloniaProperty.Register<KeyboardNavigationBehavior, ICommand?>(nameof(UpCommand));

    /// <summary>
    /// Styled property for the command to execute on Down arrow key.
    /// </summary>
    public static readonly StyledProperty<ICommand?> DownCommandProperty =
        AvaloniaProperty.Register<KeyboardNavigationBehavior, ICommand?>(nameof(DownCommand));

    /// <summary>
    /// Styled property for the command to execute on Left arrow key.
    /// </summary>
    public static readonly StyledProperty<ICommand?> LeftCommandProperty =
        AvaloniaProperty.Register<KeyboardNavigationBehavior, ICommand?>(nameof(LeftCommand));

    /// <summary>
    /// Styled property for the command to execute on Right arrow key.
    /// </summary>
    public static readonly StyledProperty<ICommand?> RightCommandProperty =
        AvaloniaProperty.Register<KeyboardNavigationBehavior, ICommand?>(nameof(RightCommand));

    /// <summary>
    /// Styled property for the command to execute on Enter key.
    /// </summary>
    public static readonly StyledProperty<ICommand?> EnterCommandProperty =
        AvaloniaProperty.Register<KeyboardNavigationBehavior, ICommand?>(nameof(EnterCommand));

    /// <summary>
    /// Styled property for the command to execute on Escape key.
    /// </summary>
    public static readonly StyledProperty<ICommand?> EscapeCommandProperty =
        AvaloniaProperty.Register<KeyboardNavigationBehavior, ICommand?>(nameof(EscapeCommand));

    /// <summary>
    /// Styled property for the command to execute on Tab key.
    /// </summary>
    public static readonly StyledProperty<ICommand?> TabCommandProperty =
        AvaloniaProperty.Register<KeyboardNavigationBehavior, ICommand?>(nameof(TabCommand));

    /// <summary>
    /// Styled property to enable/disable the behavior.
    /// </summary>
    public static readonly StyledProperty<bool> IsNavigationEnabledProperty =
        AvaloniaProperty.Register<KeyboardNavigationBehavior, bool>(nameof(IsNavigationEnabled), defaultValue: true);

    /// <summary>
    /// Gets or sets the command to execute when the Up arrow key is pressed.
    /// </summary>
    public ICommand? UpCommand
    {
        get => GetValue(UpCommandProperty);
        set => SetValue(UpCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the Down arrow key is pressed.
    /// </summary>
    public ICommand? DownCommand
    {
        get => GetValue(DownCommandProperty);
        set => SetValue(DownCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the Left arrow key is pressed.
    /// </summary>
    public ICommand? LeftCommand
    {
        get => GetValue(LeftCommandProperty);
        set => SetValue(LeftCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the Right arrow key is pressed.
    /// </summary>
    public ICommand? RightCommand
    {
        get => GetValue(RightCommandProperty);
        set => SetValue(RightCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the Enter key is pressed.
    /// </summary>
    public ICommand? EnterCommand
    {
        get => GetValue(EnterCommandProperty);
        set => SetValue(EnterCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the Escape key is pressed.
    /// </summary>
    public ICommand? EscapeCommand
    {
        get => GetValue(EscapeCommandProperty);
        set => SetValue(EscapeCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the Tab key is pressed.
    /// </summary>
    public ICommand? TabCommand
    {
        get => GetValue(TabCommandProperty);
        set => SetValue(TabCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the behavior is enabled.
    /// </summary>
    public bool IsNavigationEnabled
    {
        get => GetValue(IsNavigationEnabledProperty);
        set => SetValue(IsNavigationEnabledProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttached()
    {
        ArgumentNullException.ThrowIfNull(AssociatedObject);

        base.OnAttached();

        AssociatedObject.KeyDown += OnKeyDown;
        AssociatedObject.AttachedToVisualTree += OnAttachedToVisualTree;
        AssociatedObject.GotFocus += OnGotFocus;
    }

    /// <inheritdoc />
    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
        {
            AssociatedObject.KeyDown -= OnKeyDown;
            AssociatedObject.AttachedToVisualTree -= OnAttachedToVisualTree;
            AssociatedObject.GotFocus -= OnGotFocus;
        }

        base.OnDetaching();
    }

    private void OnAttachedToVisualTree(
        object? sender,
        EventArgs e)
    {
        // Set focus to the element when it's loaded so keyboard events work immediately
        if (AssociatedObject is not null && AssociatedObject.Focusable)
        {
            AssociatedObject.Focus();
        }
    }

    private void OnGotFocus(
        object? sender,
        GotFocusEventArgs e)
    {
        // Ensure keyboard focus is set when the element gets focus
        AssociatedObject?.Focus();
    }

    private void OnKeyDown(
        object? sender,
        KeyEventArgs e)
    {
        if (!IsNavigationEnabled)
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