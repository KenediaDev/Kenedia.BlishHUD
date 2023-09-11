using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Graphics.UI;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Views;
using Kenedia.Modules.OverflowTradingAssist.Models;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace Kenedia.Modules.OverflowTradingAssist.Views
{
    public class DetailedTradeView : View
    {
        private TextBox _tradePartnerTextBox;
        private NumberBox _amountNumberBox;
        private TextBox _reviewTextBox;
        private TextBox _listingTextBox;
        private ButtonImage _tradeTypeImage;
        private Dropdown _tradeTypeDropdown;

        private ButtonImage _tradePartnerButtonImage;
        private ButtonImage _tradeAmountButtonImage;
        private ButtonImage _tradeListingButtonImage;
        private ButtonImage _tradeReviewButtonImage;

        private Trade _trade;

        public DetailedTradeView(Trade trade)
        {
            _trade = trade;
        }

        protected override void Build(Blish_HUD.Controls.Container buildPanel)
        {
            base.Build(buildPanel);
            Blish_HUD.Controls.Container c = buildPanel;

            int width = DetailedTradeWindow.ContentWidth - 25;

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
                Location = new(_tradePartnerTextBox.Right + (int)(width * 0.2F), 0),
                Parent = c,
                Size = new(27),
                Texture = AsyncTexture2D.FromAssetId(156902),
                BasicTooltipText = "Trade Amount (in copper)\n1 gold = 10000 copper\n1 silver = 100 copper",
                Enabled = false,
            };
            _amountNumberBox = new NumberBox()
            {
                Location = new(_tradeAmountButtonImage.Right, 0),
                Parent = c,
                Width = (int)(width * 0.4F) - _tradeAmountButtonImage.Width,
                Value = (int)_trade.Amount,
                Height = _tradePartnerTextBox.Height,
                ValueChangedAction = (s) => _trade.Amount = s,
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

            var itemsPanel = new FlowPanel()
            {
                Parent = fp,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                Title = "Items",
                CanScroll = true,
                Height = 250,
                BackgroundColor = Color.Black * 0.2F,
                BorderColor = Color.Black,
                BorderWidth = new(2),
            };

            var paymentPanel = new FlowPanel()
            {
                Parent = fp,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                Title = "Payment",
                CanScroll = true,
                Height = 250,
                BackgroundColor = Color.Black * 0.2F,
                BorderColor = Color.Black,
                BorderWidth = new(2),
                //OnCollapse = () =>
                //{
                //    Debug.WriteLine($"EXPAND ITEMS");
                //    itemsPanel.Expand();
                //},
                //OnExpand = () =>
                //{
                //    Debug.WriteLine($"Collapse ITEMS");
                //    itemsPanel.Collapse();
                //},
            };

            //itemsPanel.OnCollapse = () =>
            //{
            //    Debug.WriteLine($"EXPAND paymentPanel");
            //    paymentPanel.Expand();
            //};
            //itemsPanel.OnExpand = () =>
            //    {
            //        Debug.WriteLine($"Collapse paymentPanel");
            //        paymentPanel.Collapse();
            //    };

        }

        private void TradeTypeImage_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _tradeTypeDropdown.SelectedItem = _trade.TradeType is TradeType.Buy ? "Sell" : _trade.TradeType is TradeType.Sell ? "Buy" : "None";
        }

        protected override void Unload()
        {
            base.Unload();
            _trade = null;
        }
    }

    public class DetailedTradeWindow : StandardWindow
    {
        public static int WindowWidth = 700;
        public static int WindowHeight = 706;

        public static int ContentWidth = WindowWidth - 55;
        public static int ContentHeight = WindowHeight - 55;

        public DetailedTradeWindow(Trade trade, AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion)
        {
            Parent = GameService.Graphics.SpriteScreen;
            Title = "❤";
            Subtitle = "❤";
            SavesPosition = true;
            Id = $"{trade?.Id} {nameof(DetailedTradeWindow)}";
            MainWindowEmblem = AsyncTexture2D.FromAssetId(156014);
            SubWindowEmblem = AsyncTexture2D.FromAssetId(156019);
            Name = $"{trade?.TradePartner}";
            //SubName = $"{trade.Id}";

            Width = WindowWidth;
            Height = WindowHeight;

            Show(new DetailedTradeView(trade));

            trade.TradeSummaryChanged += Trade_TradeSummaryChanged;
        }

        private void Trade_TradeSummaryChanged(object sender, Trade e)
        {
            Name = $"{e.TradePartner}";
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            CurrentView?.DoUnload();
        }
    }
}
