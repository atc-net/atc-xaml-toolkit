namespace Atc.XamlToolkit;

/// <summary>
/// What the generated <c>Get{Name}</c> and <c>Set{Name}</c> methods of an attached property
/// do when they are called with a <see langword="null"/> element.
/// </summary>
public enum NullElementHandling
{
    /// <summary>
    /// <c>Set{Name}</c> does nothing; <c>Get{Name}</c> throws a <see cref="NullReferenceException"/>.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Both methods throw an <see cref="ArgumentNullException"/> that names the parameter.
    /// </summary>
    ThrowArgumentNullException = 1,

    /// <summary>
    /// No check: both methods use the element directly and throw a
    /// <see cref="NullReferenceException"/>.
    /// </summary>
    None = 2,
}