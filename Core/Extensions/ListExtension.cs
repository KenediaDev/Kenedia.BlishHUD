using System;
using System.Collections.Generic;
using System.Linq;

namespace Kenedia.Modules.Core.Extensions
{
    public static class ListExtension
    {
        public static bool ContainsAny<T>(this IEnumerable<T> sequence, params T[] matches)
        {
            return matches.Any(value => sequence.Contains(value));
        }

        public static bool ContainsAll<T>(this IEnumerable<T> sequence, params T[] matches)
        {
            return matches.All(value => sequence.Contains(value));
        }

        public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (T item in list)
            {
                action(item);
            }
        }

        public static void ForEach<T>(this IList<T> list, Action<T> action)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (T item in list)
            {
                action(item);
            }
        }

        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}
