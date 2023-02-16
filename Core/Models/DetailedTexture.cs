using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

        public bool Hovered { get; protected set; }

        public AsyncTexture2D Texture { get => _texture; set => Common.SetProperty(ref _texture, value, () => ApplyBounds(), value != null); }

        public AsyncTexture2D HoveredTexture { get => _hoveredTexture; set => Common.SetProperty(ref _hoveredTexture, value, () => ApplyBounds(), value != null); }

        public AsyncTexture2D FallBackTexture { get => _fallbackTexture; set => Common.SetProperty(ref _fallbackTexture, value, () => ApplyBounds(), value != null); }

        public Rectangle TextureRegion { get; set; }

        public Rectangle FallbackRegion { get; set; }

        public Rectangle Bounds { get; set; }

        public Rectangle FallbackBounds { get; set; }

        public virtual void Draw(Control ctrl, SpriteBatch spriteBatch, Point? mousePos = null, Color? color = null, Color? bgColor = null, bool? forceHover = null, float? rotation = null, Vector2? origin = null)
        {
            if (FallBackTexture != null || Texture != null)
            {
                origin ??= Vector2.Zero;
                color ??= Color.White;
                rotation ??= 0F;

                Hovered = forceHover == true || (forceHover == null && mousePos != null && Bounds.Contains((Point)mousePos));

                if (Texture != null)
                {
                    spriteBatch.DrawOnCtrl(
                        ctrl,
                        Hovered && HoveredTexture != null ? HoveredTexture : Texture ?? FallBackTexture,
                        Bounds,
                        TextureRegion,
                        (Color)color,
                        (float)rotation,
                        (Vector2)origin);
                }
                else
                {
                    spriteBatch.DrawOnCtrl(
                        ctrl,
                        FallBackTexture,
                        FallbackBounds == Rectangle.Empty ? Bounds : FallbackBounds,
                        FallbackRegion,
                        (Color)color,
                        (float)rotation,
                        (Vector2)origin);
                }

                if (bgColor != null)
                {
                    spriteBatch.DrawOnCtrl(
                    ctrl,
                    ContentService.Textures.Pixel,
                    Bounds,
                    Rectangle.Empty,
                    (Color)bgColor,
                    (float)rotation,
                    (Vector2)origin);
                }
            }
        }

        public virtual void Draw(Control ctrl, SpriteBatch spriteBatch, SpriteEffects? effect, Point? mousePos = null, Color? color = null, Color? bgColor = null, bool? forceHover = null, float? rotation = null, Vector2? origin = null)
        {
            if (FallBackTexture != null || Texture != null)
            {
                effect ??= SpriteEffects.FlipHorizontally;
                origin ??= Vector2.Zero;
                color ??= Color.White;
                rotation ??= 0F;

                Hovered = forceHover == true || (forceHover == null && mousePos != null && Bounds.Contains((Point)mousePos));

                if (Texture != null)
                {
                    spriteBatch.DrawOnCtrl(
                        ctrl,
                        Hovered && HoveredTexture != null ? HoveredTexture : Texture ?? FallBackTexture,
                        Bounds,
                        TextureRegion,
                        (Color)color,
                        (float)rotation,
                        (Vector2)origin,
                        (SpriteEffects)effect);
                }
                else
                {
                    spriteBatch.DrawOnCtrl(
                        ctrl,
                        FallBackTexture,
                        FallbackBounds == Rectangle.Empty ? Bounds : FallbackBounds,
                        FallbackRegion,
                        (Color)color,
                        (float)rotation,
                        (Vector2)origin,
                        (SpriteEffects)effect);
                }

                if (bgColor != null)
                {
                    spriteBatch.DrawOnCtrl(
                    ctrl,
                    ContentService.Textures.Pixel,
                    Bounds,
                    Rectangle.Empty,
                    (Color)bgColor,
                    (float)rotation,
                    (Vector2)origin,
                    (SpriteEffects)effect);
                }
            }
        }

        private void ApplyBounds(bool force = false)
        {
            if (TextureRegion == Rectangle.Empty || force) TextureRegion = (Texture ?? FallBackTexture).Bounds;
            if (FallbackRegion == Rectangle.Empty || force) FallbackRegion = (Texture ?? FallBackTexture).Bounds;
            if (Bounds == Rectangle.Empty || force) Bounds = (Texture ?? FallBackTexture).Bounds;
        }
    }
}
