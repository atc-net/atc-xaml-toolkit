// ReSharper disable InvertIf
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
namespace Atc.XamlToolkit.SourceGenerators.Extensions.CodeAnalysis;

internal static class NamedTypeSymbolExtensions
{
    public static List<DtoPropertyInfo> ExtractProperties(
        this INamedTypeSymbol namedTypeSymbol,
        List<string>? ignorePropertyNames = null)
    {
        var ignoreSet = ignorePropertyNames is not null
            ? new HashSet<string>(ignorePropertyNames, StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);

        var isRecord = namedTypeSymbol.IsRecord;

        // Get primary constructor parameters for records
        var primaryConstructorParameters = new HashSet<string>(StringComparer.Ordinal);
        if (isRecord)
        {
            var primaryConstructor = namedTypeSymbol.Constructors.FirstOrDefault(c =>
                c.Parameters.Length > 0 &&
                c.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is RecordDeclarationSyntax);

            if (primaryConstructor is not null)
            {
                foreach (var parameter in primaryConstructor.Parameters)
                {
                    primaryConstructorParameters.Add(parameter.Name);
                }
            }
        }

        return namedTypeSymbol
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public &&
                        !p.IsStatic &&
                        !ignoreSet.Contains(p.Name))
            .Select(p =>
            {
                var isRecordParameter = isRecord &&
                                        primaryConstructorParameters.Contains(p.Name);

                // A setter the view model cannot call makes the property read-only.
                // Init-only record parameters are still writable through a 'with' expression.
                var isReadOnly = p.SetMethod is null ||
                                 p.SetMethod.DeclaredAccessibility != Accessibility.Public ||
                                 (p.SetMethod.IsInitOnly && !isRecordParameter);
                var attributes = p.ExtractPropertyAttributes();
                var documentationComments = p.ExtractPropertyDocumentationComments();

                return new DtoPropertyInfo(
                    p.Name.EscapeKeyword(),
                    p.Type.ToString(),
                    isRecordParameter,
                    isReadOnly,
                    new EquatableArray<string>(attributes.ToArray()),
                    new EquatableArray<string>(documentationComments?.ToArray() ?? []))
                {
                    UseEqualityComparer = !p.Type.SupportsEqualityOperator(),
                    RequiredNamespaces = p.ExtractPropertyAttributeNamespaces(),
                };
            })
            .ToList();
    }

    public static List<DtoMethodInfo> ExtractMethods(
        this INamedTypeSymbol namedTypeSymbol,
        List<string>? ignoreMethodNames = null)
    {
        var ignoreSet = ignoreMethodNames is not null
            ? new HashSet<string>(ignoreMethodNames, StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);

        return namedTypeSymbol
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.DeclaredAccessibility == Accessibility.Public &&
                        m is { IsStatic: false, MethodKind: MethodKind.Ordinary, IsImplicitlyDeclared: false } &&
                        m.Name != "ToString" &&
                        !m.IsGenericMethod &&
                        !OverridesObjectMember(m) &&
                        !ignoreSet.Contains(m.Name))
            .Select(m =>
            {
                var parameters = m.Parameters
                    .Select(p => new DtoMethodParameterInfo(
                        p.Name,
                        p.Type.ToString(),
                        GetParameterModifier(p),
                        GetParameterDefaultValue(p)))
                    .ToArray();

                return new DtoMethodInfo(
                    m.Name,
                    m.ReturnType.ToString(),
                    new EquatableArray<DtoMethodParameterInfo>(parameters));
            })
            .ToList();
    }

    // Equals(object) and GetHashCode() overrides cannot be forwarded without 'override'.
    private static bool OverridesObjectMember(IMethodSymbol methodSymbol)
    {
        if (!methodSymbol.IsOverride)
        {
            return false;
        }

        var rootMethod = methodSymbol;
        while (rootMethod.OverriddenMethod is not null)
        {
            rootMethod = rootMethod.OverriddenMethod;
        }

        return rootMethod.ContainingType.SpecialType == SpecialType.System_Object;
    }

    private static string GetParameterModifier(IParameterSymbol parameterSymbol)
    {
        if (parameterSymbol.IsParams)
        {
            return "params";
        }

        return parameterSymbol.RefKind switch
        {
            RefKind.Ref => "ref",
            RefKind.Out => "out",
            RefKind.In => "in",
            _ => string.Empty,
        };
    }

    private static string? GetParameterDefaultValue(
        IParameterSymbol parameterSymbol)
    {
        if (!parameterSymbol.HasExplicitDefaultValue)
        {
            return null;
        }

        var type = parameterSymbol.Type;
        var isNullableValueType = type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;

        var value = parameterSymbol.ExplicitDefaultValue;
        if (value is null)
        {
            return type.IsReferenceType || isNullableValueType
                ? "null"
                : "default";
        }

        var literal = SymbolDisplay.FormatPrimitive(value, quoteStrings: true, useHexadecimalNumbers: false);

        if (isNullableValueType && type is INamedTypeSymbol { TypeArguments.Length: 1 } nullableType)
        {
            type = nullableType.TypeArguments[0];
        }

        // The default value of an enum parameter is its underlying number.
        return type.TypeKind == TypeKind.Enum
            ? $"({type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})({literal})"
            : literal;
    }

    /// <summary>
    /// Gets the namespace of the type; empty for a type in the global namespace.
    /// </summary>
    /// <param name="namedTypeSymbol">The type symbol.</param>
    /// <returns>The namespace name, or an empty string.</returns>
    public static string GetNamespaceName(this INamedTypeSymbol namedTypeSymbol)
        => namedTypeSymbol.ContainingNamespace is { IsGlobalNamespace: false } containingNamespace
            ? containingNamespace.ToDisplayString()
            : string.Empty;

    /// <summary>
    /// Gets the type parameter list of the type, e.g. <c>&lt;TKey, TValue&gt;</c>;
    /// empty for a non-generic type.
    /// </summary>
    /// <param name="namedTypeSymbol">The type symbol.</param>
    /// <returns>The type parameter list, or an empty string.</returns>
    public static string GetTypeParameterList(
        this INamedTypeSymbol namedTypeSymbol)
        => namedTypeSymbol.TypeParameters.Length == 0
            ? string.Empty
            : $"<{string.Join(", ", namedTypeSymbol.TypeParameters.Select(x => x.Name.EscapeKeyword()))}>";

    /// <summary>
    /// Gets the partial declarations of the types the type is nested in, outermost first.
    /// </summary>
    /// <param name="namedTypeSymbol">The type symbol.</param>
    /// <returns>The declarations, e.g. <c>partial class Outer&lt;T&gt;</c>.</returns>
    public static EquatableArray<string> GetContainingTypeDeclarations(
        this INamedTypeSymbol namedTypeSymbol)
    {
        var declarations = new List<string>();

        for (var containingType = namedTypeSymbol.ContainingType;
             containingType is not null;
             containingType = containingType.ContainingType)
        {
            var keyword = containingType.TypeKind switch
            {
                TypeKind.Struct => containingType.IsRecord ? "record struct" : "struct",
                TypeKind.Interface => "interface",
                _ => containingType.IsRecord ? "record" : "class",
            };

            declarations.Insert(0, $"partial {keyword} {containingType.Name.EscapeKeyword()}{containingType.GetTypeParameterList()}");
        }

        return new EquatableArray<string>(declarations.ToArray());
    }

    /// <summary>
    /// Gets a file name stem that is unique for the type: namespace, containing types and
    /// name, where generic types get their arity appended (<c>Name`1</c>).
    /// </summary>
    /// <param name="namedTypeSymbol">The type symbol.</param>
    /// <returns>The hint name without extension.</returns>
    public static string GetHintName(this INamedTypeSymbol namedTypeSymbol)
    {
        var parts = new List<string>();

        for (var type = namedTypeSymbol;
             type is not null;
             type = type.ContainingType)
        {
            parts.Insert(0, type.MetadataName);
        }

        var namespaceName = namedTypeSymbol.GetNamespaceName();
        if (namespaceName.Length > 0)
        {
            parts.Insert(0, namespaceName);
        }

        return string.Join(".", parts);
    }

    public static string GetAccessModifier(
        this INamedTypeSymbol namedTypeSymbol)
        => namedTypeSymbol.DeclaredAccessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Private => "private",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            _ => string.Empty,
        };

    public static bool HasObservableDtoViewModelAttribute(
        this INamedTypeSymbol namedTypeSymbol)
        => namedTypeSymbol
            .GetAttributes()
            .FirstOrDefault(attr => attr.GetToolkitAttributeName()
                is NameConstants.ObservableDtoViewModelAttribute
                or NameConstants.ObservableDtoViewModel) is not null;

    public static bool HasBaseTypeThePropertyName(
        this INamedTypeSymbol declaringType,
        string propertyName)
    {
        var pascalName = propertyName.EnsureFirstCharacterToUpper();
        for (var baseType = declaringType.BaseType;
             baseType is not null;
             baseType = baseType.BaseType)
        {
            if (baseType
                .GetMembers(pascalName)
                .Any(m => m.Kind == SymbolKind.Property))
            {
                return true;
            }
        }

        return false;
    }

    public static bool InheritsFrom(
        this INamedTypeSymbol namedTypeSymbol,
        params string[] baseClassNames)
    {
        var baseType = namedTypeSymbol.BaseType;
        while (baseType is not null)
        {
            if (baseClassNames.Contains(baseType.Name, StringComparer.Ordinal))
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Checks if the class symbol inherits from ViewModelBase or ObservableObject.
    /// Since all partial declarations resolve to the same type symbol, checking
    /// inheritance once is sufficient.
    /// </summary>
    /// <param name="classSymbol">The class symbol to check.</param>
    /// <returns>
    /// A tuple containing:
    /// - HasAnyBase: True if the class inherits from any valid base class
    /// - HasViewModelBase: True if the class inherits from ViewModelBase, MainWindowViewModelBase, or ViewModelDialogBase
    /// </returns>
    public static (bool HasAnyBase, bool HasViewModelBase) CheckBaseClasses(
        this INamedTypeSymbol classSymbol)
    {
        if (classSymbol.InheritsFromToolkitType(
                NameConstants.ViewModelBase,
                NameConstants.MainWindowViewModelBase,
                NameConstants.ViewModelDialogBase))
        {
            return (true, true);
        }

        return classSymbol.InheritsFromToolkitType(NameConstants.ObservableObject)
            ? (true, false)
            : (false, false);
    }

    // Like InheritsFrom, but a same-named base class of another library does not count.
    private static bool InheritsFromToolkitType(
        this INamedTypeSymbol namedTypeSymbol,
        params string[] baseClassNames)
    {
        var baseType = namedTypeSymbol.BaseType;
        while (baseType is not null)
        {
            if (baseClassNames.Contains(baseType.Name, StringComparer.Ordinal) &&
                baseType.IsToolkitTypeOrUnresolved())
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Checks if the class symbol is a valid framework element target for source generation.
    /// This checks inheritance, naming conventions, and the presence of RoutedEvent attributes.
    /// </summary>
    /// <param name="classSymbol">The class symbol to check.</param>
    /// <returns>True if the class is a valid framework element target; otherwise, false.</returns>
    public static bool HasAnythingAroundFrameworkElement(
        this INamedTypeSymbol classSymbol)
    {
        if (classSymbol.InheritsFrom(
                NameConstants.UserControl,
                NameConstants.DependencyObject,
                NameConstants.FrameworkElement))
        {
            return true;
        }

        if (classSymbol.Name.EndsWith(NameConstants.Attach, StringComparison.Ordinal) ||
            classSymbol.Name.EndsWith(NameConstants.Behavior, StringComparison.Ordinal) ||
            classSymbol.Name.EndsWith(NameConstants.Helper, StringComparison.Ordinal))
        {
            return true;
        }

        // Symbol-based check replaces SyntaxTree.ToString().Contains("[RoutedEvent")
        return classSymbol.GetMembers()
            .Any(m => m.GetAttributes()
                .Any(a => a.GetToolkitAttributeName()
                    is NameConstants.RoutedEventAttribute
                    or NameConstants.RoutedEvent));
    }
}