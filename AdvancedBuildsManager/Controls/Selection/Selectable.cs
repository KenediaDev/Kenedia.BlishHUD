using Blish_HUD;
using Blish_HUD.Input;
using Kenedia.Modules.AdvancedBuildsManager.DataModels.Items;
using Kenedia.Modules.AdvancedBuildsManager.Models.Templates;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel;

namespace Kenedia.Modules.AdvancedBuildsManager.Controls.Selection
{
    public class Selectable : Blish_HUD.Controls.Control
    {
        private Rectangle _iconBounds;
        private Rectangle _nameBounds;
        private Rectangle _descriptionBounds;

        private Color RarityColor = Color.White;

        public Selectable()
        {
            Size = new(64, 64);
            BackgroundColor = Color.Black * 0.2F;
        }

        public enum TargetType
        {
            Single,
            Group,
            GroupEmpty,
            All,
            AllEmpty,
        }

        public enum SelectableType
        {
            None,
            Rune,
            Sigil,
            Infusion,
            Stat
        }

        public SelectableType Type { get; set => Common.SetProperty(ref field , value, OnTypeChanged); }

        public Action OnClickAction { get; set; }

        public GearTemplateSlot TemplateSlot { get; set; }

        public BaseItem Item { get; set => Common.SetProperty(ref field, value, SetItem); }

        public Template Template { get; set => Common.SetProperty(ref field, value); }

        public GearTemplateSlot ActiveSlot { get; set; }

        public GearSubSlotType SubSlotType { get; set; }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _iconBounds = new Rectangle(3, 3, Height - 6, Height - 6);
            _nameBounds = new Rectangle(_iconBounds.Right + 10, 3, Width - _iconBounds.Right, Content.DefaultFont16.LineHeight * 2);
            _descriptionBounds = new Rectangle(_nameBounds.Left, _nameBounds.Bottom + 3, _nameBounds.Width, Height - 3 - _nameBounds.Bottom);
        }
        
        protected virtual void OnTypeChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            RecalculateLayout();

            if (Item is not null)
            {
                spriteBatch.DrawFrame(this, bounds, Item.Rarity.GetColor(), 2);

                if (Item.Icon is not null)
                {
                    spriteBatch.DrawOnCtrl(this, Item.Icon, _iconBounds);
                }

                if (!string.IsNullOrEmpty(Item.Name))
                {
                    //spriteBatch.DrawStringOnCtrl(this, Item.Name, Content.DefaultFont16, _nameBounds, Item.Rarity.GetColor(), true, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Top);
                }

                if (!string.IsNullOrEmpty(Item.Description))
                {
                    //spriteBatch.DrawStringOnCtrl(this, Item.Description, Content.DefaultFont12, _descriptionBounds, Color.White, true);
                }
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            OnClickAction?.Invoke();
        }

        private void SetItem()
        {
            string txt = Item == null ? null : Item.Name + $" [{Item.Id}] " + (Item.Type == Gw2Sharp.WebApi.V2.Models.ItemType.Weapon ? Environment.NewLine + (Item as Weapon).WeaponType.ToSkillWeapon() : string.Empty);

            if (Item is not null && !string.IsNullOrEmpty(Item.DisplayText))
            {
                txt += Environment.NewLine + Item.Description;
            }

            BasicTooltipText = txt;
        }
    }
}
