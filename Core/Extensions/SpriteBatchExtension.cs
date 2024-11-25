using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Structs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using static Blish_HUD.ContentService;

namespace Kenedia.Modules.Core.Extensions
{
    public static class SpriteBatchExtension
    {
        public static void DrawRectangleCenteredRotation(this SpriteBatch spriteBatch, Texture2D textureImage, Rectangle rectangleAreaToDrawAt, Color color, float rotationInRadians, bool flipVertically, bool flipHorizontally)
        {
            SpriteEffects seffects = SpriteEffects.None;
            if (flipHorizontally)
                seffects |= SpriteEffects.FlipHorizontally;
            if (flipVertically)
                seffects |= SpriteEffects.FlipVertically;

            // We must make a couple adjustments in order to properly center this.
            Rectangle r = rectangleAreaToDrawAt;
            var destination = new Rectangle(r.X + r.Width / 2, r.Y + r.Height / 2, r.Width, r.Height);
            var originOffset = new Vector2(textureImage.Width / 2, textureImage.Height / 2);

            // This is a full spriteBatch.Draw method it has lots of parameters to fully control the draw.
            spriteBatch.Draw(textureImage, destination, new Rectangle(0, 0, textureImage.Width, textureImage.Height), color, rotationInRadians, originOffset, seffects, 0);
        }

        public static void DrawCenteredRotationOnCtrl(this SpriteBatch spriteBatch, Control ctrl, Texture2D textureImage, Rectangle rectangleAreaToDrawAt, Rectangle sourceRectangle, Color color, float rotationInRadians, bool flipVertically, bool flipHorizontally)
        {
            SpriteEffects seffects = SpriteEffects.None;
            if (flipHorizontally)
                seffects |= SpriteEffects.FlipHorizontally;
            if (flipVertically)
                seffects |= SpriteEffects.FlipVertically;

            // We must make a couple adjustments in order to properly center this.
            Rectangle r = rectangleAreaToDrawAt;
            var destination = new Rectangle(r.X + r.Width / 2, r.Y + r.Height / 2, r.Width, r.Height);
            var originOffset = new Vector2(textureImage.Width / 2, textureImage.Height / 2);

            // This is a full spriteBatch.Draw method it has lots of parameters to fully control the draw.
            spriteBatch.DrawOnCtrl(ctrl, textureImage, destination, sourceRectangle, color, rotationInRadians, originOffset, seffects);
        }

        public static void DrawFrame(this SpriteBatch spriteBatch, Control ctrl, Rectangle _selectorBounds, Color borderColor, int width = 1)
        {
            // Top
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(_selectorBounds.Left + width, _selectorBounds.Top, _selectorBounds.Width - (width * 2), width), Rectangle.Empty, borderColor * 0.8f);

            // Bottom
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(_selectorBounds.Left + width, _selectorBounds.Bottom - width, _selectorBounds.Width - (width * 2), width), Rectangle.Empty, borderColor * 0.8f);

            // Left
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(_selectorBounds.Left, _selectorBounds.Top, width, _selectorBounds.Height), Rectangle.Empty, borderColor * 0.8f);

            // Right
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(_selectorBounds.Right - width, _selectorBounds.Top, width, _selectorBounds.Height), Rectangle.Empty, borderColor * 0.8f);
        }

        public static void DrawFrame(this SpriteBatch spriteBatch, Control ctrl, Rectangle _selectorBounds, Color borderColor, RectangleDimensions? borderDimensions)
        {
            var border = borderDimensions ?? new RectangleDimensions(2);

            // Top
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(_selectorBounds.Left + border.Left, _selectorBounds.Top, _selectorBounds.Width - border.Horizontal, border.Top), Rectangle.Empty, borderColor * 0.8f);

            // Bottom
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(_selectorBounds.Left + border.Left, _selectorBounds.Bottom - border.Bottom, _selectorBounds.Width - border.Horizontal, border.Bottom), Rectangle.Empty, borderColor * 0.8f);

            // Left
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(_selectorBounds.Left, _selectorBounds.Top, border.Left, _selectorBounds.Height), Rectangle.Empty, borderColor * 0.8f);

            // Right
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(_selectorBounds.Right - border.Right, _selectorBounds.Top, border.Right, _selectorBounds.Height), Rectangle.Empty, borderColor * 0.8f);
        }

        public static void DrawHalfCircleOnCtrl(this SpriteBatch spriteBatch, Control ctrl, Rectangle destinationRectangle, float radius, int sides, Color color, float thickness = 1f, float layerDepth = 0.0f)
        {
            var tDest = destinationRectangle.ToBounds(ctrl.AbsoluteBounds);
            spriteBatch.DrawPolygon(new Vector2(tDest.X, tDest.Y), CreateHalfCircle((double)radius, sides), color, thickness, layerDepth);
        }

        // Adapted from the CreateCircle function in MonoGame.Extended.ShapeExtensions 
        private static Vector2[] CreateHalfCircle(double radius, int sides, float rotation = 0)
        {
            var halfCircle = new Vector2[sides + 1];
            double angleStep = Math.PI / sides;
            double currentAngle = MathHelper.ToRadians(rotation);

            for (int i = 0; i <= sides; i++)
            {
                halfCircle[i] = new Vector2(
                    (float)(radius * Math.Cos(currentAngle)),
                    (float)(radius * Math.Sin(currentAngle))
                );
                currentAngle += angleStep;
            }

            return halfCircle;
        }
    }
}
