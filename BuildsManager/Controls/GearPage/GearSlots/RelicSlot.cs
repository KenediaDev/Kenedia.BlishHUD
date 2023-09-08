using Container = Blish_HUD.Controls.Container;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Blish_HUD;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.TemplateEntries;
using static Kenedia.Modules.BuildsManager.Controls.Selection.SelectionPanel;
using Kenedia.Modules.BuildsManager.Res;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage.GearSlots
{
    public class RelicSlot : GearSlot
    {
        private Rectangle _titleBounds;
        private Rectangle _statBounds;

        private string _relicName;
        private string _relicDescription;

        public RelicSlot(TemplateSlotType gearSlot, Container parent, TemplatePresenter templatePresenter) : base(gearSlot, parent, templatePresenter)
        {
            ItemControl.Placeholder.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\relic_slot.png");
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

            spriteBatch.DrawStringOnCtrl(this, _relicName, Content.DefaultFont16, _titleBounds, ItemControl?.Item?.Rarity.GetColor() ?? Color.White * 0.5F);
            spriteBatch.DrawStringOnCtrl(this, _relicDescription, Content.DefaultFont12, _statBounds, Color.White, false, HorizontalAlignment.Left, VerticalAlignment.Top);
        }

        protected override void SetItems(object sender, EventArgs e)
        {
            base.SetItems(sender, e);

            var relic = TemplatePresenter?.Template?[Slot] as RelicTemplateEntry;
            Item = relic?.Relic;

            _relicName = relic?.Relic?.Name ?? strings.Relic;
            _relicDescription = relic?.Relic?.Description.InterpretItemDescription();
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var a = AbsoluteBounds;

            if (ItemControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<Relic>(ItemControl, new Rectangle(a.Location, Point.Zero).Add(ItemControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Item, (relic) =>
                {
                    Item = relic;
                });
            }
        }

        protected override void OnItemChanged(object sender, Core.Models.ValueChangedEventArgs<BaseItem> e)
        {
            base.OnItemChanged(sender, e);

            if (TemplatePresenter?.Template[Slot] is RelicTemplateEntry entry)
                entry.Relic = Item as Relic;
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => strings.Reset, () => string.Format(strings.ResetEntry, strings.Relic), () =>
            {
                Item = null;
            });
        }
    }
}
