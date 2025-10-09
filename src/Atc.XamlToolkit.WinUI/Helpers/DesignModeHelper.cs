namespace Atc.XamlToolkit.Helpers;

/// <summary>
/// Helper class to detect if the application is running in design mode.
/// </summary>
public static class DesignModeHelper
{
    /// <summary>
    /// Gets a value indicating whether the code is running in design mode (Visual Studio designer or Blend).
    /// </summary>
    public static bool IsInDesignMode
        => Windows.ApplicationModel.DesignMode.DesignModeEnabled;
}