using Blish_HUD.Content;
using Kenedia.Modules.Core.Views;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Kenedia.Modules.OverflowTradingAssist.Services;
using Kenedia.Modules.OverflowTradingAssist.Res;
using Tab = Blish_HUD.Controls.Tab;
using Kenedia.Modules.OverflowTradingAssist.Models;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.OverflowTradingAssist.Controls;
using Kenedia.Modules.Core.Extensions;

namespace Kenedia.Modules.OverflowTradingAssist.Views
{
    public class MainWindow : TabbedWindow
    {
        public static int WindowWidth = 1200;
        public static int WindowHeight = 900;

        public static int ContentWidth = 1130;
        public static int ContentHeight = 825;

        private readonly TradePresenter _tradePresenter = new();
        private readonly MailingService _mailingService;
        private readonly Func<List<Trade>> _getTrades;
        private List<TradeHistoryEntryControl> _tradeHistoryEntries = [];
        private TradeHistoryView _historyView;
        private TradeView _tradeView;

        public MainWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, MailingService mailingService, Func<List<Trade>> getTrades) : base(background, windowRegion, contentRegion)
        {
            _mailingService = mailingService;
            _getTrades = getTrades;
            Width = WindowWidth;
            Height = WindowHeight;

            Tabs.Add(new Tab(AsyncTexture2D.FromAssetId(156753), () =>
            {
                if (_tradeView is not null)
                    _tradeView.TradeAdded -= View_TradeAdded;

                _tradeView = new TradeView(_mailingService, _tradePresenter);
                _tradeView.TradeAdded += View_TradeAdded;

                return _tradeView;
            },
            "Trade"));
            Tabs.Add(new Tab(AsyncTexture2D.FromAssetId(156746), () =>
            {
                if (_historyView is not null)
                    _historyView.TradeHistoryEntriesLoaded -= View_TradeHistoryEntriesLoaded;

                _historyView = new TradeHistoryView(_getTrades?.Invoke(), _tradeHistoryEntries);
                _historyView.TradeHistoryEntriesLoaded += View_TradeHistoryEntriesLoaded;

                return _historyView;
            }, "Trade History"));

            _mailingService.MailReady += MailingService_MailReady;
            _mailingService.TimeElapsed += MailingService_TimeElapsed;
        }

        private void View_TradeAdded(object sender, Trade e)
        {
            if (_getTrades?.Invoke() is List<Trade> trades)
            {
                if (!trades.Contains(e))
                {
                    trades.Add(e);
                }
            }
        }

        private void View_TradeHistoryEntriesLoaded(object sender, List<TradeHistoryEntryControl> e)
        {
            _tradeHistoryEntries = e;
        }

        public Trade Trade { get; set => Common.SetProperty(field, value, v => field = v, ApplyTrade); }

        private void ApplyTrade(object sender, ValueChangedEventArgs<Trade> e)
        {
            _tradePresenter.Trade = e.NewValue;
        }

        private void MailingService_TimeElapsed(object sender, EventArgs e)
        {
            SubName = string.Format(strings.WaitForMail, _mailingService.RemainingMailDelay);
            SubNameColor = Color.OrangeRed;
        }

        private void MailingService_MailReady(object sender, EventArgs e)
        {
            SubName = strings.MailReady;
            SubNameColor = Color.Lime;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            _historyView?.CancelLoad();
            _tradeHistoryEntries?.DisposeAll();
        }
    }
}
