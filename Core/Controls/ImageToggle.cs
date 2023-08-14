using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using ICheckable = Blish_HUD.Controls.ICheckable;

namespace Kenedia.Modules.Core.Controls
{
    public class ImageToggle : Control, ICheckable
    {
        private readonly AsyncTexture2D _exTexture = AsyncTexture2D.FromAssetId(784262);

        private bool _clicked;
        private Action<bool> _onCheckChanged;

        private Rectangle _xTextureRectangle;
        private Rectangle _xDrawRectangle;
        private bool _checked;
        private Func<string> _setLocalizedTooltip;

        public ImageToggle()
        {

        }

        public ImageToggle(Action<bool> onChanged)
            : this()
        {
            OnCheckChanged = onChanged;
        }

        public event EventHandler<CheckChangedEvent> CheckedChanged;

        public Func<string> SetLocalizedTooltip
        {
            get => _setLocalizedTooltip;
            set
            {
                _setLocalizedTooltip = value;
                BasicTooltipText = value?.Invoke();
            }
        }

        public AsyncTexture2D Texture { get; set; }

        public AsyncTexture2D HoveredTexture { get; set; }

        public AsyncTexture2D ActiveTexture { get; set; }

        public AsyncTexture2D ClickedTexture { get; set; }

        public Rectangle TextureRectangle { get; set; }

        public Rectangle SizeRectangle { get; set; }

        public Color ImageColor { get; set; } = Color.White;

        public Color? ActiveColor { get; set; } = Color.White;

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

        public Action<bool> OnCheckChanged
        {
            get => _onCheckChanged;
            set => Common.SetProperty(ref _onCheckChanged, value);
        }

        private void OnCheckedChanged()
        {
            CheckedChanged?.Invoke(this, new(_checked));
        }

        private AsyncTexture2D GetTexture()
        {
            return _clicked && ClickedTexture is not null ? ClickedTexture
                : Checked && ActiveTexture is not null ? ActiveTexture
                : MouseOver && HoveredTexture is not null ? HoveredTexture
                : Texture;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            AsyncTexture2D texture = GetTexture();

            if (texture is not null)
            {
                _clicked = _clicked && MouseOver;

                spriteBatch.DrawOnCtrl(
                    this,
                    texture,
                    SizeRectangle != Rectangle.Empty ? SizeRectangle : bounds,
                    TextureRectangle == Rectangle.Empty ? texture.Bounds : TextureRectangle,
                    Checked ? (ActiveColor ?? ImageColor) : ImageColor,
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
            OnCheckChanged?.Invoke(Checked);
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
