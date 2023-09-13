using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.OverflowTradingAssist.DataModels;
using System;

namespace Kenedia.Modules.OverflowTradingAssist.Models
{
    public class ItemAmount
    {
        private Item _item = Item.UnkownItem;
        private int _amount = 1;
        private decimal _value = 0;

        public event ValueChangedEventHandler<Item> ItemChanged;

        public event ValueChangedEventHandler<int> AmountChanged;

        public event ValueChangedEventHandler<decimal> ValueChanged;

        public event EventHandler DeleteRequested;

        public Item Item { get => _item; set => Common.SetProperty(ref _item, value, OnItemChanged); }

        public int Amount { get => _amount; set => Common.SetProperty(ref _amount, value, OnAmountChanged); }

        public decimal Value { get => _value; set => Common.SetProperty(ref _value, value, OnValueChanged); }

        private void OnAmountChanged(object sender, ValueChangedEventArgs<int> e)
        {
            AmountChanged?.Invoke(this, e);
        }

        private void OnItemChanged(object sender, ValueChangedEventArgs<Item> e)
        {
            ItemChanged?.Invoke(this, e);
        }

        private void OnValueChanged(object sender, ValueChangedEventArgs<decimal> e)
        {
            ValueChanged?.Invoke(this, e);
        }

        public void RequestDelete()
        {
            DeleteRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
