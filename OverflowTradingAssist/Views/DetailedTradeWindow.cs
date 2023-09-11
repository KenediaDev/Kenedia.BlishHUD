using Blish_HUD;
using Blish_HUD.Content;
using Kenedia.Modules.Core.Views;
using Kenedia.Modules.OverflowTradingAssist.Models;
using Microsoft.Xna.Framework;

namespace Kenedia.Modules.OverflowTradingAssist.Views
{
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
