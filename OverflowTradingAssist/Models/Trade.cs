using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Newtonsoft.Json;

namespace Kenedia.Modules.OverflowTradingAssist.Models
{
    public class Trade
    {
        private bool _fireEvents = true;
        private string _tradePartner;
        private TradeType _tradeType;
        private string _tradeListingLink;
        private string _reviewLink;

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

            if (!_fireEvents) return;

            RequestSave();
            TotalTradeValueChanged?.Invoke(this, ItemValue);
        }

        private void Item_ValueChanged(object sender, ValueChangedEventArgs<decimal> e)
        {
            if (!_fireEvents) return;

            RequestSave();
            TotalTradeValueChanged?.Invoke(this, ItemValue);
        }

        public event EventHandler<Trade> TradeSummaryChanged;

        public event EventHandler<decimal> TotalTradeValueChanged;

        public Guid Id { get; set; } = Guid.NewGuid();

        public ObservableCollection<ItemAmount> Items { get; } = new();

        public ObservableCollection<ItemAmount> Payment { get; } = new();

        [JsonIgnore]
        public string ItemSummary => string.Join(", ", Items.Select(e => $"{e.Amount} x  {e.Item.Name}"));

        [JsonIgnore]
        public string PaymentSummary => string.Join(", ", Payment.Select(e => $"{e.Amount} x  {e.Item.Name}"));

        public string TradePartner { get => _tradePartner; set => Common.SetProperty(ref _tradePartner, value, OnTradePartnerChanged); }

        public DateTime Date { get; set; } = DateTime.Now;

        public string ReviewLink { get => _reviewLink; set => Common.SetProperty(ref _reviewLink ,  value, OnReviewChanged); }

        public string TradeListingLink { get => _tradeListingLink; set => Common.SetProperty(ref _tradeListingLink, value, OnListingChanged); }

        [JsonIgnore]
        public decimal ItemValue => Items?.Sum(e => e.Amount * e.Value) ?? 0;

        [JsonIgnore]
        public decimal PaymentValue => Payment.Sum(e => e.Amount * e.Value);

        [JsonIgnore]
        public decimal Value { get; set; }

        public TradeType TradeType { get => _tradeType; set => Common.SetProperty(ref _tradeType, value, OnTradeTypeChanged); }

        [JsonIgnore]
        public bool ExcelSaveRequested { get; set; }

        [JsonIgnore]
        public bool ExcelDeleteRequested { get; set; }

        [JsonIgnore]
        public bool TradeSaveRequested { get; set; }

        [JsonIgnore]
        public bool TradeDeleteRequested { get; set; }

        public bool IsValidTrade()
        {
            return !string.IsNullOrEmpty(TradePartner) && ItemValue > 0;
        }

        private void OnListingChanged(object sender, ValueChangedEventArgs<string> e)
        {
            RequestSave();
        }

        private void OnReviewChanged(object sender, ValueChangedEventArgs<string> e)
        {
            RequestSave();
        }

        private void OnTradePartnerChanged(object sender, ValueChangedEventArgs<string> e)
        {
            TradeSummaryChanged?.Invoke(this, this);
            RequestSave();
        }

        private void OnAmountChanged(object sender, ValueChangedEventArgs<decimal> e)
        {
            TradeSummaryChanged?.Invoke(this, this);
            RequestSave();
        }

        private void OnTradeTypeChanged(object sender, ValueChangedEventArgs<TradeType> e)
        {
            TradeSummaryChanged?.Invoke(this, this);
            RequestSave();
        }

        public void RequestSave()
        {
            TradeSaveRequested = true;
            ExcelSaveRequested = true;
        }

        public void RequestDelete()
        {
            TradeDeleteRequested = true;
            ExcelDeleteRequested = true;
        }

        public void SetDetails(Trade detailed)
        {
            if (detailed is null) return;

            TradePartner = detailed.TradePartner;
            Date = detailed.Date;
            ReviewLink = detailed.ReviewLink;
            TradeListingLink = detailed.TradeListingLink;
            TradeType = detailed.TradeType;

            Items.Clear();
            detailed.Items.ForEach(Items.Add);

            Payment.Clear();
            detailed.Payment.ForEach(Payment.Add);

            TotalTradeValueChanged?.Invoke(this, ItemValue);
            TradeSummaryChanged?.Invoke(this, this);
        }

        public void SetTradeSaved()
        {
            TradeSaveRequested = false;
            TradeDeleteRequested = false;
        }
    }
}
