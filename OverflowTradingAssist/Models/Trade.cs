using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;

namespace Kenedia.Modules.OverflowTradingAssist.Models
{
    public class Trade
    {
        private string _tradePartner;
        private TradeType _tradeType;

        public Trade()
        {
            Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in e.OldItems?.OfType<ItemAmount>() ?? Enumerable.Empty<ItemAmount>())
            {
                item.ValueChanged -= Item_ValueChanged;
            }

            foreach (var item in e.NewItems?.OfType<ItemAmount>() ?? Enumerable.Empty<ItemAmount>())
            {
                item.ValueChanged += Item_ValueChanged;
            }

            TotalTradeValueChanged?.Invoke(this, Amount);
        }

        private void Item_ValueChanged(object sender, ValueChangedEventArgs<decimal> e)
        {
            TotalTradeValueChanged?.Invoke(this, Amount);
        }

        public event EventHandler<Trade> TradeSummaryChanged;

        public event EventHandler<decimal> TotalTradeValueChanged;

        public Guid Id { get; set; } = Guid.NewGuid();

        public ObservableCollection<ItemAmount> Items { get; } = new();

        public ObservableCollection<ItemAmount> Payment { get; } = new();

        public string ItemSummary => string.Join(", ", Items.Select(e => $"{e.Amount} x  {e.Item.Name}"));

        public string TradePartner { get => _tradePartner; set => Common.SetProperty(ref _tradePartner, value, OnTradePartnerChanged); }

        public DateTime Date { get; set; } = DateTime.Now;

        public string ReviewLink { get; set; }

        public string TradeListingLink { get; set; }

        public decimal Amount => Items.Sum(e => e.Amount * e.Value);

        public TradeType TradeType { get => _tradeType; set => Common.SetProperty(ref _tradeType, value, OnTradeTypeChanged); }

        public bool IsValidTrade()
        {
            return !string.IsNullOrEmpty(TradePartner);
        }

        private void OnTradePartnerChanged(object sender, ValueChangedEventArgs<string> e)
        {
            TradeSummaryChanged?.Invoke(this, this);
        }

        private void OnAmountChanged(object sender, ValueChangedEventArgs<decimal> e)
        {
            TradeSummaryChanged?.Invoke(this, this);
        }

        private void OnTradeTypeChanged(object sender, ValueChangedEventArgs<TradeType> e)
        {
            TradeSummaryChanged?.Invoke(this, this);
        }
    }
}
