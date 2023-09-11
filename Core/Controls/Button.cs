using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;

namespace Kenedia.Modules.Core.Controls
{
    public class Button : StandardButton, ILocalizable
    {
        private Texture2D _textureButtonIdle = Content.GetTexture("common/button-states");
        private Texture2D _textureButtonBorder = Content.GetTexture("button-border");

        private Func<string> _setLocalizedText;
        private Func<string> _setLocalizedTooltip;

        public Button()
        {            
            LocalizingService.LocaleChanged += UserLocale_SettingChanged;
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

        private Rectangle _layoutIconBounds;
        private Rectangle _layoutTextBounds;
        private Rectangle _underlineBounds;

        public bool Selected { get; set; }

        public bool SelectedTint { get; set; } = false;

        public void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            if (SetLocalizedText is not null) Text = SetLocalizedText?.Invoke();
            if (SetLocalizedTooltip is not null) BasicTooltipText = SetLocalizedTooltip?.Invoke();
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            ClickAction?.Invoke();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            _textureButtonBorder = null;
            _textureButtonIdle = null;

            GameService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            Size2 textDimensions = GetTextDimensions();
            int num = (int)((float)(_size.X / 2) - (textDimensions.Width / 2f));

            if (Icon is not null)
            {
                num = (!(textDimensions.Width > 0f)) ? (num + 8) : (num + 10);
                Point point = ResizeIcon ? new Point(16) : new(Math.Min(Math.Min(Icon.Texture.Bounds.Size.X, Width), Math.Min(Icon.Texture.Bounds.Size.Y, Height - 7)));
                _layoutIconBounds = new Rectangle(num - point.X - 4, (_size.Y / 2) - (point.Y / 2), point.X, point.Y);
            }

            _layoutTextBounds = new Rectangle(num, (Height  - (int)textDimensions.Height) / 2, (int)textDimensions.Width, (int)textDimensions.Height);
            _underlineBounds = new Rectangle(num, _layoutTextBounds.Bottom - 3, (int)textDimensions.Width, 2);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            RecalculateLayout();

            if (_enabled)
            {
                spriteBatch.DrawOnCtrl(this, _textureButtonIdle, new Rectangle(3, 3, _size.X - 6, _size.Y - 5), new Rectangle(AnimationState * 350, 0, 350, 20), !SelectedTint || Selected ? Color.White : new Color(175, 175, 175));
            }
            else
            {
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(3, 3, _size.X - 6, _size.Y - 5), Color.FromNonPremultiplied(121, 121, 121, 255));
            }

            spriteBatch.DrawOnCtrl(this, _textureButtonBorder, new Rectangle(2, 0, Width - 5, 4), new Rectangle(0, 0, 1, 4));
            spriteBatch.DrawOnCtrl(this, _textureButtonBorder, new Rectangle(Width - 4, 2, 4, Height - 3), new Rectangle(0, 1, 4, 1));
            spriteBatch.DrawOnCtrl(this, _textureButtonBorder, new Rectangle(3, Height - 4, Width - 6, 4), new Rectangle(1, 0, 1, 4));
            spriteBatch.DrawOnCtrl(this, _textureButtonBorder, new Rectangle(0, 2, 4, Height - 3), new Rectangle(0, 3, 4, 1));
            if (Icon is not null)
            {
                spriteBatch.DrawOnCtrl(this, Icon, _layoutIconBounds);
            }

            _textColor = _enabled ? Color.Black : Color.FromNonPremultiplied(51, 51, 51, 255);
            DrawText(spriteBatch, _layoutTextBounds);

            if (Selected)
            {
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, _underlineBounds, Color.Black * 0.6f);
            }
        }
    }
}
