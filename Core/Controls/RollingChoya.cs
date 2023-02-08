using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Res;
using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Kenedia.Modules.Core.Controls
{
    public class RollingChoya: Control
    {
        private double _start;
        private int _xOffset;

        public RollingChoya()
        {
        }

        public event EventHandler ChoyaLeftBounds;
        
        public int Steps { get; set; } = 360;

        public int TravelDistance { get; set; } = 4;

        public bool CaptureInput { get; set; } = true;

        public Color TextureColor { get; set; } = Color.White;

        public bool CanMove { get; set; } = true;

        public AsyncTexture2D ChoyaTexture { get; set; }

        protected override CaptureType CapturesInput()
        {
            return CaptureInput ? base.CapturesInput() : CaptureType.None;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (ChoyaTexture != null)
            {
                double ms = GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds;
                double duration = ms - _start;
                float rotation = (float)(duration / Steps);

                int size = Math.Min(Width, Height);
                int choyaSize = Math.Min(ChoyaTexture.Bounds.Width, ChoyaTexture.Bounds.Height);

                _xOffset += CanMove ? TravelDistance : 0;

                if (_xOffset >= Width + ((choyaSize / 2) - (choyaSize * 0.15)))
                {
                    _xOffset = -(int)((choyaSize / 2) - (choyaSize * 0.15));
                }

                var choyaRect = new Rectangle(new(CanMove ? _xOffset : Width / 2, Height / 2), new(size));
                if (ChoyaTexture != null) spriteBatch.DrawOnCtrl(this, ChoyaTexture, choyaRect, ChoyaTexture.Bounds, TextureColor, rotation, new(choyaSize / 2));

                if (!bounds.Contains(choyaRect.Location))
                {
                    ChoyaLeftBounds?.Invoke(this, null);
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
        }
    }
}
