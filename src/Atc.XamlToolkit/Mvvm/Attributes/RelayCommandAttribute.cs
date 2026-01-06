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
    public RelayCommandAttribute(string commandName)
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
    /// When <see langword="true" />, the command will be dispatched to a background thread (using <c>Task.Run</c> or similar),
    /// ensuring that long-running operations do not block the UI thread.
    /// When <see langword="false" />, the command executes on the current thread.
    /// </summary>
    public bool ExecuteOnBackgroundThread { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the generated command will automatically set
    /// the ViewModel's <c>IsBusy</c> property to <see langword="true" /> when execution starts,
    /// and back to <see langword="false" /> when execution completes (even on exceptions).
    /// </summary>
    public bool AutoSetIsBusy { get; set; }

    /// <summary>
    /// Gets or sets cancellation support.
    /// - Null (default): Auto-detect from method signature (has CancellationToken parameter?)
    /// - True: Force cancellation support (adds CancellationToken parameter if missing)
    /// - False: Force no cancellation support (ignores CancellationToken parameter)
    /// </summary>
    public bool SupportsCancellation { get; set; }

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(CommandName)}: {CommandName}, {nameof(CanExecute)}: {CanExecute}, {nameof(InvertCanExecute)}: {InvertCanExecute}, {nameof(ParameterValue)}: {ParameterValue}, {nameof(ParameterValues)}: {ParameterValues}, {nameof(ExecuteOnBackgroundThread)}: {ExecuteOnBackgroundThread}, {nameof(AutoSetIsBusy)}: {AutoSetIsBusy}, {nameof(SupportsCancellation)}: {SupportsCancellation}";
}