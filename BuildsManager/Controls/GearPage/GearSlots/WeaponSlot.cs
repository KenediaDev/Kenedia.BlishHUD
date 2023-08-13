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
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.TemplateEntries;
using static Kenedia.Modules.BuildsManager.Controls.Selection.SelectionPanel;
using System.Linq;
using ItemWeaponType = Gw2Sharp.WebApi.V2.Models.ItemWeaponType;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage.GearSlots
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

        private Stat _stat;
        private Sigil _sigil;
        private Sigil _pvpSigil;
        private Infusion _infusion;

        private Rectangle _sigilBounds;
        private Rectangle _pvpSigilBounds;
        private Rectangle _infusionBounds;

        public WeaponSlot(TemplateSlotType gearSlot, Container parent, TemplatePresenter templatePresenter) : base(gearSlot, parent, templatePresenter)
        {
            _infusionControl.Placeholder.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");

            _sigilControl.Parent = this;
            _pvpSigilControl.Parent = this;
            _infusionControl.Parent = this;

            TemplatePresenter.GameModeChanged += TemplatePresenter_GameModeChanged;
        }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        public Sigil Sigil { get => _sigil; set => Common.SetProperty(ref _sigil, value, OnSigilChanged); }

        public Sigil PvpSigil { get => _pvpSigil; set => Common.SetProperty(ref _pvpSigil, value, OnPvpSigilChanged); }

        public Infusion Infusion { get => _infusion; set => Common.SetProperty(ref _infusion, value, OnInfusionChanged); }

        public WeaponSlot OtherHandSlot { get; set; }
                
        private void TemplatePresenter_GameModeChanged(object sender, Core.Models.ValueChangedEventArgs<GameModeType> e)
        {
            if (e.NewValue == GameModeType.PvP)
            {
                _sigilControl.Visible = false;
                _infusionControl.Visible = false;
                _pvpSigilControl.Visible = true;
            }
            else
            {
                _sigilControl.Visible = true;
                _infusionControl.Visible = true;
                _pvpSigilControl.Visible = false;
            }
        }

        private void SetGroupStat(Stat stat = null, bool overrideExisting = false)
        {
            foreach (var slot in SlotGroup)
            {
                if (slot.Slot is TemplateSlotType.Aquatic or TemplateSlotType.AltAquatic)
                {
                    var entry = TemplatePresenter.Template[slot.Slot] as AquaticWeaponTemplateEntry;
                    entry.Stat = overrideExisting ? stat : entry.Stat ?? stat;
                    (slot as AquaticWeaponSlot).Stat = overrideExisting ? stat : (slot as AquaticWeaponSlot).Stat ?? stat;
                }
                else
                {
                    var entry = TemplatePresenter.Template[slot.Slot] as WeaponTemplateEntry;
                    entry.Stat = overrideExisting ? stat : entry.Stat ?? stat;
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
                    var entry = TemplatePresenter.Template[slot.Slot] as AquaticWeaponTemplateEntry;
                    entry.Sigil1 = overrideExisting ? sigil : entry.Sigil1 ?? sigil;
                    entry.Sigil2 = overrideExisting ? sigil : entry.Sigil2 ?? sigil;
                    (slot as AquaticWeaponSlot).Sigil1 = overrideExisting ? sigil : (slot as AquaticWeaponSlot).Sigil1 ?? sigil;
                    (slot as AquaticWeaponSlot).Sigil2 = overrideExisting ? sigil : (slot as AquaticWeaponSlot).Sigil2 ?? sigil;
                }
                else
                {
                    var entry = TemplatePresenter.Template[slot.Slot] as WeaponTemplateEntry;
                    entry.Sigil = overrideExisting ? sigil : entry.Sigil ?? sigil;
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
                    var entry = TemplatePresenter.Template[slot.Slot] as AquaticWeaponTemplateEntry;
                    entry.Infusion1 = overrideExisting ? infusion : entry.Infusion1 ?? infusion;
                    entry.Infusion2 = overrideExisting ? infusion : entry.Infusion2 ?? infusion;
                    (slot as AquaticWeaponSlot).Infusion1 = overrideExisting ? infusion : (slot as AquaticWeaponSlot).Infusion1 ?? infusion;
                    (slot as AquaticWeaponSlot).Infusion2 = overrideExisting ? infusion : (slot as AquaticWeaponSlot).Infusion2 ?? infusion;
                }
                else
                {
                    var entry = TemplatePresenter.Template[slot.Slot] as WeaponTemplateEntry;
                    entry.Infusion = overrideExisting ? infusion : entry.Infusion ?? infusion;
                    (slot as WeaponSlot).Infusion = overrideExisting ? infusion : (slot as WeaponSlot).Infusion ?? infusion;
                }
            }
        }

        private void SetGroupPvpSigil(Sigil sigil = null, bool overrideExisting = false)
        {
            foreach (var slot in SlotGroup)
            {
                if (slot.Slot is TemplateSlotType.Aquatic or TemplateSlotType.AltAquatic)
                {

                }
                else
                {
                    var entry = TemplatePresenter.Template[slot.Slot] as WeaponTemplateEntry;
                    entry.PvpSigil = overrideExisting ? sigil : entry.PvpSigil ?? sigil;
                    (slot as WeaponSlot).PvpSigil = overrideExisting ? sigil : (slot as WeaponSlot).PvpSigil ?? sigil;
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

        protected override void SetItems(object sender, EventArgs e)
        {
            base.SetItems(sender, e);

            var armor = TemplatePresenter?.Template?[Slot] as WeaponTemplateEntry;

            Infusion = armor?.Infusion;
            Sigil = armor?.Sigil;
            PvpSigil = armor?.PvpSigil;
            Stat = armor?.Stat;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var a = AbsoluteBounds;

            if (ItemControl.MouseOver && TemplatePresenter.IsPve)
            {
                SelectionPanel?.SetAnchor<Stat>(this, new Rectangle(a.Location, Point.Zero).Add(ItemControl.LocalBounds), SelectionTypes.Stats, Slot, GearSubSlotType.None, (stat) =>
                {
                    (TemplatePresenter?.Template[Slot] as WeaponTemplateEntry).Stat = stat;
                    Stat = stat;
                }, (TemplatePresenter?.Template[Slot] as WeaponTemplateEntry).Weapon?.StatChoices ?? BuildsManager.Data.Weapons.Values.FirstOrDefault()?.StatChoices,
                (TemplatePresenter?.Template[Slot] as WeaponTemplateEntry).Weapon?.AttributeAdjustment);
            }

            if (_pvpSigilControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<Sigil>(this, new Rectangle(a.Location, Point.Zero).Add(_pvpSigilControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Sigil, (sigil) =>
                {
                    (TemplatePresenter?.Template[Slot] as WeaponTemplateEntry).PvpSigil = sigil;
                    PvpSigil = sigil;
                });
            }

            if (_sigilControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<Sigil>(this, new Rectangle(a.Location, Point.Zero).Add(_sigilControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Sigil, (sigil) =>
                {
                    (TemplatePresenter?.Template[Slot] as WeaponTemplateEntry).Sigil = sigil;
                    Sigil = sigil;
                });
            }

            if (_infusionControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<Infusion>(this, new Rectangle(a.Location, Point.Zero).Add(_infusionControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) =>
                {
                    (TemplatePresenter?.Template[Slot] as WeaponTemplateEntry).Infusion = infusion;
                    Infusion = infusion;
                });
            }

            if (_changeWeaponTexture.Hovered || (ItemControl.MouseOver && TemplatePresenter.IsPvp))
            {
                SelectionPanel?.SetAnchor<Weapon>(this, new Rectangle(a.Location, Point.Zero).Add(ItemControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Item, SelectWeapon);
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => "Reset", () => "Reset weapon, stat, sigils and infusion", () =>
            {
                Stat = null;
                Sigil = null;
                PvpSigil = null;
                Infusion = null;
                Item = null;
            }, new()
            {
                new(() => "Weapon", () => "Reset the weapon.", () => Item = null ),
                new(() => "Stat", () => "Reset the stat.", () => Stat = null ),
                new(() => "Sigil", () => "Reset the sigil.", () => Sigil = null ),
                new(() => "PvP Sigil", () => "Reset the PvP sigil.", () => PvpSigil = null ),
                new(() => "Infusion", () => "Reset the infusion.", () => Infusion = null ),
                });

            CreateSubMenu(() => "Fill", () => "Fill the weapon, stat, sigils and infusions of all empty weapon slots", () =>
            {
                SetGroupWeapon(Item as Weapon, false);
                SetGroupStat(Stat, false);
                SetGroupSigil(Sigil, false);
                SetGroupPvpSigil(PvpSigil, false);
                SetGroupInfusion(Infusion, false);
            }, new()
            {
                new(() => "Weapon", () => "Fill the weapon for all empty weapon slots.", () => SetGroupWeapon(Item as Weapon, false)),
                new(() => "Stat", () => "Fill all empty stat slots.", () => SetGroupStat(Stat, false)),
                new(() => "Sigil", () => "Fill all empty sigil slots.", () => SetGroupSigil(Sigil, false)),
                new(() => "PvP Sigil", () => "Fill all empty PvP sigil slots.", () => SetGroupPvpSigil(PvpSigil, false)),
                new(() => "Infusion", () => "Fill all empty infusion slots.", () => SetGroupInfusion(Infusion, false)),
                });

            CreateSubMenu(() => "Override", () => "Override the weapon, stat, sigils and infusions of all weapon slots", () =>
            {
                SetGroupWeapon(Item as Weapon, true);
                SetGroupStat(Stat, true);
                SetGroupSigil(Sigil, true);
                SetGroupPvpSigil(PvpSigil, true);
                SetGroupInfusion(Infusion, true);
            }, new()
            {
                new(() => "Weapon", () => "Override all weapon slots.", () => SetGroupWeapon(Item as Weapon, true)),
                new(() => "Stat", () => "Override all stat slots.", () => SetGroupStat(Stat, true)),
                new(() => "Sigil", () => "Override all sigil slots.", () => SetGroupSigil(Sigil, true)),
                new(() => "PvP Sigil", () => "Override all PvP sigil slots.", () => SetGroupPvpSigil(PvpSigil, true)),
                new(() => "Infusion", () => "Override all infusion slots.", () => SetGroupInfusion(Infusion, true)),
                });

            CreateSubMenu(() => "Reset all weapons", () => "Reset all weapons, stats, sigils and infusions for all weapon slots", () =>
            {
                SetGroupWeapon(null, true);
                SetGroupStat(null, true);
                SetGroupSigil(null, true);
                SetGroupPvpSigil(null, true);
                SetGroupInfusion(null, true);
            }, new()
            {
                new(() => "Weapons", () => "Reset all weapons of all weapon slots.", () => SetGroupWeapon(null, true)),
                new(() => "Stats", () => "Reset all stats of all weapon slots.", () => SetGroupStat(null, true)),
                new(() => "Sigils", () => "Reset all sigils of all weapon slots.", () => SetGroupSigil(null, true)),
                new(() => "PvP Sigils", () => "Reset all PvP sigils of all weapon slots.", () => SetGroupPvpSigil(null, true)),
                new(() => "Infusions", () => "Reset all infusions of all weapon slots.", () => SetGroupInfusion(null, true) ),
                });
        }

        public void SelectWeapon(Weapon item)
        {
            if (item == null)
            {
                (TemplatePresenter?.Template[Slot] as WeaponTemplateEntry).Weapon = item;
                Item = item;
                return;
            }

            if (item.WeaponType is ItemWeaponType.Trident or ItemWeaponType.Speargun or ItemWeaponType.Harpoon)
                return;

            if (item.WeaponType.IsTwoHanded() && Slot is not TemplateSlotType.MainHand and not TemplateSlotType.AltMainHand)
                return;

            var template = TemplatePresenter.Template;
            var otherHand =
                Slot is TemplateSlotType.MainHand ? TemplateSlotType.OffHand :
                Slot is TemplateSlotType.AltMainHand ? TemplateSlotType.AltOffHand :
                Slot is TemplateSlotType.OffHand ? TemplateSlotType.MainHand :
                Slot is TemplateSlotType.AltOffHand ? TemplateSlotType.AltMainHand :
                TemplateSlotType.None;

            if (item.WeaponType.IsTwoHanded() || (Slot is TemplateSlotType.OffHand or TemplateSlotType.AltOffHand && (OtherHandSlot?.Item as Weapon)?.WeaponType.IsTwoHanded() == true))
            {
                if (template[otherHand] is WeaponTemplateEntry offHand)
                {
                    offHand.Weapon = null;
                    if (OtherHandSlot is not null) OtherHandSlot.Item = null;
                }
            }

            (TemplatePresenter?.Template[Slot] as WeaponTemplateEntry).Weapon = item;
            Item = item;
            ItemControl.Item = item;
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
            OtherHandSlot = null;
        }
    }
}
