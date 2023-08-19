using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Kenedia.Modules.Core.Models;
using Blish_HUD;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public class SkillSelector : Selector<Skill>
    {
        private readonly DetailedTexture _selectingFrame = new(157147);

        public SkillSelector()
        {
            ContentPanel.BorderWidth = new(2, 0, 2, 2);

        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);
            RecalculateLayout();

            int p = 10;
            Rectangle r = new(Point.Zero.Add(new(-p, -2)), new(Width + (p * 2), SelectableSize.Y + 8));
            ContentPanel.BorderWidth = new(2);
            HeaderPanel.BorderWidth = new(0, 0, 0, 2);
            HeaderPanel.BorderColor = Color.Transparent;

            spriteBatch.DrawCenteredRotationOnCtrl(this, _selectingFrame.Texture, r, _selectingFrame.TextureRegion, Color.White, 0.0F, true, true);

            r = new(new(0, 0), new(ContentPanel.Width, SelectableSize.Y));
            HeaderPanel?.SetBounds(r);
        }

        protected override void Recalculate(object sender, Core.Models.ValueChangedEventArgs<Point> e)
        {
            base.Recalculate(sender, e);

        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (ContentPanel is not null && HeaderPanel is not null)
            {
                int p = 24;
                Rectangle r = new(Point.Zero.Substract(new(p, _selectingFrame.Size.Y + 3)), new(Width + (p * 2), SelectableSize.Y + 6));
                r = new(Point.Zero, new(ContentPanel.Width, SelectableSize.Y));
                HeaderPanel?.SetBounds(r);
            }
        }

        protected override Blish_HUD.Controls.CaptureType CapturesInput()
        {
            return HeaderPanel.MouseOver ? Blish_HUD.Controls.CaptureType.None : base.CapturesInput();
        }
    }
}
