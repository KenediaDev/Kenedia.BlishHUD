using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
