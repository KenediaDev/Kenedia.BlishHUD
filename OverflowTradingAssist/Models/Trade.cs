using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.OverflowTradingAssist.DataModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kenedia.Modules.OverflowTradingAssist.Models
{
    public class TradeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Trade);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            Trade trade = new();

            if (existingValue is Trade existingTrade)
            {
                trade = existingTrade;
            }

            var jsonObject = JObject.Load(reader);

            trade.DisableEvents();

            // Deserialize properties manually
            trade.Id = jsonObject["Id"].ToObject<Guid>();
            trade.Items.Clear();
            trade.Payment.Clear();

            // Deserialize Items and Payment collections
            var itemsArray = jsonObject["Items"].ToObject<List<ItemAmount>>(serializer);
            var paymentArray = jsonObject["Payment"].ToObject<List<ItemAmount>>(serializer);

            itemsArray.ForEach(trade.Items.Add);
            paymentArray.ForEach(trade.Payment.Add);

            trade.TradePartner = jsonObject["TradePartner"].ToObject<string>();
            trade.Date = jsonObject["Date"].ToObject<DateTime>();
            trade.ReviewLink = jsonObject["ReviewLink"].ToObject<string>();
            trade.TradeListingLink = jsonObject["TradeListingLink"].ToObject<string>();
            trade.TradeType = jsonObject["TradeType"].ToObject<TradeType>();

            trade.EnableEvents();

            return trade;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Trade trade)
            {
                if (value == null)
                {
                    writer.WriteNull();
                    return;
                }

                var jsonObject = new JObject(
                    new JProperty("Id", trade.Id),
                    new JProperty("Items", JArray.FromObject(trade.Items, serializer)),
                    new JProperty("Payment", JArray.FromObject(trade.Payment, serializer)),
                    new JProperty("TradePartner", trade.TradePartner),
                    new JProperty("Date", trade.Date),
                    new JProperty("ReviewLink", trade.ReviewLink),
                    new JProperty("TradeListingLink", trade.TradeListingLink),
                    new JProperty("TradeType", trade.TradeType)
                );

                jsonObject.WriteTo(writer);
            }
        }
    }

    [JsonConverter(typeof(TradeConverter))]
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
            Payment.CollectionChanged += Items_CollectionChanged;
        }

        public Trade(bool fireEvents) : this()
        {
            _fireEvents = fireEvents;
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in e.OldItems?.OfType<ItemAmount>() ?? Enumerable.Empty<ItemAmount>())
            {
                item.ValueChanged -= Item_ValueChanged;
                item.ItemChanged -= Item_ItemChanged;
                item.AmountChanged -= Item_AmountChanged;
            }

            foreach (var item in e.NewItems?.OfType<ItemAmount>() ?? Enumerable.Empty<ItemAmount>())
            {
                item.ValueChanged += Item_ValueChanged;
                item.ItemChanged += Item_ItemChanged;
                item.AmountChanged += Item_AmountChanged;
            }

            if (!_fireEvents) return;

            RequestSave();
            TotalTradeValueChanged?.Invoke(this, ItemValue);
        }

        private void Item_AmountChanged(object sender, ValueChangedEventArgs<int> e)
        {
            if (!_fireEvents) return;

            RequestSave();
            TradeSummaryChanged?.Invoke(this, this);
        }

        private void Item_ItemChanged(object sender, ValueChangedEventArgs<Item> e)
        {
            if (!_fireEvents) return;

            RequestSave();
            TradeSummaryChanged?.Invoke(this, this);
        }

        private void Item_ValueChanged(object sender, ValueChangedEventArgs<decimal> e)
        {
            if (!_fireEvents) return;

            RequestSave();
            TotalTradeValueChanged?.Invoke(this, ItemValue);
            TradeSummaryChanged?.Invoke(this, this);
        }

        public void EnableEvents()
        {
            _fireEvents = true;
        }

        public void DisableEvents()
        {
            _fireEvents = false;
        }

        public event EventHandler<Trade> DeleteRequested;

        public event EventHandler<Trade> TradeSummaryChanged;

        public event EventHandler<decimal> TotalTradeValueChanged;

        public Guid Id { get; set; } = Guid.NewGuid();

        public ObservableCollection<ItemAmount> Items { get; } = [];

        public ObservableCollection<ItemAmount> Payment { get; } = [];

        [JsonIgnore]
        public string ItemSummary => string.Join(", ", Items.Select(e => $"{e.Amount} x  {e.Item?.Name ?? Item.UnkownItem.Name}"));

        [JsonIgnore]
        public string PaymentSummary => string.Join(", ", Payment.Select(e => $"{e.Amount} x  {e.Item?.Name ?? Item.UnkownItem.Name}"));

        public string TradePartner { get => _tradePartner; set => Common.SetProperty(ref _tradePartner, value, OnTradePartnerChanged); }

        public DateTime Date { get; set; } = DateTime.Now;

        public string ReviewLink { get => _reviewLink; set => Common.SetProperty(ref _reviewLink, value, OnReviewChanged); }

        public string TradeListingLink { get => _tradeListingLink; set => Common.SetProperty(ref _tradeListingLink, value, OnListingChanged); }

        [JsonIgnore]
        public decimal ItemValue => Items?.Sum(e => e.Amount * e.Value) ?? 0;

        [JsonIgnore]
        public decimal PaymentValue => Payment?.Sum(e => e.Amount * e.Value) ?? 0;

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
            if (!_fireEvents) return;

            RequestSave();
        }

        private void OnReviewChanged(object sender, ValueChangedEventArgs<string> e)
        {
            if (!_fireEvents) return;

            RequestSave();
        }

        private void OnTradePartnerChanged(object sender, ValueChangedEventArgs<string> e)
        {
            if (!_fireEvents) return;

            TradeSummaryChanged?.Invoke(this, this);
            RequestSave();
        }

        private void OnAmountChanged(object sender, ValueChangedEventArgs<decimal> e)
        {
            if (!_fireEvents) return;

            TradeSummaryChanged?.Invoke(this, this);
            RequestSave();
        }

        private void OnTradeTypeChanged(object sender, ValueChangedEventArgs<TradeType> e)
        {
            if (!_fireEvents) return;

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
            DeleteRequested?.Invoke(this, this);
        }

        public void SetDetails(Trade detailed)
        {
            if (detailed is null) return;
            _fireEvents = false;

            TradePartner = detailed.TradePartner;
            Date = detailed.Date;
            ReviewLink = detailed.ReviewLink;
            TradeListingLink = detailed.TradeListingLink;
            TradeType = detailed.TradeType;

            Items.Clear();
            detailed.Items.ForEach(Items.Add);

            Payment.Clear();
            detailed.Payment.ForEach(Payment.Add);

            _fireEvents = true;
            TotalTradeValueChanged?.Invoke(this, ItemValue);
            TradeSummaryChanged?.Invoke(this, this);
        }

        public void SetTradeSaved()
        {
            TradeSaveRequested = false;
            ExcelSaveRequested = false;
        }
    }
}
