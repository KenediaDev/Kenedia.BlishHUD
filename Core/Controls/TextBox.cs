using Blish_HUD;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using Kenedia.Modules.Core.Services;
using System;

namespace Kenedia.Modules.Core.Controls
{
    public class TextBox : Blish_HUD.Controls.TextBox, ILocalizable
    {
        private Func<string> _setLocalizedText;
        private Func<string> _setLocalizedTooltip;
        private Func<string> _setLocalizedPlaceholder;

        public TextBox()
        {
            LocalizingService.LocaleChanged  += UserLocale_SettingChanged;
            TextChanged += OnTextChanged;
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

        public Func<string> SetLocalizedPlaceholder
        {
            get => _setLocalizedPlaceholder;
            set
            {
                _setLocalizedPlaceholder = value;
                PlaceholderText = value?.Invoke();
            }
        }

        public Action ClickAction { get; set; }

        public Action<string> EnterPressedAction { get; set; }

        public Action<string> TextChangedAction { get; set; }

        public void ResetText()
        {
            Text = null;
        }

        protected override void OnEnterPressed(EventArgs e)
        {
            base.OnEnterPressed(e);

            EnterPressedAction?.Invoke(Text);
        }

        public void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            if (SetLocalizedText is not null) Text = SetLocalizedText?.Invoke();
            if (SetLocalizedTooltip is not null) BasicTooltipText = SetLocalizedTooltip?.Invoke();
            if (SetLocalizedPlaceholder is not null) PlaceholderText = SetLocalizedPlaceholder?.Invoke();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            TextChanged -= OnTextChanged;
            GameService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            TextChangedAction?.Invoke(Text);
        }
    }
}
