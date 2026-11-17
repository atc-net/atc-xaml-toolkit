// ReSharper disable UnusedParameter.Local
// ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
namespace Atc.XamlToolkit.Behaviors;

/// <summary>
/// Behavior that executes a command when a specified event is raised.
/// </summary>
[SuppressMessage("", "MA0015:The expression does not match a parameter", Justification = "OK")]
public class EventToCommandBehavior : Microsoft.Xaml.Behaviors.Behavior<FrameworkElement>
{
    /// <summary>
    /// Dependency property for the event name to listen to.
    /// </summary>
    public static readonly DependencyProperty EventNameProperty =
        DependencyProperty.Register(
            nameof(EventName),
            typeof(string),
            typeof(EventToCommandBehavior),
            new PropertyMetadata(string.Empty, OnEventNameChanged));

    /// <summary>
    /// Dependency property for the command to execute.
    /// </summary>
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(EventToCommandBehavior),
            new PropertyMetadata(default(ICommand)));

    /// <summary>
    /// Dependency property for the command parameter.
    /// </summary>
    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(
            nameof(CommandParameter),
            typeof(object),
            typeof(EventToCommandBehavior),
            new PropertyMetadata(default(object)));

    /// <summary>
    /// Dependency property to determine if event arguments should be passed to the command.
    /// </summary>
    public static readonly DependencyProperty PassEventArgsToCommandProperty =
        DependencyProperty.Register(
            nameof(PassEventArgsToCommand),
            typeof(bool),
            typeof(EventToCommandBehavior),
            new PropertyMetadata(BooleanBoxes.FalseBox));

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, Func<EventToCommandBehavior, Delegate>> AdapterFactories = new();

    private Delegate? eventHandler;
    private System.Reflection.EventInfo? eventInfo;

    /// <summary>
    /// Gets or sets the name of the event to listen to.
    /// </summary>
    public string EventName
    {
        get => (string)GetValue(EventNameProperty);
        set => SetValue(EventNameProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the event is raised.
    /// </summary>
    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
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
        get => (bool)GetValue(PassEventArgsToCommandProperty);
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

    private static void OnEventNameChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        if (d is not EventToCommandBehavior behavior || behavior.AssociatedObject is null)
        {
            return;
        }

        behavior.DetachHandler();
        behavior.AttachHandler();
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

        eventHandler = CreateEventHandler(eventInfo.EventHandlerType!);
        eventInfo.AddEventHandler(AssociatedObject, eventHandler);
    }

    [SuppressMessage("Design", "S3011:Make sure this accessibility bypass is safe here", Justification = "Intended.")]
    private Delegate CreateEventHandler(Type handlerType)
    {
        var parameters = handlerType.GetMethod(nameof(Action.Invoke))!.GetParameters();
        if (parameters.Length == 2 &&
            !parameters[0].ParameterType.IsValueType &&
            typeof(EventArgs).IsAssignableFrom(parameters[1].ParameterType))
        {
            // The handler is private, so it is looked up on this type - not on the type of a derived behavior
            var methodInfo = typeof(EventToCommandBehavior).GetMethod(nameof(OnEventRaised), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return Delegate.CreateDelegate(handlerType, this, methodInfo!);
        }

        // The handler does not have an (object, EventArgs) shape - for example DependencyPropertyChangedEventHandler,
        // whose event args is a struct - so it is bridged with a compiled adapter, created once per handler type.
        return AdapterFactories.GetOrAdd(handlerType, CreateAdapterFactory)(this);
    }

    [SuppressMessage("Design", "S3011:Make sure this accessibility bypass is safe here", Justification = "Intended.")]
    private static Func<EventToCommandBehavior, Delegate> CreateAdapterFactory(
        Type handlerType)
    {
        var handlerParameters = handlerType
            .GetMethod(nameof(Action.Invoke))!
            .GetParameters()
            .Select(p => System.Linq.Expressions.Expression.Parameter(p.ParameterType, p.Name))
            .ToArray();

        // Pass the second parameter (event args) if it exists, otherwise null
        System.Linq.Expressions.Expression eventArgsExpression = handlerParameters.Length > 1
            ? System.Linq.Expressions.Expression.Convert(handlerParameters[1], typeof(object))
            : System.Linq.Expressions.Expression.Constant(null, typeof(object));

        var targetMethod = typeof(EventToCommandBehavior).GetMethod(nameof(OnAdaptedEventRaised), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var behaviorParameter = System.Linq.Expressions.Expression.Parameter(typeof(EventToCommandBehavior), "behavior");
        var handler = System.Linq.Expressions.Expression.Lambda(
            handlerType,
            System.Linq.Expressions.Expression.Call(behaviorParameter, targetMethod, eventArgsExpression),
            handlerParameters);

        return System.Linq.Expressions.Expression
            .Lambda<Func<EventToCommandBehavior, Delegate>>(handler, behaviorParameter)
            .Compile();
    }

    private void DetachHandler()
    {
        if (eventInfo is null || eventHandler is null || AssociatedObject is null)
        {
            return;
        }

        eventInfo.RemoveEventHandler(AssociatedObject, eventHandler);
        eventHandler = null;
        eventInfo = null;
    }

    private void OnEventRaised(
        object? sender,
        EventArgs e)
        => OnAdaptedEventRaised(e);

    private void OnAdaptedEventRaised(object? e)
    {
        var command = Command;
        if (command is null)
        {
            return;
        }

        var parameter = GetCommandParameter(e);
        if (command.CanExecute(parameter))
        {
            command.Execute(parameter);
        }
    }

    private object? GetCommandParameter(object? e)
        => PassEventArgsToCommand
            ? e
            : CommandParameter;
}