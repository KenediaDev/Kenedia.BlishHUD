using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.OverflowTradingAssist.Controls.GearPage;
using Kenedia.Modules.OverflowTradingAssist.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.OverflowTradingAssist.Controls
{
    public class TradeItemSummary : FlowPanel
    {

    }

    public class TradeHistoryEntryControl : Panel
    {
        private readonly bool _isCreated = false;
        private readonly Label _tradePartnerLabel;
        private readonly Label _amountLabel;
        private readonly Label _itemSummaryLabel;
        private readonly Label _reviewLinkLabel;
        private readonly Label _tradeListingLinkLabel;

        public TradeHistoryEntryControl(Trade trade, Color? color = null)
        {
            color ??= Color.White;
            Trade = trade;

            HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
            WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill;
            BackgroundColor = Color.Black * 0.2F;
            BorderColor = Color.Black;
            BorderWidth = new(2);
            ContentPadding = new(8, 4, 8, 4);
            

            _tradePartnerLabel = new Label()
            {
                Parent = this,
                Text = Trade.TradePartner,
                Height = ContentRegion.Height,
                AutoSizeHeight = true,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Middle,
                TextColor = (Color)color,
            };

            _amountLabel = new Label()
            {
                Parent = this,
                Text = string.Format("{0:#g 00s 00c}", Trade.Amount),
                AutoSizeHeight = true,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Middle,
                TextColor = (Color)color,
            };

            _itemSummaryLabel = new Label()
            {
                Parent = this,
                Text = $"{Trade.ItemSummary}",
                Height = ContentRegion.Height,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Middle,
                AutoSizeHeight = true,
                TextColor = (Color)color,
            };

            _reviewLinkLabel = new Label()
            {
                Parent = this,
                Text = $"{Trade.ReviewLink}",
                Font = Content.DefaultFont12,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Middle,
                TextColor = (Color)color,
            };

            _tradeListingLinkLabel = new Label()
            {
                Parent = this,
                Text = $"{Trade.TradeListingLink}",
                Font = Content.DefaultFont12,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Middle,
                TextColor = (Color)color,
            };

            _isCreated = true;
        }

        public Trade Trade { get; }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (!_isCreated) return;

            int partner = (int)(Width * 0.15F);
            int amount = (int)(Width * 0.1F);
            int itemSummary = (int)(Width * 0.35F);
            int review = (int)(Width * 0.2F);
            int listing = (int)(Width * 0.2F);

            _tradePartnerLabel?.SetLocation(0, 0);
            _tradePartnerLabel?.SetSize(partner, _tradePartnerLabel.Height);

            _amountLabel?.SetLocation(_tradePartnerLabel.Right, 0);
            _amountLabel?.SetSize(amount, _amountLabel.Height);

            _itemSummaryLabel?.SetLocation(_amountLabel.Right, 0);
            _itemSummaryLabel?.SetSize(itemSummary, _itemSummaryLabel.Height);

            _reviewLinkLabel?.SetLocation(_itemSummaryLabel.Right, 0);
            _reviewLinkLabel?.SetSize(review, ContentRegion.Height);

            _tradeListingLinkLabel?.SetLocation(_reviewLinkLabel.Right, 0);
            _tradeListingLinkLabel?.SetSize(listing, ContentRegion.Height);
        }
    }

    public class TradeHistoryHeaderControl : Panel
    {
        private readonly bool _isCreated = false;
        private readonly Label _tradePartnerLabel;
        private readonly Label _amountLabel;
        private readonly Label _itemSummaryLabel;
        private readonly Label _reviewLinkLabel;
        private readonly Label _tradeListingLinkLabel;

        public TradeHistoryHeaderControl(string partners, double amount, string items, Color? color = null)
        {
            color ??= Color.White;

            HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
            WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill;
            BackgroundColor = Color.Black * 0.2F;
            BorderColor = Color.Black;
            BorderWidth = new(2);
            ContentPadding = new(8, 4, 8, 4);
            

            _tradePartnerLabel = new Label()
            {
                Parent = this,
                Text = partners,
                Height = ContentRegion.Height,
                AutoSizeHeight = true,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Middle,
                TextColor = (Color)color,
                Font = Content.DefaultFont16,
            };

            _amountLabel = new Label()
            {
                Parent = this,
                Text = string.Format("{0:#g 00s 00c}", amount),
                AutoSizeHeight = true,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Middle,
                TextColor = (Color)color,
                Font = Content.DefaultFont16,
            };

            _itemSummaryLabel = new Label()
            {
                Parent = this,
                Text = $"{items}",
                Height = ContentRegion.Height,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Middle,
                AutoSizeHeight = true,
                TextColor = (Color)color,
                Font = Content.DefaultFont16,
            };

            _reviewLinkLabel = new Label()
            {
                Parent = this,
                Font = Content.DefaultFont16,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Middle,
                TextColor = (Color)color,
            };

            _tradeListingLinkLabel = new Label()
            {
                Parent = this,
                Font = Content.DefaultFont16,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Middle,
                TextColor = (Color)color,
            };

            _isCreated = true;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (!_isCreated) return;

            int partner = (int)(Width * 0.15F);
            int amount = (int)(Width * 0.1F);
            int itemSummary = (int)(Width * 0.35F);
            int review = (int)(Width * 0.2F);
            int listing = (int)(Width * 0.2F);

            _tradePartnerLabel?.SetLocation(0, 0);
            _tradePartnerLabel?.SetSize(partner, _tradePartnerLabel.Height);

            _amountLabel?.SetLocation(_tradePartnerLabel.Right, 0);
            _amountLabel?.SetSize(amount, _amountLabel.Height);

            _itemSummaryLabel?.SetLocation(_amountLabel.Right, 0);
            _itemSummaryLabel?.SetSize(itemSummary, _itemSummaryLabel.Height);

            _reviewLinkLabel?.SetLocation(_itemSummaryLabel.Right, 0);
            _reviewLinkLabel?.SetSize(review, ContentRegion.Height);

            _tradeListingLinkLabel?.SetLocation(_reviewLinkLabel.Right, 0);
            _tradeListingLinkLabel?.SetSize(listing, ContentRegion.Height);
        }
    }
}
