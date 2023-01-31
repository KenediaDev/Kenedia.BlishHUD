using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Structs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Characters.Controls
{
    internal class CharacterPotraitFrame : MaskedRegion
    {
        public CharacterPotraitFrame()
        {
            Parent = Graphics.SpriteScreen;
            ZIndex = int.MaxValue;
        }

        public Color BorderColor { get; set; } = ContentService.Colors.ColonialWhite;

        public RectangleDimensions BorderWidth { get; set; } = new(2);

        public Rectangle PotraitRegion;

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            PotraitRegion = new Rectangle(Location.X + BorderWidth.Left, Location.Y + BorderWidth.Top, Size.X - BorderWidth.Horizontal, Size.Y - BorderWidth.Vertical);
        }

        protected override void OnMoved(MovedEventArgs e)
        {
            base.OnMoved(e);
            RecalculateLayout();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            Color? borderColor = BorderColor;
            if (borderColor != null)
            {
                // Top
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, BorderWidth.Top), Rectangle.Empty, (Color)borderColor * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, BorderWidth.Top / 2), Rectangle.Empty, (Color)borderColor * 0.6f);

                // Bottom
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 2, bounds.Width, BorderWidth.Bottom), Rectangle.Empty, (Color)borderColor * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 1, bounds.Width, BorderWidth.Bottom / 2), Rectangle.Empty, (Color)borderColor * 0.6f);

                // Left
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, BorderWidth.Left, bounds.Height), Rectangle.Empty, (Color)borderColor * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, BorderWidth.Left / 2, bounds.Height), Rectangle.Empty, (Color)borderColor * 0.6f);

                // Right
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 2, bounds.Top, BorderWidth.Right, bounds.Height), Rectangle.Empty, (Color)borderColor * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 1, bounds.Top, BorderWidth.Right / 2, bounds.Height), Rectangle.Empty, (Color)borderColor * 0.6f);
            }

            base.Paint(spriteBatch, PotraitRegion);
        }
    }
}
