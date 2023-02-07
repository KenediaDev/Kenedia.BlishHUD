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
        private readonly TexturesService _textureService;
        private AsyncTexture2D _choyaTexture;
        private double _start;
        private int _xOffset;

        public RollingChoya(TexturesService textureManager)
        {
            _textureService = textureManager;
            _choyaTexture = _textureService.GetTexture(textures_common.RollingChoya, nameof(textures_common.RollingChoya));            
        }

        public event EventHandler ChoyaLeftBounds;
        
        public int Steps { get; set; } = 360;

        public int TravelDistance { get; set; } = 4;

        public bool CaptureInput { get; set; } = true;

        public Color TextureColor { get; set; } = Color.White;

        public AsyncTexture2D ChoyaTexture
        {
            get => _choyaTexture;
            set => _choyaTexture = value; 
        }

        protected override CaptureType CapturesInput()
        {
            if (CaptureInput)
            {
                return base.CapturesInput();
            }

            return CaptureType.None;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            double ms = GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds;
            double duration = ms - _start;
            float rotation =(float) (duration / Steps);

            int size = Math.Min(Width, Height);
            int choyaSize = Math.Min(_choyaTexture.Bounds.Width, _choyaTexture.Bounds.Height);
            _xOffset += TravelDistance;

            if (_xOffset >= Width + (choyaSize / 4))
            {                
                _xOffset = -choyaSize / 5;
            }

            var choyaRect = new Rectangle(new(_xOffset, Height / 2), new(size));
            if (_choyaTexture != null) spriteBatch.DrawOnCtrl(this, _choyaTexture, choyaRect, _choyaTexture.Bounds, TextureColor, rotation, new(choyaSize / 2));

            if (!bounds.Contains(choyaRect.Location))
            {
                ChoyaLeftBounds?.Invoke(this, null);
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
