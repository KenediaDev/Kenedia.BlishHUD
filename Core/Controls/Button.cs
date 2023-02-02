using Blish_HUD;
using Blish_HUD.Input;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;

namespace Kenedia.Modules.Core.Controls
{
    public class Button : Blish_HUD.Controls.StandardButton, ILocalizable
    {
        private Func<string> _setLocalizedText;
        private Func<string> _setLocalizedTooltip;

        public Button()
        {
            GameService.Overlay.UserLocale.SettingChanged += UserLocale_SettingChanged;
            UserLocale_SettingChanged(null, null);
        }

        public Action ClickAction { get; set; }

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

        private bool _selected;
        private Rectangle _layoutIconBounds;
        private Rectangle _layoutTextBounds;

        public bool Selected
        {
            get { return _selected; }
            set { _selected = value; }
        }


        public void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            if (SetLocalizedText != null) Text = SetLocalizedText?.Invoke();
            if (SetLocalizedTooltip != null) BasicTooltipText = SetLocalizedTooltip?.Invoke();
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            ClickAction?.Invoke();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            GameService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            Size2 textDimensions = GetTextDimensions();
            int num = (int)((float)(_size.X / 2) - (textDimensions.Width / 2f));

            if (Icon != null)
            {
                num = (!(textDimensions.Width > 0f)) ? (num + 8) : (num + 10);
                Point point = ResizeIcon ? new Point(16) : Icon.Texture.Bounds.Size;
                _layoutIconBounds = new Rectangle(num - point.X - 4, (_size.Y / 2) - (point.Y / 2), point.X, point.Y);
            }

            _layoutTextBounds = new Rectangle(num, (int)textDimensions.Height + 2, (int)textDimensions.Width, 2);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);

            if (_selected)
            {
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, _layoutTextBounds, Color.Black * 0.6f);
            }
        }
    }
}
