using Gw2Sharp.WebApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kenedia.Modules.Core.Models
{
    public class LocalizedString : Dictionary<Locale, string>
    {
        public LocalizedString()
        {
            foreach(Locale locale in Enum.GetValues(typeof(Locale)))
            {
                Add(locale, null);
            }
        }

        public LocalizedString(string comon) : this()
        {
            this[Locale.English] = comon;
        }

        public LocalizedString(string comon, Locale lang) : this()
        {
            this[lang] = comon;
        }

        public string Text
        {
            get => GetText();
            set => SetText(value);
        }

        private void SetText(string value, Locale? lang = null)
        {
            lang ??= Blish_HUD.GameService.Overlay.UserLocale.Value;

            if (lang != null)
            {
                this[(Locale)lang] = value;
            }
        }

        public string GetText(Locale? lang = null)
        {
            lang ??= Blish_HUD.GameService.Overlay.UserLocale.Value;

            return lang != null && TryGetValue((Locale) lang, out string text) && !string.IsNullOrEmpty(text) ? text : TryGetValue(Locale.English, out string english) && !string.IsNullOrEmpty(english) ? english : null;
        }
    }
}
