// ReSharper disable InconsistentNaming
namespace Atc.XamlToolkit.SourceGenerators.Models;

/// <summary>
/// Represents the XAML platform target for code generation.
/// </summary>
internal enum XamlPlatform
{
    /// <summary>
    /// Windows Presentation Foundation (WPF).
    /// </summary>
    Wpf,

    /// <summary>
    /// Windows UI Library 3 (WinUI).
    /// </summary>
    WinUI,

    /// <summary>
    /// Avalonia UI.
    /// </summary>
    Avalonia,
}