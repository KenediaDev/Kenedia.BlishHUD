using Blish_HUD;
using System;

namespace Kenedia.Modules.Core.Interfaces
{
    interface ILocalizable
    {
       void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Gw2Sharp.WebApi.Locale> e);
    }
}
