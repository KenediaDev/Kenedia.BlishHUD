using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class ImageGrayScaled : Control
    {
        private static Color s_defaultColorHovered = new(255, 255, 255, 255);
        private static Color s_defaultColorActive = new(200, 200, 200, 200);
        private static Color s_defaultColorInActive = new(175, 175, 175, 255);
        private Rectangle _textureRectangle = Rectangle.Empty;
        private Texture2D _grayScaleTexture;
        private AsyncTexture2D _texture;

        public bool UseGrayScale { get; set; } = true;

        public AsyncTexture2D Texture
        {
            get => _texture;
            set
            {
                _texture = value;
                _texture.TextureSwapped += Texture_TextureSwapped;
                if (value != null)
                {
                    _grayScaleTexture = ToGrayScaledPalettable(value.Texture);
                }
            }
        }

        public Rectangle SizeRectangle { get; set; }

        public Rectangle TextureRectangle
        {
            get => _textureRectangle;
            set => _textureRectangle = value;
        }

        public bool Active { get; set; } = false;

        public Color ColorHovered { get; set; } = new Color(255, 255, 255, 255);

        public Color ColorActive { get; set; } = new Color(200, 200, 200, 200);

        public Color ColorInActive { get; set; } = new Color(175, 175, 175, 255);

        public float Alpha { get; set; } = 0.25f;

        public void ResetColors()
        {
            ColorHovered = s_defaultColorHovered;
            ColorActive = s_defaultColorActive;
            ColorInActive = s_defaultColorInActive;
        }

        public Texture2D ToGrayScaledPalettable(Texture2D original)
        {
            // make an empty bitmap the same size as original
            var colors = new Color[original.Width * original.Height];
            original.GetData<Color>(colors);
            var destColors = new Color[original.Width * original.Height];
            Texture2D newTexture;

            using (Blish_HUD.Graphics.GraphicsDeviceContext device = GameService.Graphics.LendGraphicsDeviceContext())
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

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (_texture != null)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    UseGrayScale && !Active && !MouseOver ? _grayScaleTexture : _texture,
                    SizeRectangle != Rectangle.Empty ? SizeRectangle : bounds,
                    _textureRectangle == Rectangle.Empty ? _texture.Bounds : _textureRectangle,
                    Active ? ColorActive : MouseOver ? ColorHovered : ColorInActive * (UseGrayScale ? 0.5f : Alpha),
                    0f,
                    default);
            }
        }

        private void Texture_TextureSwapped(object sender, ValueChangedEventArgs<Texture2D> e)
        {
            _grayScaleTexture = ToGrayScaledPalettable(_texture);
            _texture.TextureSwapped -= Texture_TextureSwapped;
        }
    }
}