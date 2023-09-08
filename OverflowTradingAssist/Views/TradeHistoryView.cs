using SizingMode = Blish_HUD.Controls.SizingMode;
using ControlFlowDirection = Blish_HUD.Controls.ControlFlowDirection;
using Container = Blish_HUD.Controls.Container;
using Kenedia.Modules.Core.Controls;
using Blish_HUD.Graphics.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kenedia.Modules.OverflowTradingAssist.Models;
using Kenedia.Modules.OverflowTradingAssist.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using Blish_HUD;
using Microsoft.Xna.Framework;

namespace Kenedia.Modules.OverflowTradingAssist.Views
{
    public class TradeHistoryView : View
    {
        private readonly Func<List<Trade>> _getTrades;

        private Label _tradePartnerLabel;
        private Label _amountLabel;
        private Label _itemSummaryLabel;
        private Label _reviewLinkLabel;
        private Label _tradeListingLinkLabel;

        private List<(double threshold, Color color, string name)> _tradeRanks = new()
        {
            (10000000000, new(56, 211, 141), "Eligible for 1 Mil. Trade Rank"),
            (5000000000, new(22, 234, 215), "Eligible for 500k Trade Rank"),
            (1000000000, new(17, 171, 205), "Eligible for 100k Trade Rank"),
            (500000000, new(127, 186, 246), "Eligible for 50k Trade Rank"),
            (250000000, new(206, 233, 250), "Eligible for 25k Trade Rank"),
            (50000000, new(185, 192, 236), "Eligible for 5k Trade Rank"),
        };

        public TradeHistoryView(Func<List<Trade>> getTrades)
        {
            _getTrades = getTrades;
        }

        protected override void Build(Container buildPanel)
        {
            base.Build(buildPanel);

            var headerPanel = new Panel()
            {
                Location = new(0, 0),
                Parent = buildPanel,
                CanScroll = true,
                Width = buildPanel.ContentRegion.Width,
                HeightSizingMode = SizingMode.AutoSize,
                ContentPadding = new(10, 4, 25, 0),
            };

            int width = headerPanel.Width;
            int partner = (int)(width * 0.15F);
            int amount = (int)(width * 0.1F);
            int itemSummary = (int)(width * 0.35F);
            int review = (int)(width * 0.2F);
            int listing = (int)(width * 0.2F);

            _tradePartnerLabel = new Label()
            {
                Text = "Trade Partner",
                Parent = headerPanel,
                Width = partner,
                Location = new(0, 0),
                HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Left,
                Font = GameService.Content.DefaultFont16,
            };

            _amountLabel = new Label()
            {
                Text = "Amount",
                Parent = headerPanel,
                Width = amount,
                Location = new(_tradePartnerLabel.Right, 0),
                HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Left,
                Font = GameService.Content.DefaultFont16,
            };

            _itemSummaryLabel = new Label()
            {
                Text = "Items",
                Parent = headerPanel,
                Width = itemSummary,
                Location = new(_amountLabel.Right, 0),
                HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Left,
                Font = GameService.Content.DefaultFont16,
            };

            _reviewLinkLabel = new Label()
            {
                Text = "Review Link",
                Parent = headerPanel,
                Width = review,
                Location = new(_itemSummaryLabel.Right, 0),
                HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Left,
                Font = GameService.Content.DefaultFont16,
            };

            _tradeListingLinkLabel = new Label()
            {
                Text = "Trade Listing",
                Parent = headerPanel,
                Width = listing,
                Location = new(_reviewLinkLabel.Right, 0),
                HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Left,
                Font = GameService.Content.DefaultFont16,
            };

            var trades = _getTrades?.Invoke();
            double totalTraded = trades.Sum(e => e.Amount);

            if (trades is not null)
            {
                var tradeRank = _tradeRanks.FirstOrDefault(e => e.threshold <= totalTraded);

                var ctrl = new TradeHistoryHeaderControl(
                    $"{trades.Select(e => e.TradePartner).Distinct().Count()}", 
                    totalTraded, 
                    tradeRank.name, 
                    tradeRank.color)
                {
                    Parent = headerPanel,
                    Location = new(_tradePartnerLabel.Left, _tradeListingLinkLabel.Bottom + 5),
                    WidthSizingMode = SizingMode.Fill,
                    BorderWidth = new(2, 2, 2, 4)
                };

                var tradeHistoryPanel = new FlowPanel()
                {
                    Location = new(0, ctrl.Bottom + 10),
                    Parent = buildPanel,
                    FlowDirection = ControlFlowDirection.SingleTopToBottom,
                    CanScroll = true,
                    WidthSizingMode = SizingMode.Fill,
                    HeightSizingMode = SizingMode.Fill,
                    ControlPadding = new(4),
                    ContentPadding = new(10, 4, 25, 0),
                };

                foreach (var trade in trades)
                {
                    _ = new TradeHistoryEntryControl(trade)
                    {
                        Parent = tradeHistoryPanel,
                    };
                }
            }
        }
    }
}
