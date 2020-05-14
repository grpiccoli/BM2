using BiblioMit.Models.Entities.Digest;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public static void AddOrSum(this ICollection<DeclarationDate> dates, DeclarationDate date)
        {
            if (date == null) return;
            if (dates == null) dates = new List<DeclarationDate>();
            if (dates.Any(d => d.Date == date.Date))
            {
                dates.FirstOrDefault(d => d.Date == date.Date).Weight += date.Weight;
            }
            else
            {
                dates.Add(date);
            }
        }
        public static async void AddOrChange(this DbSet<DeclarationDate> dates, DeclarationDate date)
        {
            if (date == null || dates == null) return;
            if (dates.Any(d => d.SernapescaDeclarationId == date.SernapescaDeclarationId && d.Date == date.Date))
            {
                var old = await dates
                    .FirstOrDefaultAsync(d => d.SernapescaDeclarationId == date.SernapescaDeclarationId && d.Date == date.Date)
                    .ConfigureAwait(false);
                old.Weight = date.Weight;
                dates.Update(old);
            }
            dates.Add(date);
        }
        public static void Move<T>(this List<T> list, int oldIndex, int newIndex)
        {
            if (list == null) return;
            T item = list[oldIndex];
            list.RemoveAt(oldIndex);
            list.Insert(newIndex, item);
        }
        public static void MoveFirst<T>(this List<T> list, Func<T, bool> func)
        {
            if (list == null) return;
            T item = list.FirstOrDefault(func);
            list.Remove(item);
            list.Prepend(item);
        }
        public static void MoveLast<T>(this List<T> list, Func<T, bool> func)
        {
            if (list == null) return;
            T item = list.FirstOrDefault(func);
            list.Remove(item);
            list.Append(item);
        }
        public static IEnumerable<TKey> ExceptNull<TKey>(this IEnumerable<TKey> list, IEnumerable<TKey> listExcept)
        {
            if (list == null) return listExcept;
            if (listExcept == null) return list;
            return list.Except(listExcept);
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
