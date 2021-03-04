using System.Collections.Generic;

namespace Excubo.Blazor.Diagrams.__Internal
{
    internal static class Extensions
    {
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