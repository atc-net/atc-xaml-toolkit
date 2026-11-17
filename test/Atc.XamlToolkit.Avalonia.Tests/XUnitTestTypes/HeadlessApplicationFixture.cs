[assembly: AssemblyFixture(typeof(Atc.XamlToolkit.Avalonia.Tests.XUnitTestTypes.HeadlessApplicationFixture))]

namespace Atc.XamlToolkit.Avalonia.Tests.XUnitTestTypes;

/// <summary>
/// Starts the headless application before any test runs, so that its UI thread is the one Avalonia
/// regards as the UI thread - not the thread of whichever test happens to touch Avalonia first.
/// </summary>
public sealed class HeadlessApplicationFixture
{
    public HeadlessApplicationFixture()
        => HeadlessRunner.Start();
}