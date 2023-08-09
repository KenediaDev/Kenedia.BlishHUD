using Blish_HUD;
using Blish_HUD.Content;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage
{
    public class AttributeTexture : DetailedTexture
    {
        public AttributeTexture()
        {
        }

        public AttributeTexture(AsyncTexture2D texture) : base(texture)
        {

        }

        public AttributeTexture(int assetid) : base(assetid)
        {

        }

        public AttributeTexture(int assetid, int hoveredAssetId) : base(assetid, hoveredAssetId)
        {

        }

        public Rectangle TextRegion { get; set; }

        public void DrawAmount(Blish_HUD.Controls.Control ctrl, SpriteBatch spriteBatch, double amount, BitmapFont font, Point? mousePos = null, Color? color = null, Color? bgColor = null, bool? forceHover = null, float? rotation = null, Vector2? origin = null)
        {
            color ??= Color.White;
            spriteBatch.DrawStringOnCtrl(ctrl, $"{amount.ToString("N0")}", font, TextRegion, (Color)color, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Middle);
        }

        public void DrawAmount(Blish_HUD.Controls.Control ctrl, SpriteBatch spriteBatch, string amount, BitmapFont font, Point? mousePos = null, Color? color = null, Color? bgColor = null, bool? forceHover = null, float? rotation = null, Vector2? origin = null)
        {
            color ??= Color.White;
            spriteBatch.DrawStringOnCtrl(ctrl, amount, font, TextRegion, (Color)color, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Middle);
        }
    }
}
