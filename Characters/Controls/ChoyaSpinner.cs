using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Kenedia.Modules.Characters.Controls
{
    public class ChoyaSpinner : Control
    {
        private readonly TextureManager _textureManager;
        private readonly AsyncTexture2D _choyaTexture;
        private readonly int _choyaSize;
        private double _start;
        private double _lastTick;
        private int _xOffset;
        private float _rotation;

        public ChoyaSpinner(TextureManager textureManager)
        {
            _textureManager = textureManager;

            _choyaTexture = _textureManager.GetControlTexture(TextureManager.ControlTextures.Choya);
            _choyaSize = Math.Min(_choyaTexture.Bounds.Width, _choyaTexture.Bounds.Height);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            double now = GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds;
            double duration = now - _start;
            _rotation = (float)(duration / 0.75 / 360);

            if (now - _lastTick > 18)
            {
                _lastTick = now;

                _xOffset += 5;

                if (_xOffset >= Width + (_choyaSize / 4))
                {
                    _xOffset = -_choyaSize / 5;
                }
            }

            int size = Math.Min(Width, Height);

            if (_choyaTexture is not null) spriteBatch.DrawOnCtrl(this, _choyaTexture, new(new(_xOffset, Height / 2), new(size)), _choyaTexture.Bounds, Color.White, _rotation, new(_choyaSize / 2));
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _start = GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds;
        }
    }
}
