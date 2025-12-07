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
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.TemplateEntries;
using System.Linq;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.BuildsManager.Services;

namespace Kenedia.Modules.BuildsManager.Controls_Old.GearPage.GearSlots
{
    public class WeaponSlot : GearSlot
    {
        private readonly DetailedTexture _changeWeaponTexture = new(2338896, 2338895)
        {
            TextureRegion = new(4, 4, 24, 24),
            DrawColor = Color.White * 0.5F,
            HoverDrawColor = Color.White,
        };

        private readonly ItemControl _sigilControl = new(new(784324) { TextureRegion = new(38, 38, 52, 52) });
        private readonly ItemControl _pvpSigilControl = new(new(784324) { TextureRegion = new(38, 38, 52, 52) }) { Visible = false };
        private readonly ItemControl _infusionControl = new(new() { TextureRegion = new(38, 38, 52, 52) });
        private Rectangle _sigilBounds;
        private Rectangle _pvpSigilBounds;
        private Rectangle _infusionBounds;

        public WeaponSlot(TemplateSlotType gearSlot, Container parent, TemplatePresenter templatePresenter, Controls.Selection.SelectionPanel selectionPanel, Data data) 
            : base(gearSlot, parent, templatePresenter, selectionPanel, data)
        {
            _infusionControl.Placeholder.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");

            _sigilControl.Parent = this;
            _pvpSigilControl.Parent = this;
            _infusionControl.Parent = this;
        }

        public Stat Stat { get; set => Common.SetProperty(ref field, value, OnStatChanged); }

        public Sigil Sigil { get; set => Common.SetProperty(ref field, value, OnSigilChanged); }

        public Sigil PvpSigil { get; set => Common.SetProperty(ref field, value, OnPvpSigilChanged); }

        public Infusion Infusion { get; set => Common.SetProperty(ref field, value, OnInfusionChanged); }

        private void AdjustForOtherSlot()
        {
            if (Item is Weapon weapon && weapon.WeaponType.IsTwoHanded())
            {
                ItemControl.Opacity = Slot is TemplateSlotType.OffHand or TemplateSlotType.AltOffHand ? 0.5F : 1F;
                return;
            }

            ItemControl.Opacity = 1F;
        }

        protected override void GameModeChanged(object sender, Core.Models.ValueChangedEventArgs<GameModeType> e)
        {
            if (e.NewValue == GameModeType.PvP)
            {
                _sigilControl.Visible = false;
                _infusionControl.Visible = false;
                _pvpSigilControl.Visible = true;
                ItemControl.ShowStat = false;

                if (SelectionPanel?.Anchor == _sigilControl && SelectionPanel.SubSlotType == GearSubSlotType.Sigil)
                {
                    var b = AbsoluteBounds;
                    SelectionPanel?.SetAnchor<Sigil>(_pvpSigilControl, new Rectangle(b.Location, Point.Zero).Add(_pvpSigilControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Sigil, (sigil) => PvpSigil = sigil);
                }
                else
                {
                    base.GameModeChanged(sender, e);
                }
            }
            else
            {
                _sigilControl.Visible = true;
                _infusionControl.Visible = true;
                _pvpSigilControl.Visible = false;
                ItemControl.ShowStat = true;

                if (SelectionPanel?.Anchor == _pvpSigilControl && SelectionPanel.SubSlotType == GearSubSlotType.Sigil)
                {
                    var b = AbsoluteBounds;
                    SelectionPanel?.SetAnchor<Sigil>(_sigilControl, new Rectangle(b.Location, Point.Zero).Add(_pvpSigilControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Sigil, (sigil) => Sigil = sigil);
                }
                else
                {
                    base.GameModeChanged(sender, e);
                }
            }
        }

        private void SetGroupPvpSigil(Sigil sigil = null, bool overrideExisting = false)
        {
            TemplatePresenter.Template?.SetGroup(Slot, TemplateSubSlotType.PvpSigil, sigil, overrideExisting);
        }

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
            int iconPadding = Slot is TemplateSlotType.OffHand or TemplateSlotType.AltOffHand ? 7 : 0;
            int textPadding = Slot is TemplateSlotType.OffHand or TemplateSlotType.AltOffHand ? 8 : 5;

            int pvpUpgradeSize = 48;
            int size = Math.Min(Width, Height);
            int padding = 2;
            _changeWeaponTexture.Bounds = new(new(ItemControl.LocalBounds.Left + padding, padding), new((int)((size - (padding * 2)) / 2.5)));

            _sigilControl.SetBounds(new(ItemControl.Right + padding, 0, upgradeSize, upgradeSize));
            _infusionControl.SetBounds(new(ItemControl.Right + padding, ItemControl.Bottom - upgradeSize, upgradeSize, upgradeSize));

            _pvpSigilControl.SetBounds(new(ItemControl.LocalBounds.Right + 2 + 5 + iconPadding, (ItemControl.LocalBounds.Height - pvpUpgradeSize) / 2, pvpUpgradeSize, pvpUpgradeSize));

            _pvpSigilBounds = new(_pvpSigilControl.Right + 10, _pvpSigilControl.Top, Width - (_pvpSigilControl.Right + 2), _pvpSigilControl.Height);

            int x = _sigilControl.Right + textPadding + 4;
            _sigilBounds = new(x, _sigilControl.Top - 1, Width - x, _sigilControl.Height);
            _infusionBounds = new(x, _infusionControl.Top, Width - x, _infusionControl.Height);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            if (TemplatePresenter.IsPve != false)
            {
                _changeWeaponTexture.Draw(this, spriteBatch, RelativeMousePosition);

                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Sigil?.DisplayText ?? string.Empty), UpgradeFont, _sigilBounds, UpgradeColor, false, HorizontalAlignment.Left, VerticalAlignment.Middle);
                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Infusion?.DisplayText ?? string.Empty), InfusionFont, _infusionBounds, InfusionColor, true, HorizontalAlignment.Left, VerticalAlignment.Middle);
            }
            else if (TemplatePresenter.IsPvp)
            {
                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(PvpSigil?.DisplayText ?? string.Empty), UpgradeFont, _pvpSigilBounds, UpgradeColor, false, HorizontalAlignment.Left, VerticalAlignment.Middle);
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

            if (TemplatePresenter?.Template?[Slot] is WeaponTemplateEntry weapon)
            {
                Item = weapon?.Weapon;
                Infusion = weapon?.Infusion1;
                Sigil = weapon?.Sigil1;
                PvpSigil = weapon?.PvpSigil;
                Stat = weapon?.Stat;

                AdjustForOtherSlot();
            }
            else
            {
                Item = null;
                Infusion = null;
                Sigil = null;
                PvpSigil = null;
                Stat = null;
            }
        }

        protected override void SetAnchor()
        {

            var a = AbsoluteBounds;
            var entry = Slot.IsOffhand() ? TemplatePresenter?.Template?[Slot] is WeaponTemplateEntry weapon ? weapon : null : null;

            if (ItemControl.MouseOver && TemplatePresenter.IsPve && entry?.Weapon?.WeaponType.IsTwoHanded() is not true)
            {
                SelectionPanel?.SetAnchor<Stat>(ItemControl, new Rectangle(a.Location, Point.Zero).Add(ItemControl.LocalBounds), SelectionTypes.Stats, Slot, GearSubSlotType.None,
                    (stat) => TemplatePresenter?.Template?.SetItem(Slot, TemplateSubSlotType.Stat, stat),
                    (TemplatePresenter?.Template[Slot] as WeaponTemplateEntry)?.Weapon?.StatChoices ?? Data.Weapons.Values.FirstOrDefault()?.StatChoices ?? [],
                    (TemplatePresenter?.Template[Slot] as WeaponTemplateEntry)?.Weapon?.AttributeAdjustment);
            }

            if (_pvpSigilControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<Sigil>(_pvpSigilControl, new Rectangle(a.Location, Point.Zero).Add(_pvpSigilControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Sigil, (sigil) => TemplatePresenter?.Template?.SetItem(Slot, TemplateSubSlotType.PvpSigil, sigil));
            }

            if (_sigilControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<Sigil>(_sigilControl, new Rectangle(a.Location, Point.Zero).Add(_sigilControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Sigil, (sigil) => TemplatePresenter.Template?.SetItem(Slot, TemplateSubSlotType.Sigil1, sigil));
            }

            if (_infusionControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<Infusion>(_infusionControl, new Rectangle(a.Location, Point.Zero).Add(_infusionControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) => TemplatePresenter.Template?.SetItem(Slot, TemplateSubSlotType.Infusion1, infusion));
            }

            if (_changeWeaponTexture.Hovered || (ItemControl.MouseOver && TemplatePresenter.IsPvp))
            {
                SelectionPanel?.SetAnchor<Weapon>(this, new Rectangle(a.Location, Point.Zero).Add(ItemControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Item, (weapon) => TemplatePresenter.Template?.SetItem(Slot, TemplateSubSlotType.Item, weapon));
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => strings.Reset, () => string.Format(strings.ResetEntry, $"{strings.Weapon}, {strings.Stat}, {strings.Sigils} {strings.And} {strings.Infusion}"), () =>
            {
                TemplatePresenter?.Template.SetItem<Weapon>(Slot, TemplateSubSlotType.Item, null);
                TemplatePresenter?.Template.SetItem<Stat>(Slot, TemplateSubSlotType.Stat, null);
                TemplatePresenter?.Template.SetItem<Sigil>(Slot, TemplateSubSlotType.Sigil1, null);
                TemplatePresenter?.Template.SetItem<Sigil>(Slot, TemplateSubSlotType.PvpSigil, null);
                TemplatePresenter?.Template.SetItem<Infusion>(Slot, TemplateSubSlotType.Infusion1, null);
            },
            [
                new(() => strings.Weapon, () => string.Format(strings.ResetEntry, strings.Weapon), () => TemplatePresenter?.Template.SetItem<Weapon>(Slot, TemplateSubSlotType.Item, null)),
                new(() => strings.Stat, () => string.Format(strings.ResetEntry, strings.Stat), () => TemplatePresenter?.Template.SetItem<Stat>(Slot, TemplateSubSlotType.Stat, null)),
                new(() => strings.Sigil, () => string.Format(strings.ResetEntry, strings.Sigil), () => TemplatePresenter?.Template.SetItem<Sigil>(Slot, TemplateSubSlotType.Sigil1, null)),
                new(() => strings.PvpSigil, () => string.Format(strings.ResetEntry, strings.PvpSigil), () => TemplatePresenter?.Template.SetItem<Sigil>(Slot, TemplateSubSlotType.PvpSigil, null)),
                new(() => strings.Infusion, () => string.Format(strings.ResetEntry, strings.Infusion), () => TemplatePresenter?.Template.SetItem<Infusion>(Slot, TemplateSubSlotType.Infusion1, null)),
                ]);

            CreateSubMenu(() => strings.Fill, () => string.Format(strings.FillEntry, $"{strings.Weapon}, {strings.Stat}, {strings.Sigils} {strings.And} {strings.Infusion} {strings.EmptyWeaponSlots}"), () =>
                 {
                     SetGroupWeapon(Item as Weapon, false);
                     SetGroupStat(Stat, false);
                     SetGroupSigil(Sigil, false);
                     SetGroupPvpSigil(PvpSigil, false);
                     SetGroupInfusion(Infusion, false);
                 },
                 [
                new(() => strings.Weapon, () => string.Format(strings.FillEntry, $"{strings.Weapon} {strings.EmptyWeaponSlots}"), () => SetGroupWeapon(Item as Weapon, false)),
                new(() => strings.Stat, () => string.Format(strings.FillEntry, $"{strings.Stat} {strings.EmptyWeaponSlots}"), () => SetGroupStat(Stat, false)),
                new(() => strings.Sigil, () => string.Format(strings.FillEntry, $"{strings.Sigil} {strings.EmptyWeaponSlots}"), () => SetGroupSigil(Sigil, false)),
                new(() => strings.PvpSigil, () => string.Format(strings.FillEntry, $"{strings.PvpSigil} {strings.EmptyWeaponSlots}"), () => SetGroupPvpSigil(PvpSigil, false)),
                new(() => strings.Infusion, () => string.Format(strings.FillEntry, $"{strings.Infusion} {strings.EmptyWeaponSlots}"), () => SetGroupInfusion(Infusion, false)),
                     ]);

            CreateSubMenu(() => strings.Override, () => string.Format(strings.OverrideEntry, $"{strings.Weapon}, {strings.Stat}, {strings.Sigils} {strings.And} {strings.Infusion} {strings.WeaponSlots}"), () =>
            {
                SetGroupWeapon(Item as Weapon, true);
                SetGroupStat(Stat, true);
                SetGroupSigil(Sigil, true);
                SetGroupPvpSigil(PvpSigil, true);
                SetGroupInfusion(Infusion, true);
            },
            [
                new(() => strings.Weapon, () => string.Format(strings.OverrideEntry, $"{strings.Weapons} {strings.WeaponSlots}"), () => SetGroupWeapon(Item as Weapon, true)),
                new(() => strings.Stat, () => string.Format(strings.OverrideEntry, $"{strings.Stat} {strings.WeaponSlots}"), () => SetGroupStat(Stat, true)),
                new(() => strings.Sigil, () => string.Format(strings.OverrideEntry, $"{strings.Sigil} {strings.WeaponSlots}"), () => SetGroupSigil(Sigil, true)),
                new(() => strings.PvpSigil, () => string.Format(strings.OverrideEntry, $"{strings.PvpSigil} {strings.WeaponSlots}"), () => SetGroupPvpSigil(PvpSigil, true)),
                new(() => strings.Infusion, () => string.Format(strings.OverrideEntry, $"{strings.Infusion} {strings.WeaponSlots}"), () => SetGroupInfusion(Infusion, true)),
                ]);

            CreateSubMenu(() => string.Format(strings.ResetAll, strings.Weapons), () => string.Format(strings.ResetEntry, $"{strings.Weapons}, {strings.Stats} , {strings.Sigils} {strings.And} {strings.Infusions} {strings.WeaponSlots}"), () =>
            {
                SetGroupWeapon(null, true);
                SetGroupStat(null, true);
                SetGroupSigil(null, true);
                SetGroupPvpSigil(null, true);
                SetGroupInfusion(null, true);
            },
            [
                new(() => strings.Weapons, () => string.Format(strings.ResetAll, $"{strings.Weapons} {strings.WeaponSlots}"), () => SetGroupWeapon(null, true)),
                new(() => strings.Stats, () => string.Format(strings.ResetAll, $"{strings.Stats} {strings.WeaponSlots}"), () => SetGroupStat(null, true)),
                new(() => strings.Sigils, () => string.Format(strings.ResetAll, $"{strings.Sigils} {strings.WeaponSlots}"), () => SetGroupSigil(null, true)),
                new(() => strings.PvpSigils, () => string.Format(strings.ResetAll, $"{strings.PvpSigils} {strings.WeaponSlots}"), () => SetGroupPvpSigil(null, true)),
                new(() => strings.Infusions, () => string.Format(strings.ResetAll, $"{strings.Infusions} {strings.WeaponSlots}"), () => SetGroupInfusion(null, true) ),
                ]);
        }

        private void OnStatChanged(object sender, Core.Models.ValueChangedEventArgs<Stat> e)
        {
            ItemControl.Stat = Stat;
        }

        private void OnSigilChanged(object sender, Core.Models.ValueChangedEventArgs<Sigil> e)
        {
            _sigilControl.Item = Sigil;
        }

        private void OnPvpSigilChanged(object sender, Core.Models.ValueChangedEventArgs<Sigil> e)
        {
            _pvpSigilControl.Item = PvpSigil;
        }

        private void OnInfusionChanged(object sender, Core.Models.ValueChangedEventArgs<Infusion> e)
        {
            _infusionControl.Item = Infusion;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Stat = null;
            Sigil = null;
            PvpSigil = null;
            Infusion = null;
        }
    }
}
