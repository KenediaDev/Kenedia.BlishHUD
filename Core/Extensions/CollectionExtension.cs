using System;
using System.Collections.Generic;

namespace Kenedia.Modules.Core.Extensions
{
    public static class CollectionExtension
    {
        public static bool TryFind<TSource>(this IEnumerable<TSource> enumerable, Predicate<TSource> compareMethod, out TSource result)
        {
            result = default;

            foreach (TSource item in enumerable)
            {
                if (compareMethod(item))
                {
                    result = item;
                    return true;
                }
            }

            return false;
        }
    }
}
