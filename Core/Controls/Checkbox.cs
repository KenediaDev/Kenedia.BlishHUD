using Blish_HUD;
using Blish_HUD.Controls;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using System;

namespace Kenedia.Modules.Core.Controls
{
    public class Checkbox : Blish_HUD.Controls.Checkbox, ILocalizable
    {
        private Func<string> _setLocalizedTooltip;
        private Func<string> _setLocalizedText;

        public Checkbox()
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

        public Action<bool> CheckedChangedAction { get; set; }

        public Color TextColor{ get => _textColor; set => Common.SetProperty(ref _textColor, value); }

        public void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            if (SetLocalizedText != null) Text = SetLocalizedText?.Invoke();
            if (SetLocalizedTooltip != null) BasicTooltipText = SetLocalizedTooltip?.Invoke();
        }

        protected override void OnCheckedChanged(CheckChangedEvent e)
        {
            base.OnCheckedChanged(e);

            CheckedChangedAction?.Invoke(e.Checked);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            GameService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;
        }
    }
}
