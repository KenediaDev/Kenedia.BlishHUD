using Gw2Sharp.WebApi;
using System;
using System.Collections.Generic;

namespace Kenedia.Modules.Core.Models
{
    public class LocalizedString : Dictionary<Locale, string>
    {
        public LocalizedString()
        {
            foreach(Locale locale in Enum.GetValues(typeof(Locale)))
            {
                if (locale is not Locale.Korean and not Locale.Chinese)
                {
                    Add(locale, null);
                }
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

        public new void Add(Locale key, string value)
        {
            if (!ContainsKey(key))
            {
                base.Add(key, value);
            }
            else
            {
                this[key] = value;
            }
        }

        private void SetText(string value, Locale? lang = null)
        {
            lang ??= Blish_HUD.GameService.Overlay.UserLocale.Value;

            if (lang is not null)
            {
                this[(Locale)lang] = value;
            }
        }

        public string GetText(Locale? lang = null)
        {
            lang ??= Blish_HUD.GameService.Overlay.UserLocale.Value;

            return lang is not null && TryGetValue((Locale) lang, out string text) && !string.IsNullOrEmpty(text) ? text : TryGetValue(Locale.English, out string english) && !string.IsNullOrEmpty(english) ? english : null;
        }
    }
}
