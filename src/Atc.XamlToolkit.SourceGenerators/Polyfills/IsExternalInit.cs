// Polyfill for netstandard2.0 — required so the C# 9+ record / init-only
// property syntax compiles. The runtime check resolves any type with this
// fully-qualified name in any assembly, so this internal type is sufficient.
namespace System.Runtime.CompilerServices;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
internal static class IsExternalInit
{
}