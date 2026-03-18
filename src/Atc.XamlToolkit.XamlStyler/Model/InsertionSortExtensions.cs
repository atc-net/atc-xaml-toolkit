namespace Atc.XamlToolkit.XamlStyler.Model;

internal static class InsertionSortExtensions
{
    /// <summary>
    /// Stable insertion sort for small collections.
    /// Used instead of List.Sort to guarantee stability for attribute ordering.
    /// </summary>
    /// <typeparam name="T">The type of list elements.</typeparam>
    /// <param name="list">The list to sort.</param>
    /// <param name="comparison">The comparison function.</param>
    public static void InsertionSort<T>(
        this List<T> list,
        Comparison<T> comparison)
    {
        for (var i = 1; i < list.Count; i++)
        {
            var key = list[i];
            var j = i - 1;

            while (j >= 0 && comparison(list[j], key) > 0)
            {
                list[j + 1] = list[j];
                j--;
            }

            list[j + 1] = key;
        }
    }
}