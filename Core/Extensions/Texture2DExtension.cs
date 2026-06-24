using Microsoft.Xna.Framework.Graphics;
using System;
using System.Drawing;
using System.IO;
using Color = Microsoft.Xna.Framework.Color;

namespace Kenedia.Modules.Core.Extensions
{
    public static class Texture2DExtension
    {
        public static Texture2D CreateTexture2D(this MemoryStream s)
        {
            if (s is null) throw new ArgumentNullException(nameof(s));
            if (s.Length == 0) throw new ArgumentException("The image stream is empty.", nameof(s));

            s.Position = 0;

            using (Blish_HUD.Graphics.GraphicsDeviceContext device = Blish_HUD.GameService.Graphics.LendGraphicsDeviceContext())
            {
                return Texture2D.FromStream(device.GraphicsDevice, s);
            }
        }

        public static Texture2D CreateTexture2D(this Bitmap bitmap)
        {
            if (bitmap is null) throw new ArgumentNullException(nameof(bitmap));

            using MemoryStream stream = new();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            return stream.CreateTexture2D();
        }

        public static Texture2D ToGrayScaledPalettable(this Texture2D original)
        {
            if(original == null || original.IsDisposed) return null;

            // make an empty bitmap the same size as original
            var colors = new Color[original.Width * original.Height];
            original.GetData(colors);
            var destColors = new Color[original.Width * original.Height];
            Texture2D newTexture;

            using (Blish_HUD.Graphics.GraphicsDeviceContext device = Blish_HUD.GameService.Graphics.LendGraphicsDeviceContext())
            {
                newTexture = new Texture2D(device.GraphicsDevice, original.Width, original.Height);
            }

            for (int i = 0; i < original.Width; i++)
            {
                for (int j = 0; j < original.Height; j++)
                {
                    // get the pixel from the original image
                    int index = i + (j * original.Width);
                    Color originalColor = colors[index];

                    // create the grayscale version of the pixel
                    float maxval = .3f + .59f + .11f + .79f;
                    float grayScale = (originalColor.R / 255f * .3f) + (originalColor.G / 255f * .59f) + (originalColor.B / 255f * .11f) + (originalColor.A / 255f * .79f);
                    grayScale /= maxval;

                    destColors[index] = new Color(grayScale, grayScale, grayScale, originalColor.A);
                }
            }

            newTexture.SetData(destColors);
            return newTexture;
        }
    }
}
