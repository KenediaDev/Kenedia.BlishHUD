﻿using Container = Blish_HUD.Controls.Container;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Blish_HUD.Content;
using Blish_HUD;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.BuildsManager.TemplateEntries;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.BuildsManager.Services;

namespace Kenedia.Modules.BuildsManager.Controls_Old.GearPage.GearSlots
{
    public class PowerCoreSlot : GearSlot
    {
        private Rectangle _titleBounds;
        private Rectangle _statBounds;

        private string _powerCoreName = strings.PowerCore;
        private string _powerCoreDescription;

        public PowerCoreSlot(TemplateSlotType gearSlot, Container parent, TemplatePresenter templatePresenter, Controls.Selection.SelectionPanel selectionPanel, Data data) 
            : base(gearSlot, parent, templatePresenter, selectionPanel, data)
        {
            ItemControl.Placeholder.Texture = AsyncTexture2D.FromAssetId(2630946);
            ItemControl.Placeholder.TextureRegion = new(38, 38, 52, 52);

            ItemColor = Color.White;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _titleBounds = new(ItemControl.LocalBounds.Right + 10, ItemControl.LocalBounds.Top + 2, Width - ItemControl.LocalBounds.Left - 20, Content.DefaultFont16.LineHeight);
            _statBounds = new(ItemControl.LocalBounds.Right + 10, _titleBounds.Bottom + 2, Width - ItemControl.LocalBounds.Left - 20, Content.DefaultFont12.LineHeight);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            spriteBatch.DrawStringOnCtrl(this, _powerCoreName, Content.DefaultFont16, _titleBounds, ItemControl?.Item?.Rarity.GetColor() ?? Color.White * 0.5F);
            spriteBatch.DrawStringOnCtrl(this, _powerCoreDescription, Content.DefaultFont12, _statBounds, Color.White, false, HorizontalAlignment.Left, VerticalAlignment.Top);
        }

        protected override void SetItemToSlotControl(object sender, TemplateSlotChangedEventArgs e)
        {
            base.SetItemToSlotControl(sender, e);

            SetItemFromTemplate();
        }

        protected override void SetItemFromTemplate()
        {
            base.SetItemFromTemplate();

            if (TemplatePresenter?.Template?[Slot] is PowerCoreTemplateEntry powerCore)
            {
                Item = powerCore.Item;
                _powerCoreName = powerCore?.PowerCore?.Name ?? strings.PowerCore;
                _powerCoreDescription = powerCore?.PowerCore?.Description ?? string.Empty;
            }
            else
            {
                _powerCoreName = strings.PowerCore;
                _powerCoreDescription = string.Empty;
            }
        }

        protected override void SetAnchor()
        {
            var a = AbsoluteBounds;

            if (ItemControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<PowerCore>(ItemControl, new Rectangle(a.Location, Point.Zero).Add(ItemControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Item, (powerCore) => TemplatePresenter.Template?.SetItem(Slot, TemplateSubSlotType.Item, powerCore));
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => strings.Reset, () => string.Format(strings.ResetEntry, strings.PowerCore), () => TemplatePresenter?.Template?.SetItem<PowerCore>(Slot, TemplateSubSlotType.Item, null));
        }
    }
}
