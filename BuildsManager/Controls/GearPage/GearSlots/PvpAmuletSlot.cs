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
        private readonly ItemControl _runeControl = new(new(784323) { TextureRegion = new(38, 38, 52, 52) });

        private Rune _rune;
        private Rectangle _runeBounds;

        public PvpAmuletSlot(TemplateSlotType gearSlot, Container parent, TemplatePresenter templatePresenter) : base(gearSlot, parent, templatePresenter)
        {
            _runeControl.Parent = this;
            ClipsBounds = false;
        }

        public Rune Rune { get => _rune; set => Common.SetProperty(ref _rune, value, OnRuneChanged); }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int upgradeSize = (ItemControl.LocalBounds.Size.Y - 4) / 2;
            int iconPadding = 0;
            int textPadding = Slot is TemplateSlotType.AquaBreather ? upgradeSize + 5 : 5;

            int pvpUpgradeSize = 48;
            _runeControl.SetBounds(new(ItemControl.LocalBounds.Right + 2 + 5 + iconPadding, (ItemControl.LocalBounds.Height - pvpUpgradeSize) / 2, pvpUpgradeSize, pvpUpgradeSize));

            _runeBounds = new(_runeControl.Right + 10, _runeControl.Top, Width - (_runeControl.Right + 2), _runeControl.Height);
            _titleBounds = new(_runeBounds.Left, _runeBounds.Top - (Content.DefaultFont16.LineHeight + 2), _runeBounds.Width, Content.DefaultFont16.LineHeight);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

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

            if (ItemControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<PvpAmulet>(this, new Rectangle(a.Location, Point.Zero).Add(ItemControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Item, (pvpAmulet) =>
                {
                    (TemplatePresenter?.Template[Slot] as PvpAmuletTemplateEntry).PvpAmulet = pvpAmulet;
                    Item = pvpAmulet;
                });
            }

            if (_runeControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<Rune>(this, new Rectangle(a.Location, Point.Zero).Add(_runeControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Rune, (rune) =>
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
            _runeControl.Item = Rune;
        }
    }
}
