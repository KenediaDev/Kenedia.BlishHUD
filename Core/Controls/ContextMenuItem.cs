using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using Kenedia.Modules.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kenedia.Modules.Core.Controls
{
    public class ContextMenuItem : ContextMenuStripItem, ILocalizable
    {
        private Func<string> _setLocalizedText;

        public ContextMenuItem()
        {

            LocalizingService.LocaleChanged += UserLocale_SettingChanged;
            UserLocale_SettingChanged(null, null);
        }

        public ContextMenuItem(Func<string> setLocalizedText)
        {
            SetLocalizedText = setLocalizedText;
        }

        public ContextMenuItem(Func<string> setLocalizedText, Action onClickAction)
        {
            SetLocalizedText = setLocalizedText;
            OnClickAction = onClickAction;
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

        public Action OnClickAction { get; set; }

        public void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            if (SetLocalizedText != null) Text = SetLocalizedText?.Invoke();
        }
                
        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            OnClickAction?.Invoke();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            LocalizingService.LocaleChanged -= UserLocale_SettingChanged;
        }
    }
}
