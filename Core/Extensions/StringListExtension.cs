﻿using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kenedia.Modules.Core.Extensions
{
    public static class StringListExtension
    {
        // Enumerate list of strings to a single string
        public static string Enumerate(this IList<string> list, string separator = ", ", string? enumerationFormat = null)
        {
            string enumFormat = enumerationFormat ?? "{0}";

            StringBuilder sb = new();
            for (int i = 0; i < list.Count; i++)
            {
                _ = sb.Append($"{string.Format(enumFormat, i + 1)}{list[i]}");
                if (i < list.Count - 1)
                {
                    _ = sb.Append(separator);
                }
            }

            return sb.ToString();
        }
        public static string Enumerate(this IEnumerable<string> list, string separator = ", ", string? enumerationFormat = null)
        {
            string enumFormat = enumerationFormat ?? "{0}";

            StringBuilder sb = new();
            int count = list.Count();

            for (int i = 0; i < count; i++)
            {
                _ = sb.Append($"{string.Format(enumFormat, i + 1)}{list.ElementAt(i)}");
                if (i < count - 1)
                {
                    _ = sb.Append(separator);
                }
            }

            return sb.ToString();
        }

    }
}
