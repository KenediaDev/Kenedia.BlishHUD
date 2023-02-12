using Blish_HUD;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using Kenedia.Modules.Core.Services;
using System;

namespace Kenedia.Modules.Core.Controls
{
    public class Label : Blish_HUD.Controls.Label, ILocalizable
    {
        private Func<string> _setLocalizedText;
        private Func<string> _setLocalizedTooltip;

        public Label()
        {
            LocalizingService.LocaleChanged  += UserLocale_SettingChanged;
            UserLocale_SettingChanged(null, null);
        }

        public Func<string> SetLocalizedText 
        {
            get => _setLocalizedText;
            set
            {
                _setLocalizedText = value;
                Text = value?.Invoke();
            } 
        }

        public Func<string> SetLocalizedTooltip
        {
            get => _setLocalizedTooltip;
            set
            {
                _setLocalizedTooltip = value;
                BasicTooltipText = value?.Invoke();
            }
        }

        public void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            if (SetLocalizedText != null) Text = SetLocalizedText?.Invoke();
            if (SetLocalizedTooltip != null) BasicTooltipText = SetLocalizedTooltip?.Invoke();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            GameService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;
        }
    }
}
