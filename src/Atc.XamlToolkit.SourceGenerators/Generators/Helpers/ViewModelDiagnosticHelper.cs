// ReSharper disable InvertIf
namespace Atc.XamlToolkit.SourceGenerators.Generators.Helpers;

/// <summary>
/// Wires up the diagnostic-only pipelines for <see cref="ViewModelGenerator"/>: missing-partial,
/// invalid <c>[ObservableProperty]</c> field shapes, dangling <c>[NotifyPropertyChangedFor]</c> /
/// <c>[NotifyCanExecuteChangedFor]</c> references, <c>[ComputedProperty]</c> dependency analysis
/// (no-deps + cycles), and <c>[NotifyDataErrorInfo]</c> base-class requirement. Each pipeline
/// translates a footgun the generator otherwise silently skips into a build-time warning.
/// </summary>
internal static class ViewModelDiagnosticHelper
{
    /// <summary>
    /// Registers all six diagnostic pipelines on the given initialization context. Call once
    /// from <c>ViewModelGenerator.Initialize</c>.
    /// </summary>
    /// <param name="context">The incremental generator initialization context.</param>
    public static void RegisterAll(
        IncrementalGeneratorInitializationContext context)
    {
        RegisterMissingPartialDiagnostic(context);
        RegisterObservablePropertyFieldDiagnostics(context);
        RegisterNotifyPropertyChangedForDiagnostics(context);
        RegisterNotifyCanExecuteChangedForDiagnostics(context);
        RegisterComputedPropertyDiagnostics(context);
        RegisterNotifyDataErrorInfoDiagnostics(context);
    }

    private static void RegisterMissingPartialDiagnostic(
        IncrementalGeneratorInitializationContext context)
    {
        // Surface a diagnostic on classes that contain [ObservableProperty] /
        // [RelayCommand] / [ComputedProperty] but are NOT declared partial — the
        // generator silently skips them, which is hard to diagnose for first-time users.
        var missingPartialClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntaxNode, _) => IsMissingPartialTarget(syntaxNode),
                transform: static (ctx, _) => GetMissingPartialDiagnostic(ctx))
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
    }

    private static void RegisterObservablePropertyFieldDiagnostics(
        IncrementalGeneratorInitializationContext context)
    {
        // [ObservableProperty] field-level validation — surface diagnostics for fields the
        // generator currently skips silently (non-private, PascalCase). Catches a class of
        // footguns where the user expects a property but the generator emits nothing.
        var invalidObservablePropertyFields = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntaxNode, _) => IsObservablePropertyFieldTarget(syntaxNode),
                transform: static (ctx, _) => GetObservablePropertyFieldDiagnostics(ctx))
            .Where(static diagnostics => diagnostics is { Count: > 0 })
            .WithTrackingName("ViewModelGenerator.ObservablePropertyFieldDiagnostics");

        context.RegisterSourceOutput(
            invalidObservablePropertyFields,
            static (spc, diagnostics) => ReportAll(spc, diagnostics));
    }

    private static void RegisterNotifyPropertyChangedForDiagnostics(
        IncrementalGeneratorInitializationContext context)
    {
        // Validate [NotifyPropertyChangedFor("X")] references.
        // Catches typos and renames that today produce a confusing
        // 'CS0103: name X does not exist' inside the generated file.
        var notifyForRefDiagnostics = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntaxNode, _) => IsClassWithNotifyPropertyChangedForUsage(syntaxNode),
                transform: static (ctx, _) => GetNotifyPropertyChangedForDiagnostics(ctx))
            .Where(static diagnostics => diagnostics is { Count: > 0 })
            .WithTrackingName("ViewModelGenerator.NotifyPropertyChangedForDiagnostics");

        context.RegisterSourceOutput(
            notifyForRefDiagnostics,
            static (spc, diagnostics) => ReportAll(spc, diagnostics));
    }

    private static void RegisterNotifyCanExecuteChangedForDiagnostics(
        IncrementalGeneratorInitializationContext context)
    {
        // Same shape as NotifyPropertyChangedFor but for command references.
        var notifyCanExecForRefDiagnostics = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntaxNode, _) => IsClassWithNotifyCanExecuteChangedForUsage(syntaxNode),
                transform: static (ctx, _) => GetNotifyCanExecuteChangedForDiagnostics(ctx))
            .Where(static diagnostics => diagnostics is { Count: > 0 })
            .WithTrackingName("ViewModelGenerator.NotifyCanExecuteChangedForDiagnostics");

        context.RegisterSourceOutput(
            notifyCanExecForRefDiagnostics,
            static (spc, diagnostics) => ReportAll(spc, diagnostics));
    }

    private static void RegisterComputedPropertyDiagnostics(
        IncrementalGeneratorInitializationContext context)
    {
        // Validate [ComputedProperty] dependency detection.
        // Surfaces diagnostics for properties whose getter doesn't reference any other property
        // (the inspector silently filters them out so the user gets nothing) and for circular
        // [ComputedProperty]→[ComputedProperty] graphs that would infinite-recurse at runtime.
        var computedPropertyDiagnostics = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntaxNode, _) => IsClassWithComputedPropertyUsage(syntaxNode),
                transform: static (ctx, _) => GetComputedPropertyDiagnostics(ctx))
            .Where(static diagnostics => diagnostics is { Count: > 0 })
            .WithTrackingName("ViewModelGenerator.ComputedPropertyDiagnostics");

        context.RegisterSourceOutput(
            computedPropertyDiagnostics,
            static (spc, diagnostics) => ReportAll(spc, diagnostics));
    }

    private static void RegisterNotifyDataErrorInfoDiagnostics(
        IncrementalGeneratorInitializationContext context)
    {
        // Validate [NotifyDataErrorInfo] inheritance requirement. The generator emits
        // ValidateProperty(...) in the setter, which only exists on ObservableValidator
        // (and ViewModelBase via inheritance). Surface a diagnostic up-front so the user
        // gets a friendly message instead of CS0103 inside the generated file.
        var notifyDataErrorInfoDiagnostics = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntaxNode, _) => IsClassWithNotifyDataErrorInfoUsage(syntaxNode),
                transform: static (ctx, _) => GetNotifyDataErrorInfoDiagnostics(ctx))
            .Where(static diagnostics => diagnostics is { Count: > 0 })
            .WithTrackingName("ViewModelGenerator.NotifyDataErrorInfoDiagnostics");

        context.RegisterSourceOutput(
            notifyDataErrorInfoDiagnostics,
            static (spc, diagnostics) => ReportAll(spc, diagnostics));
    }

    private static void ReportAll(
        SourceProductionContext spc,
        List<Diagnostic>? diagnostics)
    {
        if (diagnostics is null)
        {
            return;
        }

        foreach (var diagnostic in diagnostics)
        {
            spc.ReportDiagnostic(diagnostic);
        }
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
        if (ViewModelGeneratorHelper.HasINotifyPropertyChangedAttribute(classDeclaration.AttributeLists))
        {
            return true;
        }

        // …or DO have at least one member tagged with one of the generator-relevant attributes.
        return classDeclaration.Members.Any(member => member switch
        {
            FieldDeclarationSyntax { AttributeLists.Count: > 0 } field =>
                ViewModelGeneratorHelper.HasRelevantAttribute(field.AttributeLists),
            PropertyDeclarationSyntax { AttributeLists.Count: > 0 } property =>
                ViewModelGeneratorHelper.HasRelevantAttribute(property.AttributeLists),
            MethodDeclarationSyntax { AttributeLists.Count: > 0 } method =>
                ViewModelGeneratorHelper.HasRelevantAttribute(method.AttributeLists),
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
        => HasFieldWithAttribute(
            syntaxNode,
            NameConstants.NotifyPropertyChangedFor,
            NameConstants.NotifyPropertyChangedForAttribute);

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
                ViewModelGeneratorHelper.FieldHasObservablePropertyAttribute(fieldSymbol))
            {
                knownPropertyNames.Add(ViewModelGeneratorHelper.GetGeneratedPropertyName(fieldSymbol));
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
                        var referencedName = ViewModelGeneratorHelper.ExtractAttributeStringArgument(argument);
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
        => HasFieldWithAttribute(
            syntaxNode,
            NameConstants.NotifyCanExecuteChangedFor,
            NameConstants.NotifyCanExecuteChangedForAttribute);

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
                ViewModelGeneratorHelper.MethodHasRelayCommandAttribute(methodSymbol))
            {
                knownCommandNames.Add(ViewModelGeneratorHelper.GetGeneratedCommandName(methodSymbol));
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
                        var referencedName = ViewModelGeneratorHelper.ExtractAttributeStringArgument(argument);
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
                ViewModelGeneratorHelper.FieldHasObservablePropertyAttribute(fieldSymbol))
            {
                knownPropertyNames.Add(ViewModelGeneratorHelper.GetGeneratedPropertyName(fieldSymbol));
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

    private static bool IsClassWithNotifyDataErrorInfoUsage(
        SyntaxNode syntaxNode)
        => HasFieldWithAttribute(
            syntaxNode,
            NameConstants.NotifyDataErrorInfo,
            NameConstants.NotifyDataErrorInfoAttribute);

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

        // ObservableValidator and the framework base classes that derive from it
        // (ViewModelBase, MainWindowViewModelBase, ViewModelDialogBase) all provide
        // the protected ValidateProperty method the generator emits. Match by the
        // framework's own type names — the user's compilation may not have a metadata
        // reference to the runtime assembly, in which case the symbol's BaseType.BaseType
        // chain is not walkable past the first hop.
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

    private static bool HasFieldWithAttribute(
        SyntaxNode syntaxNode,
        string attributeName,
        string attributeNameWithSuffix)
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
                    var name = attribute.Name switch
                    {
                        GenericNameSyntax genericName => genericName.Identifier.Text,
                        _ => attribute.Name.ToString(),
                    };

                    if (name == attributeName || name == attributeNameWithSuffix)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}