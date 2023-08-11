using Container = Blish_HUD.Controls.Container;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Blish_HUD.Content;
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
using static Kenedia.Modules.BuildsManager.Controls.Selection.SelectionPanel;
using ItemWeaponType = Gw2Sharp.WebApi.V2.Models.ItemWeaponType;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage.GearSlots
{
    public class AquaticWeaponSlot : GearSlot
    {
        private readonly DetailedTexture _sigil1SlotTexture = new() { Texture = AsyncTexture2D.FromAssetId(784324), TextureRegion = new(37, 37, 54, 54), };
        private readonly DetailedTexture _sigil2SlotTexture = new() { Texture = AsyncTexture2D.FromAssetId(784324), TextureRegion = new(37, 37, 54, 54), };
        private readonly DetailedTexture _infusion1SlotTexture = new() { TextureRegion = new(37, 37, 54, 54) };
        private readonly DetailedTexture _infusion2SlotTexture = new() { TextureRegion = new(37, 37, 54, 54) };

        private readonly DetailedTexture _changeWeaponTexture = new(2338896, 2338895)
        {
            TextureRegion = new(4, 4, 24, 24),
            DrawColor = Color.White * 0.5F,
            HoverDrawColor = Color.White,
        };

        private readonly DetailedTexture _statTexture = new() { };

        private readonly ItemTexture _sigil1Texture = new() { };
        private readonly ItemTexture _sigil2Texture = new() { };

        private readonly ItemTexture _infusion1Texture = new() { };
        private readonly ItemTexture _infusion2Texture = new() { };

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
            _infusion1SlotTexture.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
            _infusion2SlotTexture.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
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

            int upgradeSize = (Icon.Bounds.Size.Y - 4) / 2;
            int iconPadding = 0;
            int textPadding = Slot is TemplateSlotType.AquaBreather ? upgradeSize + 5 : 5;

            int size = Math.Min(Width, Height);
            int padding = 3;
            _statTexture.Bounds = new(Icon.Bounds.Center.Add(new Point(-padding, -padding)), new((size - (padding * 2)) / 2));
            _changeWeaponTexture.Bounds = new(new(Icon.Bounds.Left + padding, padding), new((int)((size - (padding * 2)) / 2.5)));

            _sigil1SlotTexture.Bounds = new(Icon.Bounds.Right + 2 + iconPadding, 0, upgradeSize, upgradeSize);
            _sigil1Texture.Bounds = _sigil1SlotTexture.Bounds;

            _sigil2SlotTexture.Bounds = new(Icon.Bounds.Right + 2 + iconPadding + upgradeSize + 2, 0, upgradeSize, upgradeSize);
            _sigil2Texture.Bounds = _sigil2SlotTexture.Bounds;

            _infusion1SlotTexture.Bounds = new(Icon.Bounds.Right + 2 + iconPadding, _sigil1SlotTexture.Bounds.Bottom + 4, upgradeSize, upgradeSize);
            _infusion1Texture.Bounds = _infusion1SlotTexture.Bounds;

            _infusion2SlotTexture.Bounds = new(Icon.Bounds.Right + 2 + iconPadding + upgradeSize + 2, _sigil1SlotTexture.Bounds.Bottom + 4, upgradeSize, upgradeSize);
            _infusion2Texture.Bounds = _infusion2SlotTexture.Bounds;

            int upgradeWidth = (Width - (_sigil2SlotTexture.Bounds.Right + 2)) / 2;
            int x = _sigil2SlotTexture.Bounds.Right + textPadding + 4;
            _sigil1Bounds = new(x, _sigil1SlotTexture.Bounds.Top, upgradeWidth, _sigil1SlotTexture.Bounds.Height);
            _sigil2Bounds = new(x + upgradeWidth, _sigil1SlotTexture.Bounds.Top, upgradeWidth, _sigil1SlotTexture.Bounds.Height);

            _infusion1Bounds = new(x, _infusion1SlotTexture.Bounds.Top, upgradeWidth, _infusion1SlotTexture.Bounds.Height);
            _infusion2Bounds = new(x + upgradeWidth, _infusion1SlotTexture.Bounds.Top, upgradeWidth, _infusion1SlotTexture.Bounds.Height);
        }

        public void SelectWeapon(Weapon item)
        {
            if (item == null || item.WeaponType is ItemWeaponType.Harpoon or ItemWeaponType.Speargun or ItemWeaponType.Trident)
            {
                (TemplatePresenter?.Template[Slot] as AquaticWeaponTemplateEntry).Weapon = item;
                Item = item;
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (TemplatePresenter.IsPve != false)
            {
                base.Paint(spriteBatch, bounds);
                _statTexture.Draw(this, spriteBatch);
                _changeWeaponTexture.Draw(this, spriteBatch, RelativeMousePosition);
                _sigil1SlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
                _sigil2SlotTexture.Draw(this, spriteBatch, RelativeMousePosition);

                _sigil1Texture.Draw(this, spriteBatch, RelativeMousePosition);
                _sigil2Texture.Draw(this, spriteBatch, RelativeMousePosition);

                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Sigil1?.DisplayText ?? string.Empty), UpgradeFont, _sigil1Bounds, UpgradeColor, false, HorizontalAlignment.Left, VerticalAlignment.Middle);
                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Sigil2?.DisplayText ?? string.Empty), UpgradeFont, _sigil2Bounds, UpgradeColor, false, HorizontalAlignment.Left, VerticalAlignment.Middle);

                _infusion1SlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
                _infusion2SlotTexture.Draw(this, spriteBatch, RelativeMousePosition);

                _infusion1Texture.Draw(this, spriteBatch, RelativeMousePosition);
                _infusion2Texture.Draw(this, spriteBatch, RelativeMousePosition);

                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Infusion1?.DisplayText ?? string.Empty), InfusionFont, _infusion1Bounds, InfusionColor, true, HorizontalAlignment.Left, VerticalAlignment.Middle);
                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Infusion2?.DisplayText ?? string.Empty), InfusionFont, _infusion2Bounds, InfusionColor, true, HorizontalAlignment.Left, VerticalAlignment.Middle);
            }
        }

        protected override void SetItems(object sender, EventArgs e)
        {
            base.SetItems(sender, e);

            var armor = TemplatePresenter.Template[Slot] as AquaticWeaponTemplateEntry;

            Infusion1 = armor?.Infusion1;
            Infusion2 = armor?.Infusion2;
            Sigil1 = armor?.Sigil1;
            Sigil2 = armor?.Sigil2;

            Stat = armor?.Stat;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var a = AbsoluteBounds;

            if (Icon.Hovered)
            {
                SelectionPanel?.SetAnchor<Stat>(this, new Rectangle(a.Location, Point.Zero).Add(Icon.Bounds), SelectionTypes.Stats, Slot, GearSubSlotType.None, (stat) =>
                {
                    (TemplatePresenter?.Template[Slot] as AquaticWeaponTemplateEntry).Stat = stat;
                    Stat = stat;
                },
                (TemplatePresenter?.Template[Slot] as AquaticWeaponTemplateEntry).Weapon?.StatChoices,
                (TemplatePresenter?.Template[Slot] as AquaticWeaponTemplateEntry).Weapon?.AttributeAdjustment);
            }

            if (_sigil1SlotTexture.Hovered)
            {
                SelectionPanel?.SetAnchor<Sigil>(this, new Rectangle(a.Location, Point.Zero).Add(_sigil1SlotTexture.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Sigil, (sigil) =>
                {
                    (TemplatePresenter?.Template[Slot] as AquaticWeaponTemplateEntry).Sigil1 = sigil;
                    Sigil1 = sigil;
                });
            }

            if (_infusion1SlotTexture.Hovered)
            {
                SelectionPanel?.SetAnchor<Infusion>(this, new Rectangle(a.Location, Point.Zero).Add(_infusion1SlotTexture.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) =>
                {
                    (TemplatePresenter?.Template[Slot] as AquaticWeaponTemplateEntry).Infusion1 = infusion;
                    Infusion1 = infusion;
                });
            }

            if (_sigil2SlotTexture.Hovered)
            {
                SelectionPanel?.SetAnchor<Sigil>(this, new Rectangle(a.Location, Point.Zero).Add(_sigil2SlotTexture.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Sigil, (sigil) =>
                {
                    (TemplatePresenter?.Template[Slot] as AquaticWeaponTemplateEntry).Sigil2 = sigil;
                    Sigil2 = sigil;
                });
            }

            if (_infusion2SlotTexture.Hovered)
            {
                SelectionPanel?.SetAnchor<Infusion>(this, new Rectangle(a.Location, Point.Zero).Add(_infusion2SlotTexture.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) =>
                {
                    (TemplatePresenter?.Template[Slot] as AquaticWeaponTemplateEntry).Infusion2 = infusion;
                    Infusion2 = infusion;
                });
            }

            if (_changeWeaponTexture.Hovered)
            {
                SelectionPanel?.SetAnchor<Weapon>(this, new Rectangle(a.Location, Point.Zero).Add(Icon.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Item, (item) =>
                {
                    (TemplatePresenter?.Template[Slot] as AquaticWeaponTemplateEntry).Weapon = item;
                    Item = item;
                });
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => "Reset", () => "Reset weapon, stat, sigils and infusions", () =>
            {
                Stat = null;
                Sigil1 = null;
                Sigil2 = null;
                Infusion1 = null;
                Item = null;
            }, new()
            {
                new(() => "Weapon", () => "Reset the weapon.", () => Item = null ),
                new(() => "Stat", () => "Reset the stat.", () => Stat = null ),
                new(() => "Sigil", () => "Reset the sigils.", () => {
                    Sigil1 = null;
                    Sigil2 = null;
                }),
                new(() => "Infusion", () => "Reset the infusions.", () => {
                    Infusion1 = null;
                } ),
                });

            CreateSubMenu(() => "Fill", () => "Fill the weapon, stat, sigils and infusions of all empty weapon slots", () =>
            {
                SetGroupWeapon(Item as Weapon, false);
                SetGroupStat(Stat, false);
                SetGroupSigil(Sigil1, false);
                SetGroupInfusion(Infusion1, false);
            }, new()
            {
                new(() => "Weapon", () => "Fill the weapon for all empty weapon slots.", () => SetGroupWeapon(Item as Weapon, false)),
                new(() => "Stat", () => "Fill all empty stat slots.", () => SetGroupStat(Stat, false)),
                new(() => "Sigil", () => "Fill all empty sigil slots.", () => SetGroupSigil(Sigil1, false)),
                new(() => "Infusion", () => "Fill all empty infusion slots.", () => SetGroupInfusion(Infusion1, false)),
                });

            CreateSubMenu(() => "Override", () => "Override the weapon, stat, sigils and infusions of all weapon slots", () =>
            {
                SetGroupWeapon(Item as Weapon, true);
                SetGroupStat(Stat, true);
                SetGroupSigil(Sigil1, true);
                SetGroupInfusion(Infusion1, true);
            }, new()
            {
                new(() => "Weapon", () => "Override all weapon slots.", () => SetGroupWeapon(Item as Weapon, true)),
                new(() => "Stat", () => "Override all stat slots.", () => SetGroupStat(Stat, true)),
                new(() => "Sigil", () => "Override all sigil slots.", () => SetGroupSigil(Sigil1, true)),
                new(() => "Infusion", () => "Override all infusion slots.", () => SetGroupInfusion(Infusion1, true)),
                });

            CreateSubMenu(() => "Reset all weapons", () => "Reset all weapons, stats, sigils and infusions for all weapon slots", () =>
            {
                SetGroupWeapon(null, true);
                SetGroupStat(null, true);
                SetGroupSigil(null, true);
                SetGroupInfusion(null, true);
            }, new()
            {
                new(() => "Weapons", () => "Reset all weapons of all weapon slots.", () => SetGroupWeapon(null, true)),
                new(() => "Stats", () => "Reset all stats of all weapon slots.", () => SetGroupStat(null, true)),
                new(() => "Sigils", () => "Reset all sigils of all weapon slots.", () => SetGroupSigil(null, true)),
                new(() => "Infusions", () => "Reset all infusions of all weapon slots.", () => SetGroupInfusion(null, true) ),
                });
        }

        private void OnStatChanged(object sender, Core.Models.ValueChangedEventArgs<Stat> e)
        {
            _statTexture.Texture = Stat?.Icon;
        }

        private void OnSigil2Changed(object sender, Core.Models.ValueChangedEventArgs<Sigil> e)
        {
            _sigil2Texture.Texture = Sigil2?.Icon;
        }

        private void OnSigil1Changed(object sender, Core.Models.ValueChangedEventArgs<Sigil> e)
        {
            _sigil1Texture.Texture = Sigil1?.Icon;
        }

        private void OnInfusion1Changed(object sender, Core.Models.ValueChangedEventArgs<Infusion> e)
        {
            _infusion1Texture.Texture = Infusion1?.Icon;
        }

        private void OnInfusion2Changed(object sender, Core.Models.ValueChangedEventArgs<Infusion> e)
        {
            _infusion2Texture.Texture = Infusion2?.Icon;
        }
    }
}
