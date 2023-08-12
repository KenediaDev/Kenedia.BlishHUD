﻿using Container = Blish_HUD.Controls.Container;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Blish_HUD;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.TemplateEntries;
using static Kenedia.Modules.BuildsManager.Controls.Selection.SelectionPanel;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage.GearSlots
{
    public class RelicSlot : GearSlot
    {
        private Rectangle _titleBounds;
        private Rectangle _statBounds;

        public RelicSlot(TemplateSlotType gearSlot, Container parent, TemplatePresenter templatePresenter) : base(gearSlot, parent, templatePresenter)
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

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            spriteBatch.DrawStringOnCtrl(this, ItemControl?.Item?.Name ?? "Relic", Content.DefaultFont16, _titleBounds, ItemControl?.Item?.Rarity.GetColor() ?? Color.White * 0.5F);
            spriteBatch.DrawStringOnCtrl(this, (ItemControl?.Item as DataModels.Items.Utility)?.Details.Description ?? ItemControl?.Item?.Description ?? "Currently not available", Content.DefaultFont12, _statBounds, Color.White, false, HorizontalAlignment.Left, VerticalAlignment.Top);
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
