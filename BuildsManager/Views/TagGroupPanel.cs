using Microsoft.Xna.Framework;
using Kenedia.Modules.BuildsManager.Models;
using Microsoft.Xna.Framework.Graphics;
using Blish_HUD;
using static Blish_HUD.ContentService;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class TagGroupPanel : Blish_HUD.Controls.FlowPanel
    {
        public TagGroupPanel(TagGroup tagGroup)
        {
            TagGroup = tagGroup;
            FlowDirection = Blish_HUD.Controls.ControlFlowDirection.LeftToRight;
            WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill;
            OuterControlPadding = new(0, 30);
        }

        public TagGroup TagGroup { get; }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            var font = Content.DefaultFont14;
            spriteBatch.DrawStringOnCtrl(this, TagGroup?.Name, font, new Rectangle(0, 0, bounds.Width, font.LineHeight), StandardColors.Default);

            spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(0, font.LineHeight + 2, bounds.Width, 2), StandardColors.Default * 0.5F);
        }
    }
}
