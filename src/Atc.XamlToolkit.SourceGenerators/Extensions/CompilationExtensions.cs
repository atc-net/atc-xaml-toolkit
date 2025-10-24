// ReSharper disable ConvertIfStatementToReturnStatement
namespace Atc.XamlToolkit.SourceGenerators.Extensions;

internal static class CompilationExtensions
{
    /// <summary>
    /// Determines the XAML platform based on the compilation references.
    /// </summary>
    /// <param name="compilation">The compilation to analyze.</param>
    /// <returns>The detected XAML platform.</returns>
    public static XamlPlatform GetXamlPlatform(
        this Compilation compilation)
    {
        // Check for WinUI
        var hasWinUI = compilation.GetTypeByMetadataName("Microsoft.UI.Xaml.DependencyProperty") is not null ||
                       compilation.GetTypeByMetadataName("Microsoft.UI.Xaml.DependencyObject") is not null;

        if (hasWinUI)
        {
            return XamlPlatform.WinUI;
        }

        // Check for WPF
        var hasWpf = compilation.GetTypeByMetadataName("System.Windows.DependencyProperty") is not null ||
                     compilation.GetTypeByMetadataName("System.Windows.DependencyObject") is not null;

        if (hasWpf)
        {
            return XamlPlatform.Wpf;
        }

        // Check for Avalonia
        var hasAvalonia = compilation.GetTypeByMetadataName("Avalonia.AvaloniaProperty") is not null ||
                          compilation.GetTypeByMetadataName("Avalonia.AvaloniaObject") is not null;

        if (hasAvalonia)
        {
            return XamlPlatform.Avalonia;
        }

        // Default to WPF if no platform is detected
        return XamlPlatform.Wpf;
    }
}