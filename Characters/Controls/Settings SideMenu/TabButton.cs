using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Characters.Extensions;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class TabButton : Control
    {
        public TabButton()
        {
            Height = 25;
        }

        public bool Active { get; set; }

        public bool UseGrayScale { get; set; }

        public AsyncTexture2D Icon { get; set; }

        public AsyncTexture2D IconGrayScale { get; set; }

        public Rectangle TextureRectangle { get; set; } = Rectangle.Empty;

        public BitmapFont Font { get; set; }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            Color bgColor = Active ? Color.Transparent : Color.Black;
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Rectangle.Empty, bgColor * (MouseOver ? 0.35f : 0.55f));

            if (Icon != null)
            {
                Rectangle tRect = TextureRectangle != Rectangle.Empty ? TextureRectangle : Icon.Bounds;
                int size = bounds.Height - 4;

                if (UseGrayScale && IconGrayScale == null)
                {
                    IconGrayScale = Icon.Texture.ToGrayScaledPalettable();
                }

                spriteBatch.DrawOnCtrl(
                    this,
                    Active ? Icon : UseGrayScale ? IconGrayScale : Icon,
                    new Rectangle(2 + ((bounds.Width - size) / 2), 3, size, size),
                    tRect,
                    Active ? Color.White : new Color(75, 75, 75),
                    0f,
                    default);
            }

            Color color = Color.Black;

            // Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 2, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 1, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);// Active = true;
        }
    }
}
