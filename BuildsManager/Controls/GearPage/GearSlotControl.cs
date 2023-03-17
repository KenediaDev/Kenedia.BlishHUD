using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.BuildsManager.Controls.Selection;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using System.Linq;
using Blish_HUD.Controls;
using MonoGame.Extended.BitmapFonts;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage
{
    public class ItemTexture : DetailedTexture
    {
        private BaseItem _item;
        private Color _frameColor;

        public BaseItem Item { get => _item; set => Common.SetProperty(ref _item, value, ApplyItem); }

        private void ApplyItem()
        {
            _frameColor = Item != null ? (Color)Item?.Rarity.GetColor() : Color.White * 0.5F;
            Texture = Item?.Icon;
        }

        public void Draw(Control ctrl, SpriteBatch spriteBatch, Point? mousePos = null, Color? color = null)
        {
            if (FallBackTexture != null || Texture != null)
            {
                Hovered = mousePos != null && Bounds.Contains((Point)mousePos);
                color ??= (Hovered && HoverDrawColor != null ? HoverDrawColor : DrawColor) ?? Color.White;

                if (Texture != null)
                {
                    spriteBatch.DrawOnCtrl(
                        ctrl,
                        Texture,
                        Bounds.Add(2, 2, -4, -4),
                        TextureRegion,
                        (Color)color,
                        0F,
                        Vector2.Zero);
                }

                spriteBatch.DrawFrame(ctrl, Bounds, _frameColor, 2);
            }
        }
    }

    public class WeaponSlotControl : GearSlotControl
    {
        private readonly DetailedTexture _sigilSlotTexture = new() { Texture = AsyncTexture2D.FromAssetId(784324), TextureRegion = new(37, 37, 54, 54), };
        private readonly DetailedTexture _pvpSigilSlotTexture = new() { Texture = AsyncTexture2D.FromAssetId(784324), TextureRegion = new(37, 37, 54, 54), };
        private readonly DetailedTexture _infusionSlotTexture = new() { TextureRegion = new(37, 37, 54, 54) };

        private readonly ItemTexture _sigilTexture = new() { };
        private readonly ItemTexture _pvpSigilTexture = new() { };
        private readonly ItemTexture _infusionTexture = new() { };
        private readonly DetailedTexture _statTexture = new() { };

        private Rectangle _statBounds;
        private Rectangle _sigilBounds;
        private Rectangle _pvpSigilBounds;
        private Rectangle _infusionBounds;

        public WeaponSlotControl(GearTemplateSlot gearSlot, Container parent) : base(gearSlot, parent)
        {
            _infusionSlotTexture.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
        }

        public Sigil Sigil { get; private set; }

        public Sigil PvpSigil { get; private set; }

        public Infusion Infusion { get; private set; }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int upgradeSize = (Icon.Bounds.Size.Y - 4) / 2;
            int iconPadding = GearSlot is GearTemplateSlot.OffHand or GearTemplateSlot.AltOffHand ? 7 : 0;
            int textPadding = GearSlot is GearTemplateSlot.OffHand or GearTemplateSlot.AltOffHand ? 8 : 5;

            _sigilSlotTexture.Bounds = new(Icon.Bounds.Right + 2 + iconPadding, 0, upgradeSize, upgradeSize);
            _sigilTexture.Bounds = _sigilSlotTexture.Bounds;

            _infusionSlotTexture.Bounds = new(Icon.Bounds.Right + 2 + iconPadding, _sigilSlotTexture.Bounds.Bottom + 4, upgradeSize, upgradeSize);
            _infusionTexture.Bounds = _infusionSlotTexture.Bounds;

            int pvpUpgradeSize = 48;
            _pvpSigilSlotTexture.Bounds = new(Icon.Bounds.Right + 2 + 5 + iconPadding, (Icon.Bounds.Height - pvpUpgradeSize) / 2, pvpUpgradeSize, pvpUpgradeSize);
            _pvpSigilTexture.Bounds = _pvpSigilSlotTexture.Bounds;
            _pvpSigilBounds = new(_pvpSigilSlotTexture.Bounds.Right + 10, _pvpSigilTexture.Bounds.Top, Width - (_pvpSigilTexture.Bounds.Right + 2), _pvpSigilTexture.Bounds.Height);

            int x = _sigilSlotTexture.Bounds.Right + textPadding + 4;
            _sigilBounds = new(x, _sigilSlotTexture.Bounds.Top - 1, Width - x, _sigilSlotTexture.Bounds.Height);
            _infusionBounds = new(x, _infusionSlotTexture.Bounds.Top, Width - x, _infusionSlotTexture.Bounds.Height);

            _statTexture.Bounds = new(Icon.Bounds.Center.Add(new(-2, -2)), new(Icon.Bounds.Height / 2));
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);

            if (Template?.PvE == false)
            {
                _pvpSigilSlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
                _pvpSigilTexture.Draw(this, spriteBatch, RelativeMousePosition);
                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(PvpSigil?.DisplayText ?? string.Empty), UpgradeFont, _pvpSigilBounds, UpgradeColor, false, HorizontalAlignment.Left, VerticalAlignment.Middle);
            }
            else
            {
                _statTexture.Draw(this, spriteBatch);
                _sigilSlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
                _sigilTexture.Draw(this, spriteBatch, RelativeMousePosition);
                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Sigil?.DisplayText ?? string.Empty), UpgradeFont, _sigilBounds, UpgradeColor, false, HorizontalAlignment.Left, VerticalAlignment.Middle);

                _infusionSlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
                _infusionTexture.Draw(this, spriteBatch, RelativeMousePosition);
                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Infusion?.DisplayText ?? string.Empty), InfusionFont, _infusionBounds, InfusionColor, true, HorizontalAlignment.Left, VerticalAlignment.Middle);
            }
        }

        protected override void OnStatChanged()
        {
            base.OnStatChanged();

            _statTexture.Texture = Stat?.Icon;
        }

        protected override void SlotChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.SlotChanged(sender, e);

            var weapon = TemplateSlot as WeaponEntry;

            Item.Item = weapon.Item;

            Sigil = weapon.Sigil;
            _sigilTexture.Item = weapon.Sigil;

            Sigil = weapon.Sigil;
            _sigilTexture.Item = weapon.Sigil;

            PvpSigil = weapon.PvpSigil;
            _pvpSigilTexture.Item = weapon.PvpSigil;

            Infusion = weapon.Infusion;
            _infusionTexture.Item = weapon.Infusion;

            _statTexture.Texture = Stat?.Icon;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var a = AbsoluteBounds;

            if (_sigilSlotTexture.Hovered)
                SelectionPanel?.SetItemIdAnchor(this, new Rectangle(a.Location, Point.Zero).Add(_sigilSlotTexture.Bounds), GearSlot.ToString().ToLowercaseNamingConvention());

            if (_infusionSlotTexture.Hovered)
                SelectionPanel?.SetItemIdAnchor(this, new Rectangle(a.Location, Point.Zero).Add(_infusionSlotTexture.Bounds), GearSlot.ToString().ToLowercaseNamingConvention());

            if (_pvpSigilSlotTexture.Hovered)
                SelectionPanel?.SetItemIdAnchor(this, new Rectangle(a.Location, Point.Zero).Add(_pvpSigilSlotTexture.Bounds), GearSlot.ToString().ToLowercaseNamingConvention());
        }
    }

    public class AquaticSlotControl : GearSlotControl
    {
        private readonly DetailedTexture _sigilSlotTexture = new() { Texture = AsyncTexture2D.FromAssetId(784324), TextureRegion = new(37, 37, 54, 54), };
        private readonly DetailedTexture _sigil2SlotTexture = new() { Texture = AsyncTexture2D.FromAssetId(784324), TextureRegion = new(37, 37, 54, 54), };
        private readonly DetailedTexture _infusionSlotTexture = new() { TextureRegion = new(37, 37, 54, 54) };
        private readonly DetailedTexture _infusion2SlotTexture = new() { TextureRegion = new(37, 37, 54, 54) };

        private readonly ItemTexture _sigilTexture = new() { };
        private readonly ItemTexture _sigil2Texture = new() { };
        private readonly ItemTexture _infusionTexture = new() { };
        private readonly ItemTexture _infusion2Texture = new() { };
        private readonly DetailedTexture _statTexture = new() { };

        private Rectangle _statBounds;
        private Rectangle _sigilBounds;
        private Rectangle _sigil2Bounds;
        private Rectangle _infusionBounds;
        private Rectangle _infusion2Bounds;

        public AquaticSlotControl(GearTemplateSlot gearSlot, Container parent) : base(gearSlot, parent)
        {
            _infusionSlotTexture.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
            _infusion2SlotTexture.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");

            UpgradeFont = Content.DefaultFont16;
            InfusionFont = Content.DefaultFont12;

            MaxTextLength = 28;
        }

        public Sigil Sigil { get; private set; }

        public Sigil Sigil2 { get; private set; }

        public Infusion Infusion { get; private set; }

        public Infusion Infusion2 { get; private set; }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int upgradeSize = (Icon.Bounds.Size.Y - 4) / 2;
            int iconPadding = GearSlot is GearTemplateSlot.OffHand or GearTemplateSlot.AltOffHand ? 7 : 0;
            int textPadding = GearSlot is GearTemplateSlot.OffHand or GearTemplateSlot.AltOffHand ? 8 : 5;

            _sigilSlotTexture.Bounds = new(Icon.Bounds.Right + 2 + iconPadding, 0, upgradeSize, upgradeSize);
            _sigilTexture.Bounds = _sigilSlotTexture.Bounds;

            _sigil2SlotTexture.Bounds = new(Icon.Bounds.Right + 2 + iconPadding + upgradeSize + 2, 0, upgradeSize, upgradeSize);
            _sigil2Texture.Bounds = _sigil2SlotTexture.Bounds;

            _infusionSlotTexture.Bounds = new(Icon.Bounds.Right + 2 + iconPadding, _sigilSlotTexture.Bounds.Bottom + 4, upgradeSize, upgradeSize);
            _infusionTexture.Bounds = _infusionSlotTexture.Bounds;

            _infusion2SlotTexture.Bounds = new(Icon.Bounds.Right + 2 + iconPadding + upgradeSize + 2, _sigilSlotTexture.Bounds.Bottom + 4, upgradeSize, upgradeSize);
            _infusion2Texture.Bounds = _infusion2SlotTexture.Bounds;

            int upgradeWidth = (Width - (_sigil2SlotTexture.Bounds.Right + 2)) / 2;
            int x = _sigil2SlotTexture.Bounds.Right + textPadding + 4;
            _sigilBounds = new(x, _sigilSlotTexture.Bounds.Top , upgradeWidth, _sigilSlotTexture.Bounds.Height);
            _sigil2Bounds = new(x + upgradeWidth, _sigilSlotTexture.Bounds.Top , upgradeWidth, _sigilSlotTexture.Bounds.Height);

            _infusionBounds = new(x, _infusionSlotTexture.Bounds.Top, upgradeWidth, _infusionSlotTexture.Bounds.Height);
            _infusion2Bounds = new(x + upgradeWidth, _infusionSlotTexture.Bounds.Top, upgradeWidth, _infusionSlotTexture.Bounds.Height);

            _statTexture.Bounds = new(Icon.Bounds.Center.Add(new(-2, -2)), new(Icon.Bounds.Height / 2));
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Template?.PvE == false)
            {

            }
            else
            {
                base.Paint(spriteBatch, bounds);

                _statTexture.Draw(this, spriteBatch);

                _sigilSlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
                _sigilTexture.Draw(this, spriteBatch, RelativeMousePosition);
                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Sigil?.DisplayText ?? string.Empty), UpgradeFont, _sigilBounds, UpgradeColor, false, HorizontalAlignment.Left, VerticalAlignment.Middle);

                _sigil2SlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
                _sigil2Texture.Draw(this, spriteBatch, RelativeMousePosition);
                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Sigil2?.DisplayText ?? string.Empty), UpgradeFont, _sigil2Bounds, UpgradeColor, false, HorizontalAlignment.Left, VerticalAlignment.Middle);

                _infusionSlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
                _infusionTexture.Draw(this, spriteBatch, RelativeMousePosition);
                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Infusion?.DisplayText ?? string.Empty), InfusionFont, _infusionBounds, InfusionColor, true, HorizontalAlignment.Left, VerticalAlignment.Middle);

                _infusion2SlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
                _infusion2Texture.Draw(this, spriteBatch, RelativeMousePosition);
                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Infusion2?.DisplayText ?? string.Empty), InfusionFont, _infusion2Bounds, InfusionColor, true, HorizontalAlignment.Left, VerticalAlignment.Middle);
            }
        }

        protected override void OnStatChanged()
        {
            base.OnStatChanged();

            _statTexture.Texture = Stat?.Icon;
        }

        protected override void SlotChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.SlotChanged(sender, e);

            var weapon = TemplateSlot as WeaponEntry;

            Item.Item = weapon.Item;

            Sigil = weapon.Sigil;
            _sigilTexture.Item = weapon.Sigil;

            Sigil = weapon.Sigil;
            _sigilTexture.Item = weapon.Sigil;

            Sigil2 = weapon.Sigil2;
            _sigil2Texture.Item = weapon.Sigil2;

            Infusion = weapon.Infusion;
            _infusionTexture.Item = weapon.Infusion;

            Infusion2 = weapon.Infusion2;
            _infusion2Texture.Item = weapon.Infusion2;

            _statTexture.Texture = Stat?.Icon;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var a = AbsoluteBounds;

            if (_sigilSlotTexture.Hovered)
                SelectionPanel?.SetItemIdAnchor(this, new Rectangle(a.Location, Point.Zero).Add(_sigilSlotTexture.Bounds), GearSlot.ToString().ToLowercaseNamingConvention());

            if (_sigil2SlotTexture.Hovered)
                SelectionPanel?.SetItemIdAnchor(this, new Rectangle(a.Location, Point.Zero).Add(_sigil2SlotTexture.Bounds), GearSlot.ToString().ToLowercaseNamingConvention());

            if (_infusionSlotTexture.Hovered)
                SelectionPanel?.SetItemIdAnchor(this, new Rectangle(a.Location, Point.Zero).Add(_infusionSlotTexture.Bounds), GearSlot.ToString().ToLowercaseNamingConvention());

            if (_infusion2SlotTexture.Hovered)
                SelectionPanel?.SetItemIdAnchor(this, new Rectangle(a.Location, Point.Zero).Add(_infusion2SlotTexture.Bounds), GearSlot.ToString().ToLowercaseNamingConvention());
        }
    }

    public class ArmorSlotControl : GearSlotControl
    {
        private readonly DetailedTexture _runeSlotTexture = new() { Texture = AsyncTexture2D.FromAssetId(784323), TextureRegion = new(37, 37, 54, 54), };
        private readonly DetailedTexture _infusionSlotTexture = new() { TextureRegion = new(37, 37, 54, 54) };

        private readonly ItemTexture _runeTexture = new() { };
        private readonly ItemTexture _infusionTexture = new() { };
        private readonly DetailedTexture _statTexture = new() { };

        private Rectangle _statBounds;
        private Rectangle _runeBounds;
        private Rectangle _infusionBounds;

        public ArmorSlotControl(GearTemplateSlot gearSlot, Container parent) : base(gearSlot, parent)
        {
            _infusionSlotTexture.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
        }

        public Rune Rune { get; private set; }

        public Infusion Infusion { get; private set; }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int upgradeSize = (Icon.Bounds.Size.Y - 4) / 2;
            int iconPadding = 0;
            int textPadding = GearSlot is GearTemplateSlot.AquaBreather ? upgradeSize + 5 : 5;

            _runeSlotTexture.Bounds = new(Icon.Bounds.Right + 2 + iconPadding, 0, upgradeSize, upgradeSize);
            _runeTexture.Bounds = _runeSlotTexture.Bounds;

            _infusionSlotTexture.Bounds = new(Icon.Bounds.Right + 2 + iconPadding, _runeSlotTexture.Bounds.Bottom + 4, upgradeSize, upgradeSize);
            _infusionTexture.Bounds = _infusionSlotTexture.Bounds;

            int x = _runeSlotTexture.Bounds.Right + textPadding + 4;
            _runeBounds = new(x, _runeSlotTexture.Bounds.Top - 1, Width - x, _runeSlotTexture.Bounds.Height);
            _infusionBounds = new(x, _infusionSlotTexture.Bounds.Top, Width - x, _infusionSlotTexture.Bounds.Height);

            _statTexture.Bounds = new(Icon.Bounds.Center.Add(new(-2, -2)), new(Icon.Bounds.Height / 2));
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Template?.PvE != false)
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

        protected override void OnStatChanged()
        {
            base.OnStatChanged();

            _statTexture.Texture = Stat?.Icon;
        }

        protected override void SlotChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.SlotChanged(sender, e);

            var armor = TemplateSlot as ArmorEntry;

            Item.Item = armor.Item;

            Rune = armor.Rune;
            _runeTexture.Item = armor.Rune;

            Infusion = armor.Infusion;
            _infusionTexture.Item = armor.Infusion;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var a = AbsoluteBounds;

            if (_runeSlotTexture.Hovered)
                SelectionPanel?.SetItemIdAnchor(this, new Rectangle(a.Location, Point.Zero).Add(_runeSlotTexture.Bounds), GearSlot.ToString().ToLowercaseNamingConvention());

            if (_infusionSlotTexture.Hovered)
                SelectionPanel?.SetItemIdAnchor(this, new Rectangle(a.Location, Point.Zero).Add(_infusionSlotTexture.Bounds), GearSlot.ToString().ToLowercaseNamingConvention());
        }
    }

    public class JuwellerySlotControl : GearSlotControl
    {
        private readonly DetailedTexture _infusion1SlotTexture = new() { TextureRegion = new(37, 37, 54, 54) };
        private readonly DetailedTexture _infusion2SlotTexture = new() { TextureRegion = new(37, 37, 54, 54) };
        private readonly DetailedTexture _infusion3SlotTexture = new() { TextureRegion = new(37, 37, 54, 54) };

        private readonly ItemTexture _infusion1Texture = new() { };
        private readonly ItemTexture _infusion2Texture = new() { };
        private readonly ItemTexture _infusion3Texture = new() { };

        private readonly DetailedTexture _statTexture = new() { };

        public JuwellerySlotControl(GearTemplateSlot gearSlot, Container parent) : base(gearSlot, parent)
        {
            _infusion1SlotTexture.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
            _infusion2SlotTexture.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
            _infusion3SlotTexture.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
            ItemColor = Color.Gray;
        }

        public Infusion Infusion1 { get; private set; }

        public Infusion Infusion2 { get; private set; }

        public Infusion Infusion3 { get; private set; }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int infusionSize = (Icon.Bounds.Size.Y - 4) / 3;

            _infusion1SlotTexture.Bounds = new(Icon.Bounds.Right + 2, Icon.Bounds.Top, infusionSize, infusionSize);
            _infusion1Texture.Bounds = _infusion1SlotTexture.Bounds;

            _infusion2SlotTexture.Bounds = new(Icon.Bounds.Right + 2, Icon.Bounds.Top + ((infusionSize + 2) * 1), infusionSize, infusionSize);
            _infusion2Texture.Bounds = _infusion2SlotTexture.Bounds;

            _infusion3SlotTexture.Bounds = new(Icon.Bounds.Right + 2, Icon.Bounds.Top + ((infusionSize + 2) * 2), infusionSize, infusionSize);
            _infusion3Texture.Bounds = _infusion3SlotTexture.Bounds;

            _statTexture.Bounds = new(Icon.Bounds.Center.Add(new(-2, -2)), new(Icon.Bounds.Height / 2));
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Template?.PvE != false)
            {
                base.Paint(spriteBatch, bounds);

                _infusion1SlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
                _infusion1Texture.Draw(this, spriteBatch, RelativeMousePosition);

                if (GearSlot is GearTemplateSlot.Ring_1 or GearTemplateSlot.Ring_2 or GearTemplateSlot.Back)
                {
                    _infusion2SlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
                    _infusion2Texture.Draw(this, spriteBatch, RelativeMousePosition);
                }

                if (GearSlot is GearTemplateSlot.Ring_1 or GearTemplateSlot.Ring_2)
                {
                    _infusion3SlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
                    _infusion3Texture.Draw(this, spriteBatch, RelativeMousePosition);
                }

                _statTexture.Draw(this, spriteBatch, RelativeMousePosition);
            }
        }

        protected override void OnStatChanged()
        {
            base.OnStatChanged();

            _statTexture.Texture = Stat?.Icon;
        }

        protected override void SlotChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.SlotChanged(sender, e);

            var item = TemplateSlot as JuwelleryEntry;

            Item.Item = item.Item;

            Infusion1 = item.Infusion;
            _infusion1Texture.Item = item.Infusion;

            Infusion2 = item.Infusion2;
            _infusion2Texture.Item = item.Infusion;

            Infusion3 = item.Infusion3;
            _infusion3Texture.Item = item.Infusion;

            _statTexture.Texture = Stat?.Icon;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var a = AbsoluteBounds;

            if (_infusion1SlotTexture.Hovered)
                SelectionPanel?.SetItemIdAnchor(this, new Rectangle(a.Location, Point.Zero).Add(_infusion1SlotTexture.Bounds), GearSlot.ToString().ToLowercaseNamingConvention());

            if (_infusion2SlotTexture.Hovered)
                SelectionPanel?.SetItemIdAnchor(this, new Rectangle(a.Location, Point.Zero).Add(_infusion2SlotTexture.Bounds), GearSlot.ToString().ToLowercaseNamingConvention());

            if (_infusion3SlotTexture.Hovered)
                SelectionPanel?.SetItemIdAnchor(this, new Rectangle(a.Location, Point.Zero).Add(_infusion3SlotTexture.Bounds), GearSlot.ToString().ToLowercaseNamingConvention());
        }
    }

    public class AmuletSlotControl : GearSlotControl
    {
        private readonly DetailedTexture _enrichmentSlotTexture = new() { TextureRegion = new(37, 37, 54, 54) };
        private readonly ItemTexture _enrichmentTexture = new() { };

        private readonly DetailedTexture _statTexture = new() { };

        public AmuletSlotControl(GearTemplateSlot gearSlot, Container parent) : base(gearSlot, parent)
        {
            _enrichmentSlotTexture.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
            ItemColor = Color.Gray;
        }

        public Enrichment Enrichment { get; private set; }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int infusionSize = (Icon.Bounds.Size.Y - 4) / 3;

            _enrichmentSlotTexture.Bounds = new(Icon.Bounds.Right + 2, Icon.Bounds.Top, infusionSize, infusionSize);
            _enrichmentTexture.Bounds = _enrichmentSlotTexture.Bounds;

            _statTexture.Bounds = new(Icon.Bounds.Center.Add(new(-2, -2)), new(Icon.Bounds.Height / 2));
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Template?.PvE != false)
            {
                base.Paint(spriteBatch, bounds);

                _enrichmentSlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
                _enrichmentTexture.Draw(this, spriteBatch, RelativeMousePosition);

                _statTexture.Draw(this, spriteBatch, RelativeMousePosition);
            }
        }

        protected override void OnStatChanged()
        {
            base.OnStatChanged();

            _statTexture.Texture = Stat?.Icon;
        }

        protected override void SlotChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.SlotChanged(sender, e);

            var item = TemplateSlot as JuwelleryEntry;

            Item.Item = item.Item;

            Enrichment = item.Enrichment;
            _enrichmentTexture.Item = item.Enrichment;

            _statTexture.Texture = Stat?.Icon;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var a = AbsoluteBounds;

            if (_enrichmentSlotTexture.Hovered)
                SelectionPanel?.SetItemIdAnchor(this, new Rectangle(a.Location, Point.Zero).Add(_enrichmentSlotTexture.Bounds), GearSlot.ToString().ToLowercaseNamingConvention());
        }
    }

    public class PvpAmuletSlotControl : GearSlotControl
    {
        public PvpAmuletSlotControl(GearTemplateSlot gearSlot, Container parent) : base(gearSlot, parent)
        {
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);

        }
    }

    public class JadeBotControl : GearSlotControl
    {
        public JadeBotControl(GearTemplateSlot gearSlot, Container parent) : base(gearSlot, parent)
        {
            Icon.Texture = AsyncTexture2D.FromAssetId(2630946);
            Icon.TextureRegion = new(36, 36, 56, 56);
            Size = new(45, 45);
        }
    }

    public class UtilityControl : GearSlotControl
    {
        public UtilityControl(GearTemplateSlot gearSlot, Container parent) : base(gearSlot, parent)
        {
            Icon.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\utilityslot.png");
            Size = new(45, 45);
        }
    }

    public class NourishmentControl : GearSlotControl
    {
        public NourishmentControl(GearTemplateSlot gearSlot, Container parent) : base(gearSlot, parent)
        {
            Icon.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\foodslot.png");
            Size = new(45, 45);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);

        }
    }

    public class GearSlotControl : Control
    {
        private GearTemplateSlot _gearSlot = GearTemplateSlot.None;
        private Template _template;

        protected readonly DetailedTexture Icon = new() { TextureRegion = new(37, 37, 54, 54) };
        protected readonly ItemTexture Item = new() { };

        protected int MaxTextLength = 52;

        protected Color StatColor = Color.White;
        protected Color UpgradeColor = Color.Orange;
        protected Color InfusionColor = new(153, 238, 221);
        protected Color ItemColor = Color.Gray;

        protected BitmapFont StatFont = Content.DefaultFont16;
        protected BitmapFont UpgradeFont = Content.DefaultFont18;
        protected BitmapFont InfusionFont = Content.DefaultFont12;

        private GearTemplateEntry _templateSlot;
        private Stat _stat;

        public GearTemplateSlot GearSlot { get => _gearSlot; set => Common.SetProperty(ref _gearSlot, value, ApplySlot); }

        public GearSlotControl()
        {
            Size = new(380, 64);
            ClipsBounds = false;
        }

        public GearSlotControl(GearTemplateSlot gearSlot, Container parent) : this()
        {
            GearSlot = gearSlot;
            Parent = parent;
        }

        public Action ClickAction { get; set; }

        public Template Template
        {
            get => _template; set
            {
                var temp = _template;
                if (Common.SetProperty(ref _template, value, ApplyTemplate, value != null))
                {
                    if (temp != null) temp.Changed -= TemplateChanged;
                    if (_template != null) _template.Changed += TemplateChanged;
                }
            }
        }

        public GearTemplateEntry TemplateSlot
        {
            get => _templateSlot;
            set
            {
                var temp = _templateSlot;
                if (Common.SetProperty(ref _templateSlot, value))
                {
                    if (temp != null) temp.PropertyChanged -= SlotChanged;
                    if (_templateSlot != null)
                    {
                        _templateSlot.PropertyChanged += SlotChanged;
                        SlotChanged(this, new(nameof(TemplateSlot)));
                    }
                }
            }
        }

        protected virtual void SlotChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }

        public SelectionPanel SelectionPanel { get; set; }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        protected virtual void OnStatChanged()
        {

        }

        public void ApplyTemplate()
        {
            if (GearSlot.IsArmor())
            {
                TemplateSlot = Template?.GearTemplate.Armors[GearSlot];
            }
            else if (GearSlot.IsWeapon())
            {
                TemplateSlot = Template?.GearTemplate.Weapons[GearSlot];
            }
            else if (GearSlot.IsJuwellery())
            {
                TemplateSlot = Template?.GearTemplate.Juwellery[GearSlot];
            }
            else if (GearSlot == GearTemplateSlot.PvpAmulet)
            {
                //TemplateSlot = Template?.GearTemplate.PvpAmulet[GearSlot];
            }
            else
            {
                TemplateSlot = Template?.GearTemplate.Common[GearSlot];
            }

            RecalculateLayout();

            if (GearSlot.IsArmor())
            {
                Item.Item = Template?.GearTemplate.Armors[GearSlot].Item;
                Stat = Template?.GearTemplate.Armors[GearSlot].Stat != null && BuildsManager.Data.Stats.TryGetValue((int)Template?.GearTemplate.Armors[GearSlot].Stat, out Stat stat) ? stat : null;
            }
            else if (GearSlot.IsWeapon())
            {
                Item.Item = Template?.GearTemplate.Weapons[GearSlot].Item;
                Stat = Template?.GearTemplate.Weapons[GearSlot].Stat != null && BuildsManager.Data.Stats.TryGetValue((int)Template?.GearTemplate.Weapons[GearSlot].Stat, out Stat stat) ? stat : null;
            }
            else if (GearSlot.IsJuwellery())
            {
                Item.Item = Template?.GearTemplate.Juwellery[GearSlot].Item;
                Stat = Template?.GearTemplate.Juwellery[GearSlot].Stat != null && BuildsManager.Data.Stats.TryGetValue((int)Template?.GearTemplate.Juwellery[GearSlot].Stat, out Stat stat) ? stat : null;
            }
            else if (GearSlot is GearTemplateSlot.PvpAmulet)
            {
                //Item.Item = Template?.GearTemplate.PvpAmulet[GearSlot].Item;
                //if (Template != null) Upgrade1 = BuildsManager.Data.PvpRunes.Where(e => e.Value.MappedId == Template.GearTemplate.PvpAmulet[GearSlot].RuneIds[0]).FirstOrDefault().Value;
                //Stat = Template?.GearTemplate.PvpAmulet[GearSlot].Stat != null && BuildsManager.Data.Stats.TryGetValue((int)Template?.GearTemplate.PvpAmulet[GearSlot].Stat, out Stat stat) ? stat : null;
                Stat = null;
            }
            else
            {
                Item.Item = Template?.GearTemplate.Common[GearSlot].Item;
                Stat = null;
            }
        }

        private void TemplateChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ApplyTemplate();
        }

        protected virtual void ApplySlot()
        {
            var assetIds = new Dictionary<GearTemplateSlot, int>()
            {
                {GearTemplateSlot.AquaBreather, 156308},
                {GearTemplateSlot.Head, 156307},
                {GearTemplateSlot.Shoulder, 156311},
                {GearTemplateSlot.Chest, 156297},
                {GearTemplateSlot.Hand, 156306},
                {GearTemplateSlot.Leg, 156309},
                {GearTemplateSlot.Foot, 156300},
                {GearTemplateSlot.MainHand, 156316},
                {GearTemplateSlot.OffHand, 156320},
                {GearTemplateSlot.Aquatic, 156313},
                {GearTemplateSlot.AltMainHand, 156316},
                {GearTemplateSlot.AltOffHand, 156320},
                {GearTemplateSlot.AltAquatic, 156313},
                {GearTemplateSlot.Back, 156293},
                {GearTemplateSlot.Amulet, 156310},
                {GearTemplateSlot.Accessory_1, 156298},
                {GearTemplateSlot.Accessory_2, 156299},
                {GearTemplateSlot.Ring_1, 156301},
                {GearTemplateSlot.Ring_2, 156302},
                {GearTemplateSlot.PvpAmulet, 784322},
            };

            if (assetIds.TryGetValue(GearSlot, out int assetId))
            {
                Icon.Texture = AsyncTexture2D.FromAssetId(assetId);
            }

            RecalculateLayout();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int size = Math.Min(Width, Height);
            Icon.Bounds = new(0, 0, size, size);
            Item.Bounds = new(0, 0, size, size);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            Icon.Draw(this, spriteBatch, RelativeMousePosition);
            Item.Draw(this, spriteBatch, RelativeMousePosition, ItemColor);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            var a = AbsoluteBounds;

            ClickAction?.Invoke();

            if (Icon.Hovered)
                SelectionPanel?.SetGearAnchor(this, new Rectangle(a.Location, Point.Zero).Add(Icon.Bounds), GearSlot, GearSlot.ToString().ToLowercaseNamingConvention());
        }

        protected string GetDisplayString(string s)
        {
            return s.Length > MaxTextLength ? s.Substring(0, MaxTextLength) + "..." : s;
        }
    }
}
