using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework.Graphics;
using System;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class ImageToggleButton : Control
    {
        private readonly Action<bool> _onChanged;
        private bool _clicked;

        public ImageToggleButton()
        {

        }

        public ImageToggleButton(Action<bool> onChanged)
            : this()
        {
            _onChanged = onChanged;
        }

        public bool Active { get; set; }

        public Color ColorHovered { get; set; } = new(255, 255, 255, 255);

        public Color ColorClicked { get; set; } = new(0, 0, 255, 255);

        public Color ColorDefault { get; set; } = new(255, 255, 255, 255);

        public Color ColorActive{ get; set; } = new(255, 255, 255, 255);

        public AsyncTexture2D Texture { get; set; }

        public AsyncTexture2D HoveredTexture { get; set; }

        public AsyncTexture2D ActiveTexture { get; set; }

        public AsyncTexture2D ClickedTexture { get; set; }

        public Rectangle SizeRectangle { get; set; }

        public Rectangle TextureRectangle { get; set; }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            Active = !Active;
            _onChanged?.Invoke(Active);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Texture != null)
            {
                AsyncTexture2D texture = _clicked && ClickedTexture != null ? ClickedTexture : Active && ActiveTexture != null ? ActiveTexture  : MouseOver && HoveredTexture != null ? HoveredTexture : Texture;
                _clicked = _clicked && MouseOver;

                spriteBatch.DrawOnCtrl(
                    this,
                    texture,
                    SizeRectangle != Rectangle.Empty ? SizeRectangle : bounds,
                    TextureRectangle == Rectangle.Empty ? texture.Bounds : TextureRectangle,
                    Active ? ColorActive : MouseOver ? ColorHovered : MouseOver && _clicked ? ColorClicked : ColorDefault,
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