using Container = Blish_HUD.Controls.Container;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Utility;
using Blish_HUD;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.BuildsManager.TemplateEntries;
using Kenedia.Modules.BuildsManager.Res;

namespace Kenedia.Modules.BuildsManager.Controls_Old.GearPage.GearSlots
{
    public class PvpAmuletSlot : GearSlot
    {
        private Rectangle _titleBounds;
        private readonly ItemControl _runeControl = new(new(784323) { TextureRegion = new(38, 38, 52, 52) });

        private Rune? _rune;
        private Rectangle _runeBounds;

        public PvpAmuletSlot(TemplateSlotType gearSlot, Container parent, TemplatePresenter templatePresenter, Controls.Selection.SelectionPanel selectionPanel) : base(gearSlot, parent, templatePresenter, selectionPanel)
        {
            _runeControl.Parent = this;
            ClipsBounds = false;
        }

        public Rune? Rune { get => _rune; set => Common.SetProperty(ref _rune, value, OnRuneChanged); }

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

        protected override void SetItemToSlotControl(object sender, TemplateSlotChangedEventArgs e)
        {
            base.SetItemToSlotControl(sender, e);

            SetItemFromTemplate();
        }

        protected override void SetItemFromTemplate()
        {
            base.SetItemFromTemplate();

            if (TemplatePresenter?.Template?[Slot] is PvpAmuletTemplateEntry pvpAmulet)
            {
                Rune = pvpAmulet?.Rune;
                Item = pvpAmulet?.PvpAmulet;
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var a = AbsoluteBounds;

            if (ItemControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<PvpAmulet>(ItemControl, new Rectangle(a.Location, Point.Zero).Add(ItemControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Item, (pvpAmulet) => TemplatePresenter.Template?.SetItem(Slot, TemplateSubSlotType.Item, pvpAmulet));
            }

            if (_runeControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<Rune>(_runeControl, new Rectangle(a.Location, Point.Zero).Add(_runeControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Rune, (rune) => TemplatePresenter.Template?.SetItem(Slot, TemplateSubSlotType.Rune, rune));
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => strings.Reset, () => string.Format(strings.ResetEntry, $"{strings.Amulet} {strings.And} {strings.Rune}"), () =>
            {
                TemplatePresenter?.Template.SetItem<PvpAmulet>(Slot, TemplateSubSlotType.Item, null);
                TemplatePresenter?.Template.SetItem<Rune>(Slot, TemplateSubSlotType.Rune, null);
            },
            [
                new(() => strings.Amulet,() => string.Format(strings.ResetEntry, strings.Amulet),() => TemplatePresenter?.Template.SetItem<PvpAmulet>(Slot, TemplateSubSlotType.Item, null)),
                new(() => strings.Rune,() => string.Format(strings.ResetEntry, strings.Rune),() => TemplatePresenter?.Template.SetItem<Rune>(Slot, TemplateSubSlotType.Rune, null)),
            ]);
        }

        private void OnRuneChanged(object sender, Core.Models.ValueChangedEventArgs<Rune> e)
        {
            _runeControl.Item = Rune;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Rune = null;
        }
    }
}
