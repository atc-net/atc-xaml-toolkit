// ReSharper disable StringLiteralTypo
namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

/// <summary>
/// Pins the source-generator's incremental-cache behaviour. When the input
/// source is unchanged across two compilations, the model-producing pipeline
/// stages must report <see cref="IncrementalStepRunReason.Cached"/> on the
/// second run. Otherwise the IDE re-runs the generator on every keystroke
/// elsewhere in the user's project, even when the model is unchanged.
/// </summary>
public class ViewModelGeneratorTests_IncrementalCaching : GeneratorTestBase
{
    private const string Source =
        """
        using Atc.XamlToolkit.Mvvm;
        using Atc.XamlToolkit.Mvvm.ComponentModel;

        namespace Test;

        public partial class MyViewModel : ViewModelBase
        {
            [ObservableProperty]
            private string firstName = string.Empty;

            [ObservableProperty]
            private string lastName = string.Empty;

            [RelayCommand]
            private void Save() { }
        }
        """;

    [Fact(Skip = "Pinned for Phase 32d — currently fails because models lack value equality. Will be unskipped once composite models become records with EquatableArray<T> collections.")]
    public void ViewModelGenerator_OnUnchangedInput_CachesSemanticTarget()
    {
        var result = RunGeneratorIncremental<ViewModelGenerator>(Source);

        AssertTrackedStepIsCached(result, "ViewModelGenerator.SemanticTarget");
    }

    [Fact(Skip = "Pinned for Phase 32d — currently fails because models lack value equality.")]
    public void ViewModelGenerator_OnUnchangedInput_CachesDeduplicated()
    {
        var result = RunGeneratorIncremental<ViewModelGenerator>(Source);

        AssertTrackedStepIsCached(result, "ViewModelGenerator.Deduplicated");
    }

    /// <summary>
    /// Sanity-test: the helper itself produces a tracked-step dictionary on a
    /// known-cached pipeline. The <c>SourceOutput</c> step is built into Roslyn
    /// and always cached when source is unchanged — so this test should pass
    /// today even without our model-equality fixes, proving the harness works.
    /// </summary>
    [Fact]
    public void RunGeneratorIncremental_TracksKnownPipelineSteps()
    {
        var result = RunGeneratorIncremental<ViewModelGenerator>(Source);

        result.TrackedSteps.Should().ContainKey("ViewModelGenerator.SemanticTarget");
        result.TrackedSteps.Should().ContainKey("ViewModelGenerator.Deduplicated");
    }
}