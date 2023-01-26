using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Characters.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class SizeablePanel : Container
    {
        private const int s_resizeHandleSize = 16;

        private readonly AsyncTexture2D _resizeTexture = AsyncTexture2D.FromAssetId(156009);
        private readonly AsyncTexture2D _resizeTextureHovered = AsyncTexture2D.FromAssetId(156010);

        private bool _dragging;
        private bool _resizing;
        private bool _mouseOverResizeHandle;

        private Point _resizeStart;
        private Point _dragStart;
        private Point _draggingStart;

        private Rectangle _resizeHandleBounds = Rectangle.Empty;

        public SizeablePanel()
        {
        }

        public bool ShowResizeOnlyOnMouseOver { get; set; }

        public Point MaxSize { get; set; } = new Point(499, 499);

        public Color TintColor { get; set; } = Color.Black * 0.5f;

        public bool TintOnHover { get; set; }

        private Rectangle ResizeCorner => new(LocalBounds.Right - 15, LocalBounds.Bottom - 15, 15, 15);

        public void ToggleVisibility()
        {
            Visible = !Visible;
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            _dragging = _dragging && MouseOver;
            _resizing = _resizing && MouseOver;
            _mouseOverResizeHandle = _mouseOverResizeHandle && MouseOver;

            if (_dragging)
            {
                Location = Input.Mouse.Position.Add(new Point(-_draggingStart.X, -_draggingStart.Y));
            }

            if (_resizing)
            {
                Point nOffset = Input.Mouse.Position - _dragStart;
                Point newSize = _resizeStart + nOffset;
                Size = new Point(MathHelper.Clamp(newSize.X, 50, MaxSize.X), MathHelper.Clamp(newSize.Y, 25, MaxSize.Y));
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _resizeHandleBounds = new Rectangle(
                Width - _resizeTexture.Width,
                Height - _resizeTexture.Height,
                _resizeTexture.Width,
                _resizeTexture.Height);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if (TintOnHover && MouseOver)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    ContentService.Textures.Pixel,
                    bounds,
                    Rectangle.Empty,
                    TintColor,
                    0f,
                    default);
            }

            if (_resizeTexture != null && (!ShowResizeOnlyOnMouseOver || MouseOver))
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    _resizing || _mouseOverResizeHandle ? _resizeTextureHovered : _resizeTexture,
                    new Rectangle(bounds.Right - _resizeTexture.Width - 1, bounds.Bottom - _resizeTexture.Height - 1, _resizeTexture.Width, _resizeTexture.Height),
                    _resizeTexture.Bounds,
                    Color.White,
                    0f,
                    default);
            }

            // var color = MouseOver ? ContentService.Colors.ColonialWhite : Color.Transparent;
            Color color = ContentService.Colors.ColonialWhite;

            // Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            // Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 2, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 1, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 2, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 1, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
        {
            base.OnLeftMouseButtonReleased(e);
            _dragging = false;
            _resizing = false;
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            base.OnLeftMouseButtonPressed(e);

            _resizing = ResizeCorner.Contains(e.MousePosition);
            _resizeStart = Size;
            _dragStart = Input.Mouse.Position;

            _dragging = !_resizing;
            _draggingStart = _dragging ? RelativeMousePosition : Point.Zero;
        }

        protected virtual Point HandleWindowResize(Point newSize)
        {
            return new Point(
                MathHelper.Clamp(newSize.X, ContentRegion.X, 1024),
                MathHelper.Clamp(newSize.Y, ContentRegion.Y, 1024));
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            ResetMouseRegionStates();

            if (_resizeHandleBounds.Contains(RelativeMousePosition)
                  && RelativeMousePosition.X > _resizeHandleBounds.Right - s_resizeHandleSize
                  && RelativeMousePosition.Y > _resizeHandleBounds.Bottom - s_resizeHandleSize)
            {
                _mouseOverResizeHandle = true;
            }

            base.OnMouseMoved(e);
        }

        private void ResetMouseRegionStates()
        {
            _mouseOverResizeHandle = false;
        }
    }
}
