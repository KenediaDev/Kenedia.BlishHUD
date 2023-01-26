using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.Characters.Controls
{
    public class ImageToggleEx : Control
    {
        private readonly AsyncTexture2D _exTexture = AsyncTexture2D.FromAssetId(784262);

        private bool _clicked;
        private readonly Action<bool> _onChanged;

        private Rectangle _xTextureRectangle;
        private Rectangle _xDrawRectangle;
        private bool _checked;

        public ImageToggleEx()
        {

        }

        public ImageToggleEx(Action<bool> onChanged)
            : this()
        {
            _onChanged = onChanged;
        }

        public event EventHandler<CheckChangedEvent> CheckedChanged;

        public AsyncTexture2D Texture { get; set; }

        public AsyncTexture2D HoveredTexture { get; set; }

        public AsyncTexture2D ActiveTexture { get; set; }

        public AsyncTexture2D ClickedTexture { get; set; }

        public Rectangle TextureRectangle { get; set; }

        public Rectangle SizeRectangle { get; set; }

        public bool ShowX { get; set; }

        public bool Checked 
        { 
            get => _checked; 
            set 
            {
                _checked = value;
                OnCheckedChanged();
            } 
        }

        private void OnCheckedChanged()
        {
            CheckedChanged?.Invoke(this, new(_checked));
        }

        private AsyncTexture2D GetTexture()
        {
            return _clicked && ClickedTexture != null ? ClickedTexture
                : Checked && ActiveTexture != null ? ActiveTexture
                : MouseOver && HoveredTexture != null ? HoveredTexture
                : Texture;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            AsyncTexture2D texture = GetTexture();

            if (texture != null)
            {
                _clicked = _clicked && MouseOver;

                spriteBatch.DrawOnCtrl(
                    this,
                    texture,
                    SizeRectangle != Rectangle.Empty ? SizeRectangle : bounds,
                    TextureRectangle == Rectangle.Empty ? texture.Bounds : TextureRectangle,
                    Color.White,
                    0f,
                    default);
            }

            if (ShowX && !Checked)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    _exTexture,
                    _xDrawRectangle,
                    _xTextureRectangle,
                    Color.White);
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int size = Math.Min(Width / 2, Height / 2);
            _xDrawRectangle = new Rectangle(Width - size, Height - size, size, size);
            _xTextureRectangle = new Rectangle(4, 4, 28, 28);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            Checked = !Checked;
            _onChanged?.Invoke(Checked);
            CheckedChanged?.Invoke(this, new(Checked));
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
