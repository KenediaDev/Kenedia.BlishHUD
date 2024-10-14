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
using Kenedia.Modules.BuildsManager.Services;
using System.Diagnostics;

namespace Kenedia.Modules.BuildsManager.Controls_Old.GearPage.GearSlots
{
    public class BackSlot : GearSlot
    {
        private readonly ItemControl _infusion1Control = new(new() { TextureRegion = new(38, 38, 52, 52) });
        private readonly ItemControl _infusion2Control = new(new() { TextureRegion = new(38, 38, 52, 52) });

        private Stat? _stat;
        private Infusion? _infusion1;
        private Infusion? _infusion2;

        public BackSlot(TemplateSlotType gearSlot, Container parent, TemplatePresenter templatePresenter, Controls.Selection.SelectionPanel selectionPanel, Data data)
            : base(gearSlot, parent, templatePresenter, selectionPanel, data)
        {
            _infusion1Control.Placeholder.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
            _infusion2Control.Placeholder.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");

            _infusion1Control.Parent = this;
            _infusion2Control.Parent = this;
        }

        protected override void OnDataLoaded()
        {
            base.OnDataLoaded();
            ItemControl.Item = Data.Backs[74155];
        }

        public Stat? Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        public Infusion? Infusion1 { get => _infusion1; set => Common.SetProperty(ref _infusion1, value, OnInfusion1Changed); }

        public Infusion? Infusion2 { get => _infusion2; set => Common.SetProperty(ref _infusion2, value, OnInfusion2Changed); }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int infusionSize = (ItemControl.LocalBounds.Size.Y - 4) / 3;

            _infusion1Control.SetBounds(new(ItemControl.LocalBounds.Right + 2, ItemControl.LocalBounds.Top, infusionSize, infusionSize));
            _infusion2Control.SetBounds(new(ItemControl.LocalBounds.Right + 2, ItemControl.LocalBounds.Top + ((infusionSize + 2) * 1), infusionSize, infusionSize));
        }

        protected override void SetItemToSlotControl(object sender, TemplateSlotChangedEventArgs e)
        {
            base.SetItemToSlotControl(sender, e);

            SetItemFromTemplate();
        }

        protected override void SetItemFromTemplate()
        {
            base.SetItemFromTemplate();

            if (TemplatePresenter?.Template?[Slot] is BackTemplateEntry back)
            {
                Infusion1 = back?.Infusion1;
                Infusion2 = back?.Infusion2;

                Stat = back?.Stat;
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
                    (TemplatePresenter?.Template[Slot] as BackTemplateEntry).Back?.StatChoices,
                    (TemplatePresenter?.Template[Slot] as BackTemplateEntry).Back?.AttributeAdjustment);
            }

            if (_infusion1Control.MouseOver)
            {
                SelectionPanel?.SetAnchor<Infusion>(_infusion1Control, new Rectangle(a.Location, Point.Zero).Add(_infusion1Control.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) => TemplatePresenter?.Template?.SetItem(Slot, TemplateSubSlotType.Infusion1, infusion));
            }

            if (_infusion2Control.MouseOver)
            {
                SelectionPanel?.SetAnchor<Infusion>(_infusion2Control, new Rectangle(a.Location, Point.Zero).Add(_infusion2Control.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) => TemplatePresenter?.Template?.SetItem(Slot, TemplateSubSlotType.Infusion2, infusion));
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => strings.Reset, () => string.Format(strings.ResetEntry, $"{strings.Stat} {strings.And} {strings.Infusions}"), () =>
            {
                TemplatePresenter?.Template.SetItem<Stat>(Slot, TemplateSubSlotType.Stat, null);
                TemplatePresenter?.Template.SetItem<Infusion>(Slot, TemplateSubSlotType.Infusion1, null);
                TemplatePresenter?.Template.SetItem<Infusion>(Slot, TemplateSubSlotType.Infusion2, null);
            },
            [
                new(() => strings.Stat,() => string.Format(strings.ResetEntry, strings.Stat),() => TemplatePresenter?.Template.SetItem<Stat>(Slot, TemplateSubSlotType.Stat, null)),
                new(() => strings.Infusions,() => string.Format(strings.ResetEntry, strings.Infusions),() => {
                    TemplatePresenter?.Template.SetItem<Infusion>(Slot, TemplateSubSlotType.Infusion1, null);
                    TemplatePresenter?.Template.SetItem<Infusion>(Slot, TemplateSubSlotType.Infusion2, null);
                }),
            ]);

            CreateSubMenu(() => strings.Fill, () => string.Format(strings.FillEntry, $"{strings.Stat} {strings.And} {strings.Infusions} {strings.EmptyJewellerySlots}"), () =>
            {
                SetGroupStat(Stat, false);
                SetGroupInfusion(Infusion1, false);
            },
            [
                new(() => strings.Stat, () => string.Format(strings.FillEntry, $"{strings.Stat} {strings.EmptyJewellerySlots}"), () => SetGroupStat(Stat, false)),
                new(() => strings.Infusions, () => string.Format(strings.FillEntry, $"{strings.Infusions} {strings.EmptyJewellerySlots}"), () => SetGroupInfusion(Infusion1, false)),
                ]);

            CreateSubMenu(() => strings.Override, () => string.Format(strings.OverrideEntry, $"{strings.Stat} {strings.And} {strings.Infusions} {strings.JewellerySlots}"), () =>
            {
                SetGroupStat(Stat, true);
                SetGroupInfusion(Infusion1, true);
            },
            [
                new(() => strings.Stat, () => string.Format(strings.OverrideEntry, $"{strings.Stat} {strings.JewellerySlots}"), () => SetGroupStat(Stat, true)),
                new(() => strings.Infusions, () => string.Format(strings.OverrideEntry, $"{strings.Infusions} {strings.JewellerySlots}"), () => SetGroupInfusion(Infusion1, true)),
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

        private void OnStatChanged(object sender, Core.Models.ValueChangedEventArgs<Stat> e)
        {
            ItemControl.Stat = Stat;
        }

        private void OnInfusion2Changed(object sender, Core.Models.ValueChangedEventArgs<Infusion> e)
        {
            _infusion2Control.Item = Infusion2;
        }

        private void OnInfusion1Changed(object sender, Core.Models.ValueChangedEventArgs<Infusion> e)
        {
            _infusion1Control.Item = Infusion1;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Stat = null;
            Infusion1 = null;
            Infusion2 = null;
        }
    }
}
