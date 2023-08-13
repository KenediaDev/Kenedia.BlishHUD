using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using Kenedia.Modules.Core.Services;
using System;

namespace Kenedia.Modules.Core.Controls
{
    public class ContextMenuItem : ContextMenuStripItem, ILocalizable
    {
        private Func<string> _setLocalizedText;
        private Func<string> _setLocalizedTooltip;

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

        public ContextMenuItem(Func<string> setLocalizedText, Action onClickAction, Func<string> setLocalizedTooltip = null)
        {
            SetLocalizedText = setLocalizedText;
            OnClickAction = onClickAction;
            SetLocalizedTooltip = setLocalizedTooltip;
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
            if (SetLocalizedText is not null) Text = SetLocalizedText?.Invoke();
            if (SetLocalizedTooltip is not null) BasicTooltipText = SetLocalizedTooltip?.Invoke();
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
