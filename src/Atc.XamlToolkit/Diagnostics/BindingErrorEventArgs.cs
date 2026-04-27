namespace Atc.XamlToolkit.Diagnostics;

/// <summary>
/// Event data for a single binding-error trace entry. Raised by the platform-specific
/// <c>BindingErrorTraceListener.BindingErrorOccurred</c> events on WPF, WinUI, and Avalonia.
/// </summary>
public sealed class BindingErrorEventArgs(string message) : EventArgs
{
    /// <summary>
    /// Gets the formatted binding-error message, as supplied by the underlying XAML platform.
    /// </summary>
    public string Message { get; } = message;
}