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
using static Kenedia.Modules.BuildsManager.Controls.Selection.SelectionPanel;
using Kenedia.Modules.BuildsManager.Res;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage.GearSlots
{
    public class AccessoireSlot : GearSlot
    {
        private readonly ItemControl _infusionControl = new(new() { TextureRegion = new(38, 38, 52, 52) });

        private Stat _stat;
        private Infusion _infusion;

        public AccessoireSlot(TemplateSlotType gearSlot, Container parent, TemplatePresenter templatePresenter) : base(gearSlot, parent, templatePresenter)
        {
            _infusionControl.Placeholder.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
            ItemControl.Item = BuildsManager.Data.Trinkets[80002];

            _infusionControl.Parent = this;
        }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        public Infusion Infusion { get => _infusion; set => Common.SetProperty(ref _infusion, value, OnInfusionChanged); }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int infusionSize = (ItemControl.LocalBounds.Size.Y - 4) / 3;

            _infusionControl.SetBounds(new(ItemControl.Right + 1, ItemControl.Top, infusionSize, infusionSize));
        }

        protected override void SetItems(object sender, EventArgs e)
        {
            base.SetItems(sender, e);

            var armor = TemplatePresenter?.Template?[Slot] as AccessoireTemplateEntry;

            Infusion = armor?.Infusion;
            Stat = armor?.Stat;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var a = AbsoluteBounds;

            if (ItemControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<Stat>(this, new Rectangle(a.Location, Point.Zero).Add(ItemControl.LocalBounds), SelectionTypes.Stats, Slot, GearSubSlotType.None, (stat) =>
                {
                    (TemplatePresenter?.Template[Slot] as AccessoireTemplateEntry).Stat = stat;
                    Stat = stat;
                }, (TemplatePresenter?.Template[Slot] as AccessoireTemplateEntry).Accessoire?.StatChoices,
                (TemplatePresenter?.Template[Slot] as AccessoireTemplateEntry).Accessoire?.AttributeAdjustment);
            }

            if (_infusionControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<Infusion>(this, new Rectangle(a.Location, Point.Zero).Add(_infusionControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) =>
                {
                    (TemplatePresenter?.Template[Slot] as AccessoireTemplateEntry).Infusion = infusion;
                    Infusion = infusion;
                });
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => strings.Reset, () => string.Format(strings.ResetEntry, $"{strings.Stat} {strings.And} {strings.Infusion}"), () =>
            {
                Stat = null;
                Infusion = null;
            }, new()
            {
                new(() => strings.Stat,() =>  string.Format(strings.ResetEntry, strings.Stat),() => Stat = null),
                new(() => strings.Infusion,() => string.Format(strings.ResetEntry, strings.Infusion),() => Infusion = null),
            });

            CreateSubMenu(() => strings.Fill, () => string.Format(strings.FillEntry, $"{strings.Stat} {strings.And} {strings.Infusion} {strings.EmptyJewellerySlots}"), () =>
            {
                SetGroupStat(Stat, false);
                SetGroupInfusion(Infusion, false);
            }, new()
            {
                new(() => strings.Stat, () => string.Format(strings.FillEntry, $"{strings.Stat} {strings.EmptyJewellerySlots}"), () => SetGroupStat(Stat, false)),
                new(() => strings.Infusion, () => string.Format(strings.FillEntry, $"{strings.Infusion} {strings.EmptyJewellerySlots}"), () => SetGroupInfusion(Infusion, false)),
                });

            CreateSubMenu(() => strings.Override, () => string.Format(strings.OverrideEntry, $"{strings.Stat} {strings.And} {strings.Infusion} {strings.JewellerySlots}"), () =>
            {
                SetGroupStat(Stat, true);
                SetGroupInfusion(Infusion, true);
            }, new()
            {
                new(() => strings.Stat, () => string.Format(strings.OverrideEntry, $"{strings.Stat} {strings.JewellerySlots}"), () => SetGroupStat(Stat, true)),
                new(() => strings.Infusion, () => string.Format(strings.OverrideEntry, $"{strings.Infusion} {strings.JewellerySlots}"), () => SetGroupInfusion(Infusion, true)),
                });

            CreateSubMenu(() => string.Format(strings.ResetAll, strings.Jewellery), () => string.Format(strings.ResetEntry, $"{strings.Stats} {strings.And} {strings.Infusions} {strings.JewellerySlots}"), () =>
            {
                SetGroupStat(null, true);
                SetGroupInfusion(null, true);
            }, new()
            { new(() => strings.Stats,() => string.Format(strings.ResetEntry, $"{strings.Stats} {strings.JewellerySlots}"),() => SetGroupStat(null, true)),
                new(() => strings.Infusions,() => string.Format(strings.ResetEntry, $"{strings.Infusions} {strings.JewellerySlots}"),() => SetGroupInfusion(null, true)), });
        }

        private void SetGroupStat(Stat stat, bool overrideExisting)
        {
            foreach (var slot in SlotGroup)
            {
                switch (slot.Slot)
                {
                    case TemplateSlotType.Accessory_1:
                    case TemplateSlotType.Accessory_2:
                        var accessoire = slot as AccessoireSlot;
                        accessoire.Stat = overrideExisting ? stat : accessoire.Stat ?? stat;
                        (TemplatePresenter.Template[slot.Slot] as AccessoireTemplateEntry).Stat = overrideExisting ? stat : accessoire.Stat ?? stat; ;
                        break;

                    case TemplateSlotType.Back:
                        var back = slot as BackSlot;
                        back.Stat = overrideExisting ? stat : back.Stat ?? stat;
                        (TemplatePresenter.Template[slot.Slot] as BackTemplateEntry).Stat = overrideExisting ? stat : back.Stat ?? stat; ;
                        break;

                    case TemplateSlotType.Amulet:
                        var amulet = slot as AmuletSlot;
                        amulet.Stat = overrideExisting ? stat : amulet.Stat ?? stat;
                        (TemplatePresenter.Template[slot.Slot] as AmuletTemplateEntry).Stat = overrideExisting ? stat : amulet.Stat ?? stat; ;
                        break;

                    case TemplateSlotType.Ring_1:
                    case TemplateSlotType.Ring_2:
                        var ring = slot as RingSlot;
                        ring.Stat = overrideExisting ? stat : ring.Stat ?? stat;
                        (TemplatePresenter.Template[slot.Slot] as RingTemplateEntry).Stat = overrideExisting ? stat : ring.Stat ?? stat; ;
                        break;
                }
            }
        }

        private void SetGroupInfusion(Infusion infusion, bool overrideExisting)
        {
            foreach (var slot in SlotGroup)
            {
                switch (slot.Slot)
                {
                    case TemplateSlotType.Accessory_1:
                    case TemplateSlotType.Accessory_2:
                        var accessoire = slot as AccessoireSlot;
                        accessoire.Infusion = overrideExisting ? infusion : accessoire.Infusion ?? infusion;
                        (TemplatePresenter.Template[slot.Slot] as AccessoireTemplateEntry).Infusion = overrideExisting ? infusion : accessoire.Infusion ?? infusion;
                        break;

                    case TemplateSlotType.Back:
                        var back = slot as BackSlot;

                        back.Infusion1 = overrideExisting ? infusion : back.Infusion1 ?? infusion;
                        back.Infusion2 = overrideExisting ? infusion : back.Infusion2 ?? infusion;

                        (TemplatePresenter.Template[slot.Slot] as BackTemplateEntry).Infusion1 = overrideExisting ? infusion : back.Infusion1 ?? infusion;
                        (TemplatePresenter.Template[slot.Slot] as BackTemplateEntry).Infusion2 = overrideExisting ? infusion : back.Infusion2 ?? infusion;
                        break;

                    case TemplateSlotType.Ring_1:
                    case TemplateSlotType.Ring_2:
                        var ring = slot as RingSlot;

                        ring.Infusion1 = overrideExisting ? infusion : ring.Infusion1 ?? infusion;
                        ring.Infusion2 = overrideExisting ? infusion : ring.Infusion2 ?? infusion;
                        ring.Infusion3 = overrideExisting ? infusion : ring.Infusion3 ?? infusion;

                        (TemplatePresenter.Template[slot.Slot] as RingTemplateEntry).Infusion1 = overrideExisting ? infusion : ring.Infusion1 ?? infusion;
                        (TemplatePresenter.Template[slot.Slot] as RingTemplateEntry).Infusion2 = overrideExisting ? infusion : ring.Infusion2 ?? infusion;
                        (TemplatePresenter.Template[slot.Slot] as RingTemplateEntry).Infusion3 = overrideExisting ? infusion : ring.Infusion3 ?? infusion;
                        break;
                }
            }
        }

        private void OnInfusionChanged(object sender, Core.Models.ValueChangedEventArgs<Infusion> e)
        {
            _infusionControl.Item = Infusion;
        }

        private void OnStatChanged(object sender, Core.Models.ValueChangedEventArgs<Stat> e)
        {
            ItemControl.Stat = Stat;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Stat = null;
            Infusion = null;
        }
    }
}
