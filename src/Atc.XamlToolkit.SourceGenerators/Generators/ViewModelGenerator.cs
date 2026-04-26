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

        // Pipeline 3: [ObservableProperty] field-level validation — surface
        // diagnostics for fields the generator currently skips silently
        // (non-private, PascalCase). Catches a class of footguns where the
        // user expects a property but the generator emits nothing.
        var invalidObservablePropertyFields = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntaxNode, _) => IsObservablePropertyFieldTarget(syntaxNode),
                transform: static (context, _) => GetObservablePropertyFieldDiagnostics(context))
            .Where(static diagnostics => diagnostics is { Count: > 0 })
            .WithTrackingName("ViewModelGenerator.ObservablePropertyFieldDiagnostics");

        context.RegisterSourceOutput(
            invalidObservablePropertyFields,
            static (spc, diagnostics) =>
            {
                if (diagnostics is null)
                {
                    return;
                }

                foreach (var diagnostic in diagnostics)
                {
                    spc.ReportDiagnostic(diagnostic);
                }
            });

        // Pipeline 4: validate [NotifyPropertyChangedFor("X")] references.
        // Catches typos and renames that today produce a confusing
        // 'CS0103: name X does not exist' inside the generated file.
        var notifyForRefDiagnostics = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntaxNode, _) => IsClassWithNotifyPropertyChangedForUsage(syntaxNode),
                transform: static (context, _) => GetNotifyPropertyChangedForDiagnostics(context))
            .Where(static diagnostics => diagnostics is { Count: > 0 })
            .WithTrackingName("ViewModelGenerator.NotifyPropertyChangedForDiagnostics");

        context.RegisterSourceOutput(
            notifyForRefDiagnostics,
            static (spc, diagnostics) =>
            {
                if (diagnostics is null)
                {
                    return;
                }

                foreach (var diagnostic in diagnostics)
                {
                    spc.ReportDiagnostic(diagnostic);
                }
            });

        // Pipeline 5: validate [NotifyCanExecuteChangedFor("X")] references.
        // Same shape as pipeline 4 but for command references.
        var notifyCanExecForRefDiagnostics = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntaxNode, _) => IsClassWithNotifyCanExecuteChangedForUsage(syntaxNode),
                transform: static (context, _) => GetNotifyCanExecuteChangedForDiagnostics(context))
            .Where(static diagnostics => diagnostics is { Count: > 0 })
            .WithTrackingName("ViewModelGenerator.NotifyCanExecuteChangedForDiagnostics");

        context.RegisterSourceOutput(
            notifyCanExecForRefDiagnostics,
            static (spc, diagnostics) =>
            {
                if (diagnostics is null)
                {
                    return;
                }

                foreach (var diagnostic in diagnostics)
                {
                    spc.ReportDiagnostic(diagnostic);
                }
            });

        // Pipeline 6: validate [ComputedProperty] dependency detection.
        // Surface a diagnostic for properties whose getter doesn't reference
        // any other property — the inspector silently filters them out today,
        // so the user gets nothing and can't tell why.
        var computedPropertyDiagnostics = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntaxNode, _) => IsClassWithComputedPropertyUsage(syntaxNode),
                transform: static (context, _) => GetComputedPropertyDiagnostics(context))
            .Where(static diagnostics => diagnostics is { Count: > 0 })
            .WithTrackingName("ViewModelGenerator.ComputedPropertyDiagnostics");

        context.RegisterSourceOutput(
            computedPropertyDiagnostics,
            static (spc, diagnostics) =>
            {
                if (diagnostics is null)
                {
                    return;
                }

                foreach (var diagnostic in diagnostics)
                {
                    spc.ReportDiagnostic(diagnostic);
                }
            });

        // Pipeline 7: classes annotated with [INotifyPropertyChanged] — emit the
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

        // Pipeline 8: validate [NotifyDataErrorInfo] inheritance requirement.
        // The generator emits ValidateProperty(...) in the setter, which only
        // exists on ObservableValidator (and ViewModelBase via inheritance).
        // Surface a diagnostic up-front so the user gets a friendly message
        // instead of CS0103 inside the generated file.
        var notifyDataErrorInfoDiagnostics = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntaxNode, _) => IsClassWithNotifyDataErrorInfoUsage(syntaxNode),
                transform: static (context, _) => GetNotifyDataErrorInfoDiagnostics(context))
            .Where(static diagnostics => diagnostics is { Count: > 0 })
            .WithTrackingName("ViewModelGenerator.NotifyDataErrorInfoDiagnostics");

        context.RegisterSourceOutput(
            notifyDataErrorInfoDiagnostics,
            static (spc, diagnostics) =>
            {
                if (diagnostics is null)
                {
                    return;
                }

                foreach (var diagnostic in diagnostics)
                {
                    spc.ReportDiagnostic(diagnostic);
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
                    NameConstants.NotifyCanExecuteChangedFor or NameConstants.NotifyCanExecuteChangedForAttribute or

                    // Opts the setter into inline validation via ObservableValidator.ValidateProperty
                    NameConstants.NotifyDataErrorInfo or NameConstants.NotifyDataErrorInfoAttribute)
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
            NamespaceName: classSymbol.ContainingNamespace.ToDisplayString(),
            ClassName: classSymbol.Name,
            ClassAccessModifier: classSymbol.GetAccessModifier(),
            XamlPlatform: context.SemanticModel.Compilation.GetXamlPlatform(),
            PropertiesToGenerate: new EquatableArray<ObservablePropertyToGenerate>(allObservableProperties.ToArray()),
            RelayCommandsToGenerate: new EquatableArray<RelayCommandToGenerate>(allRelayCommands.ToArray()));

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

    private static bool IsObservablePropertyFieldTarget(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not FieldDeclarationSyntax fieldDeclaration)
        {
            return false;
        }

        if (fieldDeclaration.AttributeLists.Count == 0)
        {
            return false;
        }

        foreach (var attributeList in fieldDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var attributeName = attribute.Name switch
                {
                    GenericNameSyntax genericName => genericName.Identifier.Text,
                    _ => attribute.Name.ToString(),
                };

                if (attributeName is
                    NameConstants.ObservableProperty or
                    NameConstants.ObservablePropertyAttribute)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static List<Diagnostic>? GetObservablePropertyFieldDiagnostics(
        GeneratorSyntaxContext context)
    {
        var fieldDeclaration = (FieldDeclarationSyntax)context.Node;
        List<Diagnostic>? diagnostics = null;

        // The accessibility check is on the declaration's modifiers; default
        // to private when no modifier is present.
        var hasPrivateModifier = fieldDeclaration.Modifiers
            .Any(m => m.IsKind(SyntaxKind.PrivateKeyword));
        var hasNonPrivateModifier = fieldDeclaration.Modifiers
            .Any(m => m.IsKind(SyntaxKind.PublicKeyword)
                || m.IsKind(SyntaxKind.InternalKeyword)
                || m.IsKind(SyntaxKind.ProtectedKeyword));

        foreach (var variable in fieldDeclaration.Declaration.Variables)
        {
            var fieldName = variable.Identifier.Text;
            if (string.IsNullOrEmpty(fieldName))
            {
                continue;
            }

            if (!hasPrivateModifier && hasNonPrivateModifier)
            {
                diagnostics ??= [];
                diagnostics.Add(DiagnosticFactory.CreateObservablePropertyFieldNotPrivate(
                    fieldName,
                    variable.Identifier.GetLocation()));
            }

            if (char.IsUpper(fieldName[0]))
            {
                diagnostics ??= [];
                diagnostics.Add(DiagnosticFactory.CreateObservablePropertyFieldNameNotCamelCase(
                    fieldName,
                    variable.Identifier.GetLocation()));
            }
        }

        return diagnostics;
    }

    private static bool IsClassWithNotifyPropertyChangedForUsage(
        SyntaxNode syntaxNode)
    {
        if (syntaxNode is not ClassDeclarationSyntax classDeclaration)
        {
            return false;
        }

        foreach (var member in classDeclaration.Members)
        {
            if (member is not FieldDeclarationSyntax fieldDeclaration ||
                fieldDeclaration.AttributeLists.Count == 0)
            {
                continue;
            }

            foreach (var attributeList in fieldDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var attributeName = attribute.Name switch
                    {
                        GenericNameSyntax genericName => genericName.Identifier.Text,
                        _ => attribute.Name.ToString(),
                    };

                    if (attributeName is
                        NameConstants.NotifyPropertyChangedFor or
                        NameConstants.NotifyPropertyChangedForAttribute)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK — single-pass diagnostic emission across all fields with [NotifyPropertyChangedFor].")]
    private static List<Diagnostic>? GetNotifyPropertyChangedForDiagnostics(
        GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
        if (classSymbol is null)
        {
            return null;
        }

        // Build the set of property names that will exist on the final type:
        //   1. Declared properties (across all partial halves)
        //   2. Properties to be generated from [ObservableProperty] fields
        var knownPropertyNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var member in classSymbol.GetMembers())
        {
            if (member is IPropertySymbol propertySymbol)
            {
                knownPropertyNames.Add(propertySymbol.Name);
                continue;
            }

            if (member is IFieldSymbol fieldSymbol &&
                FieldHasObservablePropertyAttribute(fieldSymbol))
            {
                knownPropertyNames.Add(GetGeneratedPropertyName(fieldSymbol));
            }
        }

        List<Diagnostic>? diagnostics = null;

        foreach (var member in classDeclaration.Members)
        {
            if (member is not FieldDeclarationSyntax fieldDeclaration)
            {
                continue;
            }

            foreach (var attributeList in fieldDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var attributeName = attribute.Name switch
                    {
                        GenericNameSyntax genericName => genericName.Identifier.Text,
                        _ => attribute.Name.ToString(),
                    };

                    if (attributeName is not (
                        NameConstants.NotifyPropertyChangedFor or
                        NameConstants.NotifyPropertyChangedForAttribute))
                    {
                        continue;
                    }

                    if (attribute.ArgumentList is null)
                    {
                        continue;
                    }

                    var fieldName = fieldDeclaration.Declaration.Variables
                        .FirstOrDefault()?.Identifier.Text
                        ?? string.Empty;

                    foreach (var argument in attribute.ArgumentList.Arguments)
                    {
                        var referencedName = ExtractAttributeStringArgument(argument);
                        if (referencedName is null)
                        {
                            continue;
                        }

                        if (knownPropertyNames.Contains(referencedName))
                        {
                            continue;
                        }

                        diagnostics ??= [];
                        diagnostics.Add(DiagnosticFactory.CreateNotifyPropertyChangedForNonExistentTarget(
                            fieldName,
                            referencedName,
                            argument.GetLocation()));
                    }
                }
            }
        }

        return diagnostics;
    }

    private static bool IsClassWithNotifyCanExecuteChangedForUsage(
        SyntaxNode syntaxNode)
    {
        if (syntaxNode is not ClassDeclarationSyntax classDeclaration)
        {
            return false;
        }

        foreach (var member in classDeclaration.Members)
        {
            if (member is not FieldDeclarationSyntax fieldDeclaration ||
                fieldDeclaration.AttributeLists.Count == 0)
            {
                continue;
            }

            foreach (var attributeList in fieldDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var attributeName = attribute.Name switch
                    {
                        GenericNameSyntax genericName => genericName.Identifier.Text,
                        _ => attribute.Name.ToString(),
                    };

                    if (attributeName is
                        NameConstants.NotifyCanExecuteChangedFor or
                        NameConstants.NotifyCanExecuteChangedForAttribute)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK — single-pass diagnostic emission across all fields with [NotifyCanExecuteChangedFor].")]
    private static List<Diagnostic>? GetNotifyCanExecuteChangedForDiagnostics(
        GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
        if (classSymbol is null)
        {
            return null;
        }

        var knownCommandNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var member in classSymbol.GetMembers())
        {
            // User-declared property whose name ends in "Command" — covers
            // hand-written IRelayCommand properties without forcing us to
            // load the type symbol.
            if (member is IPropertySymbol propertySymbol &&
                propertySymbol.Name.EndsWith(NameConstants.Command, StringComparison.Ordinal))
            {
                knownCommandNames.Add(propertySymbol.Name);
                continue;
            }

            if (member is IMethodSymbol methodSymbol &&
                MethodHasRelayCommandAttribute(methodSymbol))
            {
                knownCommandNames.Add(GetGeneratedCommandName(methodSymbol));
            }
        }

        List<Diagnostic>? diagnostics = null;

        foreach (var member in classDeclaration.Members)
        {
            if (member is not FieldDeclarationSyntax fieldDeclaration)
            {
                continue;
            }

            foreach (var attributeList in fieldDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var attributeName = attribute.Name switch
                    {
                        GenericNameSyntax genericName => genericName.Identifier.Text,
                        _ => attribute.Name.ToString(),
                    };

                    if (attributeName is not (
                        NameConstants.NotifyCanExecuteChangedFor or
                        NameConstants.NotifyCanExecuteChangedForAttribute))
                    {
                        continue;
                    }

                    if (attribute.ArgumentList is null)
                    {
                        continue;
                    }

                    var fieldName = fieldDeclaration.Declaration.Variables
                        .FirstOrDefault()?.Identifier.Text
                        ?? string.Empty;

                    foreach (var argument in attribute.ArgumentList.Arguments)
                    {
                        var referencedName = ExtractAttributeStringArgument(argument);
                        if (referencedName is null)
                        {
                            continue;
                        }

                        if (knownCommandNames.Contains(referencedName))
                        {
                            continue;
                        }

                        diagnostics ??= [];
                        diagnostics.Add(DiagnosticFactory.CreateNotifyCanExecuteChangedForNonExistentTarget(
                            fieldName,
                            referencedName,
                            argument.GetLocation()));
                    }
                }
            }
        }

        return diagnostics;
    }

    private static bool MethodHasRelayCommandAttribute(
        IMethodSymbol methodSymbol)
    {
        foreach (var attribute in methodSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.Name
                is NameConstants.RelayCommand
                or NameConstants.RelayCommandAttribute)
            {
                return true;
            }
        }

        return false;
    }

    private static string GetGeneratedCommandName(IMethodSymbol methodSymbol)
    {
        // Mirror RelayCommandInspector.AppendRelayCommandToGenerate naming logic:
        //   1. Honour Name attribute argument when present.
        //   2. Otherwise upper-case the method name.
        //   3. Strip "Handler" suffix.
        //   4. Append "Command" if not already there.
        //   5. Append "X" if the result equals the method name (collision avoidance).
        string commandName = methodSymbol.Name.EnsureFirstCharacterToUpper();

        foreach (var attribute in methodSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.Name is not (
                NameConstants.RelayCommand
                or NameConstants.RelayCommandAttribute))
            {
                continue;
            }

            // ExtractConstructorArgumentValues maps positional arg #0 to key
            // "Name" (the same convention RelayCommandInspector uses).
            var args = attribute.ExtractConstructorArgumentValues();
            if (args.TryGetValue(NameConstants.Name, out var explicitName) &&
                !string.IsNullOrEmpty(explicitName))
            {
                commandName = explicitName!.EnsureFirstCharacterToUpper();
            }

            break;
        }

        if (commandName.EndsWith(NameConstants.Handler, StringComparison.Ordinal))
        {
            commandName = commandName.Substring(0, commandName.Length - NameConstants.Handler.Length);
        }

        if (!commandName.EndsWith(NameConstants.Command, StringComparison.Ordinal))
        {
            commandName += NameConstants.Command;
        }

        if (commandName == methodSymbol.Name)
        {
            commandName += "X";
        }

        return commandName;
    }

    private static bool IsClassWithComputedPropertyUsage(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not ClassDeclarationSyntax classDeclaration)
        {
            return false;
        }

        foreach (var member in classDeclaration.Members)
        {
            if (member is not PropertyDeclarationSyntax propertyDeclaration ||
                propertyDeclaration.AttributeLists.Count == 0)
            {
                continue;
            }

            foreach (var attributeList in propertyDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var attributeName = attribute.Name switch
                    {
                        GenericNameSyntax genericName => genericName.Identifier.Text,
                        _ => attribute.Name.ToString(),
                    };

                    if (attributeName is
                        NameConstants.ComputedProperty or
                        NameConstants.ComputedPropertyAttribute)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK — single-pass diagnostic emission across all [ComputedProperty] members.")]
    private static List<Diagnostic>? GetComputedPropertyDiagnostics(
        GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
        if (classSymbol is null)
        {
            return null;
        }

        // Build the set of identifiers a getter might reference and that
        // count as a tracked dependency: declared properties + properties
        // generated from [ObservableProperty] fields.
        var knownPropertyNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var member in classSymbol.GetMembers())
        {
            if (member is IPropertySymbol propertySymbol)
            {
                knownPropertyNames.Add(propertySymbol.Name);
                continue;
            }

            if (member is IFieldSymbol fieldSymbol &&
                FieldHasObservablePropertyAttribute(fieldSymbol))
            {
                knownPropertyNames.Add(GetGeneratedPropertyName(fieldSymbol));
            }
        }

        // Map each [ComputedProperty] to its declaration and to the set of
        // OTHER [ComputedProperty] names referenced in its getter — used for
        // cycle detection below.
        var computedPropertyDeclarations = new Dictionary<string, PropertyDeclarationSyntax>(StringComparer.Ordinal);
        foreach (var member in classDeclaration.Members)
        {
            if (member is PropertyDeclarationSyntax propertyDeclaration &&
                HasComputedPropertyAttribute(propertyDeclaration))
            {
                computedPropertyDeclarations[propertyDeclaration.Identifier.Text] = propertyDeclaration;
            }
        }

        List<Diagnostic>? diagnostics = null;

        foreach (var entry in computedPropertyDeclarations)
        {
            if (!HasComputedPropertyDependency(entry.Value, knownPropertyNames))
            {
                diagnostics ??= [];
                diagnostics.Add(DiagnosticFactory.CreateComputedPropertyNoDependencies(
                    entry.Key,
                    entry.Value.Identifier.GetLocation()));
            }
        }

        // Detect cycles in the [ComputedProperty] → [ComputedProperty]
        // sub-graph. Linear dependency chains and references to
        // [ObservableProperty] don't participate.
        var computedDependencyEdges = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        foreach (var entry in computedPropertyDeclarations)
        {
            var edges = new HashSet<string>(StringComparer.Ordinal);
            foreach (var referenced in CollectIdentifierReferences(entry.Value))
            {
                if (referenced != entry.Key &&
                    computedPropertyDeclarations.ContainsKey(referenced))
                {
                    edges.Add(referenced);
                }
            }

            computedDependencyEdges[entry.Key] = edges;
        }

        var participantsInCycles = FindCycleParticipants(computedDependencyEdges);
        foreach (var propertyName in participantsInCycles)
        {
            var declaration = computedPropertyDeclarations[propertyName];
            var cyclePath = BuildCyclePath(propertyName, computedDependencyEdges);

            diagnostics ??= [];
            diagnostics.Add(DiagnosticFactory.CreateComputedPropertyCycle(
                propertyName,
                cyclePath,
                declaration.Identifier.GetLocation()));
        }

        return diagnostics;
    }

    private static IEnumerable<string> CollectIdentifierReferences(
        PropertyDeclarationSyntax propertyDeclaration)
    {
        IEnumerable<IdentifierNameSyntax> identifiers;

        if (propertyDeclaration.ExpressionBody is not null)
        {
            identifiers = propertyDeclaration
                .ExpressionBody
                .DescendantNodes()
                .OfType<IdentifierNameSyntax>();
        }
        else
        {
            var getter = propertyDeclaration.AccessorList?.Accessors
                .FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
            if (getter is null)
            {
                yield break;
            }

            identifiers = getter
                .DescendantNodes()
                .OfType<IdentifierNameSyntax>();
        }

        foreach (var node in identifiers)
        {
            yield return node.Identifier.ValueText;
        }
    }

    private static HashSet<string> FindCycleParticipants(
        Dictionary<string, HashSet<string>> edges)
    {
        var participants = new HashSet<string>(StringComparer.Ordinal);

        // A node participates in a cycle iff it is reachable from itself via
        // its own out-edges. Run a DFS from each node.
        foreach (var start in edges.Keys)
        {
            if (CanReachSelf(start, edges))
            {
                participants.Add(start);
            }
        }

        return participants;
    }

    private static bool CanReachSelf(
        string start,
        Dictionary<string, HashSet<string>> edges)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var stack = new Stack<string>();
        if (!edges.TryGetValue(start, out var initialEdges))
        {
            return false;
        }

        foreach (var first in initialEdges)
        {
            stack.Push(first);
        }

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current == start)
            {
                return true;
            }

            if (!visited.Add(current))
            {
                continue;
            }

            if (edges.TryGetValue(current, out var nextEdges))
            {
                foreach (var next in nextEdges)
                {
                    stack.Push(next);
                }
            }
        }

        return false;
    }

    private static string BuildCyclePath(
        string start,
        Dictionary<string, HashSet<string>> edges)
    {
        // BFS to find the shortest cycle starting and ending at `start`,
        // for a more useful diagnostic message.
        var queue = new Queue<List<string>>();
        if (edges.TryGetValue(start, out var startEdges))
        {
            foreach (var first in startEdges)
            {
                queue.Enqueue([start, first]);
            }
        }

        var visited = new HashSet<string>(StringComparer.Ordinal) { start };

        while (queue.Count > 0)
        {
            var path = queue.Dequeue();
            var tail = path[path.Count - 1];

            if (tail == start && path.Count > 2)
            {
                return string.Join(" → ", path);
            }

            if (!visited.Add(tail))
            {
                continue;
            }

            if (!edges.TryGetValue(tail, out var nextEdges))
            {
                continue;
            }

            foreach (var next in nextEdges)
            {
                if (next == start || !visited.Contains(next))
                {
                    var newPath = new List<string>(path) { next };
                    queue.Enqueue(newPath);
                }
            }
        }

        return start;
    }

    private static bool HasComputedPropertyAttribute(
        PropertyDeclarationSyntax propertyDeclaration)
    {
        foreach (var attributeList in propertyDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var attributeName = attribute.Name switch
                {
                    GenericNameSyntax genericName => genericName.Identifier.Text,
                    _ => attribute.Name.ToString(),
                };

                if (attributeName is
                    NameConstants.ComputedProperty or
                    NameConstants.ComputedPropertyAttribute)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasComputedPropertyDependency(
        PropertyDeclarationSyntax propertyDeclaration,
        HashSet<string> knownPropertyNames)
    {
        // Mirror ComputedPropertyInspector.AnalyzePropertyDependencies — scan
        // the getter (expression body or `get { ... }`) for IdentifierName
        // references that match a known property.
        IEnumerable<IdentifierNameSyntax> identifierNodes;

        if (propertyDeclaration.ExpressionBody is not null)
        {
            identifierNodes = propertyDeclaration
                .ExpressionBody
                .DescendantNodes()
                .OfType<IdentifierNameSyntax>();
        }
        else
        {
            var getter = propertyDeclaration.AccessorList?.Accessors
                .FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
            if (getter is null)
            {
                return false;
            }

            identifierNodes = getter
                .DescendantNodes()
                .OfType<IdentifierNameSyntax>();
        }

        foreach (var node in identifierNodes)
        {
            if (knownPropertyNames.Contains(node.Identifier.ValueText))
            {
                return true;
            }
        }

        return false;
    }

    private static bool FieldHasObservablePropertyAttribute(
        IFieldSymbol fieldSymbol)
    {
        foreach (var attribute in fieldSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.Name
                is NameConstants.ObservableProperty
                or NameConstants.ObservablePropertyAttribute)
            {
                return true;
            }
        }

        return false;
    }

    private static string GetGeneratedPropertyName(IFieldSymbol fieldSymbol)
    {
        // Honour an explicit PropertyName argument on the [ObservableProperty]
        // attribute when present; otherwise derive from the camelCase field
        // name (matches ObservablePropertyInspector behaviour).
        foreach (var attribute in fieldSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.Name is not (
                NameConstants.ObservableProperty
                or NameConstants.ObservablePropertyAttribute))
            {
                continue;
            }

            if (attribute.ConstructorArguments.Length > 0 &&
                attribute.ConstructorArguments[0].Value is string explicitName &&
                !string.IsNullOrEmpty(explicitName))
            {
                return explicitName!.EnsureFirstCharacterToUpper();
            }
        }

        return fieldSymbol.Name
            .RemovePrefixFromField()
            .EnsureFirstCharacterToUpper();
    }

    private static string? ExtractAttributeStringArgument(
        AttributeArgumentSyntax argument)
    {
        // Either a literal string ("Foo") or a nameof(Foo) expression.
        if (argument.Expression is LiteralExpressionSyntax literal &&
            literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            return literal.Token.ValueText;
        }

        if (argument.Expression is not InvocationExpressionSyntax invocation)
        {
            return null;
        }

        if (invocation.Expression is not IdentifierNameSyntax invokedName ||
            invokedName.Identifier.Text != "nameof" ||
            invocation.ArgumentList.Arguments.Count != 1)
        {
            return null;
        }

        return invocation.ArgumentList.Arguments[0].Expression is IdentifierNameSyntax id
            ? id.Identifier.Text
            : null;
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

    private static bool IsClassWithNotifyDataErrorInfoUsage(
        SyntaxNode syntaxNode)
    {
        if (syntaxNode is not ClassDeclarationSyntax classDeclaration)
        {
            return false;
        }

        foreach (var member in classDeclaration.Members)
        {
            if (member is not FieldDeclarationSyntax fieldDeclaration ||
                fieldDeclaration.AttributeLists.Count == 0)
            {
                continue;
            }

            foreach (var attributeList in fieldDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var attributeName = attribute.Name switch
                    {
                        GenericNameSyntax genericName => genericName.Identifier.Text,
                        _ => attribute.Name.ToString(),
                    };

                    if (attributeName is
                        NameConstants.NotifyDataErrorInfo or
                        NameConstants.NotifyDataErrorInfoAttribute)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK — single-pass diagnostic emission across all fields with [NotifyDataErrorInfo].")]
    private static List<Diagnostic>? GetNotifyDataErrorInfoDiagnostics(
        GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
        if (classSymbol is null)
        {
            return null;
        }

        // ObservableValidator and the framework base classes that derive
        // from it (ViewModelBase, MainWindowViewModelBase, ViewModelDialogBase)
        // all provide the protected ValidateProperty method the generator
        // emits. Match by the framework's own type names — the user's
        // compilation may not have a metadata reference to the runtime
        // assembly, in which case the symbol's BaseType.BaseType chain is
        // not walkable past the first hop.
        if (classSymbol.InheritsFrom(
                NameConstants.ObservableValidator,
                NameConstants.ViewModelBase,
                NameConstants.MainWindowViewModelBase,
                NameConstants.ViewModelDialogBase))
        {
            return null;
        }

        List<Diagnostic>? diagnostics = null;

        foreach (var member in classDeclaration.Members)
        {
            if (member is not FieldDeclarationSyntax fieldDeclaration)
            {
                continue;
            }

            var hasNotifyDataErrorInfo = false;
            foreach (var attributeList in fieldDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var attributeName = attribute.Name switch
                    {
                        GenericNameSyntax genericName => genericName.Identifier.Text,
                        _ => attribute.Name.ToString(),
                    };

                    if (attributeName is
                        NameConstants.NotifyDataErrorInfo or
                        NameConstants.NotifyDataErrorInfoAttribute)
                    {
                        hasNotifyDataErrorInfo = true;
                        break;
                    }
                }

                if (hasNotifyDataErrorInfo)
                {
                    break;
                }
            }

            if (!hasNotifyDataErrorInfo)
            {
                continue;
            }

            foreach (var variable in fieldDeclaration.Declaration.Variables)
            {
                diagnostics ??= [];
                diagnostics.Add(DiagnosticFactory.CreateNotifyDataErrorInfoRequiresObservableValidator(
                    variable.Identifier.Text,
                    classSymbol.Name,
                    variable.Identifier.GetLocation()));
            }
        }

        return diagnostics;
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