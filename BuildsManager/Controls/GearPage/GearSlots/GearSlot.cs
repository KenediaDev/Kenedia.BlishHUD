using Control = Blish_HUD.Controls.Control;
using Container = Blish_HUD.Controls.Container;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using ItemRarity = Gw2Sharp.WebApi.V2.Models.ItemRarity;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.BuildsManager.Controls.Selection;
using SharpDX.Direct3D9;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.BuildsManager.TemplateEntries;

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
            Size = new(380, 64);
            ClipsBounds = true;

            Menu = new();

            TemplatePresenter = templatePresenter;
            Slot = gearSlot;
            Parent = parent;

            TemplatePresenter.LoadedGearFromCode += OnGearCodeLoaded;
            TemplatePresenter.TemplateChanged += OnGearCodeLoaded;
        }

        public SelectionPanel SelectionPanel { get; set; }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            Icon.Draw(this, spriteBatch, RelativeMousePosition);
            ItemTexture.Draw(this, spriteBatch, RelativeMousePosition, ItemColor);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            var a = AbsoluteBounds;

            //ClickAction?.Invoke();

            if (Icon.Hovered)
                Debug.WriteLine($"Something");
            //SelectionPanel?.SetGearAnchor(this, new Rectangle(a.Location, Point.Zero).Add(Icon.Bounds), GearSlot, GearSubSlotType.Item, GearSlot.ToString().ToLowercaseNamingConvention(), (item) => TemplateSlot?.SetItem(item));
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

        protected virtual void OnGearCodeLoaded(object sender, EventArgs e)
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

            ContextMenuStripItem menuItem;
            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "Reset", null));

            var resetMenu = menuItem.Submenu = new();
            menuItem = resetMenu.AddMenuItem(new ContextMenuItem(() => "Stat", null));
            menuItem = resetMenu.AddMenuItem(new ContextMenuItem(() => "Rune", null));
            menuItem = resetMenu.AddMenuItem(new ContextMenuItem(() => "Infusion", null));

            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "Fill", null));
            var fillMenu = menuItem.Submenu = new();
            menuItem = fillMenu.AddMenuItem(new ContextMenuItem(() => "Stat", null));
            menuItem = fillMenu.AddMenuItem(new ContextMenuItem(() => "Rune", null));
            menuItem = fillMenu.AddMenuItem(new ContextMenuItem(() => "Infusion", null));

            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "Override", null));
            var overrideMenu = menuItem.Submenu = new();
            menuItem = overrideMenu.AddMenuItem(new ContextMenuItem(() => "Stat", null));
            menuItem = overrideMenu.AddMenuItem(new ContextMenuItem(() => "Rune", null));
            menuItem = overrideMenu.AddMenuItem(new ContextMenuItem(() => "Infusion", null));

            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "All Armor", null));
            var groupMenu = menuItem.Submenu = new();
            menuItem = groupMenu.AddMenuItem(new ContextMenuItem(() => "Reset Stats", null));
            menuItem = groupMenu.AddMenuItem(new ContextMenuItem(() => "Reset Runes", null));
            menuItem = groupMenu.AddMenuItem(new ContextMenuItem(() => "Reset Infusions", null));
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

        protected override void OnGearCodeLoaded(object sender, EventArgs e)
        {
            base.OnGearCodeLoaded(sender, e);

            var armor = TemplatePresenter.Template[Slot] as ArmorTemplateEntry;
            
            Infusion = armor?.Infusion;
            Rune = armor?.Rune;
            Stat = armor?.Stat;
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

            ContextMenuStripItem menuItem;
            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "Reset", null));

            var _resetMenu = menuItem.Submenu = new();
            menuItem = _resetMenu.AddMenuItem(new ContextMenuItem(() => "Stat", null));
            menuItem = _resetMenu.AddMenuItem(new ContextMenuItem(() => "Sigil", null));
            menuItem = _resetMenu.AddMenuItem(new ContextMenuItem(() => "Infusion", null));

            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "Fill", null));
            var fillMenu = menuItem.Submenu = new();
            menuItem = fillMenu.AddMenuItem(new ContextMenuItem(() => "Stat", null));
            menuItem = fillMenu.AddMenuItem(new ContextMenuItem(() => "Sigil", null));
            menuItem = fillMenu.AddMenuItem(new ContextMenuItem(() => "Infusion", null));

            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "Override", null));
            var overrideMenu = menuItem.Submenu = new();
            menuItem = overrideMenu.AddMenuItem(new ContextMenuItem(() => "Stat", null));
            menuItem = overrideMenu.AddMenuItem(new ContextMenuItem(() => "Sigil", null));
            menuItem = overrideMenu.AddMenuItem(new ContextMenuItem(() => "Infusion", null));

            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "All Weapons", null));
            var groupMenu = menuItem.Submenu = new();
            menuItem = groupMenu.AddMenuItem(new ContextMenuItem(() => "Reset Stats", null));
            menuItem = groupMenu.AddMenuItem(new ContextMenuItem(() => "Reset Sigils", null));
            menuItem = groupMenu.AddMenuItem(new ContextMenuItem(() => "Reset Infusions", null));
        }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        public Sigil Sigil { get => _sigil; set => Common.SetProperty(ref _sigil, value, OnSigilChanged); }

        public Sigil PvpSigil { get => _pvpSigil; set => Common.SetProperty(ref _pvpSigil, value, OnPvpSigilChanged); }

        public Infusion Infusion { get => _infusion; set => Common.SetProperty(ref _infusion, value, OnInfusionChanged); }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int upgradeSize = (Icon.Bounds.Size.Y - 4) / 2;
            int iconPadding = Slot is TemplateSlot.OffHand or TemplateSlot.AltOffHand ? 7 : 0;
            int textPadding = Slot is TemplateSlot.OffHand or TemplateSlot.AltOffHand ? 8 : 5;

            int size = Math.Min(Width, Height);
            int padding = 3;
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

        protected override void OnGearCodeLoaded(object sender, EventArgs e)
        {
            base.OnGearCodeLoaded(sender, e);

            var armor = TemplatePresenter.Template[Slot] as WeaponTemplateEntry;

            Infusion = armor?.Infusion;
            Sigil = armor?.Sigil;
            PvpSigil = armor?.PvpSigil;
            Stat = armor?.Stat;
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

            ContextMenuStripItem menuItem;
            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "Reset", null));

            var _resetMenu = menuItem.Submenu = new();
            menuItem = _resetMenu.AddMenuItem(new ContextMenuItem(() => "Stat", null));
            menuItem = _resetMenu.AddMenuItem(new ContextMenuItem(() => "Sigils", null));
            menuItem = _resetMenu.AddMenuItem(new ContextMenuItem(() => "Infusions", null));

            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "Fill", null));
            var fillMenu = menuItem.Submenu = new();
            menuItem = fillMenu.AddMenuItem(new ContextMenuItem(() => "Stat", null));
            menuItem = fillMenu.AddMenuItem(new ContextMenuItem(() => "Sigil", null));
            menuItem = fillMenu.AddMenuItem(new ContextMenuItem(() => "Infusion", null));

            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "Override", null));
            var overrideMenu = menuItem.Submenu = new();
            menuItem = overrideMenu.AddMenuItem(new ContextMenuItem(() => "Stat", null));
            menuItem = overrideMenu.AddMenuItem(new ContextMenuItem(() => "Sigil", null));
            menuItem = overrideMenu.AddMenuItem(new ContextMenuItem(() => "Infusion", null));

            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "All Weapons", null));
            var groupMenu = menuItem.Submenu = new();
            menuItem = groupMenu.AddMenuItem(new ContextMenuItem(() => "Reset Stats", null));
            menuItem = groupMenu.AddMenuItem(new ContextMenuItem(() => "Reset Sigils", null));
            menuItem = groupMenu.AddMenuItem(new ContextMenuItem(() => "Reset Infusions", null));
        }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        public Sigil Sigil1 { get => _sigil1; set => Common.SetProperty(ref _sigil1, value, OnSigil1Changed); }

        public Sigil Sigil2 { get => _sigil2; set => Common.SetProperty(ref _sigil2, value, OnSigil2Changed); }

        public Infusion Infusion1 { get => _infusion1; set => Common.SetProperty(ref _infusion1, value, OnInfusion1Changed); }

        public Infusion Infusion2 { get => _infusion2; set => Common.SetProperty(ref _infusion2, value, OnInfusion2Changed); }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int upgradeSize = (Icon.Bounds.Size.Y - 4) / 2;
            int iconPadding = 0;
            int textPadding = Slot is TemplateSlot.AquaBreather ? upgradeSize + 5 : 5;

            int size = Math.Min(Width, Height);
            int padding = 3;
            _statTexture.Bounds = new(Icon.Bounds.Center.Add(new Point(-padding, -padding)), new((size - (padding * 2)) / 2));

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

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (TemplatePresenter.IsPve != false)
            {
                base.Paint(spriteBatch, bounds);
                _statTexture.Draw(this, spriteBatch);
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

        protected override void OnGearCodeLoaded(object sender, EventArgs e)
        {
            base.OnGearCodeLoaded(sender, e);

            var armor = TemplatePresenter.Template[Slot] as AquaticWeaponTemplateEntry;

            Infusion1 = armor?.Infusion_1;
            Infusion2 = armor?.Infusion_2;
            Sigil1 = armor?.Sigil_1;
            Sigil2 = armor?.Sigil_2;

            Stat = armor?.Stat;
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

            ContextMenuStripItem menuItem;
            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "Reset", null));

            var _resetMenu = menuItem.Submenu = new();
            menuItem = _resetMenu.AddMenuItem(new ContextMenuItem(() => "Stat", null));
            menuItem = _resetMenu.AddMenuItem(new ContextMenuItem(() => "Infusions", null));

            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "Fill", null));
            var fillMenu = menuItem.Submenu = new();
            menuItem = fillMenu.AddMenuItem(new ContextMenuItem(() => "Stat", null));
            menuItem = fillMenu.AddMenuItem(new ContextMenuItem(() => "Infusion", null));

            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "Override", null));
            var overrideMenu = menuItem.Submenu = new();
            menuItem = overrideMenu.AddMenuItem(new ContextMenuItem(() => "Stat", null));
            menuItem = overrideMenu.AddMenuItem(new ContextMenuItem(() => "Infusion", null));

            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "All Juwellery", null));
            var groupMenu = menuItem.Submenu = new();
            menuItem = groupMenu.AddMenuItem(new ContextMenuItem(() => "Reset Stats", null));
            menuItem = groupMenu.AddMenuItem(new ContextMenuItem(() => "Reset Infusions", null));
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

        protected override void OnGearCodeLoaded(object sender, EventArgs e)
        {
            base.OnGearCodeLoaded(sender, e);

            var armor = TemplatePresenter.Template[Slot] as BackTemplateEntry;

            Infusion1 = armor?.Infusion_1;
            Infusion2 = armor?.Infusion_2;

            Stat = armor?.Stat;
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

            ContextMenuStripItem menuItem;
            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "Reset", null));

            var _resetMenu = menuItem.Submenu = new();
            menuItem = _resetMenu.AddMenuItem(new ContextMenuItem(() => "Stat", null));
            menuItem = _resetMenu.AddMenuItem(new ContextMenuItem(() => "Enrichment", null));

            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "Fill", null));
            var fillMenu = menuItem.Submenu = new();
            menuItem = fillMenu.AddMenuItem(new ContextMenuItem(() => "Stat", null));

            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "Override", null));
            var overrideMenu = menuItem.Submenu = new();
            menuItem = overrideMenu.AddMenuItem(new ContextMenuItem(() => "Stat", null));

            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "All Juwellery", null));
            var groupMenu = menuItem.Submenu = new();
            menuItem = groupMenu.AddMenuItem(new ContextMenuItem(() => "Reset Stats", null));
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

        protected override void OnGearCodeLoaded(object sender, EventArgs e)
        {
            base.OnGearCodeLoaded(sender, e);

            var armor = TemplatePresenter.Template[Slot] as AmuletTemplateEntry;

            Enrichment = armor?.Enrichment;
            Stat = armor?.Stat;
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

            ContextMenuStripItem menuItem;
            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "Reset", null));

            var _resetMenu = menuItem.Submenu = new();
            menuItem = _resetMenu.AddMenuItem(new ContextMenuItem(() => "Stat", null));
            menuItem = _resetMenu.AddMenuItem(new ContextMenuItem(() => "Infusion", null));

            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "Fill", null));
            var fillMenu = menuItem.Submenu = new();
            menuItem = fillMenu.AddMenuItem(new ContextMenuItem(() => "Stat", null));
            menuItem = fillMenu.AddMenuItem(new ContextMenuItem(() => "Infusion", null));

            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "Override", null));
            var overrideMenu = menuItem.Submenu = new();
            menuItem = overrideMenu.AddMenuItem(new ContextMenuItem(() => "Stat", null));
            menuItem = overrideMenu.AddMenuItem(new ContextMenuItem(() => "Infusion", null));

            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "All Juwellery", null));
            var groupMenu = menuItem.Submenu = new();
            menuItem = groupMenu.AddMenuItem(new ContextMenuItem(() => "Reset Stats", null));
            menuItem = groupMenu.AddMenuItem(new ContextMenuItem(() => "Reset Infusions", null));
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

        protected override void OnGearCodeLoaded(object sender, EventArgs e)
        {
            base.OnGearCodeLoaded(sender, e);

            var armor = TemplatePresenter.Template[Slot] as AccessoireTemplateEntry;

            Infusion = armor?.Infusion;
            Stat = armor?.Stat;
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

            ContextMenuStripItem menuItem;
            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "Reset", null));

            var _resetMenu = menuItem.Submenu = new();
            menuItem = _resetMenu.AddMenuItem(new ContextMenuItem(() => "Stat", null));
            menuItem = _resetMenu.AddMenuItem(new ContextMenuItem(() => "Infusions", null));

            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "Fill", null));
            var fillMenu = menuItem.Submenu = new();
            menuItem = fillMenu.AddMenuItem(new ContextMenuItem(() => "Stat", null));
            menuItem = fillMenu.AddMenuItem(new ContextMenuItem(() => "Infusion", null));

            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "Override", null));
            var overrideMenu = menuItem.Submenu = new();
            menuItem = overrideMenu.AddMenuItem(new ContextMenuItem(() => "Stat", null));
            menuItem = overrideMenu.AddMenuItem(new ContextMenuItem(() => "Infusion", null));

            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "All Juwellery", null));
            var groupMenu = menuItem.Submenu = new();
            menuItem = groupMenu.AddMenuItem(new ContextMenuItem(() => "Reset Stats", null));
            menuItem = groupMenu.AddMenuItem(new ContextMenuItem(() => "Reset Infusions", null));
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

        protected override void OnGearCodeLoaded(object sender, EventArgs e)
        {
            base.OnGearCodeLoaded(sender, e);

            var armor = TemplatePresenter.Template[Slot] as RingTemplateEntry;

            Infusion1 = armor?.Infusion_1;
            Infusion2 = armor?.Infusion_2;
            Infusion3 = armor?.Infusion_3;
            Stat = armor?.Stat;
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

            ContextMenuStripItem menuItem;
            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "Reset", null));

            var _resetMenu = menuItem.Submenu = new();
            menuItem = _resetMenu.AddMenuItem(new ContextMenuItem(() => "Amulet", null));
            menuItem = _resetMenu.AddMenuItem(new ContextMenuItem(() => "Rune", null));
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

        protected override void OnGearCodeLoaded(object sender, EventArgs e)
        {
            base.OnGearCodeLoaded(sender, e);

            var armor = TemplatePresenter.Template[Slot] as PvpAmuletTemplateEntry;

            Rune = armor?.Rune;
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

            ContextMenuStripItem menuItem;
            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "Reset", null));
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

        protected override void OnGearCodeLoaded(object sender, EventArgs e)
        {
            base.OnGearCodeLoaded(sender, e);

            var nourishment = TemplatePresenter.Template[Slot] as NourishmentEntry;
            Item = nourishment?.Item;
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

            ContextMenuStripItem menuItem;
            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "Reset", null));
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

        protected override void OnGearCodeLoaded(object sender, EventArgs e)
        {
            base.OnGearCodeLoaded(sender, e);

            var utility = TemplatePresenter.Template[Slot] as UtilityEntry;
            Item = utility?.Item;
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

            ContextMenuStripItem menuItem;
            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "Reset", null));
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

        protected override void OnGearCodeLoaded(object sender, EventArgs e)
        {
            base.OnGearCodeLoaded(sender, e);

            var jadebotcore = TemplatePresenter.Template[Slot] as JadeBotTemplateEntry;
            Item = jadebotcore?.Item;
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

            ContextMenuStripItem menuItem;
            menuItem = Menu.AddMenuItem(new ContextMenuItem(() => "Reset", null));
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

        protected override void OnGearCodeLoaded(object sender, EventArgs e)
        {
            base.OnGearCodeLoaded(sender, e);

            var relic = TemplatePresenter.Template[Slot] as RelicTemplateEntry;
            Item = relic?.Item;
        }
    }
}
