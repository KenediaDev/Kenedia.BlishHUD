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
using Kenedia.Modules.BuildsManager.TemplateEntries;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.BuildsManager.Services;

namespace Kenedia.Modules.BuildsManager.Controls_Old.GearPage.GearSlots
{
    public class RelicSlot : GearSlot
    {
        private Rectangle _titleBounds;
        private Rectangle _statBounds;

        private string _relicName = strings.Relic;
        private string _relicDescription;

        public RelicSlot(TemplateSlotType gearSlot, Container parent, TemplatePresenter templatePresenter, Controls.Selection.SelectionPanel selectionPanel, Data data)
            : base(gearSlot, parent, templatePresenter, selectionPanel, data)
        {
            ItemControl.Placeholder.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\relic_slot.png");
            ItemControl.Placeholder.TextureRegion = new(38, 38, 52, 52);
            ItemColor = Color.White;
        }

        public RelicSlot PairedSlot { get; set; }

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

        protected override void SetItemToSlotControl(object sender, TemplateSlotChangedEventArgs e)
        {
            base.SetItemToSlotControl(sender, e);

            SetItemFromTemplate();
        }

        protected override void SetItemFromTemplate()
        {
            base.SetItemFromTemplate();

            if (TemplatePresenter?.Template?[Slot] is PveRelicTemplateEntry pveRelic)
            {
                Item = pveRelic.Relic;
                _relicName = pveRelic?.Relic?.Name ?? strings.Relic;
                _relicDescription = pveRelic?.Relic?.Description;
            }
            else if (TemplatePresenter?.Template?[Slot] is PvpRelicTemplateEntry pvpRelic)
            {
                Item = pvpRelic.Relic;

                _relicName = pvpRelic?.Relic?.Name ?? strings.Relic;
                _relicDescription = pvpRelic?.Relic?.Description;
            }
            else
            {
                _relicName = strings.Relic;
                _relicDescription = string.Empty;
            }
        }

        protected override void SetAnchor()
        {

            var a = AbsoluteBounds;

            if (ItemControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<Relic>(ItemControl, new Rectangle(a.Location, Point.Zero).Add(ItemControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Item, (relic) => TemplatePresenter.Template?.SetItem(Slot, TemplateSubSlotType.Item, relic));
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => strings.Reset, () => string.Format(strings.ResetEntry, strings.Relic), () => TemplatePresenter?.Template?.SetItem<Relic>(Slot, TemplateSubSlotType.Item, null));
        }

        protected override void GameModeChanged(object sender, Core.Models.ValueChangedEventArgs<GameModeType> e)
        {
            var a = SelectionPanel?.Anchor;

            if (a is not null && Children?.Contains(a) is true)
            {
                if ((e.NewValue is GameModeType.PvP && Slot is TemplateSlotType.PveRelic) || (e.NewValue is GameModeType.PvE && Slot is TemplateSlotType.PvpRelic))
                {
                    if (PairedSlot is not null)
                    {
                        var b = PairedSlot.AbsoluteBounds;
                        SelectionPanel?.SetAnchor<Relic>(PairedSlot.ItemControl, new Rectangle(b.Location, Point.Zero).Add(PairedSlot.ItemControl.LocalBounds), SelectionTypes.Items, PairedSlot.Slot, GearSubSlotType.Item, (relic) => TemplatePresenter.Template?.SetItem(PairedSlot.Slot, TemplateSubSlotType.Item, relic));
                    }
                }
            }
            else
            {
                base.GameModeChanged(sender, e);
            }
        }
    }
}
