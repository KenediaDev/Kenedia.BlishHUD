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

namespace Kenedia.Modules.OverflowTradingAssist.Views
{
    public class MainWindow : TabbedWindow
    {
        private readonly TradePresenter _tradePresenter = new();
        private readonly MailingService _mailingService;
        private Trade _trade;

        public MainWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, MailingService mailingService, Func<List<Trade>> getTrades) : base(background, windowRegion, contentRegion)
        {
            _mailingService = mailingService;

            Tabs.Add(new Tab(AsyncTexture2D.FromAssetId(156753), () => new TradeView(_mailingService, _tradePresenter), "Trade"));
            Tabs.Add(new Tab(AsyncTexture2D.FromAssetId(156746), () => new TradeHistoryView(getTrades), "Trade History"));

            _mailingService.MailReady += MailingService_MailReady;
            _mailingService.TimeElapsed += MailingService_TimeElapsed;
        }

        public Trade Trade { get => _trade; set => Common.SetProperty(ref _trade, value, ApplyTrade); }

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
    }
}
