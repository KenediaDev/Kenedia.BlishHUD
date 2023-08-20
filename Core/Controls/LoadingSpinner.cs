using Blish_HUD;
using Blish_HUD.Controls;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using Kenedia.Modules.Core.Services;
using System;

namespace Kenedia.Modules.Core.Controls
{
    public class LoadingSpinner : Blish_HUD.Controls.LoadingSpinner, ILocalizable
    {
        private Func<string> _setLocalizedTooltip;

        public LoadingSpinner()
        {
            LocalizingService.LocaleChanged  += UserLocale_SettingChanged;
            UserLocale_SettingChanged(null, null);
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

        public CaptureType? CaptureInput { get; set; }

        public void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            if (SetLocalizedTooltip is not null) BasicTooltipText = SetLocalizedTooltip?.Invoke();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            GameService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureInput ?? base.CapturesInput();
        }
    }
}
