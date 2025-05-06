using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Shared;

public static partial class Extensions
{
    public static void AddRangeToBeginning<T>(this List<T> list, IEnumerable<T> items)
    {
        if (list == null) throw new ArgumentNullException(nameof(list));
        if (items == null) throw new ArgumentNullException(nameof(items));

        list.InsertRange(0, items);
    }

    // Compare strings in a natural way, e.g. "file2" comes before "file10"
    public static int CompareToNatural(this string? a, string? b)
    {
        if (a == null || b == null)
            return string.Compare(a, b, StringComparison.Ordinal);

        var regex = new Regex(@"\d+|\D+"); // Matches numbers (\d+) or non-numbers (\D+)
        var matchesA = regex.Matches(a);
        var matchesB = regex.Matches(b);

        int count = Math.Min(matchesA.Count, matchesB.Count);

        for (int i = 0; i < count; i++)
        {
            var partA = matchesA[i].Value;
            var partB = matchesB[i].Value;

            // If both parts are numeric, compare as numbers
            if (int.TryParse(partA, out int numA) && int.TryParse(partB, out int numB))
            {
                int numComparison = numA.CompareTo(numB);
                if (numComparison != 0)
                    return numComparison;
            }
            else
            {
                // Compare as strings
                int strComparison = string.Compare(partA, partB, StringComparison.Ordinal);
                if (strComparison != 0)
                    return strComparison;
            }
        }

        // If all parts so far are equal, compare by length
        return matchesA.Count.CompareTo(matchesB.Count);
    }

    public static string[] SplitNewlines(this string str)
    {
        return str.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
    }

    public static bool Unset(this string? str)
    {
        return string.IsNullOrEmpty(str);
    }

    public static bool IsSet([NotNullWhen(true)] this string? str)
    {
        return !string.IsNullOrEmpty(str);
    }

    public static T? SafeGet<T>(this IList<T> list, int index)
    {
        if (list == null || index < 0 || index >= list.Count)
            return default(T);
        return list[index];
    }

    public static void AddIfNotExists<T>(this List<T> source, T item)
    {
        if (!source.Contains(item))
        {
            source.Add(item);
        }
    }

    public static List<T> OrEmpty<T>(this List<T> source)
    {
        if (source == null) return new List<T>();
        return source;
    }

    public static string DelimAppend(this string str, string appendStr)
    {
        if (string.IsNullOrEmpty(str))
        {
            return appendStr.Substring(1);
        }
        return str + appendStr;
    }

    public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue createValue) where TKey : notnull
    {
        if (!dict.ContainsKey(key))
        {
            dict[key] = createValue;
        }
        return dict[key];
    }

    // Use this overload when the value is expensive to create
    public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> createValueFunc) where TKey : notnull
    {
        if (!dict.ContainsKey(key))
        {
            dict[key] = createValueFunc();
        }
        return dict[key];
    }

    public static bool HasDuplicate<T, TKey>(this IList<T> list, Func<T, TKey> keySelector, out TKey? firstDupValue)
    {
        HashSet<TKey> items = new();
        foreach (T item in list)
        {
            TKey key = keySelector(item);
            if (items.Contains(key))
            {
                firstDupValue = key;
                return true;
            }
            items.Add(key);
        }
        firstDupValue = default;
        return false;
    }

    public static List<TResult> SelectList<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        return source.Select(selector).ToList();
    }
}
