using Control = Blish_HUD.Controls.Control;
using Container = Blish_HUD.Controls.Container;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Blish_HUD.Content;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using Blish_HUD;
using Blish_HUD.Input;
using System.Diagnostics;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using MonoGame.Extended.BitmapFonts;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.BuildsManager.Controls.Selection;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.BuildsManager.TemplateEntries;
using static Kenedia.Modules.BuildsManager.Controls.Selection.SelectionPanel;
using System.Linq;
using ItemWeaponType = Gw2Sharp.WebApi.V2.Models.ItemWeaponType;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage.GearSlots
{
    public class GearSlot : Control
    {
        protected readonly DetailedTexture Icon = new() { TextureRegion = new(37, 37, 54, 54) };
        private TemplateSlot _slot = TemplateSlot.None;

        protected int MaxTextLength = 52;
        protected Color StatColor = Color.White;
        protected Color UpgradeColor = Color.Orange;
        protected Color InfusionColor = new(153, 238, 221);
        protected Color ItemColor = Color.Gray;

        protected BitmapFont StatFont = Content.DefaultFont16;
        protected BitmapFont UpgradeFont = Content.DefaultFont18;
        protected BitmapFont InfusionFont = Content.DefaultFont12;

        protected TemplatePresenter TemplatePresenter;

        protected ItemTexture ItemTexture { get; } = new() { };

        public BaseItem Item { get => ItemTexture.Item; set => ItemTexture.Item = value; }

        public TemplateSlot Slot { get => _slot; set => Common.SetProperty(ref _slot, value, ApplySlot); }

        public GearSlot(TemplateSlot gearSlot, Container parent, TemplatePresenter templatePresenter)
        {
            TemplatePresenter = templatePresenter;
            Slot = gearSlot;
            Parent = parent;

            Size = new(380, 64);
            ClipsBounds = true;

            Menu = new();
            CreateSubMenus();

            TemplatePresenter.LoadedGearFromCode += SetItems;
            TemplatePresenter.TemplateChanged += SetItems;
        }

        public SelectionPanel SelectionPanel { get; set; }

        public List<GearSlot> SlotGroup { get; set; }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            Icon.Draw(this, spriteBatch, RelativeMousePosition);
            ItemTexture.Draw(this, spriteBatch, RelativeMousePosition, ItemColor);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            //ClickAction?.Invoke();
        }

        protected virtual void ApplySlot()
        {
            var assetIds = new Dictionary<TemplateSlot, int>()
            {
                { TemplateSlot.AquaBreather, 156308},
                { TemplateSlot.Head, 156307},
                { TemplateSlot.Shoulder, 156311},
                { TemplateSlot.Chest, 156297},
                { TemplateSlot.Hand, 156306},
                { TemplateSlot.Leg, 156309},
                { TemplateSlot.Foot, 156300},
                { TemplateSlot.MainHand, 156316},
                { TemplateSlot.OffHand, 156320},
                { TemplateSlot.Aquatic, 156313},
                { TemplateSlot.AltMainHand, 156316},
                { TemplateSlot.AltOffHand, 156320},
                { TemplateSlot.AltAquatic, 156313},
                { TemplateSlot.Back, 156293},
                { TemplateSlot.Amulet, 156310},
                { TemplateSlot.Accessory_1, 156298},
                { TemplateSlot.Accessory_2, 156299},
                { TemplateSlot.Ring_1, 156301},
                { TemplateSlot.Ring_2, 156302},
                { TemplateSlot.PvpAmulet, 784322},
            };

            if (assetIds.TryGetValue(Slot, out int assetId))
            {
                Icon.Texture = AsyncTexture2D.FromAssetId(assetId);
            }

            RecalculateLayout();
        }

        protected string GetDisplayString(string s)
        {
            return s.Length > MaxTextLength ? s.Substring(0, MaxTextLength) + "..." : s;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int size = Math.Min(Width, Height);
            Icon.Bounds = new(0, 0, size, size);
            ItemTexture.Bounds = new(0, 0, size, size);
        }

        protected virtual void SetItems(object sender, EventArgs e)
        {

        }

        protected void CreateSubMenu(Func<string> menuGroupName, Func<string> menuGroupTooltip = null, Action menuGroupAction = null, List<(Func<string> text, Func<string> tooltip, Action action)> menuItems = null)
        {
            if (menuItems == null)
            {
                _ = Menu.AddMenuItem(new ContextMenuItem(menuGroupName, menuGroupAction, menuGroupTooltip));
                return;
            }

            var menuGroup = Menu.AddMenuItem(new ContextMenuItem(menuGroupName, menuGroupAction, menuGroupTooltip)).Submenu = new();

            foreach (var (text, tooltip, action) in menuItems ?? new())
            {
                _ = menuGroup.AddMenuItem(new ContextMenuItem(text, action, tooltip));
            }
        }

        protected virtual void CreateSubMenus()
        {

        }
    }

    public class ArmorSlot : GearSlot
    {
        private readonly DetailedTexture _runeSlotTexture = new() { Texture = AsyncTexture2D.FromAssetId(784323), TextureRegion = new(37, 37, 54, 54), };
        private readonly DetailedTexture _infusionSlotTexture = new() { TextureRegion = new(37, 37, 54, 54) };

        private readonly DetailedTexture _statTexture = new() { };

        private readonly ItemTexture _runeTexture = new() { };
        private readonly ItemTexture _infusionTexture = new() { };

        private Stat _stat;
        private Rune _rune;
        private Infusion _infusion;

        private Rectangle _runeBounds;
        private Rectangle _infusionBounds;

        public ArmorSlot(TemplateSlot gearSlot, Container parent, TemplatePresenter templatePresenter) : base(gearSlot, parent, templatePresenter)
        {
            _infusionSlotTexture.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
        }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        public Rune Rune { get => _rune; set => Common.SetProperty(ref _rune, value, OnRuneChanged); }

        public Infusion Infusion { get => _infusion; set => Common.SetProperty(ref _infusion, value, OnInfusionChanged); }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int upgradeSize = (Icon.Bounds.Size.Y - 4) / 2;
            int iconPadding = 0;
            int textPadding = Slot is TemplateSlot.AquaBreather ? upgradeSize + 5 : 5;

            int size = Math.Min(Width, Height);
            int padding = 3;
            _statTexture.Bounds = new(Icon.Bounds.Center.Add(new Point(-padding, -padding)), new((size - (padding * 2)) / 2));

            _runeSlotTexture.Bounds = new(Icon.Bounds.Right + 2 + iconPadding, 0, upgradeSize, upgradeSize);
            _runeTexture.Bounds = _runeSlotTexture.Bounds;

            _infusionSlotTexture.Bounds = new(Icon.Bounds.Right + 2 + iconPadding, _runeSlotTexture.Bounds.Bottom + 4, upgradeSize, upgradeSize);
            _infusionTexture.Bounds = _infusionSlotTexture.Bounds;

            int x = _runeSlotTexture.Bounds.Right + textPadding + 4;
            _runeBounds = new(x, _runeSlotTexture.Bounds.Top - 1, Width - x, _runeSlotTexture.Bounds.Height);
            _infusionBounds = new(x, _infusionSlotTexture.Bounds.Top, Width - x, _infusionSlotTexture.Bounds.Height);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (TemplatePresenter.IsPve != false)
            {
                base.Paint(spriteBatch, bounds);
                _statTexture.Draw(this, spriteBatch);
                _runeSlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
                _runeTexture.Draw(this, spriteBatch, RelativeMousePosition);
                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Rune?.DisplayText ?? string.Empty), UpgradeFont, _runeBounds, UpgradeColor, false, HorizontalAlignment.Left, VerticalAlignment.Middle);

                _infusionSlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
                _infusionTexture.Draw(this, spriteBatch, RelativeMousePosition);
                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Infusion?.DisplayText ?? string.Empty), InfusionFont, _infusionBounds, InfusionColor, true, HorizontalAlignment.Left, VerticalAlignment.Middle);
            }
        }

        protected override void SetItems(object sender, EventArgs e)
        {
            base.SetItems(sender, e);

            var armor = TemplatePresenter.Template[Slot] as ArmorTemplateEntry;

            Infusion = armor?.Infusion;
            Rune = armor?.Rune;
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
                    (TemplatePresenter?.Template[Slot] as ArmorTemplateEntry).Stat = stat;
                    Stat = stat;
                },
                (TemplatePresenter?.Template[Slot] as ArmorTemplateEntry).Armor?.StatChoices,
                (TemplatePresenter?.Template[Slot] as ArmorTemplateEntry).Armor?.AttributeAdjustment);
            }

            if (_runeSlotTexture.Hovered)
            {
                SelectionPanel?.SetAnchor<Rune>(this, new Rectangle(a.Location, Point.Zero).Add(_runeSlotTexture.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Rune, (rune) =>
                {
                    (TemplatePresenter?.Template[Slot] as ArmorTemplateEntry).Rune = rune;
                    Rune = rune;
                });
            }

            if (_infusionSlotTexture.Hovered)
            {
                SelectionPanel?.SetAnchor<Infusion>(this, new Rectangle(a.Location, Point.Zero).Add(_infusionSlotTexture.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) =>
                {
                    (TemplatePresenter?.Template[Slot] as ArmorTemplateEntry).Infusion = infusion;
                    Infusion = infusion;
                });
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => "Reset", () => "Reset stat, rune and infusion", () =>
            {
                Stat = null;
                Rune = null;
                Infusion = null;
            }, new()
            {
                new(() => "Stat", () => "Reset the stat.", () => Stat = null ),
                new(() => "Rune", () => "Reset the rune.", () => Rune = null ),
                new(() => "Infusion", () => "Reset the infusion.", () => Infusion = null ),
                });

            CreateSubMenu(() => "Fill", () => "Fill the stat, rune and infusion of all empty armor slots", () =>
            {
                SetGroupStat(Stat, false);
                SetGroupRune(Rune, false);
                SetGroupInfusion(Infusion, false);
            }, new()
            {
                new(() => "Stat", () => "Fill all empty stat slots.", () => SetGroupStat(Stat, false)),
                new(() => "Rune", () => "Fill all empty rune slots.", () => SetGroupRune(Rune, false)),
                new(() => "Infusion", () => "Fill all empty infusion slots.", () => SetGroupInfusion(Infusion, false)),
                });

            CreateSubMenu(() => "Override", () => "Override the stat, rune and infusion of all armor slots", () =>
            {
                SetGroupStat(Stat, true);
                SetGroupRune(Rune, true);
                SetGroupInfusion(Infusion, true);
            }, new()
            {
                new(() => "Stat", () => "Override all stat slots.", () => SetGroupStat(Stat, true)),
                new(() => "Rune", () => "Override all rune slots.", () => SetGroupRune(Rune, true)),
                new(() => "Infusion", () => "Override all infusion slots.", () => SetGroupInfusion(Infusion, true)),
                });

            CreateSubMenu(() => "Reset all armor", () => "Reset all stats, runes and infusions for all armor slots", () =>
            {
                SetGroupStat(null, true);
                SetGroupRune(null, true);
                SetGroupInfusion(null, true);
            }, new()
            {
                new(() => "Stats", () => "Reset all stats of all armor slots.", () => SetGroupStat(null, true)),
                new(() => "Runes", () => "Reset all runes of all armor slots.", () => SetGroupRune(null, true)),
                new(() => "Infusions", () => "Reset all infusions of all armor slots.", () => SetGroupInfusion(null, true) ),
                });
        }

        private void SetGroupStat(Stat stat, bool overrideExisting)
        {
            foreach (var slot in SlotGroup)
            {
                var armor = slot as ArmorSlot;

                if (overrideExisting || armor.Stat == null)
                {
                    armor.Stat = stat;
                    (TemplatePresenter.Template[armor.Slot] as ArmorTemplateEntry).Stat = stat;
                }
            }
        }

        private void SetGroupRune(Rune rune, bool overrideExisting)
        {
            foreach (var slot in SlotGroup)
            {
                var armor = slot as ArmorSlot;

                if (overrideExisting || armor.Rune == null)
                {
                    armor.Rune = rune;
                    (TemplatePresenter.Template[armor.Slot] as ArmorTemplateEntry).Rune = rune;
                }
            }
        }

        private void SetGroupInfusion(Infusion infusion, bool overrideExisting)
        {
            foreach (var slot in SlotGroup)
            {
                var armor = slot as ArmorSlot;

                if (overrideExisting || armor.Infusion == null)
                {
                    armor.Infusion = infusion;
                    (TemplatePresenter.Template[armor.Slot] as ArmorTemplateEntry).Infusion = infusion;
                }
            }
        }

        private void OnStatChanged(object sender, Core.Models.ValueChangedEventArgs<Stat> e)
        {
            _statTexture.Texture = Stat?.Icon;
        }

        private void OnRuneChanged(object sender, Core.Models.ValueChangedEventArgs<Rune> e)
        {
            _runeTexture.Texture = Rune?.Icon;
        }

        private void OnInfusionChanged(object sender, Core.Models.ValueChangedEventArgs<Infusion> e)
        {
            _infusionTexture.Texture = Infusion?.Icon;
        }
    }

    public class WeaponSlot : GearSlot
    {
        private readonly DetailedTexture _sigilSlotTexture = new() { Texture = AsyncTexture2D.FromAssetId(784324), TextureRegion = new(37, 37, 54, 54), };
        private readonly DetailedTexture _pvpSigilSlotTexture = new() { Texture = AsyncTexture2D.FromAssetId(784324), TextureRegion = new(37, 37, 54, 54), };
        private readonly DetailedTexture _infusionSlotTexture = new() { TextureRegion = new(37, 37, 54, 54) };

        private readonly DetailedTexture _changeWeaponTexture = new(2338896, 2338895)
        {
            TextureRegion = new(4, 4, 24, 24),
            DrawColor = Color.White * 0.5F,
            HoverDrawColor = Color.White,
        };

        private readonly DetailedTexture _statTexture = new() { };

        private readonly ItemTexture _sigilTexture = new() { };
        private readonly ItemTexture _pvpSigilTexture = new() { };
        private readonly ItemTexture _infusionTexture = new() { };

        private Stat _stat;
        private Sigil _sigil;
        private Sigil _pvpSigil;
        private Infusion _infusion;

        private Rectangle _sigilBounds;
        private Rectangle _pvpSigilBounds;
        private Rectangle _infusionBounds;

        public WeaponSlot(TemplateSlot gearSlot, Container parent, TemplatePresenter templatePresenter) : base(gearSlot, parent, templatePresenter)
        {
            _infusionSlotTexture.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
        }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        public Sigil Sigil { get => _sigil; set => Common.SetProperty(ref _sigil, value, OnSigilChanged); }

        public Sigil PvpSigil { get => _pvpSigil; set => Common.SetProperty(ref _pvpSigil, value, OnPvpSigilChanged); }

        public Infusion Infusion { get => _infusion; set => Common.SetProperty(ref _infusion, value, OnInfusionChanged); }

        public WeaponSlot OtherHandSlot { get; set; }

        private void SetGroupStat(Stat stat = null, bool overrideExisting = false)
        {
            foreach (var slot in SlotGroup)
            {
                if (slot.Slot is TemplateSlot.Aquatic or TemplateSlot.AltAquatic)
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
                if (slot.Slot is TemplateSlot.Aquatic or TemplateSlot.AltAquatic)
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
                if (slot.Slot is TemplateSlot.Aquatic or TemplateSlot.AltAquatic)
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
                if (slot.Slot is TemplateSlot.Aquatic or TemplateSlot.AltAquatic)
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
                if (slot.Slot is TemplateSlot.Aquatic or TemplateSlot.AltAquatic)
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
            int iconPadding = Slot is TemplateSlot.OffHand or TemplateSlot.AltOffHand ? 7 : 0;
            int textPadding = Slot is TemplateSlot.OffHand or TemplateSlot.AltOffHand ? 8 : 5;

            int size = Math.Min(Width, Height);
            int padding = 3;
            _changeWeaponTexture.Bounds = new(new(Icon.Bounds.Left + padding, padding), new((int)((size - (padding * 2)) / 2.5)));
            _statTexture.Bounds = new(Icon.Bounds.Center.Add(new Point(-padding, -padding)), new((size - (padding * 2)) / 2));

            _sigilSlotTexture.Bounds = new(Icon.Bounds.Right + 2 + iconPadding, 0, upgradeSize, upgradeSize);
            _sigilTexture.Bounds = _sigilSlotTexture.Bounds;

            int pvpUpgradeSize = 48;
            _pvpSigilSlotTexture.Bounds = new(Icon.Bounds.Right + 2 + 5 + iconPadding, (Icon.Bounds.Height - pvpUpgradeSize) / 2, pvpUpgradeSize, pvpUpgradeSize);
            _pvpSigilTexture.Bounds = _pvpSigilSlotTexture.Bounds;
            _pvpSigilBounds = new(_pvpSigilSlotTexture.Bounds.Right + 10, _pvpSigilTexture.Bounds.Top, Width - (_pvpSigilTexture.Bounds.Right + 2), _pvpSigilTexture.Bounds.Height);

            _infusionSlotTexture.Bounds = new(Icon.Bounds.Right + 2 + iconPadding, _sigilSlotTexture.Bounds.Bottom + 4, upgradeSize, upgradeSize);
            _infusionTexture.Bounds = _infusionSlotTexture.Bounds;

            int x = _sigilSlotTexture.Bounds.Right + textPadding + 4;
            _sigilBounds = new(x, _sigilSlotTexture.Bounds.Top - 1, Width - x, _sigilSlotTexture.Bounds.Height);
            _infusionBounds = new(x, _infusionSlotTexture.Bounds.Top, Width - x, _infusionSlotTexture.Bounds.Height);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);

            if (TemplatePresenter.IsPve != false)
            {
                _statTexture.Draw(this, spriteBatch);
                _changeWeaponTexture.Draw(this, spriteBatch, RelativeMousePosition);
                _sigilSlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
                _sigilTexture.Draw(this, spriteBatch, RelativeMousePosition);
                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Sigil?.DisplayText ?? string.Empty), UpgradeFont, _sigilBounds, UpgradeColor, false, HorizontalAlignment.Left, VerticalAlignment.Middle);

                _infusionSlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
                _infusionTexture.Draw(this, spriteBatch, RelativeMousePosition);
                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Infusion?.DisplayText ?? string.Empty), InfusionFont, _infusionBounds, InfusionColor, true, HorizontalAlignment.Left, VerticalAlignment.Middle);
            }
            else if (TemplatePresenter.IsPvp)
            {
                _pvpSigilSlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
                _pvpSigilTexture.Draw(this, spriteBatch, RelativeMousePosition);
                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(PvpSigil?.DisplayText ?? string.Empty), UpgradeFont, _pvpSigilBounds, UpgradeColor, false, HorizontalAlignment.Left, VerticalAlignment.Middle);
            }
        }

        protected override void SetItems(object sender, EventArgs e)
        {
            base.SetItems(sender, e);

            var armor = TemplatePresenter.Template[Slot] as WeaponTemplateEntry;

            Infusion = armor?.Infusion;
            Sigil = armor?.Sigil;
            PvpSigil = armor?.PvpSigil;
            Stat = armor?.Stat;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var a = AbsoluteBounds;

            if (Icon.Hovered && TemplatePresenter.IsPve)
            {
                SelectionPanel?.SetAnchor<Stat>(this, new Rectangle(a.Location, Point.Zero).Add(Icon.Bounds), SelectionTypes.Stats, Slot, GearSubSlotType.None, (stat) =>
                {
                    (TemplatePresenter?.Template[Slot] as WeaponTemplateEntry).Stat = stat;
                    Stat = stat;
                }, (TemplatePresenter?.Template[Slot] as WeaponTemplateEntry).Weapon?.StatChoices ?? BuildsManager.Data.Weapons.Values.FirstOrDefault()?.StatChoices,
                (TemplatePresenter?.Template[Slot] as WeaponTemplateEntry).Weapon?.AttributeAdjustment);
            }

            if (_pvpSigilSlotTexture.Hovered)
            {
                SelectionPanel?.SetAnchor<Sigil>(this, new Rectangle(a.Location, Point.Zero).Add(_pvpSigilSlotTexture.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Sigil, (sigil) =>
                {
                    (TemplatePresenter?.Template[Slot] as WeaponTemplateEntry).PvpSigil = sigil;
                    PvpSigil = sigil;
                });
            }

            if (_sigilSlotTexture.Hovered)
            {
                SelectionPanel?.SetAnchor<Sigil>(this, new Rectangle(a.Location, Point.Zero).Add(_sigilSlotTexture.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Sigil, (sigil) =>
                {
                    (TemplatePresenter?.Template[Slot] as WeaponTemplateEntry).Sigil = sigil;
                    Sigil = sigil;
                });
            }

            if (_infusionSlotTexture.Hovered)
            {
                SelectionPanel?.SetAnchor<Infusion>(this, new Rectangle(a.Location, Point.Zero).Add(_infusionSlotTexture.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) =>
                {
                    (TemplatePresenter?.Template[Slot] as WeaponTemplateEntry).Infusion = infusion;
                    Infusion = infusion;
                });
            }

            if (_changeWeaponTexture.Hovered || (Icon.Hovered && TemplatePresenter.IsPvp))
            {
                SelectionPanel?.SetAnchor<Weapon>(this, new Rectangle(a.Location, Point.Zero).Add(Icon.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Item, SelectWeapon);
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

            if (item.WeaponType.IsTwoHanded() && Slot is not TemplateSlot.MainHand and not TemplateSlot.AltMainHand)
                return;

            var template = TemplatePresenter.Template;
            var otherHand =
                Slot is TemplateSlot.MainHand ? TemplateSlot.OffHand :
                Slot is TemplateSlot.AltMainHand ? TemplateSlot.AltOffHand :
                Slot is TemplateSlot.OffHand ? TemplateSlot.MainHand :
                Slot is TemplateSlot.AltOffHand ? TemplateSlot.AltMainHand :
                TemplateSlot.None;

            if (item.WeaponType.IsTwoHanded() || (Slot is TemplateSlot.OffHand or TemplateSlot.AltOffHand && (OtherHandSlot?.Item as Weapon)?.WeaponType.IsTwoHanded() == true))
            {
                if (template[otherHand] is WeaponTemplateEntry offHand)
                {
                    offHand.Weapon = null;
                    if (OtherHandSlot != null) OtherHandSlot.Item = null;
                }
            }

            (TemplatePresenter?.Template[Slot] as WeaponTemplateEntry).Weapon = item;
            Item = item;
        }

        private void OnStatChanged(object sender, Core.Models.ValueChangedEventArgs<Stat> e)
        {
            _statTexture.Texture = Stat?.Icon;
        }

        private void OnSigilChanged(object sender, Core.Models.ValueChangedEventArgs<Sigil> e)
        {
            _sigilTexture.Texture = Sigil?.Icon;
        }

        private void OnPvpSigilChanged(object sender, Core.Models.ValueChangedEventArgs<Sigil> e)
        {
            _pvpSigilTexture.Texture = PvpSigil?.Icon;
        }

        private void OnInfusionChanged(object sender, Core.Models.ValueChangedEventArgs<Infusion> e)
        {
            _infusionTexture.Texture = Infusion?.Icon;
        }
    }

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

        public AquaticWeaponSlot(TemplateSlot gearSlot, Container parent, TemplatePresenter templatePresenter) : base(gearSlot, parent, templatePresenter)
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
                if (slot.Slot is TemplateSlot.Aquatic or TemplateSlot.AltAquatic)
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
                if (slot.Slot is TemplateSlot.Aquatic or TemplateSlot.AltAquatic)
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
                if (slot.Slot is TemplateSlot.Aquatic or TemplateSlot.AltAquatic)
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
                if (slot.Slot is TemplateSlot.Aquatic or TemplateSlot.AltAquatic)
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
            int textPadding = Slot is TemplateSlot.AquaBreather ? upgradeSize + 5 : 5;

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

    public class BackSlot : GearSlot
    {
        private readonly DetailedTexture _infusion1SlotTexture = new() { TextureRegion = new(37, 37, 54, 54) };
        private readonly DetailedTexture _infusion2SlotTexture = new() { TextureRegion = new(37, 37, 54, 54) };

        private readonly DetailedTexture _statTexture = new() { };

        private readonly ItemTexture _infusion1Texture = new() { };
        private readonly ItemTexture _infusion2Texture = new() { };

        private Stat _stat;
        private Infusion _infusion1;
        private Infusion _infusion2;

        public BackSlot(TemplateSlot gearSlot, Container parent, TemplatePresenter templatePresenter) : base(gearSlot, parent, templatePresenter)
        {
            _infusion1SlotTexture.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
            _infusion2SlotTexture.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
            ItemTexture.Item = BuildsManager.Data.Backs[94947];
        }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        public Infusion Infusion1 { get => _infusion1; set => Common.SetProperty(ref _infusion1, value, OnInfusion1Changed); }

        public Infusion Infusion2 { get => _infusion2; set => Common.SetProperty(ref _infusion2, value, OnInfusion2Changed); }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int size = Math.Min(Width, Height);
            int padding = 3;
            _statTexture.Bounds = new(Icon.Bounds.Center.Add(new Point(-padding, -padding)), new((size - (padding * 2)) / 2));

            int infusionSize = (Icon.Bounds.Size.Y - 4) / 3;

            _infusion1SlotTexture.Bounds = new(Icon.Bounds.Right + 2, Icon.Bounds.Top, infusionSize, infusionSize);
            _infusion1Texture.Bounds = _infusion1SlotTexture.Bounds;

            _infusion2SlotTexture.Bounds = new(Icon.Bounds.Right + 2, Icon.Bounds.Top + ((infusionSize + 2) * 1), infusionSize, infusionSize);
            _infusion2Texture.Bounds = _infusion2SlotTexture.Bounds;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);
            _statTexture.Draw(this, spriteBatch);

            _infusion1SlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
            _infusion2SlotTexture.Draw(this, spriteBatch, RelativeMousePosition);

            _infusion1Texture.Draw(this, spriteBatch, RelativeMousePosition);
            _infusion2Texture.Draw(this, spriteBatch, RelativeMousePosition);
        }

        protected override void SetItems(object sender, EventArgs e)
        {
            base.SetItems(sender, e);

            var armor = TemplatePresenter.Template[Slot] as BackTemplateEntry;

            Infusion1 = armor?.Infusion1;
            Infusion2 = armor?.Infusion2;

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
                    (TemplatePresenter?.Template[Slot] as BackTemplateEntry).Stat = stat;
                    Stat = stat;
                }, (TemplatePresenter?.Template[Slot] as BackTemplateEntry).Back?.StatChoices,
                (TemplatePresenter?.Template[Slot] as BackTemplateEntry).Back?.AttributeAdjustment);
            }

            if (_infusion1SlotTexture.Hovered)
            {
                SelectionPanel?.SetAnchor<Infusion>(this, new Rectangle(a.Location, Point.Zero).Add(_infusion1SlotTexture.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) =>
                {
                    (TemplatePresenter?.Template[Slot] as BackTemplateEntry).Infusion1 = infusion;
                    Infusion1 = infusion;
                });
            }

            if (_infusion2SlotTexture.Hovered)
            {
                SelectionPanel?.SetAnchor<Infusion>(this, new Rectangle(a.Location, Point.Zero).Add(_infusion2SlotTexture.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) =>
                {
                    (TemplatePresenter?.Template[Slot] as BackTemplateEntry).Infusion2 = infusion;
                    Infusion2 = infusion;
                });
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => "Reset", () => "Reset stat and infusion", () =>
            {
                Stat = null;
                Infusion1 = null;
                Infusion2 = null;
            }, new()
            {
                new(() => "Stat",() => "Reset the stat.",() => Stat = null),
                new(() => "Infusion",() => "Reset the infusions",() => {
                Infusion1 = null;
                Infusion2 = null;
                }),
            });

            CreateSubMenu(() => "Fill", () => "Fill the stat and infusions of all empty juwellery slots", () =>
            {
                SetGroupStat(Stat, false);
                SetGroupInfusion(Infusion1, false);
            }, new()
            {
                new(() => "Stat", () => "Fill all empty stat slots.", () => SetGroupStat(Stat, false)),
                new(() => "Infusion", () => "Fill all empty infusion slots.", () => SetGroupInfusion(Infusion1, false)),
                });

            CreateSubMenu(() => "Override", () => "Override the stat and infusions of all juwellery slots", () =>
            {
                SetGroupStat(Stat, true);
                SetGroupInfusion(Infusion1, true);
            }, new()
            {
                new(() => "Stat", () => "Override all stat slots.", () => SetGroupStat(Stat, true)),
                new(() => "Infusion", () => "Override all infusion slots.", () => SetGroupInfusion(Infusion1, true)),
                });

            CreateSubMenu(() => "Reset all juwellery", () => "Reset all stats and infusions for all juwellery slots", () =>
            {
                SetGroupStat(null, true);
                SetGroupInfusion(null, true);
            }, new()
            {
                new(() => "Stats",() => "Reset all stats of all juwellery slots.",() => SetGroupStat(null, true)),
                new(() => "Infusions",() => "Reset all infusions of all juwellery slots.",() => SetGroupInfusion(null, true)),
            });
        }

        private void SetGroupStat(Stat stat, bool overrideExisting)
        {
            foreach (var slot in SlotGroup)
            {
                switch (slot.Slot)
                {
                    case TemplateSlot.Accessory_1:
                    case TemplateSlot.Accessory_2:
                        var accessoire = slot as AccessoireSlot;
                        accessoire.Stat = overrideExisting ? stat : accessoire.Stat ?? stat;
                        (TemplatePresenter.Template[slot.Slot] as AccessoireTemplateEntry).Stat = overrideExisting ? stat : accessoire.Stat ?? stat; ;
                        break;

                    case TemplateSlot.Back:
                        var back = slot as BackSlot;
                        back.Stat = overrideExisting ? stat : back.Stat ?? stat;
                        (TemplatePresenter.Template[slot.Slot] as BackTemplateEntry).Stat = overrideExisting ? stat : back.Stat ?? stat; ;
                        break;

                    case TemplateSlot.Amulet:
                        var amulet = slot as AmuletSlot;
                        amulet.Stat = overrideExisting ? stat : amulet.Stat ?? stat;
                        (TemplatePresenter.Template[slot.Slot] as AmuletTemplateEntry).Stat = overrideExisting ? stat : amulet.Stat ?? stat; ;
                        break;

                    case TemplateSlot.Ring_1:
                    case TemplateSlot.Ring_2:
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
                    case TemplateSlot.Accessory_1:
                    case TemplateSlot.Accessory_2:
                        var accessoire = slot as AccessoireSlot;
                        accessoire.Infusion = overrideExisting ? infusion : accessoire.Infusion ?? infusion;
                        (TemplatePresenter.Template[slot.Slot] as AccessoireTemplateEntry).Infusion = overrideExisting ? infusion : accessoire.Infusion ?? infusion;
                        break;

                    case TemplateSlot.Back:
                        var back = slot as BackSlot;

                        back.Infusion1 = overrideExisting ? infusion : back.Infusion1 ?? infusion;
                        back.Infusion2 = overrideExisting ? infusion : back.Infusion2 ?? infusion;

                        (TemplatePresenter.Template[slot.Slot] as BackTemplateEntry).Infusion1 = overrideExisting ? infusion : back.Infusion1 ?? infusion;
                        (TemplatePresenter.Template[slot.Slot] as BackTemplateEntry).Infusion2 = overrideExisting ? infusion : back.Infusion2 ?? infusion;
                        break;

                    case TemplateSlot.Ring_1:
                    case TemplateSlot.Ring_2:
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

        private void OnStatChanged(object sender, Core.Models.ValueChangedEventArgs<Stat> e)
        {
            _statTexture.Texture = Stat?.Icon;
        }

        private void OnInfusion2Changed(object sender, Core.Models.ValueChangedEventArgs<Infusion> e)
        {
            _infusion2Texture.Texture = Infusion2?.Icon;
        }

        private void OnInfusion1Changed(object sender, Core.Models.ValueChangedEventArgs<Infusion> e)
        {
            _infusion1Texture.Texture = Infusion1?.Icon;
        }
    }

    public class AmuletSlot : GearSlot
    {
        private readonly DetailedTexture _enrichmentSlotTexture = new() { TextureRegion = new(37, 37, 54, 54) };
        private readonly DetailedTexture _statTexture = new() { };

        private readonly ItemTexture _enrichmentTexture = new() { };

        private Stat _stat;
        private Enrichment _enrichment;

        public AmuletSlot(TemplateSlot gearSlot, Container parent, TemplatePresenter templatePresenter) : base(gearSlot, parent, templatePresenter)
        {
            _enrichmentSlotTexture.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
            ItemTexture.Item = BuildsManager.Data.Trinkets[79980];
        }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        public Enrichment Enrichment { get => _enrichment; set => Common.SetProperty(ref _enrichment, value, OnEnrichmentChanged); }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int size = Math.Min(Width, Height);
            int padding = 3;
            _statTexture.Bounds = new(Icon.Bounds.Center.Add(new Point(-padding, -padding)), new((size - (padding * 2)) / 2));

            int infusionSize = (Icon.Bounds.Size.Y - 4) / 3;

            _enrichmentSlotTexture.Bounds = new(Icon.Bounds.Right + 2, Icon.Bounds.Top, infusionSize, infusionSize);
            _enrichmentTexture.Bounds = _enrichmentSlotTexture.Bounds;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);
            _statTexture.Draw(this, spriteBatch);

            _enrichmentSlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
            _enrichmentTexture.Draw(this, spriteBatch, RelativeMousePosition);
        }

        protected override void SetItems(object sender, EventArgs e)
        {
            base.SetItems(sender, e);

            var armor = TemplatePresenter.Template[Slot] as AmuletTemplateEntry;

            Enrichment = armor?.Enrichment;
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
                    (TemplatePresenter?.Template[Slot] as AmuletTemplateEntry).Stat = stat;
                    Stat = stat;
                }, (TemplatePresenter?.Template[Slot] as AmuletTemplateEntry).Amulet?.StatChoices,
                (TemplatePresenter?.Template[Slot] as AmuletTemplateEntry).Amulet?.AttributeAdjustment);
            }

            if (_enrichmentSlotTexture.Hovered)
            {
                SelectionPanel?.SetAnchor<Enrichment>(this, new Rectangle(a.Location, Point.Zero).Add(_enrichmentSlotTexture.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Enrichment, (enrichment) =>
                {
                    (TemplatePresenter?.Template[Slot] as AmuletTemplateEntry).Enrichment = enrichment;
                    Enrichment = enrichment;
                });
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => "Reset", () => "Reset stat and enrichment", () =>
            {
                Stat = null;
                Enrichment = null;
            }, new()
            {
                new(() => "Stat", () => "Reset the stat.", () => Stat = null ),
                new(() => "Enrichment", () => "Reset the enrichment.", () => Enrichment = null )});

            CreateSubMenu(() => "Fill", () => "Fill the stat of all empty juwellery slots", () => SetGroupStat(Stat, false), new()
            {
                new(() => "Stat", () => "Fill all empty stat slots.", () => SetGroupStat(Stat, false)),
                });

            CreateSubMenu(() => "Override", () => "Override the stat of all juwellery slots", () => SetGroupStat(Stat, true), new()
            {
                new(() => "Stat", () => "Override all stat slots.", () => SetGroupStat(Stat, true)),
                });

            CreateSubMenu(() => "Reset all juwellery", () => "Reset all stats and infusions for all juwellery slots", () => SetGroupStat(null, true), new()
            {
                new(() => "Stats", () => "Reset all stats of all juwellery slots.", () => SetGroupStat(null, true)),
                });
        }

        private void SetGroupStat(Stat stat, bool overrideExisting)
        {
            foreach (var slot in SlotGroup)
            {
                switch (slot.Slot)
                {
                    case TemplateSlot.Accessory_1:
                    case TemplateSlot.Accessory_2:
                        var accessoire = slot as AccessoireSlot;
                        accessoire.Stat = overrideExisting ? stat : accessoire.Stat ?? stat;
                        (TemplatePresenter.Template[slot.Slot] as AccessoireTemplateEntry).Stat = overrideExisting ? stat : accessoire.Stat ?? stat; ;
                        break;

                    case TemplateSlot.Back:
                        var back = slot as BackSlot;
                        back.Stat = overrideExisting ? stat : back.Stat ?? stat;
                        (TemplatePresenter.Template[slot.Slot] as BackTemplateEntry).Stat = overrideExisting ? stat : back.Stat ?? stat; ;
                        break;

                    case TemplateSlot.Amulet:
                        var amulet = slot as AmuletSlot;
                        amulet.Stat = overrideExisting ? stat : amulet.Stat ?? stat;
                        (TemplatePresenter.Template[slot.Slot] as AmuletTemplateEntry).Stat = overrideExisting ? stat : amulet.Stat ?? stat; ;
                        break;

                    case TemplateSlot.Ring_1:
                    case TemplateSlot.Ring_2:
                        var ring = slot as RingSlot;
                        ring.Stat = overrideExisting ? stat : ring.Stat ?? stat;
                        (TemplatePresenter.Template[slot.Slot] as RingTemplateEntry).Stat = overrideExisting ? stat : ring.Stat ?? stat; ;
                        break;
                }
            }
        }

        private void OnEnrichmentChanged(object sender, Core.Models.ValueChangedEventArgs<Enrichment> e)
        {
            _enrichmentTexture.Texture = Enrichment?.Icon;
        }

        private void OnStatChanged(object sender, Core.Models.ValueChangedEventArgs<Stat> e)
        {
            _statTexture.Texture = Stat?.Icon;
        }
    }

    public class AccessoireSlot : GearSlot
    {
        private readonly DetailedTexture _infusionSlotTexture = new() { TextureRegion = new(37, 37, 54, 54) };

        private readonly DetailedTexture _statTexture = new() { };

        private readonly ItemTexture _infusionTexture = new() { };

        private Stat _stat;
        private Infusion _infusion;

        public AccessoireSlot(TemplateSlot gearSlot, Container parent, TemplatePresenter templatePresenter) : base(gearSlot, parent, templatePresenter)
        {
            _infusionSlotTexture.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
            ItemTexture.Item = BuildsManager.Data.Trinkets[80002];
        }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        public Infusion Infusion { get => _infusion; set => Common.SetProperty(ref _infusion, value, OnInfusionChanged); }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int size = Math.Min(Width, Height);
            int padding = 3;
            _statTexture.Bounds = new(Icon.Bounds.Center.Add(new Point(-padding, -padding)), new((size - (padding * 2)) / 2));

            int infusionSize = (Icon.Bounds.Size.Y - 4) / 3;

            _infusionSlotTexture.Bounds = new(Icon.Bounds.Right + 2, Icon.Bounds.Top, infusionSize, infusionSize);
            _infusionTexture.Bounds = _infusionSlotTexture.Bounds;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);
            _statTexture.Draw(this, spriteBatch);

            _infusionSlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
            _infusionTexture.Draw(this, spriteBatch, RelativeMousePosition);
        }

        protected override void SetItems(object sender, EventArgs e)
        {
            base.SetItems(sender, e);

            var armor = TemplatePresenter.Template[Slot] as AccessoireTemplateEntry;

            Infusion = armor?.Infusion;
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
                    (TemplatePresenter?.Template[Slot] as AccessoireTemplateEntry).Stat = stat;
                    Stat = stat;
                }, (TemplatePresenter?.Template[Slot] as AccessoireTemplateEntry).Accessoire?.StatChoices,
                (TemplatePresenter?.Template[Slot] as AccessoireTemplateEntry).Accessoire?.AttributeAdjustment);
            }

            if (_infusionSlotTexture.Hovered)
            {
                SelectionPanel?.SetAnchor<Infusion>(this, new Rectangle(a.Location, Point.Zero).Add(_infusionSlotTexture.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) =>
                {
                    (TemplatePresenter?.Template[Slot] as AccessoireTemplateEntry).Infusion = infusion;
                    Infusion = infusion;
                });
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => "Reset", () => "Reset stat and infusion", () =>
            {
                Stat = null;
                Infusion = null;
            }, new()
            {
                new(() => "Stat",() => "Reset the stat.",() => Stat = null),
                new(() => "Infusion",() => "Reset the infusion.",() => Infusion = null),
            });

            CreateSubMenu(() => "Fill", () => "Fill the stat and infusions of all empty juwellery slots", () =>
            {
                SetGroupStat(Stat, false);
                SetGroupInfusion(Infusion, false);
            }, new()
            {
                new(() => "Stat", () => "Fill all empty stat slots.", () => SetGroupStat(Stat, false)),
                new(() => "Infusion", () => "Fill all empty infusion slots.", () => SetGroupInfusion(Infusion, false)),
                });

            CreateSubMenu(() => "Override", () => "Override the stat and infusions of all juwellery slots", () =>
            {
                SetGroupStat(Stat, true);
                SetGroupInfusion(Infusion, true);
            }, new()
            {
                new(() => "Stat", () => "Override all stat slots.", () => SetGroupStat(Stat, true)),
                new(() => "Infusion", () => "Override all infusion slots.", () => SetGroupInfusion(Infusion, true)),
                });

            CreateSubMenu(() => "Reset all juwellery", () => "Reset all stats and infusions for all juwellery slots", () =>
            {
                SetGroupStat(null, true);
                SetGroupInfusion(null, true);
            }, new()
            { new(() => "Stats",() => "Reset all stats of all juwellery slots.",() => SetGroupStat(null, true)),
                new(() => "Infusions",() => "Reset all infusions of all juwellery slots.",() => SetGroupInfusion(null, true)), });
        }

        private void SetGroupStat(Stat stat, bool overrideExisting)
        {
            foreach (var slot in SlotGroup)
            {
                switch (slot.Slot)
                {
                    case TemplateSlot.Accessory_1:
                    case TemplateSlot.Accessory_2:
                        var accessoire = slot as AccessoireSlot;
                        accessoire.Stat = overrideExisting ? stat : accessoire.Stat ?? stat;
                        (TemplatePresenter.Template[slot.Slot] as AccessoireTemplateEntry).Stat = overrideExisting ? stat : accessoire.Stat ?? stat; ;
                        break;

                    case TemplateSlot.Back:
                        var back = slot as BackSlot;
                        back.Stat = overrideExisting ? stat : back.Stat ?? stat;
                        (TemplatePresenter.Template[slot.Slot] as BackTemplateEntry).Stat = overrideExisting ? stat : back.Stat ?? stat; ;
                        break;

                    case TemplateSlot.Amulet:
                        var amulet = slot as AmuletSlot;
                        amulet.Stat = overrideExisting ? stat : amulet.Stat ?? stat;
                        (TemplatePresenter.Template[slot.Slot] as AmuletTemplateEntry).Stat = overrideExisting ? stat : amulet.Stat ?? stat; ;
                        break;

                    case TemplateSlot.Ring_1:
                    case TemplateSlot.Ring_2:
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
                    case TemplateSlot.Accessory_1:
                    case TemplateSlot.Accessory_2:
                        var accessoire = slot as AccessoireSlot;
                        accessoire.Infusion = overrideExisting ? infusion : accessoire.Infusion ?? infusion;
                        (TemplatePresenter.Template[slot.Slot] as AccessoireTemplateEntry).Infusion = overrideExisting ? infusion : accessoire.Infusion ?? infusion;
                        break;

                    case TemplateSlot.Back:
                        var back = slot as BackSlot;

                        back.Infusion1 = overrideExisting ? infusion : back.Infusion1 ?? infusion;
                        back.Infusion2 = overrideExisting ? infusion : back.Infusion2 ?? infusion;

                        (TemplatePresenter.Template[slot.Slot] as BackTemplateEntry).Infusion1 = overrideExisting ? infusion : back.Infusion1 ?? infusion;
                        (TemplatePresenter.Template[slot.Slot] as BackTemplateEntry).Infusion2 = overrideExisting ? infusion : back.Infusion2 ?? infusion;
                        break;

                    case TemplateSlot.Ring_1:
                    case TemplateSlot.Ring_2:
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
            _infusionTexture.Texture = Infusion?.Icon;
        }

        private void OnStatChanged(object sender, Core.Models.ValueChangedEventArgs<Stat> e)
        {
            _statTexture.Texture = Stat?.Icon;
        }
    }

    public class RingSlot : GearSlot
    {
        private readonly DetailedTexture _infusion1SlotTexture = new() { TextureRegion = new(37, 37, 54, 54) };
        private readonly DetailedTexture _infusion2SlotTexture = new() { TextureRegion = new(37, 37, 54, 54) };
        private readonly DetailedTexture _infusion3SlotTexture = new() { TextureRegion = new(37, 37, 54, 54) };

        private readonly DetailedTexture _statTexture = new() { };

        private readonly ItemTexture _infusion1Texture = new() { };
        private readonly ItemTexture _infusion2Texture = new() { };
        private readonly ItemTexture _infusion3Texture = new() { };

        private Stat _stat;

        private Infusion _infusion1;
        private Infusion _infusion2;
        private Infusion _infusion3;

        private Rectangle _runeBounds;
        private Rectangle _infusionBounds;

        public RingSlot(TemplateSlot gearSlot, Container parent, TemplatePresenter templatePresenter) : base(gearSlot, parent, templatePresenter)
        {
            _infusion1SlotTexture.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
            _infusion2SlotTexture.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
            _infusion3SlotTexture.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
            ItemTexture.Item = BuildsManager.Data.Trinkets[80058];
        }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        public Infusion Infusion1 { get => _infusion1; set => Common.SetProperty(ref _infusion1, value, OnInfusion1Changed); }

        public Infusion Infusion2 { get => _infusion2; set => Common.SetProperty(ref _infusion2, value, OnInfusion2Changed); }

        public Infusion Infusion3 { get => _infusion3; set => Common.SetProperty(ref _infusion3, value, OnInfusion3Changed); }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int size = Math.Min(Width, Height);
            int padding = 3;
            _statTexture.Bounds = new(Icon.Bounds.Center.Add(new Point(-padding, -padding)), new((size - (padding * 2)) / 2));

            int infusionSize = (Icon.Bounds.Size.Y - 4) / 3;

            _infusion1SlotTexture.Bounds = new(Icon.Bounds.Right + 2, Icon.Bounds.Top, infusionSize, infusionSize);
            _infusion1Texture.Bounds = _infusion1SlotTexture.Bounds;

            _infusion2SlotTexture.Bounds = new(Icon.Bounds.Right + 2, Icon.Bounds.Top + ((infusionSize + 2) * 1), infusionSize, infusionSize);
            _infusion2Texture.Bounds = _infusion2SlotTexture.Bounds;

            _infusion3SlotTexture.Bounds = new(Icon.Bounds.Right + 2, Icon.Bounds.Top + ((infusionSize + 2) * 2), infusionSize, infusionSize);
            _infusion3Texture.Bounds = _infusion3SlotTexture.Bounds;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);
            _statTexture.Draw(this, spriteBatch);

            _infusion1SlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
            _infusion2SlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
            _infusion3SlotTexture.Draw(this, spriteBatch, RelativeMousePosition);

            _infusion1Texture.Draw(this, spriteBatch, RelativeMousePosition);
            _infusion2Texture.Draw(this, spriteBatch, RelativeMousePosition);
            _infusion3Texture.Draw(this, spriteBatch, RelativeMousePosition);
        }

        protected override void SetItems(object sender, EventArgs e)
        {
            base.SetItems(sender, e);

            var armor = TemplatePresenter.Template[Slot] as RingTemplateEntry;

            Infusion1 = armor?.Infusion1;
            Infusion2 = armor?.Infusion2;
            Infusion3 = armor?.Infusion3;
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
                    (TemplatePresenter?.Template[Slot] as RingTemplateEntry).Stat = stat;
                    Stat = stat;
                }, (TemplatePresenter?.Template[Slot] as RingTemplateEntry).Ring?.StatChoices,
                (TemplatePresenter?.Template[Slot] as RingTemplateEntry).Ring?.AttributeAdjustment);
            }

            if (_infusion1SlotTexture.Hovered)
            {
                SelectionPanel?.SetAnchor<Infusion>(this, new Rectangle(a.Location, Point.Zero).Add(_infusion1SlotTexture.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) =>
                {
                    (TemplatePresenter?.Template[Slot] as RingTemplateEntry).Infusion1 = infusion;
                    Infusion1 = infusion;
                });
            }

            if (_infusion2SlotTexture.Hovered)
            {
                SelectionPanel?.SetAnchor<Infusion>(this, new Rectangle(a.Location, Point.Zero).Add(_infusion2SlotTexture.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) =>
                {
                    (TemplatePresenter?.Template[Slot] as RingTemplateEntry).Infusion2 = infusion;
                    Infusion2 = infusion;
                });
            }

            if (_infusion3SlotTexture.Hovered)
            {
                SelectionPanel?.SetAnchor<Infusion>(this, new Rectangle(a.Location, Point.Zero).Add(_infusion3SlotTexture.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) =>
                {
                    (TemplatePresenter?.Template[Slot] as RingTemplateEntry).Infusion3 = infusion;
                    Infusion3 = infusion;
                });
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => "Reset", () => "Reset stat and infusion", () =>
            {
                Stat = null;
                Infusion1 = null;
                Infusion2 = null;
                Infusion3 = null;
            }, new()
            {
                new(() => "Stat",() => "Reset the stat.",() => Stat = null),
                new(() => "Infusion",() => "Reset the infusion.",() =>
                {
                    Infusion1 = null;
                    Infusion2 = null;
                    Infusion3 = null;
                }),
            });

            CreateSubMenu(() => "Fill", () => "Fill the stat and infusions of all empty juwellery slots", () =>
            {
                SetGroupStat(Stat, false);
                SetGroupInfusion(Infusion1, false);
            }, new()
            {
                new(() => "Stat", () => "Fill all empty stat slots.", () => SetGroupStat(Stat, false)),
                new(() => "Infusion", () => "Fill all empty infusion slots.", () => SetGroupInfusion(Infusion1, false)),
                });

            CreateSubMenu(() => "Override", () => "Override the stat and infusions of all juwellery slots", () =>
            {
                SetGroupStat(Stat, true);
                SetGroupInfusion(Infusion1, true);
            }, new()
            {
                new(() => "Stat", () => "Override all stat slots.", () => SetGroupStat(Stat, true)),
                new(() => "Infusion", () => "Override all infusion slots.", () => SetGroupInfusion(Infusion1, true)),
                });

            CreateSubMenu(() => "Reset all juwellery", () => "Reset all stats and infusions for all juwellery slots", () =>
            {
                SetGroupStat(null, true);
                SetGroupInfusion(null, true);
            }, new()
            {
                new(() => "Stats",() => "Reset all stats of all juwellery slots.",() => SetGroupStat(null, true)),
                new(() => "Infusions",() => "Reset all infusions of all juwellery slots.",() => SetGroupInfusion(null, true)),
            });
        }

        private void SetGroupStat(Stat stat, bool overrideExisting)
        {
            foreach (var slot in SlotGroup)
            {
                switch (slot.Slot)
                {
                    case TemplateSlot.Accessory_1:
                    case TemplateSlot.Accessory_2:
                        var accessoire = slot as AccessoireSlot;
                        accessoire.Stat = overrideExisting ? stat : accessoire.Stat ?? stat;
                        (TemplatePresenter.Template[slot.Slot] as AccessoireTemplateEntry).Stat = overrideExisting ? stat : accessoire.Stat ?? stat; ;
                        break;

                    case TemplateSlot.Back:
                        var back = slot as BackSlot;
                        back.Stat = overrideExisting ? stat : back.Stat ?? stat;
                        (TemplatePresenter.Template[slot.Slot] as BackTemplateEntry).Stat = overrideExisting ? stat : back.Stat ?? stat; ;
                        break;

                    case TemplateSlot.Amulet:
                        var amulet = slot as AmuletSlot;
                        amulet.Stat = overrideExisting ? stat : amulet.Stat ?? stat;
                        (TemplatePresenter.Template[slot.Slot] as AmuletTemplateEntry).Stat = overrideExisting ? stat : amulet.Stat ?? stat; ;
                        break;

                    case TemplateSlot.Ring_1:
                    case TemplateSlot.Ring_2:
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
                    case TemplateSlot.Accessory_1:
                    case TemplateSlot.Accessory_2:
                        var accessoire = slot as AccessoireSlot;
                        accessoire.Infusion = overrideExisting ? infusion : accessoire.Infusion ?? infusion;
                        (TemplatePresenter.Template[slot.Slot] as AccessoireTemplateEntry).Infusion = overrideExisting ? infusion : accessoire.Infusion ?? infusion;
                        break;

                    case TemplateSlot.Back:
                        var back = slot as BackSlot;

                        back.Infusion1 = overrideExisting ? infusion : back.Infusion1 ?? infusion;
                        back.Infusion2 = overrideExisting ? infusion : back.Infusion2 ?? infusion;

                        (TemplatePresenter.Template[slot.Slot] as BackTemplateEntry).Infusion1 = overrideExisting ? infusion : back.Infusion1 ?? infusion;
                        (TemplatePresenter.Template[slot.Slot] as BackTemplateEntry).Infusion2 = overrideExisting ? infusion : back.Infusion2 ?? infusion;
                        break;

                    case TemplateSlot.Ring_1:
                    case TemplateSlot.Ring_2:
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

        private void OnStatChanged(object sender, Core.Models.ValueChangedEventArgs<Stat> e)
        {
            _statTexture.Texture = Stat?.Icon;
        }

        private void OnInfusion1Changed(object sender, Core.Models.ValueChangedEventArgs<Infusion> e)
        {
            _infusion1Texture.Texture = Infusion1?.Icon;
        }

        private void OnInfusion2Changed(object sender, Core.Models.ValueChangedEventArgs<Infusion> e)
        {
            _infusion2Texture.Texture = Infusion2?.Icon;
        }

        private void OnInfusion3Changed(object sender, Core.Models.ValueChangedEventArgs<Infusion> e)
        {
            _infusion3Texture.Texture = Infusion3?.Icon;
        }
    }

    public class PvpAmuletSlot : GearSlot
    {
        private Rectangle _titleBounds;
        private readonly DetailedTexture _runeSlotTexture = new() { Texture = AsyncTexture2D.FromAssetId(784323), TextureRegion = new(37, 37, 54, 54), };

        private readonly ItemTexture _runeTexture = new() { };

        private Rune _rune;
        private Rectangle _runeBounds;

        public PvpAmuletSlot(TemplateSlot gearSlot, Container parent, TemplatePresenter templatePresenter) : base(gearSlot, parent, templatePresenter)
        {
            ClipsBounds = false;
        }

        public Rune Rune { get => _rune; set => Common.SetProperty(ref _rune, value, OnRuneChanged); }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int upgradeSize = (Icon.Bounds.Size.Y - 4) / 2;
            int iconPadding = 0;
            int textPadding = Slot is TemplateSlot.AquaBreather ? upgradeSize + 5 : 5;

            int pvpUpgradeSize = 48;
            _runeSlotTexture.Bounds = new(Icon.Bounds.Right + 2 + 5 + iconPadding, (Icon.Bounds.Height - pvpUpgradeSize) / 2, pvpUpgradeSize, pvpUpgradeSize);
            _runeTexture.Bounds = _runeSlotTexture.Bounds;
            _runeBounds = new(_runeSlotTexture.Bounds.Right + 10, _runeTexture.Bounds.Top, Width - (_runeTexture.Bounds.Right + 2), _runeTexture.Bounds.Height);

            _titleBounds = new(_runeBounds.Left, _runeBounds.Top - (Content.DefaultFont16.LineHeight + 2), _runeBounds.Width, Content.DefaultFont16.LineHeight);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);

            _runeSlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
            _runeTexture.Draw(this, spriteBatch, RelativeMousePosition);
            spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Rune?.DisplayText ?? string.Empty), UpgradeFont, _runeBounds, UpgradeColor, false, HorizontalAlignment.Left, VerticalAlignment.Middle);

            //spriteBatch.DrawStringOnCtrl(this, ItemTexture?.Item?.Name ?? "Pvp Amulet", Content.DefaultFont16, _titleBounds, ItemTexture?.Item?.Rarity.GetColor() ?? Color.White * 0.5F);
        }

        protected override void SetItems(object sender, EventArgs e)
        {
            base.SetItems(sender, e);

            var armor = TemplatePresenter.Template[Slot] as PvpAmuletTemplateEntry;

            Rune = armor?.Rune;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var a = AbsoluteBounds;

            if (Icon.Hovered)
            {
                SelectionPanel?.SetAnchor<PvpAmulet>(this, new Rectangle(a.Location, Point.Zero).Add(Icon.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Item, (pvpAmulet) =>
                {
                    (TemplatePresenter?.Template[Slot] as PvpAmuletTemplateEntry).PvpAmulet = pvpAmulet;
                    Item = pvpAmulet;
                });
            }

            if (_runeSlotTexture.Hovered)
            {
                SelectionPanel?.SetAnchor<Rune>(this, new Rectangle(a.Location, Point.Zero).Add(_runeSlotTexture.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Rune, (rune) =>
                {
                    (TemplatePresenter?.Template[Slot] as PvpAmuletTemplateEntry).Rune = rune;
                    Rune = rune;
                });
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => "Reset", () => "Reset amulet and rune", () =>
            {
                Item = null;
                Rune = null;
            }, new()
            {
                new(() => "Amulet",() => "Reset the amulet",() => Item = null),
                new(() => "Rune",() => "Reset the rune.",() => Rune = null),
            });
        }

        private void OnRuneChanged(object sender, Core.Models.ValueChangedEventArgs<Rune> e)
        {
            _runeTexture.Texture = Rune?.Icon;
        }
    }

    public class NourishmentSlot : GearSlot
    {
        private Rectangle _titleBounds;
        private Rectangle _statBounds;

        public NourishmentSlot(TemplateSlot gearSlot, Container parent, TemplatePresenter templatePresenter) : base(gearSlot, parent, templatePresenter)
        {
            Icon.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\foodslot.png");
            ItemColor = Color.White;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _titleBounds = new(Icon.Bounds.Right + 10, Icon.Bounds.Top + 2, Width - Icon.Bounds.Left - 20, Content.DefaultFont16.LineHeight);
            _statBounds = new(Icon.Bounds.Right + 10, _titleBounds.Bottom + 2, Width - Icon.Bounds.Left - 20, Content.DefaultFont12.LineHeight);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);

            spriteBatch.DrawStringOnCtrl(this, ItemTexture?.Item?.Name ?? "", Content.DefaultFont16, _titleBounds, ItemTexture?.Item?.Rarity.GetColor() ?? Color.White);
            spriteBatch.DrawStringOnCtrl(this, (ItemTexture?.Item as Nourishment)?.Details.Description ?? ItemTexture?.Item?.Description, Content.DefaultFont12, _statBounds, Color.White, false, HorizontalAlignment.Left, VerticalAlignment.Top);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var a = AbsoluteBounds;

            if (Icon.Hovered)
            {
                SelectionPanel?.SetAnchor<Nourishment>(this, new Rectangle(a.Location, Point.Zero).Add(Icon.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Item, (nourishment) =>
                {
                    (TemplatePresenter?.Template[Slot] as NourishmentEntry).Nourishment = nourishment;
                    Item = nourishment;
                });
            }
        }

        protected override void SetItems(object sender, EventArgs e)
        {
            base.SetItems(sender, e);

            var nourishment = TemplatePresenter.Template[Slot] as NourishmentEntry;
            Item = nourishment?.Nourishment;
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => "Reset", () => "Reset nourishment", () => Item = null);
        }
    }

    public class UtilitySlot : GearSlot
    {
        private Rectangle _titleBounds;
        private Rectangle _statBounds;

        public UtilitySlot(TemplateSlot gearSlot, Container parent, TemplatePresenter templatePresenter) : base(gearSlot, parent, templatePresenter)
        {
            Icon.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\utilityslot.png");
            ItemColor = Color.White;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _titleBounds = new(Icon.Bounds.Right + 10, Icon.Bounds.Top + 2, Width - Icon.Bounds.Left - 20, Content.DefaultFont16.LineHeight);
            _statBounds = new(Icon.Bounds.Right + 10, _titleBounds.Bottom + 2, Width - Icon.Bounds.Left - 20, Content.DefaultFont12.LineHeight);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);

            spriteBatch.DrawStringOnCtrl(this, ItemTexture?.Item?.Name ?? "", Content.DefaultFont16, _titleBounds, ItemTexture?.Item?.Rarity.GetColor() ?? Color.White);
            spriteBatch.DrawStringOnCtrl(this, (ItemTexture?.Item as DataModels.Items.Utility)?.Details.Description ?? ItemTexture?.Item?.Description, Content.DefaultFont12, _statBounds, Color.White, false, HorizontalAlignment.Left, VerticalAlignment.Top);
        }

        protected override void SetItems(object sender, EventArgs e)
        {
            base.SetItems(sender, e);

            var utility = TemplatePresenter.Template[Slot] as UtilityEntry;
            Item = utility?.Utility;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var a = AbsoluteBounds;

            if (Icon.Hovered)
            {
                SelectionPanel?.SetAnchor<DataModels.Items.Utility>(this, new Rectangle(a.Location, Point.Zero).Add(Icon.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Item, (utility) =>
                {
                    (TemplatePresenter?.Template[Slot] as UtilityEntry).Utility = utility;
                    Item = utility;
                });
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => "Reset", () => "Reset utility", () => Item = null);
        }
    }

    public class JadeBotCoreSlot : GearSlot
    {
        private Rectangle _titleBounds;
        private Rectangle _statBounds;

        public JadeBotCoreSlot(TemplateSlot gearSlot, Container parent, TemplatePresenter templatePresenter) : base(gearSlot, parent, templatePresenter)
        {
            Icon.Texture = AsyncTexture2D.FromAssetId(2630946);
            Icon.TextureRegion = new(36, 36, 56, 56);
            ItemColor = Color.White;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _titleBounds = new(Icon.Bounds.Right + 10, Icon.Bounds.Top + 2, Width - Icon.Bounds.Left - 20, Content.DefaultFont16.LineHeight);
            _statBounds = new(Icon.Bounds.Right + 10, _titleBounds.Bottom + 2, Width - Icon.Bounds.Left - 20, Content.DefaultFont12.LineHeight);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);

            spriteBatch.DrawStringOnCtrl(this, ItemTexture?.Item?.Name ?? "Jade Bot Core", Content.DefaultFont16, _titleBounds, ItemTexture?.Item?.Rarity.GetColor() ?? Color.White * 0.5F);
            spriteBatch.DrawStringOnCtrl(this, (ItemTexture?.Item as DataModels.Items.Utility)?.Details.Description ?? ItemTexture?.Item?.Description ?? "Currently not available", Content.DefaultFont12, _statBounds, Color.White, false, HorizontalAlignment.Left, VerticalAlignment.Top);
        }

        protected override void SetItems(object sender, EventArgs e)
        {
            base.SetItems(sender, e);

            var jadebotcore = TemplatePresenter.Template[Slot] as JadeBotTemplateEntry;
            Item = jadebotcore?.JadeBotCore;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            return;

            var a = AbsoluteBounds;

            if (Icon.Hovered)
            {
                SelectionPanel?.SetAnchor<JadeBotCore>(this, new Rectangle(a.Location, Point.Zero).Add(Icon.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Item, (jadeBotCore) =>
                {
                    (TemplatePresenter?.Template[Slot] as JadeBotTemplateEntry).JadeBotCore = jadeBotCore;
                    Item = jadeBotCore;
                });
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            return;
            CreateSubMenu(() => "Reset", () => "Reset jade bot core", () => Item = null);
        }
    }

    public class RelicSlot : GearSlot
    {
        private Rectangle _titleBounds;
        private Rectangle _statBounds;

        public RelicSlot(TemplateSlot gearSlot, Container parent, TemplatePresenter templatePresenter) : base(gearSlot, parent, templatePresenter)
        {
            Icon.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\relic_slot.png");
            ItemColor = Color.White;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _titleBounds = new(Icon.Bounds.Right + 10, Icon.Bounds.Top + 2, Width - Icon.Bounds.Left - 20, Content.DefaultFont16.LineHeight);
            _statBounds = new(Icon.Bounds.Right + 10, _titleBounds.Bottom + 2, Width - Icon.Bounds.Left - 20, Content.DefaultFont12.LineHeight);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);

            spriteBatch.DrawStringOnCtrl(this, ItemTexture?.Item?.Name ?? "Relic", Content.DefaultFont16, _titleBounds, ItemTexture?.Item?.Rarity.GetColor() ?? Color.White * 0.5F);
            spriteBatch.DrawStringOnCtrl(this, (ItemTexture?.Item as DataModels.Items.Utility)?.Details.Description ?? ItemTexture?.Item?.Description ?? "Currently not available", Content.DefaultFont12, _statBounds, Color.White, false, HorizontalAlignment.Left, VerticalAlignment.Top);
        }

        protected override void SetItems(object sender, EventArgs e)
        {
            base.SetItems(sender, e);

            var relic = TemplatePresenter.Template[Slot] as RelicTemplateEntry;
            Item = relic?.Relic;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            return;

            var a = AbsoluteBounds;

            if (Icon.Hovered)
            {
                SelectionPanel?.SetAnchor<Relic>(this, new Rectangle(a.Location, Point.Zero).Add(Icon.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Item, (relic) =>
                {
                    (TemplatePresenter?.Template[Slot] as RelicTemplateEntry).Relic = relic;
                    Item = relic;
                });
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            return;
            CreateSubMenu(() => "Reset", () => "Reset relic", () => Item = null);
        }
    }
}
