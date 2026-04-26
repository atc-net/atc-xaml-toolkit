// Adapted from CommunityToolkit.Mvvm.SourceGenerators
// Original: https://github.com/CommunityToolkit/dotnet/blob/main/src/CommunityToolkit.Mvvm.SourceGenerators/Helpers/EquatableArray%7BT%7D.cs
// Licensed under the MIT License — © .NET Foundation and Contributors.
namespace Atc.XamlToolkit.SourceGenerators.Collections;

/// <summary>
/// Immutable, value-equality wrapper around an underlying array.
/// </summary>
/// <remarks>
/// Source-generator pipeline caches compare model values via <see cref="object.Equals(object?)"/>.
/// <see cref="ImmutableArray{T}"/> uses reference equality and <see cref="System.Array"/> uses
/// reference equality too, both of which defeat the cache: a fresh instance is produced on every
/// run, so models containing arrays are never reported as <c>Cached</c> on subsequent compilations.
/// Wrapping the array in this struct restores content-based equality.
/// </remarks>
/// <typeparam name="T">The element type.</typeparam>
[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "OK — implemented.")]
internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, System.Collections.Generic.IEnumerable<T>
{
    public static readonly EquatableArray<T> Empty = new(System.Array.Empty<T>());

    private readonly T[]? array;

    public EquatableArray(T[] array)
    {
        this.array = array;
    }

    public int Count => array?.Length ?? 0;

    public int Length => array?.Length ?? 0;

    public bool IsEmpty => array is null || array.Length == 0;

    public T this[int index] => array![index];

    public T[] AsArray() => array ?? System.Array.Empty<T>();

    public bool Equals(EquatableArray<T> other)
    {
        var left = array;
        var right = other.array;

        if (ReferenceEquals(left, right))
        {
            return true;
        }

        var leftLength = left?.Length ?? 0;
        var rightLength = right?.Length ?? 0;

        if (leftLength != rightLength)
        {
            return false;
        }

        var comparer = EqualityComparer<T>.Default;
        for (var i = 0; i < leftLength; i++)
        {
            if (!comparer.Equals(left![i], right![i]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj)
        => obj is EquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        if (array is null)
        {
            return 0;
        }

        // FNV-1a-style content hash. Order-sensitive (intentional — list order matters for codegen).
        unchecked
        {
            var hash = (int)2166136261;
            foreach (var item in array)
            {
                hash = (hash ^ (item?.GetHashCode() ?? 0)) * 16777619;
            }

            return hash;
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        if (array is null)
        {
            yield break;
        }

        foreach (var item in array)
        {
            yield return item;
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => GetEnumerator();

    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right) => left.Equals(right);

    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right) => !left.Equals(right);

    public static implicit operator EquatableArray<T>(T[] array) => new(array);
}