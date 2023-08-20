using Blish_HUD;

namespace Kenedia.Modules.Core.Interfaces
{
    public interface ILocalizable
    {
       void UserLocale_SettingChanged(object sender = null, ValueChangedEventArgs<Gw2Sharp.WebApi.Locale> e = null);
    }
}
