using Blish_HUD.Graphics.UI;
using System;
using System.Linq;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.OverflowTradingAssist.Services;
using Microsoft.Xna.Framework;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.OverflowTradingAssist.Models;
using Kenedia.Modules.Core.Models;
using Blish_HUD.Content;
using Kenedia.Modules.OverflowTradingAssist.Controls;
using System.Diagnostics;

namespace Kenedia.Modules.OverflowTradingAssist.Views
{

    public class TradeView : View
    {
        private readonly MailingService _mailingService;
        private ButtonImage _tradePartnerButtonImage;
        private TextBox _tradePartnerTextBox;
        private ButtonImage _tradeAmountButtonImage;
        private CoinControl _amountNumberBox;
        private ButtonImage _tradeReviewButtonImage;
        private TextBox _reviewTextBox;
        private ButtonImage _tradeListingButtonImage;
        private TextBox _listingTextBox;
        private ButtonImage _mailButton;
        private ButtonImage _tradeTypeImage;
        private TradePresenter _tradePresenter;
        private Dropdown _tradeTypeDropdown;
        private ItemPanel _itemsPanel;
        private ItemPanel _paymentPanel;

        public TradeView(MailingService mailingService, TradePresenter _tradePresenter)
        {
            _mailingService = mailingService;
            TradePresenter = _tradePresenter;
        }

        public TradePresenter TradePresenter { get => _tradePresenter; private set => Common.SetProperty(ref _tradePresenter, value, ApplyTrade); }

        public Trade Trade => TradePresenter?.Trade;

        public event EventHandler<Trade> TradeAdded;

        private void ApplyTrade(object sender, ValueChangedEventArgs<TradePresenter> e)
        {

        }

        protected override void Build(Blish_HUD.Controls.Container buildPanel)
        {
            base.Build(buildPanel);
            Blish_HUD.Controls.Container c = buildPanel;

            int width = MainWindow.ContentWidth;
            int height = MainWindow.ContentHeight;

            _tradePartnerButtonImage = new ButtonImage()
            {
                Location = new(0, 0),
                Parent = c,
                Size = new(27),
                Texture = AsyncTexture2D.FromAssetId(733268),
                BasicTooltipText = "Trade Partner",
                Enabled = false,
            };

            _tradePartnerTextBox = new TextBox()
            {
                Location = new(_tradePartnerButtonImage.Right, _tradePartnerButtonImage.Top),
                Parent = c,
                Width = (int)(width * 0.435F) - _tradePartnerButtonImage.Width,
                PlaceholderText = "Trade Partner",
                Text = _tradePresenter?.Trade?.TradePartner,
                TextChangedAction = (s) =>
                {
                    if (TradePresenter?.Trade is not null)
                    {
                        TradePresenter.Trade.TradePartner = s;
                    }
                },
            };

            _tradeTypeImage = new ButtonImage()
            {
                Location = new(_tradePartnerButtonImage.Left, _tradePartnerButtonImage.Bottom + 5),
                Parent = c,
                Size = new(27),
                Texture = AsyncTexture2D.FromAssetId(Trade?.TradeType is TradeType.Sell ? 157328 : 157326),
                BasicTooltipText = "Click to change the trade Type!"
            };
            _tradeTypeImage.Click += TradeTypeImage_Click;

            _tradeTypeDropdown = new Dropdown()
            {
                Location = new(_tradeTypeImage.Right, _tradeTypeImage.Top),
                Parent = c,
                Width = _tradePartnerTextBox.Width,
                BasicTooltipText = "Trade Type",
                Items = { "Buy", "Sell" },
                SelectedItem = Trade?.TradeType is TradeType.Sell ? "Sell" : "Buy",
                ValueChangedAction = (s) =>
                {
                    if (Trade is not null)
                    {
                        Trade.TradeType = s is "Buy" ? TradeType.Buy : s is "Sell" ? TradeType.Sell : TradeType.None;
                        _tradeTypeImage.Texture = AsyncTexture2D.FromAssetId(Trade?.TradeType is TradeType.Buy ? 157326 : 157328);
                        _tradeTypeImage.HoveredTexture = AsyncTexture2D.FromAssetId(Trade?.TradeType is TradeType.Buy ? 157327 : 157329);
                    }
                }
            };

            int buttonWidth = (((MainWindow.ContentWidth - 5) / 2) - 10) / 3;

            Blish_HUD.Controls.Control ctrl = new Button()
            {
                Parent = c,
                Location = new(_tradeTypeImage.Left, _tradeTypeImage.Bottom + 5),
                SetLocalizedText = () => "Copy Listing",
                Width = buttonWidth,
                ClickAction = SetListing,
            };

            ctrl = new Button()
            {
                Parent = c,
                Location = new(ctrl.Right + 5, ctrl.Top),
                SetLocalizedText = () => "Copy Review",
                ClickAction = SetReview,
                Width = buttonWidth,
            };

            _ = new Button()
            {
                Parent = c,
                Location = new(ctrl.Right + 5, ctrl.Top),
                SetLocalizedText = () => "Save & Add Trade",
                ClickAction = SaveTrade,
                Width = buttonWidth,
            };

            _mailButton = new()
            {
                Location = new(_tradePartnerTextBox.Right + 10, 0),
                Parent = c,
                Size = new(_tradeTypeDropdown.Bottom - _tradePartnerTextBox.Top),
                Texture = AsyncTexture2D.FromAssetId(156727),
                HoveredTexture = AsyncTexture2D.FromAssetId(157106),
                ClickAction = (b) =>
                {
                    _mailingService.SendMail();
                    _mailButton.ImageColor = Color.OrangeRed;
                    //_mailButton.Texture = AsyncTexture2D.FromAssetId(156732);
                    //_mailButton.HoveredTexture = AsyncTexture2D.FromAssetId(156732);
                    _mailingService.MailReady += MailingService_MailReady;
                },
                SetLocalizedTooltip = () => "Send Mail",
            };

            _tradeAmountButtonImage = new ButtonImage()
            {
                Location = new(_tradePartnerTextBox.Right + (int)(width * 0.10F), _tradePartnerButtonImage.Top),
                Parent = c,
                Size = new(27),
                Texture = AsyncTexture2D.FromAssetId(156758),
                Enabled = false,
                Visible = false,
            };

            _tradeListingButtonImage = new ButtonImage()
            {
                Location = new(((MainWindow.ContentWidth - 5) / 2) + 5, _tradePartnerButtonImage.Top),
                Parent = c,
                Size = new(27),
                Texture = AsyncTexture2D.FromAssetId(255379),
                Enabled = false,
                BasicTooltipText = "Listing Link",
            };

            _listingTextBox = new TextBox()
            {
                Location = new(_tradeListingButtonImage.Right, _tradeListingButtonImage.Top),
                Parent = c,
                Width = ((MainWindow.ContentWidth - 5) / 2) - _tradeListingButtonImage.Width,
                PlaceholderText = "Listing Link",
                Text = Trade?.TradeListingLink,
                TextChangedAction = (s) =>
                {
                    if (Trade is not null)
                        Trade.TradeListingLink = s;
                }
            };

            _tradeReviewButtonImage = new ButtonImage()
            {
                Location = new(_tradeListingButtonImage.Left, _tradeListingButtonImage.Bottom + 5),
                Parent = c,
                Size = new(27),
                Texture = AsyncTexture2D.FromAssetId(1234950),
                Enabled = false,
                BasicTooltipText = "Review Link",
            };
            _reviewTextBox = new TextBox()
            {
                Location = new(_tradeReviewButtonImage.Right, _tradeReviewButtonImage.Top),
                Parent = c,
                Width = ((MainWindow.ContentWidth - 5) / 2) - _tradeReviewButtonImage.Width,
                PlaceholderText = "Review Link",
                Text = Trade?.ReviewLink,
                TextChangedAction = (s) =>
                {
                    if (Trade is not null)
                        Trade.ReviewLink = s;
                },
            };

            int coinWidth = 400;
            _amountNumberBox = new CoinControl()
            {
                Location = new((width - coinWidth), _tradeTypeImage.Bottom + 10),
                Parent = c,
                Width = coinWidth,
                Value = Trade?.Value ?? 0,
                Height = _tradePartnerTextBox.Height,
                //ValueChangedAction = (s) => _trade.Amount = s,
                Enabled = false,
                HideBackground = true,
                Font = Blish_HUD.GameService.Content.DefaultFont16,
            };

            var fp = new FlowPanel()
            {
                Parent = c,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                Height = MainWindow.ContentHeight - _amountNumberBox.Bottom - 5,
                Location = new(0, _amountNumberBox.Bottom + 5),
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new(5),
            };

            _itemsPanel = new ItemPanel()
            {
                Parent = fp,
                Title = "Items",
                CanScroll = true,
                Width = (MainWindow.ContentWidth - 5) / 2,
                Height = fp.ContentRegion.Height,
                BackgroundColor = Color.Black * 0.2F,
                BorderColor = Color.Black,
                BorderWidth = new(2),
                ClickAction = () => AddItem(),
            };

            _paymentPanel = new ItemPanel()
            {
                Parent = fp,
                Title = "Payment",
                CanScroll = true,
                Width = (MainWindow.ContentWidth - 5) / 2,
                Height = fp.ContentRegion.Height,
                BackgroundColor = Color.Black * 0.2F,
                BorderColor = Color.Black,
                BorderWidth = new(2),
                ClickAction = () => AddPayment(),
            };

            foreach (var item in Trade?.Items ?? Enumerable.Empty<ItemAmount>())
            {
                AddItem(item);
            }

            foreach (var item in Trade?.Payment ?? Enumerable.Empty<ItemAmount>())
            {
                AddPayment(item);
            }

            Debug.WriteLine($"{buildPanel.AbsoluteBounds}");
        }

        private void MailingService_MailReady(object sender, EventArgs e)
        {
            //_mailButton.Texture = AsyncTexture2D.FromAssetId(156727);
            //_mailButton.HoveredTexture = AsyncTexture2D.FromAssetId(157106);
            _mailButton.ImageColor = Color.White;
        }

        private void SaveTrade()
        {
            if (Trade is not null)
            {
                Trade?.RequestSave();
                TradeAdded?.Invoke(this, Trade);
            }
        }

        private async void SetListing()
        {
            try
            {
                Debug.WriteLine($"{nameof(SetListing)}");
                string[] textParts = new string[]
                {
                    Trade?.TradeType is TradeType.Buy ? "**[WTB]** " : "**[WTS]** ",
                };

                foreach (var item in Trade?.Items ?? Enumerable.Empty<ItemAmount>())
                {
                    if (item.Item is not null)
                        textParts = textParts.Append($"{item.Amount}x {item.Item.Name} [{string.Format("{0:#g 00s 00c}", item.Value)}]{(item.Amount > 1 ? "/ea" : string.Empty)},").ToArray();
                }
                textParts[textParts.Length - 1] = (textParts.Last()?.TrimEnd(','));

                textParts = textParts.Append($"{Environment.NewLine}IGN: {OverflowTradingAssist.ModuleInstance.Paths.AccountName}").ToArray();

                _ = await Blish_HUD.ClipboardUtil.WindowsClipboardService.SetTextAsync(string.Join("", textParts));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void SetReview()
        {
            try
            {
                Debug.WriteLine($"{nameof(SetReview)}");
                string[] textParts = new string[]
                {
                    Trade?.TradeType is TradeType.Buy ? "**[Bought]** " : "**[Sold]** ",
                };

                foreach (var item in Trade?.Items ?? Enumerable.Empty<ItemAmount>())
                {
                    if (item.Item is not null)
                        textParts = textParts.Append($"{item.Amount}x {item.Item.Name},").ToArray();
                }

                textParts[textParts.Length - 1] = (textParts.Last()?.TrimEnd(','));
                textParts = textParts.Append($" from @{Trade?.TradePartner}").ToArray();
                textParts = textParts.Append($" for a total of {string.Format("{0:#g 00s 00c}", Trade?.ItemValue)}").ToArray();

                _ = await Blish_HUD.ClipboardUtil.WindowsClipboardService.SetTextAsync(string.Join("", textParts));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void AddPayment(ItemAmount itemAmount = null)
        {
            itemAmount ??= new() { Item = DataModels.Item.Coin };

            _ = new ItemEntryControl(itemAmount)
            {
                Parent = _paymentPanel,
                Width = _paymentPanel.Width - 25,
                ItemAmount = itemAmount,
            };

            itemAmount.ValueChanged += ItemAmount_ValueChanged;
            itemAmount.DeleteRequested += ItemAmount_DeleteRequested;

            if (Trade?.Payment.FirstOrDefault(e => e == itemAmount) is null)
            {
                Trade?.Payment.Add(itemAmount);
            }
        }

        private void AddItem(ItemAmount itemAmount = null)
        {
            itemAmount ??= new();

            _ = new ItemEntryControl(itemAmount)
            {
                Parent = _itemsPanel,
                Width = _itemsPanel.Width - 25,
                ItemAmount = itemAmount,
            };

            itemAmount.DeleteRequested += ItemAmount_DeleteRequested;
            itemAmount.ValueChanged += ItemAmount_ValueChanged;

            if (Trade?.Items.FirstOrDefault(e => e == itemAmount) is null)
            {
                Trade?.Items.Add(itemAmount);
            }
        }

        private void ItemAmount_DeleteRequested(object sender, EventArgs e)
        {
            if (sender is ItemAmount itemAmount)
            {
                itemAmount.DeleteRequested -= ItemAmount_DeleteRequested;
                itemAmount.ValueChanged -= ItemAmount_ValueChanged;
                _ = Trade?.Items.Remove(itemAmount);
            }
        }

        private void ItemAmount_ValueChanged(object sender, Core.Models.ValueChangedEventArgs<decimal> e)
        {
            _amountNumberBox.Value = Trade?.ItemValue ?? 0;
        }

        private void TradeTypeImage_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _tradeTypeDropdown.SelectedItem = Trade.TradeType is TradeType.Sell ? "Buy" : "Sell";
        }
    }
}
