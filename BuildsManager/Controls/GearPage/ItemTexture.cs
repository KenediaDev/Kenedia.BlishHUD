using Blish_HUD;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Blish_HUD.Controls;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage
{
    public class ItemTexture : DetailedTexture
    {
        private BaseItem _item;
        private Color _frameColor;

        public BaseItem Item { get => _item; set => Common.SetProperty(ref _item, value, ApplyItem); }

        private void ApplyItem()
        {
            _frameColor = Item != null ? (Color)Item?.Rarity.GetColor() : Color.White * 0.5F;
            Texture = Item?.Icon;
        }

        public void Draw(Control ctrl, SpriteBatch spriteBatch, Point? mousePos = null, Color? color = null)
        {
            if (FallBackTexture != null || Texture != null)
            {
                Hovered = mousePos != null && Bounds.Contains((Point)mousePos);
                color ??= (Hovered && HoverDrawColor != null ? HoverDrawColor : DrawColor) ?? Color.White;

                if (Texture != null)
                {
                    spriteBatch.DrawOnCtrl(
                        ctrl,
                        Texture,
                        Bounds.Add(2, 2, -4, -4),
                        TextureRegion,
                        (Color)color,
                        0F,
                        Vector2.Zero);
                }

                spriteBatch.DrawFrame(ctrl, Bounds, _frameColor, 2);
            }
        }
    }
}
