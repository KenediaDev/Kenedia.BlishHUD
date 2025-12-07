using Container = Blish_HUD.Controls.Container;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using Blish_HUD;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.BuildsManager.TemplateEntries;
using Kenedia.Modules.BuildsManager.Res;
using System.Linq;
using Kenedia.Modules.BuildsManager.Services;

namespace Kenedia.Modules.BuildsManager.Controls_Old.GearPage.GearSlots
{
    public class AquaticWeaponSlot : GearSlot
    {
        private readonly ItemControl _infusion1Control = new(new() { TextureRegion = new(38, 38, 52, 52) });
        private readonly ItemControl _infusion2Control = new(new() { TextureRegion = new(38, 38, 52, 52) });
        private readonly ItemControl _sigil1Control = new(new(784324) { TextureRegion = new(38, 38, 52, 52) });
        private readonly ItemControl _sigil2Control = new(new(784324) { TextureRegion = new(38, 38, 52, 52) });

        private readonly DetailedTexture _changeWeaponTexture = new(2338896, 2338895)
        {
            TextureRegion = new(4, 4, 24, 24),
            DrawColor = Color.White * 0.5F,
            HoverDrawColor = Color.White,
        };

        private Rectangle _sigil1Bounds;
        private Rectangle _sigil2Bounds;
        private Rectangle _infusion1Bounds;
        private Rectangle _infusion2Bounds;

        public AquaticWeaponSlot(TemplateSlotType gearSlot, Container parent, TemplatePresenter templatePresenter, Controls.Selection.SelectionPanel selectionPanel, Data data)
            : base(gearSlot, parent, templatePresenter, selectionPanel, data)
        {
            _infusion1Control.Placeholder.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
            _infusion2Control.Placeholder.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");

            _sigil1Control.Parent = this;
            _sigil2Control.Parent = this;
            _infusion1Control.Parent = this;
            _infusion2Control.Parent = this;
        }

        public Stat Stat { get; set => Common.SetProperty(ref field, value, OnStatChanged); }

        public Sigil Sigil1 { get; set => Common.SetProperty(ref field, value, OnSigil1Changed); }

        public Sigil Sigil2 { get; set => Common.SetProperty(ref field, value, OnSigil2Changed); }

        public Infusion Infusion1 { get; set => Common.SetProperty(ref field, value, OnInfusion1Changed); }

        public Infusion Infusion2 { get; set => Common.SetProperty(ref field, value, OnInfusion2Changed); }

        private void SetGroupStat(Stat stat = null, bool overrideExisting = false)
        {
            TemplatePresenter.Template?.SetGroup(Slot, TemplateSubSlotType.Stat, stat, overrideExisting);
        }

        private void SetGroupSigil(Sigil sigil = null, bool overrideExisting = false)
        {
            TemplatePresenter.Template?.SetGroup(Slot, TemplateSubSlotType.Sigil1, sigil, overrideExisting);
        }

        private void SetGroupInfusion(Infusion infusion = null, bool overrideExisting = false)
        {
            TemplatePresenter.Template?.SetGroup(Slot, TemplateSubSlotType.Infusion1, infusion, overrideExisting);
        }

        private void SetGroupWeapon(Weapon item = null, bool overrideExisting = false)
        {
            TemplatePresenter.Template?.SetGroup(Slot, TemplateSubSlotType.Item, item, overrideExisting);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int upgradeSize = (ItemControl.LocalBounds.Size.Y - 4) / 2;
            int textPadding = Slot is TemplateSlotType.AquaBreather ? upgradeSize + 5 : 5;

            int size = Math.Min(Width, Height);
            int padding = 2;
            _changeWeaponTexture.Bounds = new(new(ItemControl.LocalBounds.Left + padding, padding), new((int)((size - (padding * 2)) / 2.5)));

            _sigil1Control.SetBounds(new(ItemControl.Right + padding, 0, upgradeSize, upgradeSize));
            _sigil2Control.SetBounds(new(ItemControl.Right + padding + upgradeSize + padding, 0, upgradeSize, upgradeSize));

            _infusion1Control.SetBounds(new(ItemControl.Right + padding, ItemControl.Bottom - upgradeSize, upgradeSize, upgradeSize));
            _infusion2Control.SetBounds(new(ItemControl.Right + padding + upgradeSize + padding, ItemControl.Bottom - upgradeSize, upgradeSize, upgradeSize));

            int upgradeWidth = (Width - (_sigil2Control.Right + 2)) / 2;
            int x = _sigil2Control.Right + textPadding + 4;
            _sigil1Bounds = new(x, _sigil1Control.Top, upgradeWidth, _sigil1Control.Height);
            _sigil2Bounds = new(x + upgradeWidth, _sigil1Control.Top, upgradeWidth, _sigil1Control.Height);

            _infusion1Bounds = new(x, _infusion1Control.Top, upgradeWidth, _infusion1Control.Height);
            _infusion2Bounds = new(x + upgradeWidth, _infusion1Control.Top, upgradeWidth, _infusion1Control.Height);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            if (TemplatePresenter.IsPve != false)
            {
                _changeWeaponTexture.Draw(this, spriteBatch, RelativeMousePosition);

                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Sigil1?.DisplayText ?? string.Empty), UpgradeFont, _sigil1Bounds, UpgradeColor, false, HorizontalAlignment.Left, VerticalAlignment.Middle);
                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Sigil2?.DisplayText ?? string.Empty), UpgradeFont, _sigil2Bounds, UpgradeColor, false, HorizontalAlignment.Left, VerticalAlignment.Middle);

                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Infusion1?.DisplayText ?? string.Empty), InfusionFont, _infusion1Bounds, InfusionColor, true, HorizontalAlignment.Left, VerticalAlignment.Middle);
                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Infusion2?.DisplayText ?? string.Empty), InfusionFont, _infusion2Bounds, InfusionColor, true, HorizontalAlignment.Left, VerticalAlignment.Middle);
            }
        }

        protected override void SetItemToSlotControl(object sender, TemplateSlotChangedEventArgs e)
        {
            base.SetItemToSlotControl(sender, e);
            SetItemFromTemplate();
        }

        protected override void SetItemFromTemplate()
        {
            base.SetItemFromTemplate();

            if (TemplatePresenter?.Template?[Slot] is AquaticWeaponTemplateEntry aquaticWeapon)
            {
                Item = aquaticWeapon.Item;
                Infusion1 = aquaticWeapon?.Infusion1;
                Infusion2 = aquaticWeapon?.Infusion2;
                Sigil1 = aquaticWeapon?.Sigil1;
                Sigil2 = aquaticWeapon?.Sigil2;

                Stat = aquaticWeapon?.Stat;
            }
            else
            {
                Item = null;
                Infusion1 = null;
                Infusion2 = null;
                Sigil1 = null;
                Sigil2 = null;
                Stat = null;
            }
        }

        protected override void SetAnchor()
        {
            var a = AbsoluteBounds;

            if (ItemControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<Stat>(ItemControl, new Rectangle(a.Location, Point.Zero).Add(ItemControl.LocalBounds), SelectionTypes.Stats, Slot, GearSubSlotType.None,
                    (stat) => TemplatePresenter?.Template?.SetItem(Slot, TemplateSubSlotType.Stat, stat),
                    (TemplatePresenter?.Template[Slot] as AquaticWeaponTemplateEntry).Weapon?.StatChoices ?? Data.Weapons.Values.FirstOrDefault()?.StatChoices ?? [],
                    (TemplatePresenter?.Template[Slot] as AquaticWeaponTemplateEntry).Weapon?.AttributeAdjustment);
            }

            if (_sigil1Control.MouseOver)
            {
                SelectionPanel?.SetAnchor<Sigil>(_sigil1Control, new Rectangle(a.Location, Point.Zero).Add(_sigil1Control.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Sigil, (sigil) => TemplatePresenter.Template?.SetItem(Slot, TemplateSubSlotType.Sigil1, sigil));
            }

            if (_sigil2Control.MouseOver)
            {
                SelectionPanel?.SetAnchor<Sigil>(_sigil2Control, new Rectangle(a.Location, Point.Zero).Add(_sigil2Control.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Sigil, (sigil) => TemplatePresenter.Template?.SetItem(Slot, TemplateSubSlotType.Sigil2, sigil));
            }

            if (_infusion1Control.MouseOver)
            {
                SelectionPanel?.SetAnchor<Infusion>(_infusion1Control, new Rectangle(a.Location, Point.Zero).Add(_infusion1Control.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) => TemplatePresenter.Template?.SetItem(Slot, TemplateSubSlotType.Infusion1, infusion));
            }

            if (_infusion2Control.MouseOver)
            {
                SelectionPanel?.SetAnchor<Infusion>(_infusion2Control, new Rectangle(a.Location, Point.Zero).Add(_infusion2Control.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) => TemplatePresenter.Template?.SetItem(Slot, TemplateSubSlotType.Infusion2, infusion));
            }

            if (_changeWeaponTexture.Hovered)
            {
                SelectionPanel?.SetAnchor<Weapon>(this, new Rectangle(a.Location, Point.Zero).Add(ItemControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Item, (item) => TemplatePresenter.Template?.SetItem(Slot, TemplateSubSlotType.Item, item));
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => strings.Reset, () => string.Format(strings.ResetEntry, $"{strings.Weapon}, {strings.Stat}, {strings.Sigils} {strings.And} {strings.Infusions}"), () =>
            {
                TemplatePresenter?.Template.SetItem<Weapon>(Slot, TemplateSubSlotType.Item, null);
                TemplatePresenter?.Template.SetItem<Stat>(Slot, TemplateSubSlotType.Stat, null);
                TemplatePresenter?.Template.SetItem<Sigil>(Slot, TemplateSubSlotType.Sigil1, null);
                TemplatePresenter?.Template.SetItem<Sigil>(Slot, TemplateSubSlotType.Sigil2, null);
                TemplatePresenter?.Template.SetItem<Infusion>(Slot, TemplateSubSlotType.Infusion1, null);
                TemplatePresenter?.Template.SetItem<Infusion>(Slot, TemplateSubSlotType.Infusion2, null);
            },
            [
                new(() => strings.Weapon, () => string.Format(strings.ResetEntry, strings.Weapon), () => TemplatePresenter?.Template.SetItem<Weapon>(Slot, TemplateSubSlotType.Item, null)),
                new(() => strings.Stat, () => string.Format(strings.ResetEntry, strings.Stat), () => TemplatePresenter?.Template.SetItem<Stat>(Slot, TemplateSubSlotType.Stat, null)),
                new(() => strings.Sigil, () => string.Format(strings.ResetEntry, strings.Sigils), () => {
                    TemplatePresenter?.Template.SetItem<Sigil>(Slot, TemplateSubSlotType.Sigil1, null);
                    TemplatePresenter?.Template.SetItem<Sigil>(Slot, TemplateSubSlotType.Sigil2, null);
                }),
                new(() => strings.Infusions, () => string.Format(strings.ResetEntry, strings.Weapon), () => {
                    TemplatePresenter?.Template.SetItem<Infusion>(Slot, TemplateSubSlotType.Infusion1, null);
                    TemplatePresenter?.Template.SetItem<Infusion>(Slot, TemplateSubSlotType.Infusion2, null);
                } ),
                ]);

            CreateSubMenu(() => strings.Fill, () => string.Format(strings.FillEntry, $"{strings.Weapon}, {strings.Stat}, {strings.Sigils} {strings.And} {strings.Infusions} {strings.EmptyWeaponSlots}"), () =>
            {
                SetGroupWeapon(Item as Weapon, false);
                SetGroupStat(Stat, false);
                SetGroupSigil(Sigil1, false);
                SetGroupInfusion(Infusion1, false);
            },
            [
                new(() => strings.Weapon, () => string.Format(strings.FillEntry, $"{strings.Weapon} {strings.EmptyWeaponSlots}"), () => SetGroupWeapon(Item as Weapon, false)),
                new(() => strings.Stat, () => string.Format(strings.FillEntry, $"{strings.Stat} {strings.EmptyWeaponSlots}"), () => SetGroupStat(Stat, false)),
                new(() => strings.Sigil, () => string.Format(strings.FillEntry, $"{strings.Sigils} {strings.EmptyWeaponSlots}"), () => SetGroupSigil(Sigil1, false)),
                new(() => strings.Infusions, () => string.Format(strings.FillEntry, $"{strings.Infusions} {strings.EmptyWeaponSlots}"), () => SetGroupInfusion(Infusion1, false)),
                ]);

            CreateSubMenu(() => strings.Override, () => string.Format(strings.OverrideEntry, $"{strings.Weapon}, {strings.Stat}, {strings.Sigils} {strings.And} {strings.Infusions} {strings.WeaponSlots}"), () =>
            {
                SetGroupWeapon(Item as Weapon, true);
                SetGroupStat(Stat, true);
                SetGroupSigil(Sigil1, true);
                SetGroupInfusion(Infusion1, true);
            },
            [
                new(() => strings.Weapon, () => string.Format(strings.OverrideEntry, $"{strings.Weapons} {strings.WeaponSlots}"), () => SetGroupWeapon(Item as Weapon, true)),
                new(() => strings.Stat, () => string.Format(strings.OverrideEntry, $"{strings.Stat} {strings.WeaponSlots}"), () => SetGroupStat(Stat, true)),
                new(() => strings.Sigil, () => string.Format(strings.FillEntry, $"{strings.Sigils} {strings.WeaponSlots}"), () => SetGroupSigil(Sigil1, true)),
                new(() => strings.Infusions, () => string.Format(strings.OverrideEntry, $"{strings.Infusions} {strings.WeaponSlots}"), () => SetGroupInfusion(Infusion1, true)),
                ]);

            CreateSubMenu(() => string.Format(strings.ResetAll, strings.Weapons), () => string.Format(strings.ResetEntry, $"{strings.Weapons}, {strings.Stats} , {strings.Sigils} {strings.And} {strings.Infusions} {strings.WeaponSlots}"), () =>
            {
                SetGroupWeapon(null, true);
                SetGroupStat(null, true);
                SetGroupSigil(null, true);
                SetGroupInfusion(null, true);
            },
            [
                new(() => strings.Weapons, () => string.Format(strings.ResetAll, $"{strings.Weapons} {strings.WeaponSlots}"), () => SetGroupWeapon(null, true)),
                new(() => strings.Stats, () => string.Format(strings.ResetAll, $"{strings.Stats} {strings.WeaponSlots}"), () => SetGroupStat(null, true)),
                new(() => strings.Sigils, () => string.Format(strings.ResetAll, $"{strings.Sigils} {strings.WeaponSlots}"), () => SetGroupSigil(null, true)),
                new(() => strings.Infusions, () => string.Format(strings.ResetAll, $"{strings.Infusions} {strings.WeaponSlots}"), () => SetGroupInfusion(null, true) ),
                ]);
        }

        private void OnStatChanged(object sender, Core.Models.ValueChangedEventArgs<Stat> e)
        {
            ItemControl.Stat = Stat;
        }

        private void OnSigil2Changed(object sender, Core.Models.ValueChangedEventArgs<Sigil> e)
        {
            _sigil2Control.Item = Sigil2;
        }

        private void OnSigil1Changed(object sender, Core.Models.ValueChangedEventArgs<Sigil> e)
        {
            _sigil1Control.Item = Sigil1;
        }

        private void OnInfusion1Changed(object sender, Core.Models.ValueChangedEventArgs<Infusion> e)
        {
            _infusion1Control.Item = Infusion1;
        }

        private void OnInfusion2Changed(object sender, Core.Models.ValueChangedEventArgs<Infusion> e)
        {
            _infusion2Control.Item = Infusion2;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Stat = null;
            Sigil1 = null;
            Sigil2 = null;
            Infusion1 = null;
            Infusion2 = null;
        }
    }
}
