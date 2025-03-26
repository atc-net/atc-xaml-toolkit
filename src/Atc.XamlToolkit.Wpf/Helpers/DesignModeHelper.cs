namespace Atc.XamlToolkit.Helpers;

public static class DesignModeHelper
{
    public static bool IsInDesignMode
        => DesignerProperties.GetIsInDesignMode(new DependencyObject());
}