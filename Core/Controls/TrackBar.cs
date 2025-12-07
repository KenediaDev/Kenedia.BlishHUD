using Blish_HUD;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using Kenedia.Modules.Core.Services;
using System;

namespace Kenedia.Modules.Core.Controls
{
    public class TrackBar : Blish_HUD.Controls.TrackBar, ILocalizable
    {
        public TrackBar()
        {
            LocalizingService.LocaleChanged  += UserLocale_SettingChanged;
            ValueChanged += TrackBar_ValueChanged;
            UserLocale_SettingChanged(null, null);
        }

        public Func<string> SetLocalizedTooltip
        {
            get;
            set
            {
                field = value;
                BasicTooltipText = value?.Invoke();
            }
        }

        public Action<int> ValueChangedAction { get; set; }

        public void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            if (SetLocalizedTooltip is not null) BasicTooltipText = SetLocalizedTooltip?.Invoke();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            GameService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;
        }

        private void TrackBar_ValueChanged(object sender, ValueEventArgs<float> e)
        {
            ValueChangedAction?.Invoke((int)Value);
        }
    }
}
