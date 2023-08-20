using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Kenedia.Modules.Core.Models;
using Blish_HUD;
using Kenedia.Modules.BuildsManager.Models.Templates;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public class LegendSelector : Selector<Legend>
    {
        private readonly DetailedTexture _selectingFrame = new(157147);

        public LegendSelector()
        {
            ContentPanel.BorderWidth = new(2, 0, 2, 2);
            ContentPanel.ContentPadding = new(10);
            SelectableSize = new(48);
        }

        public LegendSlotType LegendSlot { get; set; } = LegendSlotType.TerrestrialActive;

        protected override void OnDataApplied(Legend item)
        {
            base.OnDataApplied(item);

            Controls.ForEach(c => c.IsSelected = c.Data == SelectedItem);
        }
        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            ContentPanel.BorderWidth = new(2);
            HeaderPanel.BorderWidth = new(0, 0, 0, 2);

            spriteBatch.DrawCenteredRotationOnCtrl(this, _selectingFrame.Texture, _selectingFrame.Bounds, _selectingFrame.TextureRegion, Color.White, 0.0F, true, true);
            //_selectingFrame.Draw(this, spriteBatch,null, Color.Red);
        }

        protected override void Recalculate(object sender, Core.Models.ValueChangedEventArgs<Point> e)
        {
            base.Recalculate(sender, e);

        }

        protected override Selectable<Legend> CreateSelectable(Legend item)
        {
            Type = SelectableType.Legend;
            Visible = true;

            return new LegendSelectable()
            {
                Parent = FlowPanel,
                Size = SelectableSize,
                Data = item,
                OnClickAction = OnClickAction,
                IsSelected = PassSelected && item.Equals(SelectedItem),
                LegendSlot = LegendSlot,
            };
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (ContentPanel is not null && HeaderPanel is not null)
            {
                ContentPanel.ContentPadding = new(8);
                int p = 10;
                Rectangle r = new(Point.Zero, new(ContentPanel.Width, SelectableSize.Y + 10));
                HeaderPanel?.SetBounds(r);

                int pad = 20;
                _selectingFrame.Bounds = new(-pad - 1, 0, HeaderPanel.Width + (pad * 2) + 4, HeaderPanel.Height + 2);
            }
        }

        protected override void SetCapture()
        {
            if (HeaderPanel is not null)
            {
                int selectorWidth = (int)(54 / (double)256 * HeaderPanel.Width);
                int pad = 5;
                BlockInputRegion = new Rectangle(
                    HeaderPanel.Location.Add(new((HeaderPanel.Width - selectorWidth) / 2, 0)),
                    new(selectorWidth, HeaderPanel.Height)).Add(new Rectangle(-pad - 2, -pad, pad * 2 + 6, pad * 2));

                CaptureInput = !HeaderPanel.MouseOver || BlockInputRegion.Contains(RelativeMousePosition);
                HeaderPanel.CaptureInput = !HeaderPanel.MouseOver || BlockInputRegion.Contains(RelativeMousePosition);
            }
        }
    }
}
