using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.OverflowTradingAssist.Controls.GearPage;
using Kenedia.Modules.OverflowTradingAssist.Models;
using Kenedia.Modules.OverflowTradingAssist.Views;
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
    public class TradeHistoryEntryControl : FlowPanel
    {
        private readonly bool _isCreated = false;
        private readonly Label _indexLabel;
        private readonly Label _tradePartnerLabel;
        private readonly Label _amountLabel;
        private readonly Label _itemSummaryLabel;
        private readonly ButtonImage _reviewLinkLabel;
        private readonly ButtonImage _tradeListingLinkLabel;
        private readonly ButtonImage _edit;
        private readonly ButtonImage _delete;
        private readonly Image _tradeImage;
        private readonly Panel _panel;
        private readonly FlowPanel _itemsPanel;
        private DetailedTradeWindow _detailedTradeView;
        private int _index;

        public TradeHistoryEntryControl(Trade trade, Color? color = null)
        {
            color ??= Color.White;
            Trade = trade;

            Trade.TradeSummaryChanged += Trade_TradeSummaryChanged;
            Trade.TotalTradeValueChanged += Trade_TotalTradeValueChanged;

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

            _indexLabel = new()
            {
                Parent = _panel,
                Text = $"{Index}.",
                Height = 32,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Middle,
                TextColor = (Color)color,
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
                Text = string.Format("{0:#g 00s 00c}", Trade.ItemValue),
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

            _itemsPanel = new()
            {
                Parent = _panel,
                Height = 32,
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new(2),
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
                SetLocalizedTooltip = () => "Details",
                Texture = AsyncTexture2D.FromAssetId(156746),
                HoveredTexture = AsyncTexture2D.FromAssetId(156747),
                Size = new(32),
                ClickAction = (b) =>
                {
                    _detailedTradeView ??= CreateDetailedView();
                    if (_detailedTradeView.Visible)
                    {
                        _detailedTradeView.Hide();
                    }
                    else
                    {
                        _detailedTradeView.Show();
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
                ClickAction = OnDeleteClick,
            };

            SetItems();
            _isCreated = true;
        }

        public event EventHandler<Trade> DeleteRequested;

        private async void OnDeleteClick(MouseEventArgs args)
        {
            var result = await new BaseDialog("Warning", "Are you sure you want to delete this trade?") { DesiredWidth = 300, AutoSize = true }.ShowDialog();

            if (result == DialogResult.OK)
            {
                Trade?.RequestDelete();
                DeleteRequested?.Invoke(this, Trade);
                Dispose();
            }
        }

        private void Trade_TotalTradeValueChanged(object sender, decimal e)
        {
            _amountLabel.Text = string.Format("{0:#g 00s 00c}", e);
        }

        private void Trade_TradeSummaryChanged(object sender, Trade e)
        {
            if (Trade is null)
                return;

            if (_tradePartnerLabel is not null)
                _tradePartnerLabel.Text = Trade.TradePartner;

            if (_amountLabel is not null)
                _amountLabel.Text = string.Format("{0:#g 00s 00c}", Trade.ItemValue);

            if (_tradeImage is not null)
            {
                _tradeImage.Texture = AsyncTexture2D.FromAssetId(Trade.TradeType is TradeType.Buy ? 157326 : Trade.TradeType is TradeType.Sell ? 157328 : 157095);
                _tradeImage.BasicTooltipText = Trade.TradeType is TradeType.Buy ? "Bought" : Trade.TradeType is TradeType.Sell ? "Sold" : "None";
            }

            SetItems();
        }

        private void SetItems()
        {
            if (_itemsPanel is null) return;

            _itemsPanel?.Children.DisposeAll();

            foreach (var e in Trade.Items)
            {
                _ = new ItemControl()
                {
                    Parent = _itemsPanel,
                    Size = new(_itemsPanel.ContentRegion.Height),
                    Item = e.Item,
                };
            }
        }

        public int Index { get => _index; set => Common.SetProperty(ref _index, value, OnIndexChanged); }

        private void OnIndexChanged(object sender, Core.Models.ValueChangedEventArgs<int> e)
        {
            _indexLabel.Text = string.Format("{0}.", e.NewValue);
        }

        private DetailedTradeWindow CreateDetailedView()
        {
            var settingsBg = AsyncTexture2D.FromAssetId(155983);
            Texture2D cutSettingsBg = settingsBg.Texture.GetRegion(0, 0, settingsBg.Width - 100, settingsBg.Height - 390);
            return new(
                Trade,
                settingsBg,
                new Rectangle(30, 30, cutSettingsBg.Width + 10, cutSettingsBg.Height),
                new Rectangle(30 + 20, 35, cutSettingsBg.Width - 20, cutSettingsBg.Height - 15));
        }

        private void ReviewLinkLabel_Click(object sender, MouseEventArgs e)
        {
            if (!string.IsNullOrEmpty(Trade?.ReviewLink))
                _ = Process.Start(Trade.ReviewLink);
        }

        private void TradeListingLinkLabel_Click(object sender, MouseEventArgs e)
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

            int width = ContentRegion.Width - _panel.ContentPadding.Horizontal - 2 - (_delete.Width + _tradeImage.Width + _edit.Width + _tradeListingLinkLabel.Width + _reviewLinkLabel.Width + 5 * 5);
            int index = (int)(width * 0.08F);
            int partner = (int)(width * 0.2F);
            int amount = (int)(width * 0.2F);
            int itemSummary = (int)(width * 0.52F);

            _indexLabel?.SetLocation(0, 0);
            _indexLabel?.SetSize(index, _tradePartnerLabel.Height);

            _tradePartnerLabel?.SetLocation(_indexLabel.Right, 0);
            _tradePartnerLabel?.SetSize(partner, _tradePartnerLabel.Height);

            _amountLabel?.SetLocation(_tradePartnerLabel.Right, 0);
            _amountLabel?.SetSize(amount, _amountLabel.Height);

            _tradeImage.SetLocation(_amountLabel.Right, 0);

            _itemsPanel?.SetLocation(_tradeImage.Right + 5, 0);
            _itemsPanel?.SetSize(itemSummary, _itemsPanel.Height);

            _reviewLinkLabel?.SetLocation(_itemsPanel.Right + 5, 0);
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
