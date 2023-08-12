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
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.BuildsManager.TemplateEntries;
using static Kenedia.Modules.BuildsManager.Controls.Selection.SelectionPanel;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage.GearSlots
{
    public class PvpAmuletSlot : GearSlot
    {
        private Rectangle _titleBounds;
        private readonly DetailedTexture _runeSlotTexture = new() { Texture = AsyncTexture2D.FromAssetId(784323), TextureRegion = new(37, 37, 54, 54), };

        private readonly ItemTexture _runeTexture = new() { };

        private Rune _rune;
        private Rectangle _runeBounds;

        public PvpAmuletSlot(TemplateSlotType gearSlot, Container parent, TemplatePresenter templatePresenter) : base(gearSlot, parent, templatePresenter)
        {
            ClipsBounds = false;
        }

        public Rune Rune { get => _rune; set => Common.SetProperty(ref _rune, value, OnRuneChanged); }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int upgradeSize = (Icon.Bounds.Size.Y - 4) / 2;
            int iconPadding = 0;
            int textPadding = Slot is TemplateSlotType.AquaBreather ? upgradeSize + 5 : 5;

            int pvpUpgradeSize = 48;
            _runeSlotTexture.Bounds = new(Icon.Bounds.Right + 2 + 5 + iconPadding, (Icon.Bounds.Height - pvpUpgradeSize) / 2, pvpUpgradeSize, pvpUpgradeSize);
            _runeTexture.Bounds = _runeSlotTexture.Bounds;
            _runeBounds = new(_runeSlotTexture.Bounds.Right + 10, _runeTexture.Bounds.Top, Width - (_runeTexture.Bounds.Right + 2), _runeTexture.Bounds.Height);

            _titleBounds = new(_runeBounds.Left, _runeBounds.Top - (Content.DefaultFont16.LineHeight + 2), _runeBounds.Width, Content.DefaultFont16.LineHeight);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

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
}
