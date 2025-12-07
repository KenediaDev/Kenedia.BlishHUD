using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using Kenedia.Modules.Core.Services;
using System;

namespace Kenedia.Modules.Core.Controls
{
    public class TextBox : Blish_HUD.Controls.TextBox, ILocalizable
    {
        public TextBox()
        {
            LocalizingService.LocaleChanged += UserLocale_SettingChanged;
            TextChanged += OnTextChanged;
            UserLocale_SettingChanged(null, null);
        }

        public Func<string> SetLocalizedText
        {
            get;
            set
            {
                field = value;
                Text = value?.Invoke();
            }
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

        public Func<string> SetLocalizedPlaceholder
        {
            get;
            set
            {
                field = value;
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

        protected override void OnClick(MouseEventArgs e)
        {
            if (!Enabled) return;

            base.OnClick(e);
        }

        protected override CaptureType CapturesInput()
        {
            return Enabled ? base.CapturesInput() : CaptureType.None;   
        }
    }
}
