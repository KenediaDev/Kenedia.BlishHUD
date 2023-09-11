using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Views;
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
    public class DetailedTradeView : StandardWindow
    {
        public DetailedTradeView(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion)
        {
        }
    }

    public class TradeHistoryEntryControl : FlowPanel
    {
        private readonly bool _isCreated = false;
        private readonly Label _tradePartnerLabel;
        private readonly Label _amountLabel;
        private readonly Label _itemSummaryLabel;
        private readonly ButtonImage _reviewLinkLabel;
        private readonly ButtonImage _tradeListingLinkLabel;
        private readonly ButtonImage _edit;
        private readonly ButtonImage _delete;
        private readonly Image _tradeImage;
        private readonly Panel _panel;

        private DetailedTradeView _detailedTradeView;

        public TradeHistoryEntryControl(Trade trade, Color? color = null)
        {
            color ??= Color.White;
            Trade = trade;

            FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleLeftToRight;
            HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
            //WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill;

            _panel = new()
            {
                Parent = this,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
                BackgroundColor = Color.Black * 0.2F,
                BorderColor = Color.Black,
                BorderWidth = new(2),
                ContentPadding = new(8, 4, 4, 4),
            };

            _tradePartnerLabel = new()
            {
                Parent = _panel,
                Text = Trade.TradePartner,
                Height = 32,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Middle,
                TextColor = (Color)color,
            };

            _amountLabel = new()
            {
                Parent = _panel,
                Text = string.Format("{0:#g 00s 00c}", Trade.Amount),
                Height = 32,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Middle,
                TextColor = (Color)color,
            };

            _tradeImage = new()
            {
                Parent = _panel,
                SetLocalizedTooltip = () => Trade.TradeType is TradeType.Buy ? "Buy" : Trade.TradeType is TradeType.Sell ? "Sell" : "None",
                Texture = AsyncTexture2D.FromAssetId(Trade.TradeType is TradeType.Buy ? 157326 : Trade.TradeType is TradeType.Sell ? 157328 : 157095),
                Size = new(32),
            };

            _itemSummaryLabel = new()
            {
                Parent = _panel,
                Text = $"{Trade.ItemSummary}",
                Height = 32,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Middle,
                TextColor = (Color)color,
            };

            _reviewLinkLabel = new()
            {
                Parent = _panel,
                SetLocalizedTooltip = () => "Open Trade Review Link",
                Size = new(32),
                Texture = AsyncTexture2D.FromAssetId(1234950),
                HoveredTexture = AsyncTexture2D.FromAssetId(1304070),
                ClickAction = (b) =>
                {
                    if (!string.IsNullOrEmpty(Trade?.ReviewLink))
                        _ = Process.Start(Trade.ReviewLink);
                },
            };
            _reviewLinkLabel.Click += ReviewLinkLabel_Click;

            _tradeListingLinkLabel = new()
            {
                Parent = _panel,
                SetLocalizedTooltip = () => "Open Trade Listing Link",
                Size = new(32),
                Texture = AsyncTexture2D.FromAssetId(255379),
                HoveredTexture = AsyncTexture2D.FromAssetId(255378),
                ClickAction = (b) =>
                {
                    if (!string.IsNullOrEmpty(Trade?.TradeListingLink))
                        _ = Process.Start(Trade.TradeListingLink);
                },
            };
            _tradeListingLinkLabel.Click += TradeListingLinkLabel_Click;

            _edit = new()
            {
                Parent = _panel,
                SetLocalizedTooltip = () => "Edit Trade",
                Texture = AsyncTexture2D.FromAssetId(2175779),
                HoveredTexture = AsyncTexture2D.FromAssetId(2175781),
                Size = new(32),
                ClickAction = (b) =>
                {
                    if (_detailedTradeView is null)
                    {
                        _detailedTradeView ??= CreateDetailedView();
                        _detailedTradeView?.Show();
                    }
                    else
                    {
                        _detailedTradeView?.Dispose();
                        _detailedTradeView = null;
                    }
                }
            };

            _delete = new()
            {
                Parent = _panel,
                SetLocalizedTooltip = () => "Delete Trade",
                Texture = AsyncTexture2D.FromAssetId(156674),
                HoveredTexture = AsyncTexture2D.FromAssetId(156675),
                Size = new(32),
            };

            _isCreated = true;
        }
        
        private DetailedTradeView CreateDetailedView()
        {
            var settingsBg = AsyncTexture2D.FromAssetId(155983);
            Texture2D cutSettingsBg = settingsBg.Texture.GetRegion(0, 0, settingsBg.Width - 100, settingsBg.Height - 390);
            return new(
                settingsBg,
                new Rectangle(30, 30, cutSettingsBg.Width + 10, cutSettingsBg.Height),
                new Rectangle(30 + 46, 35, cutSettingsBg.Width - 46, cutSettingsBg.Height - 15))
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "❤",
                Subtitle = "❤",
                SavesPosition = true,
                Id = $"{Trade.Id} MainWindow",
                MainWindowEmblem = AsyncTexture2D.FromAssetId(156014),
                SubWindowEmblem = AsyncTexture2D.FromAssetId(156019),
                Name = $"{Trade.TradePartner} - {string.Format("{0:#g 00s 00c}", Trade.Amount)}",
                Width = 400,
                Height = 500,
            };
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

        public Trade Trade { get; private set; }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (!_isCreated) return;

            _panel.Width = ContentRegion.Width - 2;

            int width = _panel.ContentRegion.Width - (_delete.Width + _tradeImage.Width + _edit.Width + _tradeListingLinkLabel.Width + _reviewLinkLabel.Width + (5 * 5));
            int partner = (int)(width * 0.2F);
            int amount = (int)(width * 0.2F);
            int itemSummary = (int)(width * 0.60F);

            _tradePartnerLabel?.SetLocation(0, 0);
            _tradePartnerLabel?.SetSize(partner, _tradePartnerLabel.Height);

            _amountLabel?.SetLocation(_tradePartnerLabel.Right, 0);
            _amountLabel?.SetSize(amount, _amountLabel.Height);

            _tradeImage.SetLocation(_amountLabel.Right, 0);

            _itemSummaryLabel?.SetLocation(_tradeImage.Right + 5, 0);
            _itemSummaryLabel?.SetSize(itemSummary, _itemSummaryLabel.Height);

            _reviewLinkLabel?.SetLocation(_itemSummaryLabel.Right + 5, 0);
            _tradeListingLinkLabel?.SetLocation(_reviewLinkLabel.Right + 5, 0);
            _edit?.SetLocation(_tradeListingLinkLabel.Right + 5, 0);
            _delete?.SetLocation(_edit.Right + 5, 0);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            _panel?.Children?.DisposeAll();
            _panel?.Dispose();
            _detailedTradeView?.Dispose();
            Trade = null;
        }
    }
}
