namespace Atc.XamlToolkit.Mvvm;

/// <summary>
/// Process-wide cache of <see cref="PropertyChangedEventArgs"/> instances keyed by property name.
/// Property names form a small, bounded set per process, so caching trades a tiny one-time
/// allocation per name for the per-raise allocation that would otherwise occur on every property
/// change. This is hot-path during data-binding storms (animations, rapid input).
/// </summary>
/// <remarks>
/// Used internally by <see cref="ObservableObject"/> and by code emitted from
/// <see cref="INotifyPropertyChangedAttribute"/>. Exposed publicly so generated code in consumer
/// assemblies can reuse the same shared cache.
/// </remarks>
public static class PropertyChangedEventArgsCache
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, PropertyChangedEventArgs> Cache =
        new(StringComparer.Ordinal);

    private static readonly PropertyChangedEventArgs EmptyArgs = new(string.Empty);

    /// <summary>
    /// Returns a cached <see cref="PropertyChangedEventArgs"/> for the supplied property name,
    /// allocating a new one only on the first request per name.
    /// </summary>
    /// <param name="propertyName">The property name; <see langword="null"/> or empty returns a shared "all properties" instance.</param>
    /// <returns>A cached <see cref="PropertyChangedEventArgs"/>.</returns>
    public static PropertyChangedEventArgs Get(string? propertyName)
        => string.IsNullOrEmpty(propertyName)
            ? EmptyArgs
            : Cache.GetOrAdd(propertyName!, static name => new PropertyChangedEventArgs(name));
}