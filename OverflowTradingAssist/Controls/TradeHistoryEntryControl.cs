using Blish_HUD.Content;
using Blish_HUD.Input;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.OverflowTradingAssist.Controls.GearPage;
using Kenedia.Modules.OverflowTradingAssist.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.OverflowTradingAssist.Controls
{
    public class TradeItemSummary : FlowPanel
    {

    }

    public class TradeHistoryEntryControl : FlowPanel
    {
        private readonly bool _isCreated = false;
        private readonly Label _tradePartnerLabel;
        private readonly Label _amountLabel;
        private readonly Label _itemSummaryLabel;
        private readonly Button _reviewLinkLabel;
        private readonly Button _tradeListingLinkLabel;
        private readonly ButtonImage _edit;
        private readonly ImageButton _delete;
        private readonly Panel _panel;

        public TradeHistoryEntryControl(Trade trade, Color? color = null)
        {
            color ??= Color.White;
            Trade = trade;

            FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleLeftToRight;
            HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
            WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill;

            _panel = new Panel()
            {
                Parent = this,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
                BackgroundColor = Color.Black * 0.2F,
                BorderColor = Color.Black,
                BorderWidth = new(2),
                ContentPadding = new(8, 4, 8, 8),
            };

            _tradePartnerLabel = new Label()
            {
                Parent = _panel,
                Text = Trade.TradePartner,
                Height = ContentRegion.Height,
                AutoSizeHeight = true,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Middle,
                TextColor = (Color)color,
            };

            _amountLabel = new Label()
            {
                Parent = _panel,
                Text = string.Format("{0:#g 00s 00c}", Trade.Amount),
                AutoSizeHeight = true,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Middle,
                TextColor = (Color)color,
            };

            _itemSummaryLabel = new Label()
            {
                Parent = _panel,
                Text = $"{Trade.ItemSummary}",
                Height = ContentRegion.Height,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Middle,
                AutoSizeHeight = true,
                TextColor = (Color)color,
            };

            _reviewLinkLabel = new Button()
            {
                Parent = _panel,
                Text = "Open Trade Review Link",
                Width = 200,
                Icon = AsyncTexture2D.FromAssetId(156762),
                ClickAction = () =>
                {
                    if (!string.IsNullOrEmpty(Trade?.ReviewLink))
                        _ = Process.Start(Trade.ReviewLink);
                },
            };
            _reviewLinkLabel.Click += ReviewLinkLabel_Click;

            _tradeListingLinkLabel = new Button()
            {
                Parent = _panel,
                Text = "Open Trade Listing Link",
                Width = 200,
                Icon = AsyncTexture2D.FromAssetId(156764),
                ClickAction = () =>
                {
                    if (!string.IsNullOrEmpty(Trade?.TradeListingLink))
                        _ = Process.Start(Trade.TradeListingLink);
                },
            };
            _tradeListingLinkLabel.Click += TradeListingLinkLabel_Click;

            _edit = new ButtonImage()
            {
                Parent = this,
                Texture = AsyncTexture2D.FromAssetId(2175779),
                HoveredTexture = AsyncTexture2D.FromAssetId(2175781),
                Size = new(20),
            };

            _delete = new ImageButton()
            {
                Parent = this,
                Texture = AsyncTexture2D.FromAssetId(2175782),
                HoveredTexture = AsyncTexture2D.FromAssetId(2175784),
                Size = new(20),
            };

            _isCreated = true;
        }

        private void ReviewLinkLabel_Click(object sender, MouseEventArgs e)
        {
            if (!string.IsNullOrEmpty(Trade?.ReviewLink))
                _ = Process.Start(Trade.ReviewLink);
        }

        private void TradeListingLinkLabel_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            if (!string.IsNullOrEmpty(Trade?.TradeListingLink))
                _ = Process.Start(Trade.TradeListingLink);
        }

        public Trade Trade { get; }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (!_isCreated) return;

            _edit.SetSize(ContentRegion.Height, ContentRegion.Height);
            _delete.SetSize(ContentRegion.Height, ContentRegion.Height);

            _panel.Width = ContentRegion.Width - (_delete.Width + (int)ControlPadding.X + _edit.Width);

            int width = _panel.ContentRegion.Width - 435;
            int partner = (int)(width * 0.25F);
            int amount = (int)(width * 0.25F);
            int itemSummary = (int)(width * 0.50F);

            _tradePartnerLabel?.SetLocation(0, 0);
            _tradePartnerLabel?.SetSize(partner, _tradePartnerLabel.Height);

            _amountLabel?.SetLocation(_tradePartnerLabel.Right, 0);
            _amountLabel?.SetSize(amount, _amountLabel.Height);

            _itemSummaryLabel?.SetLocation(_amountLabel.Right, 0);
            _itemSummaryLabel?.SetSize(itemSummary, _itemSummaryLabel.Height);

            _reviewLinkLabel?.SetLocation(_itemSummaryLabel.Right, 0);
            _tradeListingLinkLabel?.SetLocation(_reviewLinkLabel.Right, 0);
        }
    }
}
