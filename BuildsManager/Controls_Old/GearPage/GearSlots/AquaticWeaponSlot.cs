using Container = Blish_HUD.Controls.Container;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using Blish_HUD;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.BuildsManager.TemplateEntries;
using static Kenedia.Modules.BuildsManager.Controls_Old.Selection.SelectionPanel;
using ItemWeaponType = Gw2Sharp.WebApi.V2.Models.ItemWeaponType;
using Kenedia.Modules.BuildsManager.Res;
using System.Linq;

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

        private readonly DetailedTexture _statTexture = new() { };

        private Stat _stat;
        private Sigil _sigil1;
        private Sigil _sigil2;
        private Infusion _infusion1;
        private Infusion _infusion2;

        private Rectangle _sigil1Bounds;
        private Rectangle _sigil2Bounds;
        private Rectangle _infusion1Bounds;
        private Rectangle _infusion2Bounds;

        public AquaticWeaponSlot(TemplateSlotType gearSlot, Container parent, TemplatePresenter templatePresenter) : base(gearSlot, parent, templatePresenter)
        {
            _infusion1Control.Placeholder.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
            _infusion2Control.Placeholder.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");

            _sigil1Control.Parent = this;
            _sigil2Control.Parent = this;
            _infusion1Control.Parent = this;
            _infusion2Control.Parent = this;
        }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        public Sigil Sigil1 { get => _sigil1; set => Common.SetProperty(ref _sigil1, value, OnSigil1Changed); }

        public Sigil Sigil2 { get => _sigil2; set => Common.SetProperty(ref _sigil2, value, OnSigil2Changed); }

        public Infusion Infusion1 { get => _infusion1; set => Common.SetProperty(ref _infusion1, value, OnInfusion1Changed); }

        public Infusion Infusion2 { get => _infusion2; set => Common.SetProperty(ref _infusion2, value, OnInfusion2Changed); }

        private void SetGroupStat(Stat stat = null, bool overrideExisting = false)
        {
            foreach (var slot in SlotGroup)
            {
                if (slot.Slot is TemplateSlotType.Aquatic or TemplateSlotType.AltAquatic)
                {
                    (slot as AquaticWeaponSlot).Stat = overrideExisting ? stat : (slot as AquaticWeaponSlot).Stat ?? stat;
                }
                else
                {
                    (slot as WeaponSlot).Stat = overrideExisting ? stat : (slot as WeaponSlot).Stat ?? stat;
                }
            }
        }

        private void SetGroupSigil(Sigil sigil = null, bool overrideExisting = false)
        {
            foreach (var slot in SlotGroup)
            {
                if (slot.Slot is TemplateSlotType.Aquatic or TemplateSlotType.AltAquatic)
                {
                    (slot as AquaticWeaponSlot).Sigil1 = overrideExisting ? sigil : (slot as AquaticWeaponSlot).Sigil1 ?? sigil;
                    (slot as AquaticWeaponSlot).Sigil2 = overrideExisting ? sigil : (slot as AquaticWeaponSlot).Sigil2 ?? sigil;
                }
                else
                {
                    (slot as WeaponSlot).Sigil = overrideExisting ? sigil : (slot as WeaponSlot).Sigil ?? sigil;
                }
            }
        }

        private void SetGroupInfusion(Infusion infusion = null, bool overrideExisting = false)
        {
            foreach (var slot in SlotGroup)
            {
                if (slot.Slot is TemplateSlotType.Aquatic or TemplateSlotType.AltAquatic)
                {
                    (slot as AquaticWeaponSlot).Infusion1 = overrideExisting ? infusion : (slot as AquaticWeaponSlot).Infusion1 ?? infusion;
                    (slot as AquaticWeaponSlot).Infusion2 = overrideExisting ? infusion : (slot as AquaticWeaponSlot).Infusion2 ?? infusion;
                }
                else
                {
                    (slot as WeaponSlot).Infusion = overrideExisting ? infusion : (slot as WeaponSlot).Infusion ?? infusion;
                }
            }
        }

        private void SetGroupWeapon(Weapon item = null, bool overrideExisting = false)
        {
            foreach (var slot in SlotGroup)
            {
                if (slot.Slot is TemplateSlotType.Aquatic or TemplateSlotType.AltAquatic)
                {
                    if (overrideExisting || (slot as AquaticWeaponSlot).Item == null)
                        (slot as AquaticWeaponSlot).SelectWeapon(item);
                }
                else
                {
                    if (overrideExisting || (slot as WeaponSlot).Item == null)
                        (slot as WeaponSlot).SelectWeapon(item);
                }
            }
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

        public void SelectWeapon(Weapon item)
        {
            if (item == null || item.WeaponType is ItemWeaponType.Harpoon or ItemWeaponType.Speargun or ItemWeaponType.Trident)
            {
                if (TemplatePresenter?.Template[Slot] is AquaticWeaponTemplateEntry entry)
                {
                    entry.Weapon = item;
                    Item = item;
                }
            }
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

        protected override void SetItems(object sender, EventArgs e)
        {
            base.SetItems(sender, e);

            var aquaticWeapon = TemplatePresenter?.Template?[Slot] as AquaticWeaponTemplateEntry;

            Infusion1 = aquaticWeapon?.Infusion1;
            Infusion2 = aquaticWeapon?.Infusion2;
            Sigil1 = aquaticWeapon?.Sigil1;
            Sigil2 = aquaticWeapon?.Sigil2;

            Stat = aquaticWeapon?.Stat;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var a = AbsoluteBounds;

            if (ItemControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<Stat>(ItemControl, new Rectangle(a.Location, Point.Zero).Add(ItemControl.LocalBounds), SelectionTypes.Stats, Slot, GearSubSlotType.None, (stat) => Stat = stat,
                (TemplatePresenter?.Template[Slot] as AquaticWeaponTemplateEntry).Weapon?.StatChoices ?? BuildsManager.Data.Weapons.Values.FirstOrDefault()?.StatChoices,
                (TemplatePresenter?.Template[Slot] as AquaticWeaponTemplateEntry).Weapon?.AttributeAdjustment);
            }

            if (_sigil1Control.MouseOver)
            {
                SelectionPanel?.SetAnchor<Sigil>(_sigil1Control, new Rectangle(a.Location, Point.Zero).Add(_sigil1Control.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Sigil, (sigil) => Sigil1 = sigil);
            }

            if (_infusion1Control.MouseOver)
            {
                SelectionPanel?.SetAnchor<Infusion>(_infusion1Control, new Rectangle(a.Location, Point.Zero).Add(_infusion1Control.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) => Infusion1 = infusion);
            }

            if (_sigil2Control.MouseOver)
            {
                SelectionPanel?.SetAnchor<Sigil>(_sigil2Control, new Rectangle(a.Location, Point.Zero).Add(_sigil2Control.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Sigil, (sigil) => Sigil2 = sigil);
            }

            if (_infusion2Control.MouseOver)
            {
                SelectionPanel?.SetAnchor<Infusion>(_infusion2Control, new Rectangle(a.Location, Point.Zero).Add(_infusion2Control.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) => Infusion2 = infusion);
            }

            if (_changeWeaponTexture.Hovered)
            {
                SelectionPanel?.SetAnchor<Weapon>(this, new Rectangle(a.Location, Point.Zero).Add(ItemControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Item, (item) => Item = item);
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => strings.Reset, () => string.Format(strings.ResetEntry, $"{strings.Weapon}, {strings.Stat}, {strings.Sigils} {strings.And} {strings.Infusions}"), () =>
            {
                Stat = null;
                Sigil1 = null;
                Sigil2 = null;
                Infusion1 = null;
                Infusion2 = null;
                SelectWeapon(null);
            }, new()
            {
                new(() => strings.Weapon, () => string.Format(strings.ResetEntry, strings.Weapon), () => Item = null ),
                new(() => strings.Stat, () => string.Format(strings.ResetEntry, strings.Stat), () => Stat = null ),
                new(() => strings.Sigil, () => string.Format(strings.ResetEntry, strings.Sigils), () => {
                    Sigil1 = null;
                    Sigil2 = null;
                }),
                new(() => strings.Infusions, () => string.Format(strings.ResetEntry, strings.Weapon), () => {
                    Infusion1 = null;
                    Infusion2 = null;
                } ),
                });

            CreateSubMenu(() => strings.Fill, () => string.Format(strings.FillEntry, $"{strings.Weapon}, {strings.Stat}, {strings.Sigils} {strings.And} {strings.Infusions} {strings.EmptyWeaponSlots}"), () => 
            {
                SetGroupWeapon(Item as Weapon, false);
                SetGroupStat(Stat, false);
                SetGroupSigil(Sigil1, false);
                SetGroupInfusion(Infusion1, false);
            }, new()
            {
                new(() => strings.Weapon, () => string.Format(strings.FillEntry, $"{strings.Weapon} {strings.EmptyWeaponSlots}"), () => SetGroupWeapon(Item as Weapon, false)),
                new(() => strings.Stat, () => string.Format(strings.FillEntry, $"{strings.Stat} {strings.EmptyWeaponSlots}"), () => SetGroupStat(Stat, false)),
                new(() => strings.Sigil, () => string.Format(strings.FillEntry, $"{strings.Sigils} {strings.EmptyWeaponSlots}"), () => SetGroupSigil(Sigil1, false)),
                new(() => strings.Infusions, () => string.Format(strings.FillEntry, $"{strings.Infusions} {strings.EmptyWeaponSlots}"), () => SetGroupInfusion(Infusion1, false)),
                });

            CreateSubMenu(() => strings.Override, () => string.Format(strings.OverrideEntry, $"{strings.Weapon}, {strings.Stat}, {strings.Sigils} {strings.And} {strings.Infusions} {strings.WeaponSlots}"), () =>
            {
                SetGroupWeapon(Item as Weapon, true);
                SetGroupStat(Stat, true);
                SetGroupSigil(Sigil1, true);
                SetGroupInfusion(Infusion1, true);
            }, new()
            {
                new(() => strings.Weapon, () => string.Format(strings.OverrideEntry, $"{strings.Weapons} {strings.WeaponSlots}"), () => SetGroupWeapon(Item as Weapon, true)),
                new(() => strings.Stat, () => string.Format(strings.OverrideEntry, $"{strings.Stat} {strings.WeaponSlots}"), () => SetGroupStat(Stat, true)),
                new(() => strings.Sigil, () => string.Format(strings.FillEntry, $"{strings.Sigils} {strings.WeaponSlots}"), () => SetGroupSigil(Sigil1, true)),
                new(() => strings.Infusions, () => string.Format(strings.OverrideEntry, $"{strings.Infusions} {strings.WeaponSlots}"), () => SetGroupInfusion(Infusion1, true)),
                });

            CreateSubMenu(() => string.Format(strings.ResetAll, strings.Weapons), () => string.Format(strings.ResetEntry, $"{strings.Weapons}, {strings.Stats} , {strings.Sigils} {strings.And} {strings.Infusions} {strings.WeaponSlots}"), () =>
            {
                SetGroupWeapon(null, true);
                SetGroupStat(null, true);
                SetGroupSigil(null, true);
                SetGroupInfusion(null, true);
            }, new()
            {
                new(() => strings.Weapons, () => string.Format(strings.ResetAll, $"{strings.Weapons} {strings.WeaponSlots}"), () => SetGroupWeapon(null, true)),
                new(() => strings.Stats, () => string.Format(strings.ResetAll, $"{strings.Stats} {strings.WeaponSlots}"), () => SetGroupStat(null, true)),
                new(() => strings.Sigils, () => string.Format(strings.ResetAll, $"{strings.Sigils} {strings.WeaponSlots}"), () => SetGroupSigil(null, true)),
                new(() => strings.Infusions, () => string.Format(strings.ResetAll, $"{strings.Infusions} {strings.WeaponSlots}"), () => SetGroupInfusion(null, true) ),
                });
        }

        protected override void OnItemChanged(object sender, Core.Models.ValueChangedEventArgs<BaseItem> e)
        {
            base.OnItemChanged(sender, e);

            if (TemplatePresenter?.Template[Slot] is AquaticWeaponTemplateEntry entry)
                entry.Weapon = Item as Weapon;
        }

        private void OnStatChanged(object sender, Core.Models.ValueChangedEventArgs<Stat> e)
        {
            _statTexture.Texture = Stat?.Icon;
            ItemControl.Stat = Stat;

            if (TemplatePresenter?.Template[Slot] is AquaticWeaponTemplateEntry entry)
                entry.Stat = Stat;
        }

        private void OnSigil2Changed(object sender, Core.Models.ValueChangedEventArgs<Sigil> e)
        {
            _sigil2Control.Item = Sigil2;

            if (TemplatePresenter?.Template[Slot] is AquaticWeaponTemplateEntry entry)
                entry.Sigil2 = Sigil2;
        }

        private void OnSigil1Changed(object sender, Core.Models.ValueChangedEventArgs<Sigil> e)
        {
            _sigil1Control.Item = Sigil1;

            if (TemplatePresenter?.Template[Slot] is AquaticWeaponTemplateEntry entry)
                entry.Sigil1 = Sigil1;
        }

        private void OnInfusion1Changed(object sender, Core.Models.ValueChangedEventArgs<Infusion> e)
        {
            _infusion1Control.Item = Infusion1;

            if (TemplatePresenter?.Template[Slot] is AquaticWeaponTemplateEntry entry)
                entry.Infusion1 = Infusion1;
        }

        private void OnInfusion2Changed(object sender, Core.Models.ValueChangedEventArgs<Infusion> e)
        {
            _infusion2Control.Item = Infusion2;

            if (TemplatePresenter?.Template[Slot] is AquaticWeaponTemplateEntry entry)
                entry.Infusion2 = Infusion2;
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
