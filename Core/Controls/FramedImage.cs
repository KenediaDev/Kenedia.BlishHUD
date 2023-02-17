using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.TextureAtlases;
using SharpDX.MediaFoundation;
using System;
using System.Diagnostics;

namespace Kenedia.Modules.Core.Controls
{
    public class FramedImage : Control
    {
        private Rectangle _textureBounds;
        private Rectangle _iconFrameBounds;
        private Rectangle _rightIconFrameBounds;

        public FramedImage()
        {
            BackgroundColor = Color.Black * 0.1F;
        }

        public FramedImage(int assetId) : this()
        {
            Texture = AsyncTexture2D.FromAssetId(assetId);
            Size = Texture.Bounds.Size;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();   
            int xOffset = (int)(Width * 0.15);
            int yOffset = (int)(Height * 0.15);

            _textureBounds = new(xOffset / 2, yOffset /2, Width - (xOffset * 1), Height - (yOffset * 1));

            _iconFrameBounds = new(0, yOffset, Width - xOffset, Height - yOffset);
            _rightIconFrameBounds = new(xOffset, 0, Width - xOffset, Height - yOffset);
        }

        public AsyncTexture2D Texture { get; set; }

        public Rectangle TextureRegion { get; set; }

        public AsyncTexture2D IconFrame { get; set; } = AsyncTexture2D.FromAssetId(1414041);

        public Rectangle FrameTextureRegion { get; set; }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this, IconFrame, _iconFrameBounds, FrameTextureRegion == Rectangle.Empty ? IconFrame.Bounds : FrameTextureRegion, Color.White);
            spriteBatch.DrawOnCtrl(this, IconFrame, _rightIconFrameBounds, FrameTextureRegion  == Rectangle.Empty ? IconFrame.Bounds : FrameTextureRegion, Color.White, 0F, Vector2.Zero, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically);
            if (Texture != null) spriteBatch.DrawOnCtrl(this, Texture, _textureBounds, TextureRegion == Rectangle.Empty ? Texture.Bounds : TextureRegion, Color.White);
        }
    }
}
