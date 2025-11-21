using System.Collections.Generic;
using System.Linq;

namespace Kenedia.Modules.QoL.SubModules.WikiSearch
{
    public static class WikiLocale
    {
        public enum Locale
        {
            Default,
            English,
            German,
            Spanish,
            French,
            //Korean,
            //Chinese
        }

        public static Dictionary<Locale, string> Locales { get; } = new()
        {
            { Locale.Default, "Default" },
            { Locale.English, "English" },
            { Locale.German, "Deutsch" },
            { Locale.Spanish, "Español" },
            { Locale.French, "Français" },
            //{ Locale.Korean, "한국어" },
            //{ Locale.Chinese, "中文" }
        };

        public static string ToDisplayString(Locale locale)
        {
            return Locales.TryGetValue(locale, out string s) ? s : Locales[Locale.English];
        }

        public static Locale FromDisplayString(string displayString)
        {
            return Locales.FirstOrDefault(x => x.Value == displayString).Key;
        }
    }
}
