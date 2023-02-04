using Blish_HUD;
using Blish_HUD.Input;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using System;
using System.Diagnostics;

namespace Kenedia.Modules.Core.Controls
{
    public class KeybindingAssigner : Blish_HUD.Controls.KeybindingAssigner, ILocalizable
    {
        private Func<string> _setLocalizedKeyBindingName;
        private Func<string> _setLocalizedTooltip;

        public KeybindingAssigner()
        {
            GameService.Overlay.UserLocale.SettingChanged += UserLocale_SettingChanged;
            BindingChanged += KeybindingAssigner_BindingChanged;
            UserLocale_SettingChanged(null, null);
        }

        private void KeybindingAssigner_BindingChanged(object sender, EventArgs e)
        {
            KeybindChangedAction?.Invoke(KeyBinding);
        }

        public Func<string> SetLocalizedKeyBindingName 
        {
            get => _setLocalizedKeyBindingName;
            set 
            {
                _setLocalizedKeyBindingName = value;
                KeyBindingName = value?.Invoke();
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

        public Action<KeyBinding> KeybindChangedAction { get; set; }

        public void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            if (SetLocalizedKeyBindingName != null) KeyBindingName = SetLocalizedKeyBindingName?.Invoke();
            if (SetLocalizedTooltip != null) BasicTooltipText = SetLocalizedTooltip?.Invoke();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            GameService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;
            BindingChanged -= KeybindingAssigner_BindingChanged;
        }
    }
}
