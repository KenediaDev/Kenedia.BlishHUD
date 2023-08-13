using Blish_HUD;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Controls.GearPage;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class Selectable : Blish_HUD.Controls.Container
    {
        private readonly ItemControl _itemControl = new();
        private Rectangle _iconBounds;
        private Rectangle _nameBounds;
        private Rectangle _descriptionBounds;

        private Color RarityColor = Color.White;
        private BaseItem _item;
        private SelectableType _type;

        public Selectable()
        {
            Size = new(64, 64);
            BackgroundColor = Color.Black * 0.2F;

            _itemControl.Parent = this;
            _itemControl.Size = new(64);
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

        public SelectableType Type { get => _type; set => Common.SetProperty(ref _type , value, OnTypeChanged); }

        public Action OnClickAction { get; set; }

        public TemplateSlotType TemplateSlot { get; set; }

        public BaseItem Item { get => _item; set => Common.SetProperty(ref _item, value, SetItem); }

        public TemplateSlotType ActiveSlot { get; set; }

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

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            OnClickAction?.Invoke();
        }

        private void SetItem()
        {
            _itemControl.Item = Item;

            //string txt = Item == null ? null : Item.Name + $" [{Item.Id}] " + (Item.Type == Gw2Sharp.WebApi.V2.Models.ItemType.Weapon ? Environment.NewLine + (Item as Weapon).WeaponType.ToSkillWeapon() : string.Empty);

            //if (Item != null && !string.IsNullOrEmpty(Item.DisplayText))
            //{
            //    txt += Environment.NewLine + Item.Description;
            //}

            //BasicTooltipText = txt;
        }
    }
}
