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
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.OverflowTradingAssist.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kenedia.Modules.Core.Res;
using System.Diagnostics;

namespace Kenedia.Modules.OverflowTradingAssist.Views
{
    public class TradeHistoryView : View
    {
        private readonly Func<List<Trade>> _getTrades;
        private readonly List<TradeHistoryEntryControl> _tradeHistoryEntries = new();
        private Label _tradePartnerLabel;
        private Label _amountLabel;
        private Label _itemSummaryLabel;
        private FilterBox _filterBox;
        private FlowPanel _tradeHistoryPanel;

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
                Blish_HUD.Controls.Control ctrl = null;
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

                int trailing = (32 * 4) + (5 * 4);
                int width = headerPanel.Width - trailing;
                int partner = (int)(width * 0.15F);
                int amount = (int)(width * 0.15F);
                int itemSummary = (int)(width * 0.70F);

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

                ctrl = new TradeHistoryHeaderControl(
                                   $"{trades.Select(e => e.TradePartner).Distinct().Count()}",
                                   totalTraded,
                                   tradeRank)
                {
                    Parent = headerPanel,
                    Location = new(0, _itemSummaryLabel.Bottom + 5),
                    WidthSizingMode = SizingMode.Fill,
                    BorderWidth = new(0, 0, 0, 6),
                    BorderColor = Color.Black,
                    BackgroundColor = Color.Transparent,
                };

                ctrl = _filterBox = new()
                {
                    Parent = headerPanel,
                    Location = new(0, ctrl.Bottom + 5),
                    PlaceholderText = strings_common.Search,
                    FilteringOnEnter = true,
                    FilteringOnTextChange = true,
                    FilteringDelay = 150,
                    PerformFiltering = FilterTrades,
                    Width = headerPanel.ContentRegion.Width,
                };

                _tradeHistoryPanel = new()
                {
                    Location = new(0, ctrl?.Bottom + 5 ?? 0),
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
                    bool any = string.IsNullOrEmpty(_filterBox.Text);
                    string text = _filterBox.Text ?? string.Empty;
                    bool visible = true;
                    var items = trade.Items.Select(e => e?.Item?.Name?.ToLower());

                    foreach (string s in text.Trim().ToLower().Split(' '))
                    {
                        visible = visible && (items.Any(i => i.Contains(s)) || trade.TradePartner.ToLower().Contains(s));
                    }

                    _tradeHistoryEntries.Add(new TradeHistoryEntryControl(trade)
                    {
                        Parent = _tradeHistoryPanel,
                        Width = buildPanel.ContentRegion.Width,
                        Visible = visible,
                    });
                }
            }
        }

        private void FilterTrades(string? text)
        {
            bool any = string.IsNullOrEmpty(text);
            text ??= string.Empty;

            foreach (var ctrl in _tradeHistoryEntries)
            {
                var items = ctrl.Trade.Items.Select(e => e?.Item?.Name?.ToLower());
                bool visible = true;

                foreach (string s in text.Trim().ToLower().Split(' '))
                {
                    visible = visible && (items.Any(i => i.Contains(s)) || ctrl.Trade.TradePartner.ToLower().Contains(s));
                }

                ctrl.Visible = visible; 
            }

            _tradeHistoryPanel?.Invalidate();
        }

        protected override void Unload()
        {
            base.Unload();
            try
            {
                _tradeHistoryEntries?.DisposeAll();
                _tradeHistoryEntries.Clear();
            }
            catch
            {

            }
        }
    }
}
