using System.Collections.Generic;
using System.Linq;

namespace Excubo.Blazor.Diagrams.__Internal
{
    internal static class Extensions
    {
        internal static NullAllowingDictionary<TKey, List<TValue>> ToNullAllowingDictionary<TKey, TValue>(this IEnumerable<IGrouping<TKey, TValue>> groups)
        {
            var result = new NullAllowingDictionary<TKey, List<TValue>>();
            foreach (var group in groups)
            {
                result.Add(group.Key, group.ToList());
            }
            return result;
        }

        internal static void Merge<T>(this List<List<T>> target, List<List<T>> other)
        {
            for (var i = 0; i < other.Count; ++i)
            {
                if (i < target.Count)
                {
                    target[i].AddRange(other[i]);
                }
                else
                {
                    target.Add(other[i]);
                }
            }
        }
    }
}