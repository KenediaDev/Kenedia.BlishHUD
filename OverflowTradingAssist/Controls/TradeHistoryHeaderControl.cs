using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.OverflowTradingAssist.Views;
using Microsoft.Xna.Framework;

namespace Kenedia.Modules.OverflowTradingAssist.Controls
{
    public class TradeHistoryHeaderControl : Panel
    {
        private readonly bool _isCreated = false;
        private readonly Image _rankIcon;
        private readonly Label _tradePartnerLabel;
        private readonly Label _amountLabel;
        private readonly Label _itemSummaryLabel;
        private readonly TradeRank _tradeRank;

        public TradeHistoryHeaderControl(string partners, double amount, TradeRank tradeRank)
        {
            _tradeRank = tradeRank;
            var color = _tradeRank?.Color ?? Color.White;

            HeightSizingMode = Blish_HUD.Controls.SizingMode.Standard;
            WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill;
            BackgroundColor = Color.Black * 0.2F;
            BorderColor = Color.Black;
            BorderWidth = new(2);
            ContentPadding = new(8, 4, 8, 8);

            Height = 40;

            _tradePartnerLabel = new Label()
            {
                Parent = this,
                Text = partners,
                Height = ContentRegion.Height,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Middle,
                TextColor = Color.White,
                Font = Content.DefaultFont16,
            };

            _amountLabel = new Label()
            {
                Parent = this,
                Text = string.Format("{0:##g 00s 00c}", amount),
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Middle,
                Height = ContentRegion.Height,
                TextColor = Color.White,
                Font = Content.DefaultFont14,
            };

            _rankIcon = new Image()
            {
                Parent = this,
                Texture = _tradeRank?.Icon,
                Size = new(ContentRegion.Height),
            };

            _itemSummaryLabel = new Label()
            {
                Parent = this,
                Text = $"{_tradeRank.Name}",
                Height = ContentRegion.Height,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Middle,
                TextColor = color,
                Font = Content.DefaultFont16,
            };

            _isCreated = true;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (!_isCreated) return;

            int trailing = (32 * 4) + (5 * 4);
            int width = Width - trailing;
            int partner = (int)(width * 0.15F);
            int amount = (int)(width * 0.15F);
            int itemSummary = (int)(width * 0.70F);

            _tradePartnerLabel?.SetLocation(0, 0);
            _tradePartnerLabel?.SetSize(partner, _tradePartnerLabel.Height);

            _amountLabel?.SetLocation(_tradePartnerLabel.Right, 0);
            _amountLabel?.SetSize(amount, _amountLabel.Height);

            _rankIcon?.SetLocation(_amountLabel.Right, 0);

            _itemSummaryLabel?.SetLocation(_rankIcon.Right, 0);
            _itemSummaryLabel?.SetSize(itemSummary, _rankIcon.Height);
        }
    }
}
