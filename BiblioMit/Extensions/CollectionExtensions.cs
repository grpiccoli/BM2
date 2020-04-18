using System;
using System.Collections.Generic;

namespace BiblioMit.Extensions
{
    public static class CustomCollectionExtensions
    {
        public static void AddRangeOverride<TKey, TValue>(this IDictionary<TKey, TValue> dic, IDictionary<TKey, TValue> dicToAdd) =>
            dicToAdd.ForEach(x => dic[x.Key] = x.Value);
        public static void AddRangeOverride<TKey>(this IList<TKey> list, ICollection<TKey> listToAdd)
        {
            list?.Clear();
            listToAdd.ForEach(x => list.Add(x));
        }
        public static void AddRangeOverride<TKey>(this IList<TKey> list, IList<TKey> listToAdd)
        {
            list?.Clear();
            listToAdd.ForEach(x => list.Add(x));
        }
        public static void AddRangeOverride<TKey>(this IList<TKey> list, IEnumerable<TKey> listToAdd)
        {
            list?.Clear();
            listToAdd.ForEach(x => list.Add(x));
        }
        public static void AddRangeOverride<TKey>(this ICollection<TKey> list, IList<TKey> listToAdd)
        {
            list?.Clear();
            listToAdd.ForEach(x => list.Add(x));
        }
        public static void AddRangeOverride<TKey>(this ICollection<TKey> list, ICollection<TKey> listToAdd)
        {
            list?.Clear();
            listToAdd.ForEach(x => list.Add(x));
        }
        public static void AddRangeNewOnly<TKey, TValue>(this IDictionary<TKey, TValue> dic, IDictionary<TKey, TValue> dicToAdd) =>
            dicToAdd.ForEach(x => { if (!dic.ContainsKey(x.Key)) dic.Add(x.Key, x.Value); });

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dic, IDictionary<TKey, TValue> dicToAdd) =>
            dicToAdd.ForEach(x => dic.Add(x.Key, x.Value));
        public static bool ContainsKeys<TKey, TValue>(this IDictionary<TKey, TValue> dic, IEnumerable<TKey> keys)
        {
            bool result = false;
            keys.ForEachOrBreak((x) => { result = dic.ContainsKey(x); return result; });
            return result;
        }
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source != null) foreach (var item in source) action(item);
        }
        public static void ForEachOrBreak<T>(this IEnumerable<T> source, Func<T, bool> func)
        {
            if (source != null)
                foreach (var item in source)
                {
                    bool result = func(item);
                    if (result) break;
                }
        }
        public static string GetValue(this Dictionary<(int, int), string> matrix, int column, int row)
        {
            if(matrix != null && matrix.ContainsKey((column, row)))
                return matrix[(column, row)];
            return null;
        }
    }
}
