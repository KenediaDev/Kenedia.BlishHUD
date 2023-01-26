using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Color = Microsoft.Xna.Framework.Color;

namespace Kenedia.Modules.Characters.Extensions
{
    internal static class Texture2DExtension
    {
        public static Texture2D CreateTexture2D(this MemoryStream s)
        {
            Texture2D texture;

            using (Blish_HUD.Graphics.GraphicsDeviceContext device = Blish_HUD.GameService.Graphics.LendGraphicsDeviceContext())
            {
                texture = Texture2D.FromStream(device.GraphicsDevice, s);
            }

            return texture;
        }

        public static Texture2D ToGrayScaledPalettable(this Texture2D original)
        {
            // make an empty bitmap the same size as original
            var colors = new Color[original.Width * original.Height];
            original.GetData<Color>(colors);
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

            newTexture.SetData<Color>(destColors);
            return newTexture;
        }
    }
}
