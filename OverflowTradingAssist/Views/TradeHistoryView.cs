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
using Microsoft.Xna.Framework.Graphics;
using Blish_HUD.Content;

namespace Kenedia.Modules.OverflowTradingAssist.Views
{
    public class TradeRank
    {
        public static TradeRank OneMillion = new(10000000000, new(56, 211, 141), "Eligible for 1 Mil. Trade Rank", 250, OverflowTradingAssist.ModuleInstance.ContentsManager.GetTexture($@"textures\{1000000}.png"));
        public static TradeRank FiveHundredThousand = new(5000000000, new(22, 234, 215), "Eligible for 500k Trade Rank", 175, OverflowTradingAssist.ModuleInstance.ContentsManager.GetTexture($@"textures\{500000}.png"));
        public static TradeRank OneHundredThousand = new(1000000000, new(17, 171, 205), "Eligible for 100k Trade Rank", 100, OverflowTradingAssist.ModuleInstance.ContentsManager.GetTexture($@"textures\{100000}.png"));
        public static TradeRank FiftyThousand = new(500000000, new(127, 186, 246), "Eligible for 50k Trade Rank", 50, OverflowTradingAssist.ModuleInstance.ContentsManager.GetTexture($@"textures\{50000}.png"));
        public static TradeRank TwentyFiveThousand = new(250000000, new(206, 233, 250), "Eligible for 25k Trade Rank", 25, OverflowTradingAssist.ModuleInstance.ContentsManager.GetTexture($@"textures\{25000}.png"));
        public static TradeRank FiveThousand = new(50000000, new(185, 192, 236), "Eligible for 5k Trade Rank", 10, OverflowTradingAssist.ModuleInstance.ContentsManager.GetTexture($@"textures\{5000}.png"));
        public static TradeRank NoRank = new(0, new(255, 255, 255), "Eligible for no Trade Rank", 0, AsyncTexture2D.FromAssetId(154982));

        public TradeRank(double threshold, Color color, string name, int trades, AsyncTexture2D texture)
        {
            Threshold = threshold;
            Color = color;
            Name = name;
            Trades = trades;
            Icon = texture;
        }

        public double Threshold { get; set; }

        public Color Color { get; set; }

        public string Name { get; set; }

        public int Trades { get; set; }

        public AsyncTexture2D Icon { get; set; }

        public static List<TradeRank> Ranks = new()
        {
            OneMillion,
            FiveHundredThousand,
            OneHundredThousand,
            FiftyThousand,
            TwentyFiveThousand,
            FiveThousand,
            NoRank,
        };
    }

    public class TradeHistoryView : View
    {
        private readonly Func<List<Trade>> _getTrades;

        private Label _tradePartnerLabel;
        private Label _amountLabel;
        private Label _itemSummaryLabel;
        private Label _reviewLinkLabel;
        private Label _tradeListingLinkLabel;

        public TradeHistoryView(Func<List<Trade>> getTrades)
        {
            _getTrades = getTrades;
        }

        protected override void Build(Container buildPanel)
        {
            base.Build(buildPanel);

            var trades = _getTrades?.Invoke();

            if (trades is not null)
            {
                double totalTraded = trades.Sum(e => e.Amount);
                var tradeRank = TradeRank.Ranks.FirstOrDefault(e => e.Threshold <= totalTraded && e.Trades <= trades.Count);

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
                int amount = (int)(width * 0.15F);
                int itemSummary = (int)(width * 0.30F);
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
                    Location = new(_tradePartnerLabel.Right, _tradePartnerLabel.Top),
                    HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Left,
                    Font = GameService.Content.DefaultFont16,
                };

                _itemSummaryLabel = new Label()
                {
                    Text = "Items",
                    Parent = headerPanel,
                    Width = itemSummary,
                    Location = new(_amountLabel.Right, _amountLabel.Top),
                    HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Left,
                    Font = GameService.Content.DefaultFont16,
                };

                _reviewLinkLabel = new Label()
                {
                    Text = "Review Link",
                    Parent = headerPanel,
                    Width = review,
                    Location = new(_itemSummaryLabel.Right, _itemSummaryLabel.Top),
                    HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Left,
                    Font = GameService.Content.DefaultFont16,
                };

                _tradeListingLinkLabel = new Label()
                {
                    Text = "Trade Listing",
                    Parent = headerPanel,
                    Width = listing,
                    Location = new(_reviewLinkLabel.Right, _reviewLinkLabel.Top),
                    HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Left,
                    Font = GameService.Content.DefaultFont16,
                };

                var ctrl = new TradeHistoryHeaderControl(
                    $"{trades.Select(e => e.TradePartner).Distinct().Count()}",
                    totalTraded,
                    tradeRank)
                {
                    Parent = headerPanel,
                    Location = new(0, _tradeListingLinkLabel.Bottom + 5),
                    WidthSizingMode = SizingMode.Fill,
                    BorderWidth = new(0, 0, 0, 6),
                    BorderColor = Color.Black,
                    BackgroundColor = Color.Transparent,
                };

                var tradeHistoryPanel = new FlowPanel()
                {
                    Location = new(0, ctrl.Bottom + 5),
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
                        Width = buildPanel.ContentRegion.Width,
                    };
                }
            }
        }
    }
}
