using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Structs;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using Control = Blish_HUD.Controls.Control;

namespace Kenedia.Modules.Core.Controls
{
    class IconLabel : Control
    {
        private Rectangle _iconBounds;
        private Rectangle _textBounds;
        private Rectangle _totalBounds;

        public bool AutoSize;
        private BitmapFont _font = GameService.Content.DefaultFont14;
        private string _text = string.Empty;
        private DetailedTexture _texture;
        private int _innerPadding = 5;
        private RectangleDimensions _outerPadding = new(2);

        public string Text { get => _text; set => Common.SetProperty(ref _text, value, RecalculateLayout); }

        public DetailedTexture Texture { get => _texture; set => Common.SetProperty(ref _texture, value, RecalculateLayout); }

        public RectangleDimensions OuterPadding { get => _outerPadding; set => Common.SetProperty(ref _outerPadding, value, RecalculateLayout); }

        public int InnerPadding { get => _innerPadding; set => Common.SetProperty(ref _innerPadding, value, RecalculateLayout); }

        public BitmapFont Font { get => _font; set => Common.SetProperty(ref _font, value, RecalculateLayout); }

        public Color FontColor { get; set; } = Color.White;

        public bool CaptureInput { get; set; } = false;

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            var p = OuterPadding;
            var size = Font.GetStringRectangle(Text);

            int imagePadding = Texture is null ? 0 : (Math.Max(Texture.Bounds.Height, (int)size.Height) - p.Vertical - Texture.Bounds.Height) / 2;
            int textPadding = (Math.Max(Texture?.Bounds.Height ?? 0, (int)size.Height) - p.Vertical - (int)size.Height) / 2;

            _iconBounds = Texture is null ? Rectangle.Empty : new Rectangle(p.Left, p.Top + imagePadding, Texture.Bounds.Width, Texture.Bounds.Height);
            _textBounds = new(_iconBounds.Right + InnerPadding, p.Top + textPadding, (int)size.Width, (int)size.Height);

            _totalBounds = new(Point.Zero, 
                new(
                    _iconBounds.Width + _textBounds.Width + InnerPadding + p.Right,
                    Math.Max(_iconBounds.Height, _textBounds.Height) + p.Bottom
                ));

            if (AutoSize)
            {
                if (Width != _totalBounds.Width)
                {
                    Width = _totalBounds.Width;
                }

                if (Height != _totalBounds.Height)
                {
                    Height = _totalBounds.Height;
                }
            }
        }

        public override void DoUpdate(GameTime gameTime)
        {
            base.DoUpdate(gameTime);

        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Texture != null)
            {
                spriteBatch.DrawOnCtrl(this, Texture.Texture, _iconBounds, Texture.TextureRegion, Texture.DrawColor ?? Color.White, 0f, default);
            }

            spriteBatch.DrawStringOnCtrl(this, Text, Font, _textBounds, FontColor, false, true, 1, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Middle);
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureInput ? base.CapturesInput() : CaptureType.None;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            Texture = null;
        }
    }
}
