using Gw2Sharp.WebApi.V2.Models;
using System;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;

namespace Kenedia.Modules.OverflowTradingAssist.Models
{
    public class TradePresenter
    {
        private Trade _trade = new();

        public TradePresenter()
        {
            
        }

        public Trade Trade { get => _trade; set => Common.SetProperty(ref _trade, value, SetupTrade); }

        private void SetupTrade(object sender, ValueChangedEventArgs<Trade> e)
        {
            if (e.OldValue != null)
            {

            }

            if (e.NewValue != null)
            {

            }
        }
    }
}
