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
using Kenedia.Modules.BuildsManager.Controls.Selection;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.Controls_Old.GearPage.GearSlots
{
    public class AccessoireSlot : GearSlot
    {
        private readonly ItemControl _infusionControl = new(new() { TextureRegion = new(38, 38, 52, 52) });

        private Stat? _stat;
        private Infusion? _infusion;

        public AccessoireSlot(TemplateSlotType gearSlot, Container parent, TemplatePresenter templatePresenter, SelectionPanel selectionPanel) : base(gearSlot, parent, templatePresenter, selectionPanel)
        {
            _infusionControl.Placeholder.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
            ItemControl.Item = BuildsManager.Data.Trinkets[81908];

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

        protected override void SetItemToSlotControl(object sender, TemplateSlotChangedEventArgs e)
        {
            base.SetItemToSlotControl(sender, e);

            SetItemFromTemplate();
        }

        protected override void SetItemFromTemplate()
        {
            base.SetItemFromTemplate();

            if (TemplatePresenter?.Template?[Slot] is AccessoireTemplateEntry accessoire)
            {
                Infusion = accessoire?.Infusion1;
                Stat = accessoire?.Stat;
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var a = AbsoluteBounds;

            if (ItemControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<Stat>(ItemControl, new Rectangle(a.Location, Point.Zero).Add(ItemControl.LocalBounds), SelectionTypes.Stats, Slot, GearSubSlotType.None,
                    (stat) => TemplatePresenter?.Template?.SetItem(Slot, TemplateSubSlotType.Stat, stat),
                    (TemplatePresenter?.Template[Slot] as AccessoireTemplateEntry).Accessoire?.StatChoices,
                    (TemplatePresenter?.Template[Slot] as AccessoireTemplateEntry).Accessoire?.AttributeAdjustment);
            }

            if (_infusionControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<Infusion>(_infusionControl, new Rectangle(a.Location, Point.Zero).Add(_infusionControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) => TemplatePresenter.Template?.SetItem(Slot, TemplateSubSlotType.Infusion1, infusion));
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => strings.Reset, () => string.Format(strings.ResetEntry, $"{strings.Stat} {strings.And} {strings.Infusion}"), () =>
            {
                TemplatePresenter?.Template?.SetItem<Stat>(Slot, TemplateSubSlotType.Stat, null);
                TemplatePresenter?.Template?.SetItem<Infusion>(Slot, TemplateSubSlotType.Infusion1, null);
            },
            [
                new(() => strings.Stat,() =>  string.Format(strings.ResetEntry, strings.Stat),() =>  TemplatePresenter?.Template?.SetItem<Stat>(Slot, TemplateSubSlotType.Stat, null)),
                new(() => strings.Infusion,() => string.Format(strings.ResetEntry, strings.Infusion),() => TemplatePresenter?.Template?.SetItem<Infusion>(Slot, TemplateSubSlotType.Infusion1, null)),
            ]);

            CreateSubMenu(() => strings.Fill, () => string.Format(strings.FillEntry, $"{strings.Stat} {strings.And} {strings.Infusion} {strings.EmptyJewellerySlots}"), () =>
            {
                SetGroupStat(Stat, false);
                SetGroupInfusion(Infusion, false);
            },
            [
                new(() => strings.Stat, () => string.Format(strings.FillEntry, $"{strings.Stat} {strings.EmptyJewellerySlots}"), () => SetGroupStat(Stat, false)),
                new(() => strings.Infusion, () => string.Format(strings.FillEntry, $"{strings.Infusion} {strings.EmptyJewellerySlots}"), () => SetGroupInfusion(Infusion, false)),
                ]);

            CreateSubMenu(() => strings.Override, () => string.Format(strings.OverrideEntry, $"{strings.Stat} {strings.And} {strings.Infusion} {strings.JewellerySlots}"), () =>
            {
                SetGroupStat(Stat, true);
                SetGroupInfusion(Infusion, true);
            },
            [
                new(() => strings.Stat, () => string.Format(strings.OverrideEntry, $"{strings.Stat} {strings.JewellerySlots}"), () => SetGroupStat(Stat, true)),
                new(() => strings.Infusion, () => string.Format(strings.OverrideEntry, $"{strings.Infusion} {strings.JewellerySlots}"), () => SetGroupInfusion(Infusion, true)),
                ]);

            CreateSubMenu(() => string.Format(strings.ResetAll, strings.Jewellery), () => string.Format(strings.ResetEntry, $"{strings.Stats}, {strings.Enrichment} {strings.And} {strings.Infusions} {strings.JewellerySlots}"), () =>
            {
                SetGroupStat(null, true);
                SetGroupInfusion(null, true);
                TemplatePresenter.Template?.SetGroup<Enrichment>(Slot, TemplateSubSlotType.Enrichment, null, true);
            },
            [
                new(() => strings.Stats,() => string.Format(strings.ResetEntry, $"{strings.Stats} {strings.JewellerySlots}"),() => SetGroupStat(null, true)),
                new(() => strings.Infusions,() => string.Format(strings.ResetEntry, $"{strings.Infusions} {strings.JewellerySlots}"),() => SetGroupInfusion(null, true)),
                new(() => strings.Enrichment,() => string.Format(strings.ResetEntry, $"{strings.Enrichment} {strings.JewellerySlots}"),() => TemplatePresenter.Template?.SetGroup<Enrichment>(Slot, TemplateSubSlotType.Enrichment, null, true)),
            ]);
        }

        private void SetGroupStat(Stat stat = null, bool overrideExisting = false)
        {
            TemplatePresenter.Template?.SetGroup(Slot, TemplateSubSlotType.Stat, stat, overrideExisting);
        }

        private void SetGroupInfusion(Infusion infusion = null, bool overrideExisting = false)
        {
            TemplatePresenter.Template?.SetGroup(Slot, TemplateSubSlotType.Infusion1, infusion, overrideExisting);
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
