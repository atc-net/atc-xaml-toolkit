// ReSharper disable InvertIf
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
namespace Atc.XamlToolkit.SourceGenerators.Generators;

/// <summary>
/// Source generator for generating view model properties and commands.
/// </summary>
[Generator]
public sealed class ViewModelGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the source generator.
    /// </summary>
    /// <param name="context">The initialization context.</param>
    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK — three SyntaxProvider pipelines.")]
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        ////#if DEBUG
        ////        if (!System.Diagnostics.Debugger.IsAttached)
        ////        {
        ////            System.Diagnostics.Debugger.Launch();
        ////        }
        ////#endif

        var viewModelsToGenerate = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntaxNode, _) => IsSyntaxTarget(syntaxNode),
                transform: static (context, _) => GetSemanticTarget(context))
            .Where(static target => target is not null)
            .WithTrackingName("ViewModelGenerator.SemanticTarget")
            .Collect()
            .Select(static (viewModels, _) => viewModels
                .GroupBy(vm => vm!.GeneratedFileName, StringComparer.Ordinal)
                .Select(static group => group.First())
                .ToImmutableArray())
            .WithTrackingName("ViewModelGenerator.Deduplicated");

        context.RegisterSourceOutput(
            viewModelsToGenerate,
            static (spc, sources) =>
            {
                foreach (var source in sources)
                {
                    Execute(spc, source);
                }
            });

        // Surface a diagnostic on classes that contain [ObservableProperty] /
        // [RelayCommand] / [ComputedProperty] but are NOT declared partial — the
        // generator silently skips them today, which is hard to diagnose for
        // first-time users.
        var missingPartialClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntaxNode, _) => IsMissingPartialTarget(syntaxNode),
                transform: static (context, _) => GetMissingPartialDiagnostic(context))
            .Where(static d => d is not null)
            .WithTrackingName("ViewModelGenerator.MissingPartialDiagnostic");

        context.RegisterSourceOutput(
            missingPartialClasses,
            static (spc, diagnostic) =>
            {
                if (diagnostic is not null)
                {
                    spc.ReportDiagnostic(diagnostic);
                }
            });

        // Pipeline 3: classes annotated with [INotifyPropertyChanged] — emit the
        // INPC scaffolding (event + RaisePropertyChanged + OnPropertyChanged + Set<T>)
        // so the class can act as its own INPC source without inheriting from
        // ObservableObject. Compatible with [ObservableProperty] on the same class.
        var inpcTargets = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntaxNode, _) => IsINotifyPropertyChangedTarget(syntaxNode),
                transform: static (context, _) => GetINotifyPropertyChangedTarget(context))
            .Where(static target => target is not null)
            .WithTrackingName("ViewModelGenerator.INotifyPropertyChangedTarget");

        context.RegisterSourceOutput(
            inpcTargets,
            static (spc, target) =>
            {
                if (target is not null)
                {
                    ExecuteINotifyPropertyChanged(spc, target);
                }
            });
    }

    private static bool IsMissingPartialTarget(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not ClassDeclarationSyntax classDeclaration)
        {
            return false;
        }

        // Only flag classes that are NOT partial.
        if (classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
        {
            return false;
        }

        // Class-level [INotifyPropertyChanged] attribute also requires partial.
        if (HasINotifyPropertyChangedAttribute(classDeclaration.AttributeLists))
        {
            return true;
        }

        // …or DO have at least one member tagged with one of the generator-relevant attributes.
        return classDeclaration.Members.Any(member => member switch
        {
            FieldDeclarationSyntax { AttributeLists.Count: > 0 } field =>
                HasRelevantAttribute(field.AttributeLists),
            PropertyDeclarationSyntax { AttributeLists.Count: > 0 } property =>
                HasRelevantAttribute(property.AttributeLists),
            MethodDeclarationSyntax { AttributeLists.Count: > 0 } method =>
                HasRelevantAttribute(method.AttributeLists),
            _ => false,
        });
    }

    private static Diagnostic? GetMissingPartialDiagnostic(
        GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        return DiagnosticFactory.CreateMissingPartialKeyword(
            classDeclaration.Identifier.Text,
            classDeclaration.Identifier.GetLocation());
    }

    /// <summary>
    /// Determines if a given syntax node is a valid target for code generation (predicate phase).
    /// </summary>
    /// <param name="syntaxNode">The syntax node to check.</param>
    /// <returns>True if the node is a valid target; otherwise, false.</returns>
    /// <remarks>
    /// This method performs early filtering to optimize performance by checking:
    /// <list type="bullet">
    /// <item><description>The node is a partial class declaration (required for source generation)</description></item>
    /// <item><description>The class has a base list (inherits from ViewModelBase or ObservableObject)</description></item>
    /// <item><description>The class contains fields, properties, or methods with ObservableProperty, ComputedProperty, or RelayCommand attributes</description></item>
    /// </list>
    /// The semantic analysis will then check for fields/properties with ObservableProperty attribute
    /// or methods with RelayCommand attribute.
    /// </remarks>
    private static bool IsSyntaxTarget(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not ClassDeclarationSyntax classDeclaration)
        {
            return false;
        }

        // Must be partial (required for source generation)
        if (!classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
        {
            return false;
        }

        // Accept 'if' has base list (might be inheriting from ViewModelBase/ObservableObject)
        // This is important for multi-file scenarios where base list and attributes are in different files
        if (classDeclaration.BaseList is not null)
        {
            return true;
        }

        // Also accept if class has any members with ViewModel-related attributes
        // This catches classes without explicit base list but using our attributes
        return classDeclaration.Members.Any(member => member switch
        {
            // Fields with attributes (for ObservableProperty)
            FieldDeclarationSyntax { AttributeLists.Count: > 0 } field =>
                HasRelevantAttribute(field.AttributeLists),

            // Properties with attributes (for ComputedProperty)
            PropertyDeclarationSyntax { AttributeLists.Count: > 0 } property =>
                HasRelevantAttribute(property.AttributeLists),

            // Methods with attributes (for RelayCommand)
            MethodDeclarationSyntax { AttributeLists.Count: > 0 } method =>
                HasRelevantAttribute(method.AttributeLists),

            _ => false,
        });
    }

    /// <summary>
    /// Checks if the attribute lists contain any ViewModel-related attributes.
    /// This performs a fast syntax-only check for attribute names.
    /// </summary>
    /// <remarks>
    /// ViewModel attributes (all platform-agnostic, work on WPF, WinUI, and Avalonia):
    /// <list type="bullet">
    /// <item><description><b>ObservableProperty</b> - Generates a property with INotifyPropertyChanged implementation from a field</description></item>
    /// <item><description><b>ComputedProperty</b> - Generates a property that automatically raises PropertyChanged when dependencies change</description></item>
    /// <item><description><b>RelayCommand</b> - Generates IRelayCommand or IRelayCommandAsync from methods</description></item>
    /// <item><description><b>NotifyPropertyChangedFor</b> - Specifies additional properties to notify when a property changes</description></item>
    /// </list>
    /// These attributes work with classes inheriting from ViewModelBase or ObservableObject.
    /// </remarks>
    private static bool HasRelevantAttribute(
        SyntaxList<AttributeListSyntax> attributeLists)
    {
        foreach (var attributeList in attributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                // Get the attribute name - for generic attributes,
                // we need to check the base identifier name
                var attributeName = attribute.Name switch
                {
                    GenericNameSyntax genericName => genericName.Identifier.Text,
                    _ => attribute.Name.ToString(),
                };

                // Check for ViewModel attributes (with or without "Attribute" suffix)
                // All these attributes are platform-agnostic and work across WPF, WinUI, and Avalonia
                if (attributeName is

                    // Generates observable properties from fields
                    NameConstants.ObservableProperty or NameConstants.ObservablePropertyAttribute or

                    // Generates computed properties with automatic dependency tracking
                    NameConstants.ComputedProperty or NameConstants.ComputedPropertyAttribute or

                    // Generates relay commands from methods
                    NameConstants.RelayCommand or NameConstants.RelayCommandAttribute or

                    // Specifies additional properties to notify on change
                    NameConstants.NotifyPropertyChangedFor or NameConstants.NotifyPropertyChangedForAttribute or

                    // Specifies additional commands whose CanExecute should re-evaluate on change
                    NameConstants.NotifyCanExecuteChangedFor or NameConstants.NotifyCanExecuteChangedForAttribute)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Extracts the semantic target for code generation (transform phase).
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <returns>A ViewModelToGenerate object if valid; otherwise, null.</returns>
    /// <remarks>
    /// This method uses <c>CheckBaseClasses</c> to ensure the ViewModel inherits
    /// from a valid base class. This check is necessary to correctly identify ViewModels
    /// even if their base class is defined in another file.
    ///
    /// The valid base classes are:
    /// <list type="bullet">
    /// <item><description>ViewModelBase</description></item>
    /// <item><description>MainWindowViewModelBase</description></item>
    /// <item><description>ViewModelDialogBase</description></item>
    /// <item><description>ObservableObject</description></item>
    /// </list>
    /// </remarks>
    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    private static ViewModelToGenerate? GetSemanticTarget(
        GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);

        if (classSymbol is null)
        {
            return null;
        }

        // Skip if an earlier partial declaration already qualifies via IsSyntaxTarget,
        // so only the first qualifying declaration triggers the full semantic analysis.
        foreach (var declRef in classSymbol.DeclaringSyntaxReferences)
        {
            if (declRef.SyntaxTree == classDeclarationSyntax.SyntaxTree &&
                declRef.Span == classDeclarationSyntax.Span)
            {
                break;
            }

            if (IsSyntaxTarget(declRef.GetSyntax()))
            {
                return null;
            }
        }

        var allPartialDeclarations = classSymbol.GetAllPartialClassDeclarations();

        var (hasAnyBase, inheritFromViewModel) = classSymbol.CheckBaseClasses();

        // [INotifyPropertyChanged] gives the class its own RaisePropertyChanged via the
        // INPC pipeline, so it qualifies as a valid base for [ObservableProperty] etc.
        // even without ObservableObject / ViewModelBase inheritance.
        if (!hasAnyBase && !ClassHasINotifyPropertyChangedAttribute(classSymbol))
        {
            return null;
        }

        var allObservableProperties = new List<ObservablePropertyToGenerate>();
        var allRelayCommands = new List<RelayCommandToGenerate>();
        var allComputedProperties = new List<ComputedPropertyToGenerate>();

        foreach (var partialClassSyntax in allPartialDeclarations)
        {
            if (context.SemanticModel.Compilation
                    .GetSemanticModel(partialClassSyntax.SyntaxTree)
                    .GetDeclaredSymbol(partialClassSyntax) is not { } partialClassSymbol)
            {
                continue;
            }

            var result = ViewModelInspector.Inspect(
                partialClassSymbol,
                inheritFromViewModel);

            if (!result.FoundAnythingToGenerate)
            {
                continue;
            }

            result.ApplyCommandsAndProperties(
                allObservableProperties,
                allRelayCommands,
                allComputedProperties);
        }

        if (allObservableProperties.Count == 0 &&
            allRelayCommands.Count == 0)
        {
            return null;
        }

        ComputedPropertyInspector.LinkToObservableProperties(allObservableProperties, allComputedProperties);

        var viewModelToGenerate = new ViewModelToGenerate(
            namespaceName: classSymbol.ContainingNamespace.ToDisplayString(),
            className: classSymbol.Name,
            accessModifier: classSymbol.GetAccessModifier())
        {
            XamlPlatform = context.SemanticModel.Compilation.GetXamlPlatform(),
            PropertiesToGenerate = allObservableProperties,
            RelayCommandsToGenerate = allRelayCommands,
        };

        return viewModelToGenerate;
    }

    /// <summary>
    /// Executes the source generation process.
    /// </summary>
    /// <param name="context">The source production context.</param>
    /// <param name="viewModelToGenerate">The ViewModel to generate.</param>
    private static void Execute(
        SourceProductionContext context,
        ViewModelToGenerate? viewModelToGenerate)
    {
        if (viewModelToGenerate is null)
        {
            return;
        }

        var viewModelBuilder = new ViewModelBuilder
        {
            XamlPlatform = viewModelToGenerate.XamlPlatform,
        };

        viewModelBuilder.GenerateStart(viewModelToGenerate);

        if (viewModelToGenerate.ContainsRelayCommandNameDuplicates)
        {
            context.ReportDiagnostic(DiagnosticFactory.CreateContainsDuplicateNamesForRelayCommand());
        }
        else
        {
            viewModelBuilder.GenerateRelayCommands(viewModelBuilder, viewModelToGenerate.RelayCommandsToGenerate);
        }

        viewModelBuilder.GenerateProperties(viewModelToGenerate.PropertiesToGenerate);

        viewModelBuilder.GenerateRelayCommandMethods(viewModelBuilder, viewModelToGenerate.RelayCommandsToGenerate);

        viewModelBuilder.GenerateEnd();

        var sourceText = viewModelBuilder.ToSourceText();

        context.AddSource(
            viewModelToGenerate.GeneratedFileName,
            sourceText);
    }

    private static bool ClassHasINotifyPropertyChangedAttribute(
        INamedTypeSymbol classSymbol)
    {
        foreach (var attribute in classSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.Name
                is NameConstants.INotifyPropertyChanged
                or NameConstants.INotifyPropertyChangedAttribute)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsINotifyPropertyChangedTarget(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not ClassDeclarationSyntax classDeclaration)
        {
            return false;
        }

        if (!classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
        {
            return false;
        }

        return HasINotifyPropertyChangedAttribute(classDeclaration.AttributeLists);
    }

    private static bool HasINotifyPropertyChangedAttribute(
        SyntaxList<AttributeListSyntax> attributeLists)
    {
        foreach (var attributeList in attributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var attributeName = attribute.Name switch
                {
                    GenericNameSyntax genericName => genericName.Identifier.Text,
                    _ => attribute.Name.ToString(),
                };

                if (attributeName is
                    NameConstants.INotifyPropertyChanged or
                    NameConstants.INotifyPropertyChangedAttribute)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static INotifyPropertyChangedTarget? GetINotifyPropertyChangedTarget(
        GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);

        if (classSymbol is null)
        {
            return null;
        }

        // Only emit once per type — pick the first qualifying partial declaration.
        foreach (var declRef in classSymbol.DeclaringSyntaxReferences)
        {
            if (declRef.SyntaxTree == classDeclaration.SyntaxTree &&
                declRef.Span == classDeclaration.Span)
            {
                break;
            }

            if (declRef.GetSyntax() is ClassDeclarationSyntax priorDecl &&
                IsINotifyPropertyChangedTarget(priorDecl))
            {
                return null;
            }
        }

        return new INotifyPropertyChangedTarget(
            namespaceName: classSymbol.ContainingNamespace.ToDisplayString(),
            className: classSymbol.Name,
            accessModifier: classSymbol.GetAccessModifier());
    }

    private static void ExecuteINotifyPropertyChanged(
        SourceProductionContext context,
        INotifyPropertyChangedTarget? target)
    {
        if (target is null)
        {
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.ComponentModel;");
        sb.AppendLine("using System.Runtime.CompilerServices;");
        sb.AppendLine();
        sb.AppendLine($"namespace {target.NamespaceName};");
        sb.AppendLine();
        sb.AppendLine($"{target.AccessModifier} partial class {target.ClassName} : INotifyPropertyChanged");
        sb.AppendLine("{");
        sb.AppendLine("    public event PropertyChangedEventHandler? PropertyChanged;");
        sb.AppendLine();
        sb.AppendLine("    protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)");
        sb.AppendLine("        => PropertyChanged?.Invoke(this, Atc.XamlToolkit.Mvvm.PropertyChangedEventArgsCache.Get(propertyName));");
        sb.AppendLine();
        sb.AppendLine("    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)");
        sb.AppendLine("        => RaisePropertyChanged(propertyName);");
        sb.AppendLine();
        sb.AppendLine("    protected bool Set<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (EqualityComparer<T>.Default.Equals(field, newValue))");
        sb.AppendLine("        {");
        sb.AppendLine("            return false;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        field = newValue;");
        sb.AppendLine("        RaisePropertyChanged(propertyName);");
        sb.AppendLine("        return true;");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.Append("#nullable disable");

        context.AddSource(
            $"{target.ClassName}.INotifyPropertyChanged.g.cs",
            SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    [SuppressMessage("Naming", "S101:Types should be named in PascalCase", Justification = "Name intentionally mirrors the System.ComponentModel.INotifyPropertyChanged interface for symmetry with the public attribute.")]
    private sealed class INotifyPropertyChangedTarget(
        string namespaceName,
        string className,
        string accessModifier)
    {
        public string NamespaceName { get; } = namespaceName;

        public string ClassName { get; } = className;

        public string AccessModifier { get; } = accessModifier;
    }
}