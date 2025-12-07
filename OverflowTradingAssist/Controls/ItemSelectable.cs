using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.OverflowTradingAssist.Controls.GearPage;
using Kenedia.Modules.OverflowTradingAssist.DataModels;

namespace Kenedia.Modules.OverflowTradingAssist.Controls
{
    public class ItemSelectable : Panel
    {
        private readonly ItemControl _itemControl;

        public ItemSelectable()
        {
            _itemControl = new()
            {
                Parent = this,
                Width = 64,
                Height = 64,
            };
        }

        public Item Item { get; set => Common.SetProperty(ref field, value, ApplyItem);}

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
