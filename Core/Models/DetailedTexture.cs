using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Kenedia.Modules.Core.Models
{
    public class DetailedTexture
    {
        private AsyncTexture2D _texture;
        private AsyncTexture2D _hoveredTexture;
        private AsyncTexture2D _fallbackTexture;

        public DetailedTexture()
        {
        }

        public DetailedTexture(int assetId)
        {
            Texture = AsyncTexture2D.FromAssetId(assetId);
        }

        public DetailedTexture(int assetId, int hoveredAssetId)
        {
            Texture = AsyncTexture2D.FromAssetId(assetId);
            HoveredTexture = AsyncTexture2D.FromAssetId(hoveredAssetId);
        }

        public DetailedTexture(AsyncTexture2D texture)
        {
            Texture = texture;
        }

        public bool Hovered { get; private set; }

        public AsyncTexture2D Texture { get => _texture; set => Common.SetProperty(ref _texture, value, () => ApplyBounds(), value != null); }

        public AsyncTexture2D HoveredTexture { get => _hoveredTexture; set => Common.SetProperty(ref _hoveredTexture, value, () => ApplyBounds(), value != null); }

        public AsyncTexture2D FallBackTexture { get => _fallbackTexture; set => Common.SetProperty(ref _fallbackTexture, value, () => ApplyBounds(), value != null); }

        public Rectangle TextureRegion { get; set; }

        public Rectangle Bounds { get; set; }

        public virtual void Draw(Control ctrl, SpriteBatch spriteBatch, Point? mousePos = null, Color? color = null, Color? bgColor = null, bool? forceHover = null, float rotation = 0f, Vector2? origin = null)
        {
            if (FallBackTexture != null || Texture != null)
            {
                origin ??= Vector2.Zero;
                color ??= Color.White;
                mousePos ??= Point.Zero;
                Hovered = forceHover == true || (forceHover == null && Bounds.Contains((Point)mousePos));

                spriteBatch.DrawOnCtrl(
                    ctrl,
                    Hovered && HoveredTexture != null ? HoveredTexture : Texture ?? FallBackTexture,
                    Bounds,
                    TextureRegion,
                    (Color)color,
                    rotation,
                    (Vector2)origin);

                if (bgColor != null)
                {
                    spriteBatch.DrawOnCtrl(
                    ctrl,
                    ContentService.Textures.Pixel,
                    Bounds,
                    Rectangle.Empty,
                    (Color)bgColor,
                    rotation,
                    (Vector2)origin);
                }
            }
        }

        private void ApplyBounds(bool force = false)
        {
            if (TextureRegion == Rectangle.Empty || force) TextureRegion = (Texture ?? FallBackTexture).Bounds;
            if (Bounds == Rectangle.Empty || force) Bounds = (Texture ?? FallBackTexture).Bounds;
        }
    }
}
