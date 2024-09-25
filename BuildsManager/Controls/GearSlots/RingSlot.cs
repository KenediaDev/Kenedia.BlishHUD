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
    public class RingSlot : GearSlot
    {
        private readonly ItemControl _infusion1Control = new(new() { TextureRegion = new(38, 38, 52, 52) });
        private readonly ItemControl _infusion2Control = new(new() { TextureRegion = new(38, 38, 52, 52) });
        private readonly ItemControl _infusion3Control = new(new() { TextureRegion = new(38, 38, 52, 52) });

        private Stat _stat;

        private Infusion _infusion1;
        private Infusion _infusion2;
        private Infusion _infusion3;

        public RingSlot(TemplateSlotType gearSlot, Container parent, TemplatePresenter templatePresenter, Controls.Selection.SelectionPanel selectionPanel) : base(gearSlot, parent, templatePresenter, selectionPanel)
        {
            _infusion1Control.Placeholder.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
            _infusion2Control.Placeholder.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
            _infusion3Control.Placeholder.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
            ItemControl.Item = BuildsManager.Data.Trinkets[91234];

            _infusion1Control.Parent = this;
            _infusion2Control.Parent = this;
            _infusion3Control.Parent = this;
        }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        public Infusion Infusion1 { get => _infusion1; set => Common.SetProperty(ref _infusion1, value, OnInfusion1Changed); }

        public Infusion Infusion2 { get => _infusion2; set => Common.SetProperty(ref _infusion2, value, OnInfusion2Changed); }

        public Infusion Infusion3 { get => _infusion3; set => Common.SetProperty(ref _infusion3, value, OnInfusion3Changed); }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int infusionSize = (ItemControl.LocalBounds.Size.Y - 4) / 3;
            _infusion1Control.SetBounds(new(ItemControl.LocalBounds.Right + 2, ItemControl.LocalBounds.Top + ((infusionSize + 2) * 0), infusionSize, infusionSize));
            _infusion2Control.SetBounds(new(ItemControl.LocalBounds.Right + 2, ItemControl.LocalBounds.Top + ((infusionSize + 2) * 1), infusionSize, infusionSize));
            _infusion3Control.SetBounds(new(ItemControl.LocalBounds.Right + 2, ItemControl.LocalBounds.Top + ((infusionSize + 2) * 2), infusionSize, infusionSize));
        }

        protected override void SetItem(object sender, TemplateSlotChangedEventArgs e)
        {
            base.SetItem(sender, e);

            if (TemplatePresenter?.Template?[Slot] is RingTemplateEntry ring)
            {
                Infusion1 = ring?.Infusion1;
                Infusion2 = ring?.Infusion2;
                Infusion3 = ring?.Infusion3;
                Stat = ring?.Stat;
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
                    (TemplatePresenter?.Template[Slot] as RingTemplateEntry).Ring?.StatChoices,
                    (TemplatePresenter?.Template[Slot] as RingTemplateEntry).Ring?.AttributeAdjustment);
            }

            if (_infusion1Control.MouseOver)
            {
                SelectionPanel?.SetAnchor<Infusion>(_infusion1Control, new Rectangle(a.Location, Point.Zero).Add(_infusion1Control.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) => TemplatePresenter?.Template?.SetItem(Slot, TemplateSubSlotType.Infusion1, infusion));
            }

            if (_infusion2Control.MouseOver)
            {
                SelectionPanel?.SetAnchor<Infusion>(_infusion2Control, new Rectangle(a.Location, Point.Zero).Add(_infusion2Control.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) => TemplatePresenter?.Template?.SetItem(Slot, TemplateSubSlotType.Infusion2, infusion));
            }

            if (_infusion3Control.MouseOver)
            {
                SelectionPanel?.SetAnchor<Infusion>(_infusion3Control, new Rectangle(a.Location, Point.Zero).Add(_infusion3Control.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) => TemplatePresenter?.Template?.SetItem(Slot, TemplateSubSlotType.Infusion3, infusion));
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => strings.Reset, () => string.Format(strings.ResetEntry, $"{strings.Stat} {strings.And} {strings.Infusions}"), () =>
            {
                TemplatePresenter?.Template?.SetItem<Stat>(Slot, TemplateSubSlotType.Stat, null);
                TemplatePresenter?.Template?.SetItem<Infusion>(Slot, TemplateSubSlotType.Infusion1, null);
                TemplatePresenter?.Template?.SetItem<Infusion>(Slot, TemplateSubSlotType.Infusion2, null);
                TemplatePresenter?.Template?.SetItem<Infusion>(Slot, TemplateSubSlotType.Infusion3, null);
            },
            [
                new(() => strings.Stat,() => string.Format(strings.ResetEntry, strings.Stat),() => TemplatePresenter?.Template?.SetItem<Stat>(Slot, TemplateSubSlotType.Stat, null)),
                new(() => strings.Infusions,() => string.Format(strings.ResetEntry, strings.Infusions),() =>
                {
                TemplatePresenter?.Template?.SetItem<Infusion>(Slot, TemplateSubSlotType.Infusion1, null);
                TemplatePresenter?.Template?.SetItem<Infusion>(Slot, TemplateSubSlotType.Infusion2, null);
                TemplatePresenter?.Template?.SetItem<Infusion>(Slot, TemplateSubSlotType.Infusion3, null);
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

            CreateSubMenu(() => string.Format(strings.ResetAll, strings.Jewellery), () => string.Format(strings.ResetEntry, $"{strings.Stats} {strings.And} {strings.Infusions} {strings.JewellerySlots}"), () =>
            {
                SetGroupStat(null, true);
                SetGroupInfusion(null, true);
            },
            [
                new(() => strings.Stats,() => string.Format(strings.ResetEntry, $"{strings.Stats} {strings.JewellerySlots}"),() => SetGroupStat(null, true)),
                new(() => strings.Infusions,() => string.Format(strings.ResetEntry, $"{strings.Infusions} {strings.JewellerySlots}"),() => SetGroupInfusion(null, true)),
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

        private void OnInfusion1Changed(object sender, Core.Models.ValueChangedEventArgs<Infusion> e)
        {
            _infusion1Control.Item = Infusion1;
        }

        private void OnInfusion2Changed(object sender, Core.Models.ValueChangedEventArgs<Infusion> e)
        {
            _infusion2Control.Item = Infusion2;
        }

        private void OnInfusion3Changed(object sender, Core.Models.ValueChangedEventArgs<Infusion> e)
        {
            _infusion3Control.Item = Infusion3;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Stat = null;
            Infusion1 = null;
            Infusion2 = null;
            Infusion3 = null;
        }
    }
}
