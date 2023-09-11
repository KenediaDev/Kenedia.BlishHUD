using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public List<ItemAmount> Items { get; set; } = new();

        public string ItemSummary => string.Join(", ", Items.Select(e => $"{e.Amount} x  {e.Item.Name}"));

        public string TradePartner { get; set; }

        public string ReviewLink { get; set; }

        public string TradeListingLink { get; set; }

        public double Amount { get; set; }

        public TradeType TradeType { get; set; }

        public Guid Id { get; set; } = Guid.NewGuid();

        public bool IsValidTrade()
        {
            return Items.Count > 0 && !string.IsNullOrEmpty(TradePartner) && !string.IsNullOrEmpty(TradeListingLink) && Amount > 0;
        }
    }
}
