using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class ImageButton : Control
    {
        private static Color s_defaultColorHovered = new(255, 255, 255, 255);
        private static Color s_defaultColorClicked = new(255, 255, 255, 255);
        private static Color s_defaultColor = new(255, 255, 255, 255);
        private Rectangle _textureRectangle = Rectangle.Empty;
        private bool _clicked;

        public AsyncTexture2D Texture { get; set; }

        public AsyncTexture2D HoveredTexture { get; set; }

        public AsyncTexture2D ClickedTexture { get; set; }

        public Rectangle SizeRectangle { get; set; }

        public Rectangle TextureRectangle
        {
            get => _textureRectangle;
            set => _textureRectangle = value;
        }

        public Color ColorHovered { get; set; } = new(255, 255, 255, 255);

        public Color ColorClicked { get; set; } = new(0, 0, 255, 255);

        public Color ColorDefault { get; set; } = new(255, 255, 255, 255);

        public void ResetColors()
        {
            ColorHovered = s_defaultColorHovered;
            ColorClicked = s_defaultColorClicked;
            ColorDefault = s_defaultColor;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Texture != null)
            {
                AsyncTexture2D texture = _clicked && ClickedTexture != null ? ClickedTexture : MouseOver && HoveredTexture != null ? HoveredTexture : Texture;
                _clicked = _clicked && MouseOver;

                spriteBatch.DrawOnCtrl(
                    this,
                    texture,
                    SizeRectangle != Rectangle.Empty ? SizeRectangle : bounds,
                    _textureRectangle == Rectangle.Empty ? texture.Bounds : _textureRectangle,
                    MouseOver ? ColorHovered : MouseOver && _clicked ? ColorClicked : ColorDefault,
                    0f,
                    default);
            }
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            base.OnLeftMouseButtonPressed(e);
            _clicked = true;
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
        {
            base.OnLeftMouseButtonReleased(e);
            _clicked = false;
        }
    }
}