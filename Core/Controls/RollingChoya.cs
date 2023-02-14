using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Kenedia.Modules.Core.Controls
{
    public class RollingChoya : Control
    {
        private double _start;
        private float _xOffset;
        private float _yOffset;
        private bool _canMove = true;
        private Rectangle _movementBounds = Rectangle.Empty;
        private Point _startPoint = Point.Zero;
        private bool _choyaTargeted;

        public RollingChoya()
        {

        }

        public RollingChoya(InputDetectionService inputDetectionService)
        {
            InputDetectionService = inputDetectionService;
            inputDetectionService.MouseClicked += InputDetectionService_MouseClicked;
        }

        private void InputDetectionService_MouseClicked(object sender, double e)
        {
            if(ChoyaHunt && _choyaTargeted)
            {
                OnClick(null);
            }
        }

        private int ChoyaSize => Math.Min(Width, Height);

        public event EventHandler ChoyaLeftBounds;

        public int Steps { get; set; } = 360;

        public Vector2 TravelDistance { get; set; } = new(4, 4);

        public bool ChoyaHunt { get; set; } = false;

        public bool CaptureInput { get; set; } = true;

        public Color TextureColor { get; set; } = Color.White;

        public Point StartPoint
        {
            get => _startPoint;
            set
            {
                if (_startPoint != value)
                {
                    _startPoint = value;
                    _xOffset = _startPoint.X;
                    _yOffset = _startPoint.Y;
                }
            }
        }

        public bool CanMove
        {
            get => _canMove;
            set
            {
                if (_canMove != value)
                {
                    _canMove = value;
                    ResetPosition();
                }
            }
        }

        public AsyncTexture2D ChoyaTexture { get; set; }

        public Rectangle MovementBounds
        {
            get => _movementBounds;
            set => _movementBounds = value;
        }

        public InputDetectionService InputDetectionService { get; }

        protected override CaptureType CapturesInput()
        {
            return CaptureInput ? base.CapturesInput() : CaptureType.None;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
        }

        public void ResetPosition()
        {
            switch (CanMove)
            {
                case true:
                    _xOffset = ChoyaSize / 2;
                    _yOffset = 0;
                    break;

                case false:
                    int size = Math.Min(Width, Height);
                    Rectangle movementBounds = Parent != null ? Parent.AbsoluteBounds : Rectangle.Empty;
                    Location = new((movementBounds.Width - size) / 2, (movementBounds.Height - size) / 2);
                    break;

            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (ChoyaTexture != null)
            {
                double ms = GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds;
                double duration = ms - _start;
                float rotation = (float)(duration / Steps);

                _choyaTargeted = ChoyaHunt && AbsoluteBounds.Contains(Input.Mouse.Position);

                Rectangle movementBounds = Parent != null ? Parent.ContentRegion : Rectangle.Empty;

                int size = Math.Min(Width, Height);
                int choyaSize = Math.Min(ChoyaTexture.Bounds.Width, ChoyaTexture.Bounds.Height);

                _xOffset += CanMove ? TravelDistance.X : 0;
                _yOffset += CanMove ? TravelDistance.Y : 0;

                var choyaRect = new Rectangle(new(size / 2), new(size));

                if (CanMove)
                {
                    Location = new(movementBounds.X + (int)(CanMove ? _xOffset : 0), movementBounds.Y + (int)(CanMove ? _yOffset : 0));
                }

                Size = new(size);

                if (ChoyaTexture != null) spriteBatch.DrawOnCtrl(this, ChoyaTexture, choyaRect, ChoyaTexture.Bounds, _choyaTargeted ? Color.Red : TextureColor, rotation, new(choyaSize / 2));

                if (movementBounds.Width < Location.X + TravelDistance.X + (choyaSize / 20))
                {
                    ChoyaLeftBounds?.Invoke(this, null);
                    _xOffset = -(int)(choyaSize * 0.7);
                }
                else if (Location.X + TravelDistance.X < -(int)(choyaSize * 0.7))
                {
                    ChoyaLeftBounds?.Invoke(this, null);
                    _xOffset = movementBounds.Width - (int)(choyaSize * 0.05);
                }

                if (movementBounds.Height < Location.Y + TravelDistance.Y + (choyaSize / 20))
                {
                    ChoyaLeftBounds?.Invoke(this, null);
                    _yOffset = -(int)(choyaSize * 0.7);
                }
                else if (Location.Y + TravelDistance.Y < -(int)(choyaSize * 0.7))
                {
                    ChoyaLeftBounds?.Invoke(this, null);
                    _yOffset = movementBounds.Height - (int)(choyaSize * 0.05);
                }
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _start = GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            if(InputDetectionService != null) InputDetectionService.MouseClicked -= InputDetectionService_MouseClicked;
        }
    }
}
