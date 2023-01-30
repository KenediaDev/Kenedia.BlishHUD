using Blish_HUD;
using Blish_HUD.Input;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using System;

namespace Kenedia.Modules.Core.Controls
{
    public class CornerIcon : Blish_HUD.Controls.CornerIcon, ILocalizable
    {
        private Func<string> _setLocalizedTooltip;

        public CornerIcon()
        {
            GameService.Overlay.UserLocale.SettingChanged += UserLocale_SettingChanged;
            UserLocale_SettingChanged(null, null);
        }
        public Action ClickAction { get; set; }

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
            if (SetLocalizedTooltip != null) BasicTooltipText = SetLocalizedTooltip?.Invoke();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            GameService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            ClickAction?.Invoke();
        }
    }
}
