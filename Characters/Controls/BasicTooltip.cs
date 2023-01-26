using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class BasicTooltip : Control
    {
        private BitmapFont _font = GameService.Content.DefaultFont14;

        private string _text;

        public Rectangle TextureRectangle { get; set; } = new Rectangle(40, 25, 250, 250);

        public AsyncTexture2D Background { get; set; } = AsyncTexture2D.FromAssetId(156003);

        public BitmapFont Font
        {
            get => _font;
            set
            {
                _font = value;
                UpdateLayout();
            }
        }

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                if (value == null)
                {
                    Hide();
                }

                UpdateLayout();
            }
        }

        public override void DoUpdate(GameTime gameTime)
        {
            base.DoUpdate(gameTime);
            Location = new Point(Input.Mouse.Position.X, Input.Mouse.Position.Y + 25);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Font == null || Text == null)
            {
                return;
            }

            spriteBatch.DrawOnCtrl(
                this,
                Background,
                bounds,
                TextureRectangle != Rectangle.Empty ? TextureRectangle : Background.Bounds,
                Color.White,
                0f,
                default);

            spriteBatch.DrawStringOnCtrl(
                this,
                Text,
                Font,
                bounds,
                Color.White, // new Color(247, 231, 182, 97),
                false,
                HorizontalAlignment.Center,
                VerticalAlignment.Middle);

            Color color = Color.Black;

            // Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            // Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 2, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 1, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 2, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 1, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            Location = new Point(Input.Mouse.Position.X, Input.Mouse.Position.Y + 25);
        }

        private void UpdateLayout()
        {
            if (Font != null && Text != null)
            {
                MonoGame.Extended.Size2 sSize = Font.MeasureString(Text);
                Size = new Point(10 + (int)sSize.Width, 10 + (int)sSize.Height);
            }
        }
    }
}
