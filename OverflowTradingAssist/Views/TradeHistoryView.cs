using SizingMode = Blish_HUD.Controls.SizingMode;
using ControlFlowDirection = Blish_HUD.Controls.ControlFlowDirection;
using Container = Blish_HUD.Controls.Container;
using Kenedia.Modules.Core.Controls;
using Blish_HUD.Graphics.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Kenedia.Modules.OverflowTradingAssist.Models;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.OverflowTradingAssist.Controls;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Kenedia.Modules.Core.Res;
using System.Threading;

namespace Kenedia.Modules.OverflowTradingAssist.Views
{
    public class TradeHistoryView : View
    {
        private readonly List<TradeHistoryEntryControl> _tradeHistoryEntries = [];
        private readonly List<Trade> _trades = [];
        private Label _tradePartnerLabel;
        private Label _amountLabel;
        private Label _itemSummaryLabel;
        private TradeHistoryHeaderControl _tradingHeader;
        private FilterBox _filterBox;
        private FlowPanel _tradeHistoryPanel;
        private CancellationTokenSource _cts;
        private Panel _headerPanel;

        public TradeHistoryView(List<Trade> trades, List<TradeHistoryEntryControl> tradeHistoryEntries)
        {
            _trades = trades;
            _tradeHistoryEntries = tradeHistoryEntries;

            foreach (var trade in _trades)
            {
                trade.TradeSummaryChanged += Trade_TradeSummaryChanged;
                trade.DeleteRequested += Trade_DeleteRequested;
            }
        }

        private void Trade_DeleteRequested(object sender, Trade e)
        {
            e.TradeSummaryChanged -= Trade_TradeSummaryChanged;
        }

        private void Trade_TradeSummaryChanged(object sender, Trade e)
        {
            UpdateTradingSummary();
        }

        public event EventHandler<List<TradeHistoryEntryControl>> TradeHistoryEntriesLoaded;

        private void UpdateTradingSummary()
        {
            decimal totalTraded = _trades.Sum(e => e.ItemValue);
            var tradeRank = TradeRank.Ranks.FirstOrDefault(e => e.Threshold <= totalTraded && e.Trades <= _trades.Count);

            _tradingHeader.TradeRank = tradeRank;
            _tradingHeader.TotalTraded = totalTraded;
            _tradingHeader.TotalTrades = _trades?.Count() ?? 0;
        }

        protected override void Build(Container buildPanel)
        {
            base.Build(buildPanel);

            _cts = new();

            if (_trades is not null)
            {
                Blish_HUD.Controls.Control ctrl = null;
                decimal totalTraded = _trades.Sum(e => e.ItemValue);
                var tradeRank = TradeRank.Ranks.FirstOrDefault(e => e.Threshold <= totalTraded && e.Trades <= _trades.Count);

                _headerPanel = new Panel()
                {
                    Location = new(0, 0),
                    Parent = buildPanel,
                    CanScroll = true,
                    Width = buildPanel.ContentRegion.Width,
                    HeightSizingMode = SizingMode.AutoSize,
                    ContentPadding = new(0, 0, 25, 0),
                };

                int trailing = (32 * 4) + (5 * 4);
                int width = _headerPanel.Width - trailing;
                int partner = (int)(width * 0.2F);
                int amount = (int)(width * 0.2F);
                int itemSummary = (int)(width * 0.60F);

                _tradePartnerLabel = new Label()
                {
                    Text = "Trade Partner",
                    Parent = _headerPanel,
                    Width = partner,
                    Location = new(0, 0),
                    HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Left,
                    Font = GameService.Content.DefaultFont16,
                };

                _amountLabel = new Label()
                {
                    Text = "Amount",
                    Parent = _headerPanel,
                    Width = amount,
                    Location = new(_tradePartnerLabel.Right, _tradePartnerLabel.Top),
                    HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Left,
                    Font = GameService.Content.DefaultFont16,
                };

                _itemSummaryLabel = new Label()
                {
                    Text = "Items",
                    Parent = _headerPanel,
                    Width = itemSummary,
                    Location = new(_amountLabel.Right, _amountLabel.Top),
                    HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Left,
                    Font = GameService.Content.DefaultFont16,
                };

                ctrl = _tradingHeader = new TradeHistoryHeaderControl()
                {
                    Parent = _headerPanel,
                    Location = new(0, _itemSummaryLabel.Bottom + 5),
                    WidthSizingMode = SizingMode.Fill,
                    BorderWidth = new(0, 0, 0, 6),
                    BorderColor = Color.Black,
                    BackgroundColor = Color.Transparent,
                };

                ctrl = _filterBox = new()
                {
                    Parent = _headerPanel,
                    Location = new(0, ctrl.Bottom + 5),
                    PlaceholderText = strings_common.Search,
                    FilteringOnEnter = true,
                    FilteringOnTextChange = true,
                    FilteringDelay = 150,
                    PerformFiltering = FilterTrades,
                    Width = _headerPanel.ContentRegion.Width,
                };

                _tradeHistoryPanel = new()
                {
                    Location = new(0, ctrl?.Bottom + 5 ?? 0),
                    Parent = buildPanel,
                    FlowDirection = ControlFlowDirection.SingleTopToBottom,
                    CanScroll = false,
                    WidthSizingMode = SizingMode.Fill,
                    HeightSizingMode = SizingMode.Fill,
                    ControlPadding = new(4),
                    ContentPadding = new(0, 0, 25, 0),
                };

                width = buildPanel.ContentRegion.Width;

                var trades = _trades;

                if (trades is not null)
                {
                    UpdateTradingSummary();

                    var missing = trades.Except(_tradeHistoryEntries.Select(e => e.Trade));

                    if (missing.Any())
                    {
                        foreach (var trade in missing)
                        {
                            TradeHistoryEntryControl t;
                            _tradeHistoryEntries.Add(t = new TradeHistoryEntryControl(trade)
                            {
                                Parent = _tradeHistoryPanel,
                                Width = 1105,
                            });

                            t.DeleteRequested += DeleteRequested;

                            if (_cts.IsCancellationRequested)
                            {
                                return;
                            }
                        }

                        TradeHistoryEntriesLoaded?.Invoke(this, _tradeHistoryEntries);
                    }
                }

                int i = 1;
                foreach (var trade in _tradeHistoryEntries)
                {
                    trade.Index = i;
                    i++;
                }

                foreach (var trade in _tradeHistoryEntries)
                {
                    trade.Parent = _tradeHistoryPanel;
                    trade.Width = 1105;

                    if (_cts.IsCancellationRequested)
                    {
                        return;
                    }
                }

                _tradeHistoryPanel.CanScroll = true;
            }

            _tradeHistoryPanel.SortChildren<TradeHistoryEntryControl>((a, b) => a.Index.CompareTo(b.Index));
        }

        private void DeleteRequested(object sender, Trade e)
        {
            e.TradeSummaryChanged -= Trade_TradeSummaryChanged;
            _ = _tradeHistoryEntries.Remove(_tradeHistoryEntries.FirstOrDefault(ctrl => ctrl.Trade == e));

            _tradeHistoryPanel?.Invalidate();
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

        public void CancelLoad()
        {
            _cts?.Cancel();
        }

        protected override void Unload()
        {
            base.Unload();
            _cts?.Cancel();

            _headerPanel?.Children.DisposeAll();
            _headerPanel?.Dispose();
        }
    }
}
