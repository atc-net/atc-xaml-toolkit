namespace Atc.XamlToolkit.Behaviors;

/// <summary>
/// Behavior that executes a command when a specified event is raised.
/// </summary>
[SuppressMessage("", "MA0015:The expression does not match a parameter", Justification = "OK")]
[Microsoft.UI.Xaml.Markup.ContentProperty(Name = nameof(Command))]
public class EventToCommandBehavior : Microsoft.Xaml.Interactivity.Behavior<FrameworkElement>
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
            new PropertyMetadata(defaultValue: null));

    /// <summary>
    /// Dependency property for the command parameter.
    /// </summary>
    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(
            nameof(CommandParameter),
            typeof(object),
            typeof(EventToCommandBehavior),
            new PropertyMetadata(defaultValue: null));

    /// <summary>
    /// Dependency property to determine if event arguments should be passed to the command.
    /// </summary>
    public static readonly DependencyProperty PassEventArgsToCommandProperty =
        DependencyProperty.Register(
            nameof(PassEventArgsToCommand),
            typeof(bool),
            typeof(EventToCommandBehavior),
            new PropertyMetadata(BooleanBoxes.FalseBox));

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
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
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

    private static void OnEventNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not EventToCommandBehavior behavior || behavior.AssociatedObject is null)
        {
            return;
        }

        behavior.DetachHandler();
        behavior.AttachHandler();
    }

    private void AttachHandler()
    {
        ArgumentNullException.ThrowIfNull(AssociatedObject);

        if (string.IsNullOrEmpty(EventName))
        {
            return;
        }

        eventInfo = AssociatedObject.GetType().GetEvent(EventName);
        if (eventInfo is null)
        {
            throw new InvalidOperationException($"Event '{EventName}' not found on type '{AssociatedObject.GetType().Name}'");
        }

        // Create a dynamic event handler that matches the event signature
        var handlerType = eventInfo.EventHandlerType!;
        eventHandler = CreateEventHandler(handlerType);
        eventInfo.AddEventHandler(AssociatedObject, eventHandler);
    }

    [SuppressMessage("Design", "S3011:Make sure this accessibility bypass is safe here", Justification = "Intended.")]
    private Delegate CreateEventHandler(Type handlerType)
    {
        var invokeMethod = handlerType.GetMethod("Invoke")!;
        var parameters = invokeMethod.GetParameters();

        // Create parameters for the lambda expression
        var parameterExpressions = parameters.Select(p => System.Linq.Expressions.Expression.Parameter(p.ParameterType, p.Name)).ToArray();

        // Get the method to call
        var targetMethod = GetType().GetMethod(nameof(OnEventRaised), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Create the method call expression
        // Pass the second parameter (event args) if it exists, otherwise null
        System.Linq.Expressions.Expression eventArgsExpression = parameterExpressions.Length > 1
            ? System.Linq.Expressions.Expression.Convert(parameterExpressions[1], typeof(object))
            : System.Linq.Expressions.Expression.Constant(null, typeof(object));

        var callExpression = System.Linq.Expressions.Expression.Call(
            System.Linq.Expressions.Expression.Constant(this),
            targetMethod,
            eventArgsExpression);

        // Create and compile the lambda
        var lambda = System.Linq.Expressions.Expression.Lambda(handlerType, callExpression, parameterExpressions);
        return lambda.Compile();
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

    private void OnEventRaised(object? eventArgs)
    {
        var parameter = GetCommandParameter(eventArgs);
        if (Command?.CanExecute(parameter) == true)
        {
            Command.Execute(parameter);
        }
    }

    private object? GetCommandParameter(object? eventArgs)
        => PassEventArgsToCommand
            ? eventArgs
            : CommandParameter;
}