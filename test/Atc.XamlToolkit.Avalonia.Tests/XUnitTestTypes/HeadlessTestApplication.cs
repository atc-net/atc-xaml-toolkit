namespace Atc.XamlToolkit.Avalonia.Tests.XUnitTestTypes;

public sealed class HeadlessTestApplication : global::Avalonia.Application
{
    /// <summary>
    /// Gets the application lifetime of the headless application.
    /// It cannot be replaced once the application is initialized, so tests configure this substitute.
    /// </summary>
    public static global::Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime Lifetime { get; }
        = Substitute.For<global::Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime>();

    public static global::Avalonia.AppBuilder BuildAvaloniaApp()
        => global::Avalonia.Headless.AvaloniaHeadlessPlatformExtensions.UseHeadless(
            global::Avalonia.AppBuilder.Configure<HeadlessTestApplication>(),
            new global::Avalonia.Headless.AvaloniaHeadlessPlatformOptions());

    public override void RegisterServices()
    {
        ApplicationLifetime = Lifetime;
        base.RegisterServices();
    }
}