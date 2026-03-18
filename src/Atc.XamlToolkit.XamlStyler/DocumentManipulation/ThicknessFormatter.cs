namespace Atc.XamlToolkit.XamlStyler.DocumentManipulation;

internal static class ThicknessFormatter
{
    public static bool TryFormat(
        string input,
        char separator,
        out string formatted)
    {
        formatted = input;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        // Try to split by comma or space
        var parts = input.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length is not (1 or 2 or 4))
        {
            return false;
        }

        // Verify all parts are valid numbers
        foreach (var part in parts)
        {
            if (!double.TryParse(part.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                return false;
            }
        }

        formatted = string.Join(separator.ToString(CultureInfo.InvariantCulture), parts.Select(p => p.Trim()));
        return !string.Equals(formatted, input, StringComparison.Ordinal);
    }
}