using Blish_HUD;
using System;

namespace Kenedia.Modules.Core.Interfaces
{
    public interface ILocalizable
    {
        public Func<string> SetLocalizedTooltip { get; set; }

       void UserLocale_SettingChanged(object sender = null, ValueChangedEventArgs<Gw2Sharp.WebApi.Locale> e = null);
    }
}
