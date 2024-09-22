using Container = Blish_HUD.Controls.Container;
using System;
using Microsoft.Xna.Framework;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Utility;
using Blish_HUD;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.BuildsManager.TemplateEntries;
using Kenedia.Modules.BuildsManager.Res;

namespace Kenedia.Modules.BuildsManager.Controls_Old.GearPage.GearSlots
{
    public class AmuletSlot : GearSlot
    {
        private readonly ItemControl _enrichmentControl = new(new() { TextureRegion = new(38, 38, 52, 52) });

        private Stat _stat;
        private Enrichment _enrichment;

        public AmuletSlot(TemplateSlotType gearSlot, Container parent, TemplatePresenter templatePresenter, Controls.Selection.SelectionPanel selectionPanel) : base(gearSlot, parent, templatePresenter, selectionPanel)
        {
            _enrichmentControl.Placeholder.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
            ItemControl.Item = BuildsManager.Data.Trinkets[92991];

            _enrichmentControl.Parent = this;
        }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        public Enrichment Enrichment { get => _enrichment; set => Common.SetProperty(ref _enrichment, value, OnEnrichmentChanged); }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int infusionSize = (ItemControl.LocalBounds.Size.Y - 4) / 3;
            _enrichmentControl.SetBounds(new(ItemControl.LocalBounds.Right + 1, ItemControl.LocalBounds.Top, infusionSize, infusionSize));
        }

        protected override void SetItems(object sender, EventArgs e)
        {
            base.SetItems(sender, e);

            var amulet = TemplatePresenter?.Template?[Slot] as AmuletTemplateEntry;

            Enrichment = amulet?.Enrichment;
            Stat = amulet?.Stat;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var a = AbsoluteBounds;

            if (ItemControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<Stat>(ItemControl, new Rectangle(a.Location, Point.Zero).Add(ItemControl.LocalBounds), SelectionTypes.Stats, Slot, GearSubSlotType.None, (stat) =>
                Stat = stat,
                (TemplatePresenter?.Template[Slot] as AmuletTemplateEntry).Amulet?.StatChoices,
                (TemplatePresenter?.Template[Slot] as AmuletTemplateEntry).Amulet?.AttributeAdjustment);
            }

            if (_enrichmentControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<Enrichment>(_enrichmentControl, new Rectangle(a.Location, Point.Zero).Add(_enrichmentControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Enrichment, (enrichment) => Enrichment = enrichment);
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => strings.Reset, () => string.Format(strings.ResetEntry, $"{strings.Stat} {strings.And} {strings.Enrichment}"), () =>
            {
                Stat = null;
                Enrichment = null;
            }, new()
            {
                new(() => strings.Stat, () => string.Format(strings.ResetEntry, strings.Stat), () => Stat = null ),
                new(() => strings.Enrichment, () => string.Format(strings.ResetEntry, strings.Enrichment), () => Enrichment = null )});

            CreateSubMenu(() => strings.Fill, () => string.Format(strings.FillEntry, $"{strings.Stat} {strings.EmptyJewellerySlots}"), () => SetGroupStat(Stat, false), new()
            {
                new(() => strings.Stat, () => string.Format(strings.FillEntry, $"{strings.Stat} {strings.EmptyJewellerySlots}"), () => SetGroupStat(Stat, false)),
                });

            CreateSubMenu(() => strings.Override, () => string.Format(strings.Override, $"{strings.Stat} {strings.JewellerySlots}"), () => SetGroupStat(Stat, true), new()
            {
                new(() => strings.Stat, () => string.Format(strings.OverrideEntry, $"{strings.Stat} {strings.JewellerySlots}"), () => SetGroupStat(Stat, true)),
                });

            CreateSubMenu(() => string.Format(strings.ResetAll, strings.Jewellery), () => string.Format(strings.ResetEntry, $"{strings.Stats} {strings.And} {strings.Infusions} {strings.JewellerySlots}"), () => SetGroupStat(null, true), new()
            {
                new(() => strings.Stats, () => string.Format(strings.ResetAll, $"{strings.Stats} {strings.JewellerySlots}"), () => SetGroupStat(null, true)),
                });
        }

        private void SetGroupStat(Stat stat, bool overrideExisting)
        {
            foreach (var slot in SlotGroup)
            {
                switch (slot)
                {
                    case AccessoireSlot accessoire:
                        accessoire.Stat = overrideExisting ? stat : accessoire.Stat ?? stat;
                        break;

                    case BackSlot back:
                        back.Stat = overrideExisting ? stat : back.Stat ?? stat;
                        break;

                    case RingSlot ring:
                        ring.Stat = overrideExisting ? stat : ring.Stat ?? stat;
                        break;

                    case AmuletSlot amulet:
                        amulet.Stat = overrideExisting ? stat : amulet.Stat ?? stat;
                        break;
                }
            }
        }

        private void OnEnrichmentChanged(object sender, Core.Models.ValueChangedEventArgs<Enrichment> e)
        {
            _enrichmentControl.Item = Enrichment;

            if (TemplatePresenter?.Template[Slot] is AmuletTemplateEntry entry)
                entry.Enrichment = Enrichment;
        }

        private void OnStatChanged(object sender, Core.Models.ValueChangedEventArgs<Stat> e)
        {
            ItemControl.Stat = Stat;

            if (TemplatePresenter?.Template[Slot] is AmuletTemplateEntry entry)
                entry.Stat = Stat;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Stat = null;
            Enrichment = null;
        }
    }
}
