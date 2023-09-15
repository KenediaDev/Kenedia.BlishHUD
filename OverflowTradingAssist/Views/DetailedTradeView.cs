using Blish_HUD.Content;
using Blish_HUD.Graphics.UI;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.OverflowTradingAssist.Controls;
using Kenedia.Modules.OverflowTradingAssist.Models;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Linq;

namespace Kenedia.Modules.OverflowTradingAssist.Views
{
    public class DetailedTradeView : View
    {
        private TextBox _tradePartnerTextBox;
        private CoinControl _amountNumberBox;
        private TextBox _reviewTextBox;
        private TextBox _listingTextBox;
        private ButtonImage _tradeTypeImage;
        private Dropdown _tradeTypeDropdown;

        private ButtonImage _tradePartnerButtonImage;
        private ButtonImage _tradeAmountButtonImage;
        private ButtonImage _tradeListingButtonImage;
        private ButtonImage _tradeReviewButtonImage;

        private Trade _trade;
        private ItemPanel _itemsPanel;
        private ItemPanel _paymentPanel;

        public DetailedTradeView(Trade trade)
        {
            _trade = trade;
            _trade.TotalTradeValueChanged += Trade_TotalTradeValueChanged;
        }

        private void Trade_TotalTradeValueChanged(object sender, decimal e)
        {
            _amountNumberBox.Value = e;
        }

        protected override void Build(Blish_HUD.Controls.Container buildPanel)
        {
            base.Build(buildPanel);
            Blish_HUD.Controls.Container c = buildPanel;

            int width = DetailedTradeWindow.ContentWidth - 10;

            _tradePartnerButtonImage = new ButtonImage()
            {
                Location = new(0, 0),
                Parent = c,
                Size = new(27),
                Texture = AsyncTexture2D.FromAssetId(733268),
                BasicTooltipText = "Trade Partner",
                Enabled = false,
            };

            _tradePartnerTextBox = new TextBox()
            {
                Location = new(_tradePartnerButtonImage.Right, 0),
                Parent = c,
                Width = (int)(width * 0.4F) - _tradePartnerButtonImage.Width,
                PlaceholderText = "Trade Partner",
                Text = _trade.TradePartner,
                TextChangedAction = (s) => _trade.TradePartner = s,
            };

            _tradeAmountButtonImage = new ButtonImage()
            {
                Location = new(_tradePartnerTextBox.Right + (int)(width * 0.15F), 0),
                Parent = c,
                Size = new(27),
                Texture = AsyncTexture2D.FromAssetId(156758),
                BasicTooltipText = "Trade Amount (in copper)\n1 gold = 10000 copper\n1 silver = 100 copper",
                Enabled = false,
            };
            _amountNumberBox = new()
            {
                Location = new(_tradeAmountButtonImage.Right, 0),
                Parent = c,
                Width = (int)(width * 0.45F) - _tradeAmountButtonImage.Width,
                Value = _trade.ItemValue,
                Height = _tradePartnerTextBox.Height,
                //ValueChangedAction = (s) => _trade.Amount = s,
                Enabled = false,
            };

            _tradeReviewButtonImage = new ButtonImage()
            {
                Location = new(_tradePartnerButtonImage.Left, _tradePartnerButtonImage.Bottom + 5),
                Parent = c,
                Size = new(27),
                Texture = AsyncTexture2D.FromAssetId(1234950),
                Enabled = false,
                BasicTooltipText = "Review Link",
            };
            _reviewTextBox = new TextBox()
            {
                Location = new(_tradeReviewButtonImage.Right, _tradeReviewButtonImage.Top),
                Parent = c,
                Width = width - _tradeReviewButtonImage.Width,
                PlaceholderText = "Review Link",
                Text = _trade.ReviewLink,
                TextChangedAction = (s) => _trade.ReviewLink = s,
            };

            _tradeListingButtonImage = new ButtonImage()
            {
                Location = new(_tradePartnerButtonImage.Left, _reviewTextBox.Bottom + 5),
                Parent = c,
                Size = new(27),
                Texture = AsyncTexture2D.FromAssetId(255379),
                Enabled = false,
                BasicTooltipText = "Listing Link",
            };
            _listingTextBox = new TextBox()
            {
                Location = new(_tradeListingButtonImage.Right, _tradeListingButtonImage.Top),
                Parent = c,
                Width = width - _tradeListingButtonImage.Width,
                PlaceholderText = "Listing Link",
                Text = _trade.TradeListingLink,
                TextChangedAction = (s) => _trade.TradeListingLink = s,
            };

            _tradeTypeImage = new ButtonImage()
            {
                Location = new(0, _listingTextBox.Bottom + 5),
                Parent = c,
                Size = new(27),
                Texture = AsyncTexture2D.FromAssetId(_trade.TradeType is TradeType.Buy ? 157326 : _trade.TradeType is TradeType.Sell ? 157328 : 157095),
                BasicTooltipText = "Click to change the trade Type!"
            };
            _tradeTypeImage.Click += TradeTypeImage_Click;

            _tradeTypeDropdown = new Dropdown()
            {
                Location = new(_tradeTypeImage.Right, _tradeTypeImage.Top),
                Parent = c,
                Width = width - _tradeTypeImage.Width,
                BasicTooltipText = "Trade Type",
                Items = { "Buy", "Sell" },
                SelectedItem = _trade.TradeType is TradeType.Buy ? "Buy" : "Sell",
                ValueChangedAction = (s) =>
                {
                    _trade.TradeType = s is "Buy" ? TradeType.Buy : s is "Sell" ? TradeType.Sell : TradeType.None;
                    _tradeTypeImage.Texture = AsyncTexture2D.FromAssetId(_trade.TradeType is TradeType.Buy ? 157326 : 157328);
                    _tradeTypeImage.HoveredTexture = AsyncTexture2D.FromAssetId(_trade.TradeType is TradeType.Buy ? 157327 : 157329);
                }
            };

            var fp = new FlowPanel()
            {
                Parent = c,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                Location = new(0, _tradeTypeImage.Bottom + 5),
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(5),
            };

            _itemsPanel = new ItemPanel()
            {
                Parent = fp,
                Width = width,
                Title = "Items",
                CanScroll = true,
                Height = 250,
                BackgroundColor = Color.Black * 0.2F,
                BorderColor = Color.Black,
                BorderWidth = new(2),
                ClickAction = () => AddItem(),
            };

            _paymentPanel = new ItemPanel()
            {
                Parent = fp,
                Width = width,
                Title = "Payment",
                CanScroll = true,
                Height = 250,
                BackgroundColor = Color.Black * 0.2F,
                BorderColor = Color.Black,
                BorderWidth = new(2),
                ClickAction = () => AddPayment(),
            };

            foreach (var item in _trade.Items ?? Enumerable.Empty<ItemAmount>())
            {
                AddItem(item);
            }

            foreach (var item in _trade.Payment ?? Enumerable.Empty<ItemAmount>())
            {
                AddPayment(item);
            }
        }

        private void AddPayment(ItemAmount itemAmount = null)
        {
            itemAmount ??= new() { Item = DataModels.Item.Coin};

            _ = new ItemEntryControl(itemAmount)
            {
                Parent = _paymentPanel,
                Width = _paymentPanel.Width - 25,
                ItemAmount = itemAmount,
            };

            itemAmount.ValueChanged += ItemAmount_ValueChanged;
            itemAmount.DeleteRequested += ItemAmount_DeleteRequested;

            if (_trade.Payment.FirstOrDefault(e => e == itemAmount) is null)
            {
                _trade.Payment.Add(itemAmount);
            }
        }

        private void AddItem(ItemAmount itemAmount = null)
        {
            itemAmount ??= new();

            _ = new ItemEntryControl(itemAmount)
            {
                Parent = _itemsPanel,
                Width = _itemsPanel.Width - 25,
                ItemAmount = itemAmount,
            };

            itemAmount.DeleteRequested += ItemAmount_DeleteRequested;
            itemAmount.ValueChanged += ItemAmount_ValueChanged;

            if (_trade.Items.FirstOrDefault(e => e == itemAmount) is null)
            {
                _trade.Items.Add(itemAmount);
            }
        }

        private void ItemAmount_DeleteRequested(object sender, EventArgs e)
        {
            if (sender is ItemAmount itemAmount)
            {
                itemAmount.DeleteRequested -= ItemAmount_DeleteRequested;
                itemAmount.ValueChanged -= ItemAmount_ValueChanged;
                _ = _trade?.Items.Remove(itemAmount);
            }
        }

        private void ItemAmount_ValueChanged(object sender, Core.Models.ValueChangedEventArgs<decimal> e)
        {
            _amountNumberBox.Value = _trade?.ItemValue ?? 0;
        }

        private void TradeTypeImage_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _tradeTypeDropdown.SelectedItem = _trade.TradeType is TradeType.Sell ? "Buy" : "Sell";
        }

        protected override void Unload()
        {
            base.Unload();
            _trade = null;
        }
    }
}
