using Container = Blish_HUD.Controls.Container;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Blish_HUD;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Models;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.BuildsManager.TemplateEntries;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.BuildsManager.DataModels.Items;

namespace Kenedia.Modules.BuildsManager.Controls_Old.GearPage.GearSlots
{
    public class EnhancementSlot : GearSlot
    {
        private Rectangle _titleBounds;
        private Rectangle _statBounds;

        public EnhancementSlot(TemplateSlotType gearSlot, Container parent, TemplatePresenter templatePresenter, Controls.Selection.SelectionPanel selectionPanel) : base(gearSlot, parent, templatePresenter, selectionPanel)
        {
            ItemControl.Placeholder.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\utilityslot.png");
            ItemControl.Placeholder.TextureRegion = new(38, 38, 52, 52);
            ItemColor = Color.White;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _titleBounds = new(ItemControl.LocalBounds.Right + 10, ItemControl.LocalBounds.Top + 2, Width - ItemControl.LocalBounds.Left - 20, Content.DefaultFont16.LineHeight);
            _statBounds = new(ItemControl.LocalBounds.Right + 10, _titleBounds.Bottom + 2, Width - ItemControl.LocalBounds.Left - 20, Content.DefaultFont12.LineHeight);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            spriteBatch.DrawStringOnCtrl(this, ItemControl?.Item?.Name ?? strings.Enhancement, Content.DefaultFont16, _titleBounds, ItemControl?.Item?.Rarity.GetColor() ?? Color.White * 0.5F);
            spriteBatch.DrawStringOnCtrl(this, (ItemControl?.Item as DataModels.Items.Enhancement)?.Details.Description ?? ItemControl?.Item?.Description, Content.DefaultFont12, _statBounds, Color.White, false, HorizontalAlignment.Left, VerticalAlignment.Top);
        }

        protected override void SetItem(object sender, TemplateSlotChangedEventArgs e)
        {
            if (TemplatePresenter?.Template?[Slot] is EnhancementTemplateEntry enhancement)
            {
                Item = enhancement.Item;
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var a = AbsoluteBounds;

            if (ItemControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<Enhancement>(ItemControl, new Rectangle(a.Location, Point.Zero).Add(ItemControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Item, (enhancement) => TemplatePresenter.Template?.SetItem(Slot, TemplateSubSlotType.Item, enhancement));
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => strings.Reset, () => string.Format(strings.ResetEntry, strings.Enhancement), () => TemplatePresenter?.Template?.SetItem<Enhancement>(Slot, TemplateSubSlotType.Item, null));
        }
    }
}
