// ReSharper disable InvertIf
// ReSharper disable UnusedParameter.Local
namespace Atc.XamlToolkit.Behaviors;

/// <summary>
/// Behavior that executes a command when a specified event is raised.
/// </summary>
[SuppressMessage("", "MA0015:The expression does not match a parameter", Justification = "OK")]
public class EventToCommandBehavior : Avalonia.Xaml.Interactivity.Behavior<Avalonia.Interactivity.Interactive>
{
    /// <summary>
    /// Styled property for the event name to listen to.
    /// </summary>
    public static readonly StyledProperty<string> EventNameProperty =
        AvaloniaProperty.Register<EventToCommandBehavior, string>(nameof(EventName), string.Empty);

    /// <summary>
    /// Styled property for the command to execute.
    /// </summary>
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<EventToCommandBehavior, ICommand?>(nameof(Command));

    /// <summary>
    /// Styled property for the command parameter.
    /// </summary>
    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<EventToCommandBehavior, object?>(nameof(CommandParameter));

    /// <summary>
    /// Styled property to determine if event arguments should be passed to the command.
    /// </summary>
    public static readonly StyledProperty<bool> PassEventArgsToCommandProperty =
        AvaloniaProperty.Register<EventToCommandBehavior, bool>(nameof(PassEventArgsToCommand), defaultValue: false);

    private Delegate? eventHandler;
    private System.Reflection.EventInfo? eventInfo;

    /// <summary>
    /// Gets or sets the name of the event to listen to.
    /// </summary>
    public string EventName
    {
        get => GetValue(EventNameProperty);
        set => SetValue(EventNameProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the event is raised.
    /// </summary>
    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to the command.
    /// </summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether to pass the event arguments to the command parameter.
    /// </summary>
    public bool PassEventArgsToCommand
    {
        get => GetValue(PassEventArgsToCommandProperty);
        set => SetValue(PassEventArgsToCommandProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnAttached()
    {
        base.OnAttached();
        AttachHandler();
    }

    /// <inheritdoc/>
    protected override void OnDetaching()
    {
        base.OnDetaching();
        DetachHandler();
    }

    /// <inheritdoc/>
    [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Method should fail with an exception if func is null.")]
    protected override void OnPropertyChanged(
        AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property != EventNameProperty || AssociatedObject is null)
        {
            return;
        }

        DetachHandler();
        AttachHandler();
    }

    [SuppressMessage("Design", "S3011:Make sure this accessibility bypass is safe here", Justification = "Intended.")]
    private void AttachHandler()
    {
        ArgumentNullException.ThrowIfNull(AssociatedObject);

        if (string.IsNullOrEmpty(EventName))
        {
            return;
        }

        eventInfo = AssociatedObject
            .GetType()
            .GetEvent(EventName);
        if (eventInfo is null)
        {
            throw new InvalidOperationException($"Event '{EventName}' not found on type '{AssociatedObject.GetType().Name}'");
        }

        var methodInfo = GetType().GetMethod(nameof(OnEventRaised), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        eventHandler = Delegate.CreateDelegate(eventInfo.EventHandlerType!, this, methodInfo!);
        eventInfo.AddEventHandler(AssociatedObject, eventHandler);
    }

    private void DetachHandler()
    {
        if (eventInfo is not null &&
            eventHandler is not null &&
            AssociatedObject is not null)
        {
            eventInfo.RemoveEventHandler(AssociatedObject, eventHandler);
            eventHandler = null;
            eventInfo = null;
        }
    }

    private void OnEventRaised(
        object? sender,
        EventArgs e)
    {
        if (Command?.CanExecute(GetCommandParameter(e)) == true)
        {
            Command.Execute(GetCommandParameter(e));
        }
    }

    private object? GetCommandParameter(EventArgs e)
        => PassEventArgsToCommand
            ? e
            : CommandParameter;
}