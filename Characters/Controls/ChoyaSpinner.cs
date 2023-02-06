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
        private double _start;
        private int xOffset;

        public ChoyaSpinner(TextureManager textureManager)
        {
            _textureManager = textureManager;

            _choyaTexture = _textureManager.GetControlTexture(TextureManager.ControlTextures.Choya);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            double ms = GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds;
            double duration = ms - _start;

            float rotation =(float) (duration / 0.75 / 360);

            int size = Math.Min(Width, Height);
            int choyaSize = Math.Min(_choyaTexture.Bounds.Width, _choyaTexture.Bounds.Height);
            xOffset += 4;
            
            if(xOffset >= Width + (choyaSize / 4))
            {
                xOffset = -choyaSize / 5;
            }

            if (_choyaTexture != null) spriteBatch.DrawOnCtrl(this, _choyaTexture, new(new(xOffset, Height / 2), new(size)), _choyaTexture.Bounds, Color.White, rotation, new(choyaSize / 2));
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _start = GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds;
        }
    }
}
