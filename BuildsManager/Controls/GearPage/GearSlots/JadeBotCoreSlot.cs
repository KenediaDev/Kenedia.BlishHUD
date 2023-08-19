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
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.TemplateEntries;
using static Kenedia.Modules.BuildsManager.Controls.Selection.SelectionPanel;
using Kenedia.Modules.BuildsManager.Res;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage.GearSlots
{
    public class JadeBotCoreSlot : GearSlot
    {
        private Rectangle _titleBounds;
        private Rectangle _statBounds;

        public JadeBotCoreSlot(TemplateSlotType gearSlot, Container parent, TemplatePresenter templatePresenter) : base(gearSlot, parent, templatePresenter)
        {
            ItemControl.Placeholder.Texture = AsyncTexture2D.FromAssetId(2630946);
            ItemControl.Placeholder.TextureRegion = new(38, 38, 52, 52);

            ItemColor = Color.White;
            ItemControl.Tooltip = null;
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

            spriteBatch.DrawStringOnCtrl(this, ItemControl?.Item?.Name ?? strings.JadeBotCore, Content.DefaultFont16, _titleBounds, ItemControl?.Item?.Rarity.GetColor() ?? Color.White * 0.5F);
            spriteBatch.DrawStringOnCtrl(this, (ItemControl?.Item as DataModels.Items.Enhancement)?.Details.Description ?? ItemControl?.Item?.Description ?? strings.CurrentlyNotAvailable, Content.DefaultFont12, _statBounds, Color.White, false, HorizontalAlignment.Left, VerticalAlignment.Top);
        }

        protected override void SetItems(object sender, EventArgs e)
        {
            base.SetItems(sender, e);

            var jadebotcore = TemplatePresenter?.Template?[Slot] as JadeBotTemplateEntry;
            Item = jadebotcore?.JadeBotCore;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            return;

            var a = AbsoluteBounds;

            if (ItemControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<JadeBotCore>(this, new Rectangle(a.Location, Point.Zero).Add(ItemControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Item, (jadeBotCore) =>
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
            CreateSubMenu(() => strings.Reset, () => string.Format(strings.ResetEntry, strings.JadeBotCore), () => Item = null);
        }
    }
}
