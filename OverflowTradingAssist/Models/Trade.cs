using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.OverflowTradingAssist.DataModels;

namespace Kenedia.Modules.OverflowTradingAssist.Models
{
    public class ItemAmount
    {
        public Item Item { get; set; }
        public int Amount { get; set; }
    }

    public enum TradeType
    {
        None,
        Buy,
        Sell
    }

    public class Trade
    {
        private string _tradePartner;
        private decimal _amount;
        private TradeType _tradeType;

        public event EventHandler<Trade> TradeSummaryChanged;

        public Guid Id { get; set; } = Guid.NewGuid();

        public List<ItemAmount> Items { get; set; } = new();

        public string ItemSummary => string.Join(", ", Items.Select(e => $"{e.Amount} x  {e.Item.Name}"));

        public string TradePartner { get => _tradePartner; set => Common.SetProperty(ref _tradePartner, value, OnTradePartnerChanged); }

        public string ReviewLink { get; set; }

        public string TradeListingLink { get; set; }

        public decimal Amount { get => _amount; set => Common.SetProperty(ref _amount , value, OnAmountChanged); }

        public TradeType TradeType { get => _tradeType; set => Common.SetProperty(ref _tradeType , value, OnTradeTypeChanged); }

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
