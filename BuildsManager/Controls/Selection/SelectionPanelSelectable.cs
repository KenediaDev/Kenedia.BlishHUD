using Blish_HUD;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Controls_Old.GearPage;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class SelectionPanelSelectable : Panel
    {
        private readonly ItemControl _itemControl = new() { CaptureInput  = false,};
        private Rectangle _iconBounds;
        private Rectangle _nameBounds;
        private Rectangle _descriptionBounds;

        private Color _rarityColor = Color.White;
        private Color _fontColor;

        public SelectionPanelSelectable()
        {
            Height = 64;
            BackgroundColor = Color.Black * 0.2F;
            BorderColor = Color.Black;
            BorderWidth = new(2);            

            _itemControl.Parent = this;
            _itemControl.Location = new(BorderWidth.Left, BorderWidth.Top);
            _itemControl.Size = new(Height - BorderWidth.Vertical);

            Tooltip = new ItemTooltip()
            {
                SetLocalizedComment = () => Environment.NewLine + strings.ItemControlClickToCopyItem,
            };
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

        public TemplateSlotType TemplateSlot { get; set; }

        public BaseItem Item { get; set => Common.SetProperty(field, value, v => field = v, SetItem); }

        public TemplateSlotType ActiveSlot { get; set; }

        public GearSubSlotType SubSlotType { get; set; }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _itemControl.Size = new(Height - BorderWidth.Vertical);

            _iconBounds = new Rectangle(3, 3, Height - 6, Height - 6);
            _nameBounds = new Rectangle(_itemControl.Right + 5, 0, Width - _itemControl.Right, Height);
            _descriptionBounds = new Rectangle(_nameBounds.Left, _nameBounds.Bottom + 3, _nameBounds.Width, Height - 3 - _nameBounds.Bottom);
        }
        
        protected virtual void OnTypeChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            OnClickAction?.Invoke();
        }

        private void SetItem()
        {
            _itemControl.Item = Item;
            _fontColor = Item?.Rarity.GetColor() ?? Color.White;

            if (Tooltip is ItemTooltip itemTooltip)
            {
                itemTooltip.Item = Item;
            }
            //string txt = Item == null ? null : Item.Name + $" [{Item.Id}] " + (Item.Type == Gw2Sharp.WebApi.V2.Models.ItemType.Weapon ? Environment.NewLine + (Item as Weapon).WeaponType.ToSkillWeapon() : string.Empty);

            //if (Item is not null && !string.IsNullOrEmpty(Item.DisplayText))
            //{
            //    txt += Environment.NewLine + Item.Description;
            //}

            //BasicTooltipText = txt;
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            spriteBatch.DrawStringOnCtrl(this, Item?.Name, Content.DefaultFont14, _nameBounds, _fontColor, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Middle);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Item = null;
        }
    }
}
