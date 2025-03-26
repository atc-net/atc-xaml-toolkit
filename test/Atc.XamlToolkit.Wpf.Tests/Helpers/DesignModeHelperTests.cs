namespace Atc.XamlToolkit.Wpf.Tests.Helpers;

public sealed class DesignModeHelperTests
{
    [Fact]
    public void IsInDesignMode()
        => Assert.False(DesignModeHelper.IsInDesignMode);
}