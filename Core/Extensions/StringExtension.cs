using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Kenedia.Modules.Core.Extensions
{
    public static class StringExtension
    {
        private static readonly Regex s_diacritics = new(@"\p{M}");
        private static readonly Regex s_namingConventionConvert = new(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

        public static string RemoveDiacritics(this string s)
        {
            string result = s.Normalize(NormalizationForm.FormD);
            return s_diacritics.Replace(result, string.Empty).ToString().Replace("Æ", "Ae").Replace("æ", "ae").Replace("œ", "oe").Replace("Œ", "Oe");
        }

        public static string ToLowercaseNamingConvention(this string s)
        {
            return s_namingConventionConvert.Replace(s.Replace("_", ""), " ");
        }

        public static string RemoveLeadingNumbers(this string input)
        {
            // Use regular expression to remove leading numbers
            string result = Regex.Replace(input, @"^\d+", "");

            return result;
        }

        public static string RemoveSpaces(this string input)
        {
            return input.Replace(" ", "");
        }

        public static string SplitStringOnUppercase(this string input)
        {
            var result = new StringBuilder();

            foreach (char c in input)
            {
                if (char.IsUpper(c) && result.Length > 0)
                {
                    _ = result.Append(' '); // Add space before uppercase letter
                }

                _ = result.Append(c);
            }

            return result.ToString();
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

        public static bool ContainsAny(this string haystack, params string[] needles)
        {
            foreach (string needle in needles)
            {
                if (haystack.Contains(needle))
                    return true;
            }

            return false;
        }

        public static bool ContainsAnyTrimmed(this string haystack, params string[] needles)
        {
            foreach (string needle in needles)
            {
                if (haystack.Contains(needle.Trim()))
                    return true;
            }

            return false;
        }
    }
}
