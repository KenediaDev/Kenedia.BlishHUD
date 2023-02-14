using Blish_HUD;
using System;

namespace Kenedia.Modules.Core.Services
{
    public static class LocalizingService
    {
        public static bool Enabled { get; set; } = true;

        public static event EventHandler<ValueChangedEventArgs<Gw2Sharp.WebApi.Locale>> LocaleChanged;

        public static void OnLocaleChanged(object sender, ValueChangedEventArgs<Gw2Sharp.WebApi.Locale> eventArgs)
        {
            TriggerLocaleChanged(Enabled, sender, eventArgs);
        }

        public static void TriggerLocaleChanged(bool force = false, object sender = null, ValueChangedEventArgs<Gw2Sharp.WebApi.Locale> eventArgs = null)
        {
            if(force) LocaleChanged?.Invoke(sender, eventArgs);
        }
    }
}
