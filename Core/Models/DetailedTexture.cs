using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Kenedia.Modules.Core.Models
{
    public class DetailedTexture : IDisposable
    {
        private bool _isDisposed;

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

        public AsyncTexture2D Texture { get; set => Common.SetProperty(field, value, v => field = v,  () => ApplyBounds(), value is not null); }

        public AsyncTexture2D HoveredTexture { get; set => Common.SetProperty(field, value, v => field = v, () => ApplyBounds(), value is not null); }

        public AsyncTexture2D FallBackTexture { get; set => Common.SetProperty(field, value, v => field = v, () => ApplyBounds(), value is not null); }

        public Rectangle TextureRegion { get; set; }

        public Rectangle FallbackRegion { get; set; }

        public Rectangle Bounds { get; set; }

        public Point Size
        {
            get => Bounds.Size; set => Bounds = new(Bounds.Location, value);
        }

        public Rectangle FallbackBounds { get; set; }

        public Color? DrawColor { get; set; }

        public Color? HoverDrawColor { get; set; }

        public virtual void Draw(Control ctrl, SpriteBatch spriteBatch, Point? mousePos = null, Color? color = null, Color? bgColor = null, bool? forceHover = null, float? rotation = null, Vector2? origin = null)
        {
            if (_isDisposed) return;
            if (FallBackTexture is not null || Texture is not null)
            {
                origin ??= Vector2.Zero;
                rotation ??= 0F;

                Hovered = mousePos is not null && Bounds.Contains((Point)mousePos);
                color ??= ((forceHover == true || Hovered) && HoverDrawColor is not null ? HoverDrawColor : DrawColor) ?? Color.White;

                if (Texture is not null)
                {
                    spriteBatch.DrawOnCtrl(
                        ctrl,
                        (forceHover == true || Hovered) && HoveredTexture is not null ? HoveredTexture : Texture ?? FallBackTexture,
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

                if (bgColor is not null)
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
            if (_isDisposed) return;
            if (FallBackTexture is not null || Texture is not null)
            {
                effect ??= SpriteEffects.FlipHorizontally;
                origin ??= Vector2.Zero;
                rotation ??= 0F;

                Hovered = mousePos is not null && Bounds.Contains((Point)mousePos);
                color ??= (Hovered && HoverDrawColor is not null ? HoverDrawColor : DrawColor) ?? Color.White;

                if (Texture is not null)
                {
                    spriteBatch.DrawOnCtrl(
                        ctrl,
                        (forceHover == true || Hovered) && HoveredTexture is not null ? HoveredTexture : Texture ?? FallBackTexture,
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

                if (bgColor is not null)
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

        public virtual void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;
            Texture = null;
            HoveredTexture = null;
            FallBackTexture = null;
        }
    }
}
