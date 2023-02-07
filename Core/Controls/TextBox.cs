using Blish_HUD;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using System;

namespace Kenedia.Modules.Core.Controls
{
    public class TextBox : Blish_HUD.Controls.TextBox, ILocalizable
    {
        private Func<string> _setLocalizedText;
        private Func<string> _setLocalizedTooltip;

        public TextBox()
        {
            GameService.Overlay.UserLocale.SettingChanged += UserLocale_SettingChanged;
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
            if (SetLocalizedText != null) Text = SetLocalizedText?.Invoke();
            if (SetLocalizedTooltip != null) BasicTooltipText = SetLocalizedTooltip?.Invoke();
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
