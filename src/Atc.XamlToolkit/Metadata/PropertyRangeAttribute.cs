namespace Atc.XamlToolkit.Metadata;

/// <summary>
/// Specifies the valid numeric range and optional step increment for a property.
/// All values are stored as <see cref="double"/> internally.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class PropertyRangeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyRangeAttribute"/> class with a double range.
    /// </summary>
    /// <param name="minimum">The minimum allowed value.</param>
    /// <param name="maximum">The maximum allowed value.</param>
    public PropertyRangeAttribute(
        double minimum,
        double maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyRangeAttribute"/> class with a double range and step.
    /// </summary>
    /// <param name="minimum">The minimum allowed value.</param>
    /// <param name="maximum">The maximum allowed value.</param>
    /// <param name="step">The step increment for the editor control.</param>
    public PropertyRangeAttribute(
        double minimum,
        double maximum,
        double step)
    {
        Minimum = minimum;
        Maximum = maximum;
        Step = step;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyRangeAttribute"/> class with an integer range.
    /// </summary>
    /// <param name="minimum">The minimum allowed value.</param>
    /// <param name="maximum">The maximum allowed value.</param>
    public PropertyRangeAttribute(
        int minimum,
        int maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyRangeAttribute"/> class with an integer range and step.
    /// </summary>
    /// <param name="minimum">The minimum allowed value.</param>
    /// <param name="maximum">The maximum allowed value.</param>
    /// <param name="step">The step increment for the editor control.</param>
    public PropertyRangeAttribute(
        int minimum,
        int maximum,
        int step)
    {
        Minimum = minimum;
        Maximum = maximum;
        Step = step;
    }

    /// <summary>
    /// Gets the minimum allowed value.
    /// </summary>
    public double Minimum { get; }

    /// <summary>
    /// Gets the maximum allowed value.
    /// </summary>
    public double Maximum { get; }

    /// <summary>
    /// Gets the step increment for the editor control.
    /// When <see langword="null"/>, the editor determines an appropriate increment.
    /// </summary>
    public double? Step { get; }

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(Minimum)}: {Minimum}, {nameof(Maximum)}: {Maximum}, {nameof(Step)}: {Step}";
}