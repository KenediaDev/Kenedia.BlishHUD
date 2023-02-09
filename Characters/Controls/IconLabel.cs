using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class IconLabel : Control, IFontControl
    {
        private AsyncTexture2D _icon;
        private BitmapFont _font = GameService.Content.DefaultFont14;
        private string _text;
        private Rectangle _iconRectangle = Rectangle.Empty;
        private Rectangle _textRectangle = Rectangle.Empty;

        public bool CaptureInput { get; set; } = false;

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                UpdateLayout();
            }
        }

        public Color TextColor { get; set; } = Color.White;

        public AsyncTexture2D Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                if (value != null)
                {
                    UpdateLayout();
                }
            }
        }

        public BitmapFont Font
        {
            get => _font;
            set
            {
                _font = value;
                if (value != null)
                {
                    UpdateLayout();
                }
            }
        }

        public bool AutoSizeWidth { get; set; }

        public bool AutoSizeHeight { get; set; }

        public Rectangle TextureRectangle { get; set; } = Rectangle.Empty;

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            AsyncTexture2D texture = Icon;
            if (texture != null)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    texture,
                    _iconRectangle,
                    TextureRectangle == Rectangle.Empty ? texture.Bounds : TextureRectangle,
                    Color.White,
                    0f,
                    default);
            }

            spriteBatch.DrawStringOnCtrl(
                    this,
                    Text,
                    Font,
                    _textRectangle,
                    TextColor,
                    false,
                    HorizontalAlignment.Left,
                    VerticalAlignment.Middle);
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureInput ? base.CapturesInput() : CaptureType.None;
        }

        private void UpdateLayout()
        {
            MonoGame.Extended.Size2 textbounds = Font.MeasureString(Text);
            if (AutoSizeWidth)
            {
                Width = Math.Max((int)textbounds.Width + 4 + (Icon == null ? 0 : Height + 5), Height);
            }

            if (AutoSizeHeight)
            {
                Height = Math.Max((int)textbounds.Height + 4, 0);
            }
            

            _iconRectangle = Icon == null ? Rectangle.Empty : new Rectangle(2, 2, LocalBounds.Height - 4, LocalBounds.Height - 4);
            _textRectangle = new Rectangle(_iconRectangle.Right + (Icon == null ? 0 : 5), 2, LocalBounds.Width - (_iconRectangle.Right + (Icon == null ? 0 : 5) + 2), LocalBounds.Height - 4);
        }
    }
}
