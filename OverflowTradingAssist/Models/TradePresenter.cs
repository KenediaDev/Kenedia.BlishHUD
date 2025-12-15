using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;

namespace Kenedia.Modules.OverflowTradingAssist.Models
{
    public class TradePresenter
    {
        public TradePresenter()
        {
            
        }

        public Trade Trade { get; set => Common.SetProperty(field, value, v => field = v, SetupTrade); } = new();

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
