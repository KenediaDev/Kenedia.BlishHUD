using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.OverflowTradingAssist.Controls.GearPage;
using Kenedia.Modules.OverflowTradingAssist.DataModels;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.OverflowTradingAssist.Controls
{
    public class ItemSelectable : Panel
    {
        private readonly ItemControl _itemControl;
        private Item _item;

        public ItemSelectable()
        {
            _itemControl = new()
            {
                Parent = this,
                Width = 64,
                Height = 64,
            };
        }

        public Item Item { get => _item; set => Common.SetProperty(ref _item, value, ApplyItem);}

        private void ApplyItem(object sender, ValueChangedEventArgs<Item> e)
        {
            _itemControl.Item = Item;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if(_itemControl is not null)
            {
                _itemControl.Size = new(ContentRegion.Height);
            }
        }
    }
}
