namespace Atc.XamlToolkit.Avalonia.Tests.XUnitTestTypes;

public sealed class HeadlessTestApplication : global::Avalonia.Application
{
    public static global::Avalonia.AppBuilder BuildAvaloniaApp()
        => global::Avalonia.Headless.AvaloniaHeadlessPlatformExtensions.UseHeadless(
            global::Avalonia.AppBuilder.Configure<HeadlessTestApplication>(),
            new global::Avalonia.Headless.AvaloniaHeadlessPlatformOptions());
}