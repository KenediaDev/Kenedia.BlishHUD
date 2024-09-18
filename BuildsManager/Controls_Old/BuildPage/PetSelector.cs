using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Kenedia.Modules.Core.Models;
using Blish_HUD;

namespace Kenedia.Modules.BuildsManager.Controls_Old.BuildPage
{
    public class PetSelector : Selector<Pet>
    {
        private readonly DetailedTexture _selectingFrame = new(157147);
        private readonly DetailedTexture _selectedPet = new() { TextureRegion = new(16, 16, 200, 200) };

        public PetSelector()
        {
            ContentPanel.BorderWidth = new(2, 0, 2, 2);
            SelectableSize = new(64);
            SelectablePerRow = 6;
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

            _selectedPet.Draw(this, spriteBatch, null, Color.White);
            //spriteBatch.DrawFrame(this, _blockInputRegion, Color.Red, 2);
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
                ContentPanel.ContentPadding = new(8);
                int p = 4;
                Rectangle r = new(Point.Zero, new(ContentPanel.Width - (p * 4), SelectableSize.Y + 20));
                HeaderPanel?.SetBounds(r);

                int pad = 40;
                _selectingFrame.Bounds = new(-pad - (pad / 5), 0, HeaderPanel.Width + (pad * 3), HeaderPanel.Height + (pad / 10));

                int selectorWidth = (int)(54 / (double)256 * HeaderPanel.Width);
                pad = 7;
                BlockInputRegion = new Rectangle(
                    HeaderPanel.Location.Add(new((HeaderPanel.Width - selectorWidth) / 2 + pad, pad)),
                    new(selectorWidth, HeaderPanel.Height)).Add(new Rectangle(-pad, -pad, pad * 4, pad * 4));

                //_selectedPet.Bounds = _blockInputRegion.Add(new Rectangle(pad, pad, -pad * 4, -pad * 4));
                pad = 16;
                var pos = BlockInputRegion.Center;
                _selectedPet.Bounds = new(pos.X - 64, pos.Y - 60 - pad, 120, 120);
            }

            if (false && FlowPanel is not null)
            {
                Point p = FlowPanel.Size.Substract(FlowPanel.ContentRegion.Size);
                FlowPanel.Width = p.X + (SelectableSize.X * SelectablePerRow) + ((int)FlowPanel.ControlPadding.X * (SelectablePerRow - 1));
                FlowPanel.Height = p.Y + (SelectableSize.Y * Math.Max(1, (int)Math.Ceiling(Items.Count / (decimal)SelectablePerRow)))  + -(int)FlowPanel.ControlPadding.Y + ((int)FlowPanel.ControlPadding.Y * Math.Max(1, (int)Math.Ceiling(Items.Count / ((decimal)SelectablePerRow ))));

                FlowPanel.RecalculateLayout();
            }
        }

        protected override Blish_HUD.Controls.CaptureType CapturesInput()
        {
            return HeaderPanel.MouseOver ? Blish_HUD.Controls.CaptureType.None : base.CapturesInput();
        }

        protected override void OnDataApplied(Pet item)
        {
            base.OnDataApplied(item);
            _selectedPet.Texture = item?.SelectedIcon;
            Controls.ForEach(c => c.IsSelected = c.Data == SelectedItem);
        }

        protected override Selectable<Pet> CreateSelectable(Pet item)
        {
            Type = SelectableType.Pet;
            Visible = true;

            return new PetSelectable()
            {
                Parent = FlowPanel,
                Size = SelectableSize,
                Data = item,
                OnClickAction = OnClickAction,
                IsSelected = PassSelected && item.Equals(SelectedItem),
            };
        }

        protected override void SetCapture()
        {
            if (HeaderPanel is not null)
            {
                CaptureInput = !HeaderPanel.MouseOver || BlockInputRegion.Contains(RelativeMousePosition);
                HeaderPanel.CaptureInput = !HeaderPanel.MouseOver || BlockInputRegion.Contains(RelativeMousePosition);
            }
        }
    }
}
