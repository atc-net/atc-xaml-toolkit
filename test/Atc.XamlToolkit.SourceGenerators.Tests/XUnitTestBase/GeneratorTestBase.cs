// ReSharper disable NotNullOrRequiredMemberIsNotInitialized
using TestResult = Atc.XUnit.TestResult;

namespace Atc.XamlToolkit.SourceGenerators.Tests.XUnitTestBase;

public abstract class GeneratorTestBase
{
    private static MetadataReference[] metadataReferences;

    protected GeneratorTestBase()
    {
        System.Reflection.Assembly.Load("Atc.XamlToolkit.SourceGenerators");

        // Force-load the runtime library so Atc.XamlToolkit.Mvvm.* types
        // (ObservableValidator, ViewModelBase, ObservableObject, …) appear
        // in the test AppDomain and are resolvable as metadata references.
        // Without this, semantic checks like InheritsFrom() can't walk the
        // base-type chain past first-hop user-declared bases.
        System.Reflection.Assembly.Load("Atc.XamlToolkit");

        metadataReferences = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic)
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToArray<MetadataReference>();
    }

    internal static (GeneratorRunResult GeneratorResult, ImmutableArray<Diagnostic> Diagnostics) RunGenerator<T>(
        params string[] inputCodes)
        where T : IIncrementalGenerator, new()
    {
        var inputCompilation = CreateCompilation(inputCodes);

        T generator = new();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(incrementalGenerators: generator);

        driver = driver.RunGeneratorsAndUpdateCompilation(
            inputCompilation,
            out _,
            out var diagnostics);

        var runResult = driver.GetRunResult();

        var generatorResult = runResult.Results[0];

        return (generatorResult, diagnostics);
    }

    /// <summary>
    /// Runs the generator twice on equivalent compilations with
    /// <c>trackIncrementalGeneratorSteps</c> enabled. The second run's tracked
    /// step results expose the cache verdict for each named pipeline stage —
    /// <see cref="IncrementalStepRunReason.Cached"/> means the generator did
    /// not re-emit code on the second run, which is the goal when models
    /// implement value equality.
    /// </summary>
    /// <typeparam name="T">The generator type.</typeparam>
    /// <param name="inputCodes">Source files to compile for both runs.</param>
    /// <returns>The second run's <see cref="GeneratorRunResult"/>, exposing TrackedSteps.</returns>
    internal static GeneratorRunResult RunGeneratorIncremental<T>(
        params string[] inputCodes)
        where T : IIncrementalGenerator, new()
    {
        var firstCompilation = CreateCompilation(inputCodes);

        // A fresh compilation with the same source content but different
        // syntax-tree identity simulates an unrelated file edit elsewhere in
        // the user's project — what we want is for the model-producing
        // pipeline stages to report Cached because their input values are
        // value-equal even though their inputs are reference-distinct.
        var secondCompilation = CreateCompilation(inputCodes);

        T generator = new();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: null,
            parseOptions: null,
            optionsProvider: null,
            driverOptions: new GeneratorDriverOptions(
                disabledOutputs: IncrementalGeneratorOutputKind.None,
                trackIncrementalGeneratorSteps: true));

        driver = driver.RunGenerators(firstCompilation);
        driver = driver.RunGenerators(secondCompilation);

        return driver.GetRunResult().Results[0];
    }

    /// <summary>
    /// Asserts that every output entry of the given tracked step on the second
    /// run reports <see cref="IncrementalStepRunReason.Cached"/> or
    /// <see cref="IncrementalStepRunReason.Unchanged"/> — i.e. the model is
    /// value-equal across runs and the pipeline does not re-emit downstream.
    /// </summary>
    /// <param name="runResult">The second run's result from <see cref="RunGeneratorIncremental{T}"/>.</param>
    /// <param name="trackingName">The pipeline-stage tracking name to assert on.</param>
    internal static void AssertTrackedStepIsCached(
        GeneratorRunResult runResult,
        string trackingName)
    {
        if (!runResult.TrackedSteps.TryGetValue(trackingName, out var steps))
        {
            Assert.Fail(
                $"Tracked step '{trackingName}' was not present in the run result. "
                + $"Available: {string.Join(", ", runResult.TrackedSteps.Keys)}");
            return;
        }

        foreach (var step in steps)
        {
            foreach (var output in step.Outputs)
            {
                if (output.Reason is IncrementalStepRunReason.Cached
                    or IncrementalStepRunReason.Unchanged)
                {
                    continue;
                }

                Assert.Fail(
                    $"Tracked step '{trackingName}' was re-evaluated on the second run "
                    + $"(reason: {output.Reason}). Expected Cached or Unchanged. "
                    + "This usually means a model in the pipeline lacks value-based equality.");
            }
        }
    }

    internal static void AssertGeneratorRunResultAsEqual(
        string expectedCode,
        (GeneratorRunResult GeneratorResult, ImmutableArray<Diagnostic> Diagnostics) generatorResult)
    {
        var generatedCode = string.Empty;

        if (generatorResult.GeneratorResult.GeneratedSources.Length == 1)
        {
            generatedCode = generatorResult.GeneratorResult.GeneratedSources[0].SourceText.ToString();
            if (expectedCode.EnsureEnvironmentNewLines() == generatedCode.EnsureEnvironmentNewLines())
            {
                return;
            }
        }

        var testResults = new List<TestResult>
        {
            new(true, 0, $"Expected code:{Environment.NewLine}{expectedCode}{Environment.NewLine}{Environment.NewLine}"),
            new(true, 0, $"Generated code:{Environment.NewLine}{generatedCode}{Environment.NewLine}{Environment.NewLine}"),
        };

        TestResultHelper.AssertOnTestResults(testResults);
    }

    internal static void AssertGeneratorRunResultIsEmpty(
        (GeneratorRunResult GeneratorResult, ImmutableArray<Diagnostic> Diagnostics) generatorResult)
    {
        if (generatorResult.GeneratorResult.GeneratedSources.Length == 0)
        {
            return;
        }

        var generatedCode = generatorResult.GeneratorResult.GeneratedSources[0].SourceText.ToString();

        var testResults = new List<TestResult>
        {
            new(true, 0, $"Expected no code:{Environment.NewLine}{Environment.NewLine}"),
            new(true, 0, $"Generated code:{Environment.NewLine}{generatedCode}{Environment.NewLine}{Environment.NewLine}"),
        };

        TestResultHelper.AssertOnTestResults(testResults);
    }

    internal static void AssertGeneratorRunResultHasDiagnostics(
        string[] diagnosticCodes,
        (GeneratorRunResult GeneratorResult, ImmutableArray<Diagnostic> Diagnostics) generatorResult)
    {
        var collectedCodes = generatorResult.Diagnostics
            .Select(diagnostic => diagnostic.Id)
            .Order(StringComparer.Ordinal)
            .ToList();

        var orderedDiagnosticCodes = diagnosticCodes
            .Order(StringComparer.Ordinal)
            .ToList();

        if (collectedCodes.SequenceEqual(orderedDiagnosticCodes, StringComparer.Ordinal))
        {
            return;
        }

        var testResults = new List<TestResult>
        {
            new(true, 0, $"Expected error codes:{Environment.NewLine}{string.Join(", ", diagnosticCodes)}{Environment.NewLine}{Environment.NewLine}"),
            new(true, 0, $"Collected error codes:{Environment.NewLine}{string.Join(", ", collectedCodes)}{Environment.NewLine}{Environment.NewLine}"),
        };

        TestResultHelper.AssertOnTestResults(testResults);
    }

    private static Compilation CreateCompilation(params string[] sources)
    {
        var syntaxTrees = sources
            .Select(source => CSharpSyntaxTree.ParseText(source))
            .ToArray();

        return CSharpCompilation.Create(
            "compilation",
            syntaxTrees,
            metadataReferences,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}