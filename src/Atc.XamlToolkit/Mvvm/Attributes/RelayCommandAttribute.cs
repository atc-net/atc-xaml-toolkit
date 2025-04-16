// ReSharper disable RedundantAttributeUsageProperty
// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.Mvvm;

/// <summary>
/// Specifies that a property in the ViewModel should be generated for a field.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class RelayCommandAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommandAttribute"/> class.
    /// </summary>
    public RelayCommandAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommandAttribute"/> class.
    /// </summary>
    /// <param name="commandName">The name of the relay command to generate</param>
    public RelayCommandAttribute(
        string commandName)
    {
        CommandName = commandName;
    }

    /// <summary>
    /// Gets the name of command to generate.
    /// </summary>
    public string? CommandName { get; }

    /// <summary>
    /// Gets or sets the name of the canExecute command.
    /// </summary>
    public string? CanExecute { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the canExecute value should be inverted.
    /// <para>
    /// For example, if CanExecute is specified as "IsConnected" property and InvertCanExecute is true,
    /// the generated lambda will be:
    /// <code language="csharp">
    /// () => !IsConnected
    /// </code>
    /// instead of:
    /// <code language="csharp">
    /// () => IsConnected
    /// </code>
    /// </para>
    /// </summary>
    public bool InvertCanExecute { get; set; }

    /// <summary>
    /// Gets or sets the parameter value.
    /// </summary>
    public object? ParameterValue { get; set; }

    /// <summary>
    /// Gets or sets the parameter values.
    /// </summary>
    public object[]? ParameterValues { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether the associated command should be executed on a background thread.
    /// When <c>true</c>, the command will be dispatched to a background thread (using <c>Task.Run</c> or similar),
    /// ensuring that long-running operations do not block the UI thread.
    /// When <c>false</c>, the command executes on the current thread.
    /// </summary>
    public bool ExecuteOnBackgroundThread { get; set; }
}