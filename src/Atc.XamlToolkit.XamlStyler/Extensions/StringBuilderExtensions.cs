namespace Atc.XamlToolkit.XamlStyler.Extensions;

internal static class StringBuilderExtensions
{
    public static bool IsNewLine(this StringBuilder sb) =>
        sb.Length > 0 && sb[sb.Length - 1] == '\n';

    public static int LastIndexOf(
        this StringBuilder sb,
        char value)
    {
        for (var i = sb.Length - 1; i >= 0; i--)
        {
            if (sb[i] == value)
            {
                return i;
            }
        }

        return -1;
    }

    public static string Substring(
        this StringBuilder sb,
        int startIndex,
        int length) =>
        sb.ToString(startIndex, length);

    public static StringBuilder TrimEnd(
        this StringBuilder sb,
        params char[] trimChars)
    {
        var index = sb.Length;
        while (index > 0 && trimChars.Contains(sb[index - 1]))
        {
            index--;
        }

        sb.Length = index;
        return sb;
    }

    public static StringBuilder TrimUnescaped(
        this StringBuilder sb,
        params char[] trimChars)
    {
        var index = sb.Length;
        while (index > 0 && trimChars.Contains(sb[index - 1]))
        {
            if (index > 1 && sb[index - 2] == '\\')
            {
                break;
            }

            index--;
        }

        sb.Length = index;
        return sb;
    }
}