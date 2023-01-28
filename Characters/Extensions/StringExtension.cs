using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Kenedia.Modules.Characters.Extensions
{
    public static class StringExtension
    {
        private static readonly Regex s_diacritics = new(@"\p{M}");

        public static string RemoveDiacritics(this string s)
        {
            string result = s.Normalize(NormalizationForm.FormD);
            return s_diacritics.Replace(result, string.Empty).ToString().Replace("Æ", "Ae").Replace("æ", "ae").Replace("œ", "oe").Replace("Œ", "Oe");
        }

        /// <summary>
        /// Compute the distance between two strings.
        /// https://stackoverflow.com/questions/28647002/find-most-accurate-match-in-strings
        /// </summary>
        public static int LevenshteinDistance(this string source, string target)
        {
            if (string.IsNullOrEmpty(source))
            {
                return string.IsNullOrEmpty(target) ? 0 : target.Length;
            }

            if (string.IsNullOrEmpty(target)) return source.Length;

            if (source.Length > target.Length)
            {
                (source, target) = (target, source);
            }

            int m = target.Length;
            int n = source.Length;
            int[,] distance = new int[2, m + 1];
            // Initialize the distance 'matrix'
            for (int j = 1; j <= m; j++) distance[0, j] = j;

            int currentRow = 0;
            for (int i = 1; i <= n; ++i)
            {
                currentRow = i & 1;
                distance[currentRow, 0] = i;
                int previousRow = currentRow ^ 1;
                for (int j = 1; j <= m; j++)
                {
                    int cost = target[j - 1] == source[i - 1] ? 0 : 1;
                    distance[currentRow, j] = Math.Min(Math.Min(
                        distance[previousRow, j] + 1,
                        distance[currentRow, j - 1] + 1),
                        distance[previousRow, j - 1] + cost);
                }
            }

            return distance[currentRow, m];
        }
    }
}
