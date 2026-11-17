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
    /// Runs the generator, then adds an unrelated file to the compilation and runs it again -
    /// what happens in the IDE when another file is edited: the syntax trees of the given
    /// sources stay the same instances, only the compilation changes.
    /// </summary>
    /// <typeparam name="T">The generator type.</typeparam>
    /// <param name="inputCodes">Source files to compile.</param>
    /// <returns>The second run's <see cref="GeneratorRunResult"/>, exposing TrackedSteps.</returns>
    internal static GeneratorRunResult RunGeneratorIncrementalAfterUnrelatedEdit<T>(
        params string[] inputCodes)
        where T : IIncrementalGenerator, new()
    {
        var firstCompilation = CreateCompilation(inputCodes);

        var secondCompilation = firstCompilation.AddSyntaxTrees(
            CSharpSyntaxTree.ParseText("namespace Unrelated { public class Other { } }"));

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
    /// Runs the generator, then replaces one source file with an edited version and runs it
    /// again. The other syntax trees stay the same instances, as in the IDE.
    /// </summary>
    /// <typeparam name="T">The generator type.</typeparam>
    /// <param name="inputCodes">Source files to compile.</param>
    /// <param name="editedIndex">The index of the source file that is edited.</param>
    /// <param name="editedCode">The new content of that source file.</param>
    /// <returns>The second run's <see cref="GeneratorRunResult"/>, exposing TrackedOutputSteps.</returns>
    internal static GeneratorRunResult RunGeneratorIncrementalAfterEdit<T>(
        string[] inputCodes,
        int editedIndex,
        string editedCode)
        where T : IIncrementalGenerator, new()
    {
        var firstCompilation = CreateCompilation(inputCodes);

        var secondCompilation = firstCompilation.ReplaceSyntaxTree(
            firstCompilation.SyntaxTrees.ElementAt(editedIndex),
            CSharpSyntaxTree.ParseText(editedCode));

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

    /// <summary>
    /// Runs all three generators together on the given sources - the way a consumer project
    /// does - and asserts that no generator throws and that the resulting compilation
    /// (user code + generated code) has no errors.
    /// </summary>
    /// <remarks>
    /// The compilation mimics a minimal Avalonia consumer: the SDK implicit usings plus
    /// <c>Atc.XamlToolkit.Mvvm</c> and <c>Atc.XamlToolkit.Command</c>, nullable enabled, and
    /// references to <c>Atc.XamlToolkit</c>, <c>Atc.XamlToolkit.Avalonia</c> and Avalonia.
    /// </remarks>
    /// <param name="inputCodes">Source files to compile.</param>
    internal static void AssertCompilesWithAllGenerators(
        params string[] inputCodes)
    {
        var problems = CompileWithAllGenerators(inputCodes);
        if (problems.Count == 0)
        {
            return;
        }

        Assert.Fail(string.Join(Environment.NewLine, problems));
    }

    /// <summary>
    /// Runs all three generators together on the given sources and returns the generator
    /// exceptions and compile errors, followed by the generated sources.
    /// The list is empty when everything compiles.
    /// </summary>
    /// <param name="inputCodes">Source files to compile.</param>
    /// <returns>The problems found.</returns>
    internal static IReadOnlyList<string> CompileWithAllGenerators(
        params string[] inputCodes)
        => CompileWithAllGenerators(useConsumerGlobalUsings: true, inputCodes);

    /// <summary>
    /// Like <see cref="AssertCompilesWithAllGenerators"/>, but the consumer project has no
    /// global usings at all: the generated files must bring every namespace they need.
    /// </summary>
    /// <param name="inputCodes">Source files to compile.</param>
    internal static void AssertCompilesWithAllGeneratorsWithoutGlobalUsings(
        params string[] inputCodes)
    {
        var problems = CompileWithAllGenerators(useConsumerGlobalUsings: false, inputCodes);
        if (problems.Count == 0)
        {
            return;
        }

        Assert.Fail(string.Join(Environment.NewLine, problems));
    }

    /// <summary>
    /// Runs all three generators together on the given sources, in the same minimal Avalonia
    /// consumer compilation as <see cref="AssertCompilesWithAllGenerators"/>, and returns the
    /// generated source texts.
    /// </summary>
    /// <param name="inputCodes">Source files to compile.</param>
    /// <returns>The text of every generated file.</returns>
    internal static IReadOnlyList<string> GetSourcesGeneratedForAvaloniaConsumer(
        params string[] inputCodes)
        => RunAllGeneratorsForConsumer(useConsumerGlobalUsings: true, inputCodes, out _)
            .Results
            .SelectMany(x => x.GeneratedSources)
            .Select(x => x.SourceText.ToString())
            .ToList();

    private static IReadOnlyList<string> CompileWithAllGenerators(
        bool useConsumerGlobalUsings,
        string[] inputCodes)
    {
        var runResult = RunAllGeneratorsForConsumer(useConsumerGlobalUsings, inputCodes, out var outputCompilation);

        var problems = new List<string>();

        foreach (var result in runResult.Results.Where(x => x.Exception is not null))
        {
            problems.Add($"Generator exception: {result.Exception!.GetType().Name}: {result.Exception.Message}");
        }

        return CollectCompileProblems(runResult, outputCompilation, problems);
    }

    private static GeneratorDriverRunResult RunAllGeneratorsForConsumer(
        bool useConsumerGlobalUsings,
        string[] inputCodes,
        out Compilation outputCompilation)
    {
        const string consumerGlobalUsings =
            """
            global using System;
            global using System.Collections.Generic;
            global using System.IO;
            global using System.Linq;
            global using System.Threading;
            global using System.Threading.Tasks;
            global using Atc.XamlToolkit.Command;
            global using Atc.XamlToolkit.Mvvm;
            """;

        var syntaxTrees = inputCodes
            .Prepend(useConsumerGlobalUsings ? consumerGlobalUsings : "// No global usings.")
            .Select((source, index) => CSharpSyntaxTree.ParseText(source, path: $"Input{index}.cs"))
            .ToArray();

        var inputCompilation = CSharpCompilation.Create(
            "compilation",
            syntaxTrees,
            ConsumerMetadataReferences.Value,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            new ViewModelGenerator().AsSourceGenerator(),
            new ObservableDtoViewModelGenerator().AsSourceGenerator(),
            new FrameworkElementGenerator().AsSourceGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(
            inputCompilation,
            out outputCompilation,
            out _);

        return driver.GetRunResult();
    }

    private static IReadOnlyList<string> CollectCompileProblems(
        GeneratorDriverRunResult runResult,
        Compilation outputCompilation,
        List<string> problems)
    {
        problems.AddRange(
            outputCompilation
                .GetDiagnostics()
                .Where(x => x.Severity == DiagnosticSeverity.Error)
                .Select(x => $"Compile error: {x.Id} {x.GetMessage(System.Globalization.CultureInfo.InvariantCulture)} @ {x.Location.GetLineSpan()}"));

        if (problems.Count > 0)
        {
            problems.AddRange(
                runResult.Results
                    .SelectMany(x => x.GeneratedSources)
                    .Select(x => $"{Environment.NewLine}----- {x.HintName}{Environment.NewLine}{x.SourceText}"));
        }

        return problems;
    }

    private static readonly Lazy<MetadataReference[]> ConsumerMetadataReferences = new(CreateConsumerMetadataReferences);

    private static MetadataReference[] CreateConsumerMetadataReferences()
    {
        var frameworkAssemblies = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Where(path =>
            {
                var fileName = Path.GetFileName(path);
                return fileName.StartsWith("System.", StringComparison.Ordinal)
                       || fileName is "mscorlib.dll" or "netstandard.dll";
            });

        var consumerAssemblies = Directory
            .GetFiles(AppContext.BaseDirectory, "*.dll")
            .Where(path =>
            {
                var fileName = Path.GetFileName(path);
                return fileName is "Atc.XamlToolkit.dll" or "Atc.XamlToolkit.Avalonia.dll"
                       || fileName.StartsWith("Avalonia.", StringComparison.Ordinal);
            });

        return frameworkAssemblies
            .Concat(consumerAssemblies)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
            .ToArray();
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