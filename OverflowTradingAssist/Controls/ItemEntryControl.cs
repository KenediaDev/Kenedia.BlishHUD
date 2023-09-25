using Blish_HUD.Content;
using Blish_HUD.Input;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.OverflowTradingAssist.Controls.GearPage;
using Kenedia.Modules.OverflowTradingAssist.DataModels;
using Kenedia.Modules.OverflowTradingAssist.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kenedia.Modules.OverflowTradingAssist.Controls
{
    public class ItemEntryControl : Panel
    {
        private readonly ItemAmount _itemAmount;
        private readonly SelectableItemControl _itemControl;
        private readonly Label _itemLabel;

        private readonly ButtonImage _effectivePriceButton;
        private readonly CoinControl _effectivePrice;
        private readonly ButtonImage _deleteButton;
        private ItemAmount _item = new();

        public ItemEntryControl(ItemAmount itemAmount)
        {
            if (itemAmount is null) return;

            _itemAmount = itemAmount;

            HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
            BorderColor = Color.Black;
            BorderWidth = new(2);
            BackgroundColor = Color.Black * 0.2F;
            ContentPadding = new(5);

            _itemControl = new()
            {
                Parent = this,
                Size = new(30, 30),
                Location = new(0, 0),
                OnItemSelected = (item) =>
                {
                    ItemAmount.Item = item;
                    _itemLabel.Text = item?.Name;
                },
            };

            _itemLabel = new()
            {
                Parent = this,
                Location = new(_itemControl.Right + 5, _itemControl.Top),
                Text = "Item Name",
                Size = new(208, 30),
                WrapText = true,
            };

            _effectivePriceButton = new()
            {
                Texture = AsyncTexture2D.FromAssetId(156753),
                BasicTooltipText = "Effective Price",
                Location = new(_itemLabel.Right + 5, _itemLabel.Top),
                Parent = this,
                Size = new(30),
                ClickAction = GetTpPriceButton,
                Enabled = false,
            };

            _effectivePrice = new()
            {
                Parent = this,
                Location = new(_effectivePriceButton.Right, _effectivePriceButton.Top),
                Size = new(220, 30),
                ValueChangedAction = (e) => _itemAmount.Value = e,
            };

            _deleteButton = new()
            {
                Texture = AsyncTexture2D.FromAssetId(156674),
                HoveredTexture = AsyncTexture2D.FromAssetId(156675),
                BasicTooltipText = "Delete",
                Size = new(30),
                Parent = this,
                Location = new(_effectivePrice.Right + 7, _effectivePrice.Top),
                ClickAction = DeleteButton
            };

            ItemAmount.Item = DataModels.Item.UnkownItem;
        }

        private void DeleteButton(MouseEventArgs args)
        {
            var parent = Parent;
            Dispose();
            Parent = null;
            ItemAmount?.RequestDelete();

            parent?.Invalidate();
        }

        private async void GetTpPriceButton(MouseEventArgs args = null)
        {
            if (ItemAmount?.Item != null)
            {
                var price = await OverflowTradingAssist.ModuleInstance.Gw2ApiManager.Gw2ApiClient.V2.Commerce.Prices.GetAsync(ItemAmount.Item.Id);

                if (price is not null)
                {
                    _effectivePrice.Value = price.Sells.UnitPrice;
                }
            }
        }

        public ItemAmount ItemAmount { get => _item; set => Common.SetProperty(ref _item, value, OnItemChanged); }

        private void OnItemChanged(object sender, ValueChangedEventArgs<ItemAmount> e)
        {
            _itemControl.Item = e.NewValue?.Item;
            _itemLabel.Text = e.NewValue?.Item?.Name;
            _effectivePrice.Value = e.NewValue?.Value ?? 0;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int width = ContentRegion.Width;
            int height = ContentRegion.Height;

            if (_itemControl is not null && _effectivePriceButton is not null && _itemLabel is not null && _effectivePrice is not null && _deleteButton is not null)
            {
                _itemControl.Size = new Point(height);
                _itemControl.Location = new Point(0, 0);

                _effectivePriceButton.Size = new Point(height);
                _effectivePriceButton.Location = new Point(width - _effectivePriceButton.Width - _effectivePrice.Width - _deleteButton.Width - 7, 0);

                //_effectivePrice.Size = new Point(_effectivePrice.Width, height);
                _effectivePrice.Location = new Point(_effectivePriceButton.Right, 0);

                _deleteButton.Size = new Point(height);
                _deleteButton.Location = new Point(width - _deleteButton.Width, 0);

                _itemLabel.Size = new Point(_effectivePriceButton.Left - _itemControl.Right, height);
                _itemLabel.Location = new(_itemControl.Right + 5, 0);
            }

        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);
        }
    }

    public class ItemExEntryControl : Panel
    {
        private ItemControl _itemControl;
        private Label _itemLabel;
        private Label _rateLabel;

        private ButtonImage _deleteButton;
        private ButtonImage _getTpPriceButton;
        private ButtonImage _effectivePriceButton;
        private CoinControl _itemPrice;
        private CoinControl _effectivePrice;
        private NumberBox _rateBox;
        private Item _item;

        public ItemExEntryControl()
        {
            HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
            BorderColor = Color.Black;
            BorderWidth = new(2);
            BackgroundColor = Color.Black * 0.2F;
            ContentPadding = new(5);

            _itemControl = new()
            {
                Parent = this,
                Size = new(30, 30),
                Location = new(0, 0),
            };

            _itemLabel = new()
            {
                Parent = this,
                Location = new(_itemControl.Right + 5, 0),
                Text = "Item Name",
                Size = new(240, 30),
                WrapText = true,
            };

            _getTpPriceButton = new()
            {
                Texture = AsyncTexture2D.FromAssetId(255379),
                HoveredTexture = AsyncTexture2D.FromAssetId(255378),
                BasicTooltipText = "Get TP Price",
                Location = new(_itemLabel.Right, 0),
                Parent = this,
                Size = new(27),
                ClickAction = GetTpPriceButton
            };

            _itemPrice = new()
            {
                Parent = this,
                Location = new(_getTpPriceButton.Right, 0),
                Size = new(200, 27),
            };

            _deleteButton = new()
            {
                Texture = AsyncTexture2D.FromAssetId(156674),
                HoveredTexture = AsyncTexture2D.FromAssetId(156675),
                BasicTooltipText = "Delete",
                Size = new(27),
                Parent = this,
                Location = new(_itemPrice.Right + 5, _itemPrice.Top),
                ClickAction = DeleteButton
            };

            _rateBox = new()
            {
                Parent = this,
                Location = new(_itemLabel.Left, _itemLabel.Bottom + 5),
                Size = new(40, 27),
                Value = 85,
                MinValue = 1,
                MaxValue = 100,
                ShowButtons = false,
            };

            _rateLabel = new()
            {
                Parent = this,
                Location = new(_rateBox.Right + 4, _rateBox.Top),
                Text = "%",
                Size = new(20, 27),
                HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Left,
            };

            _effectivePriceButton = new()
            {
                Texture = AsyncTexture2D.FromAssetId(156753),
                BasicTooltipText = "Effective Price",
                Location = new(_getTpPriceButton.Left, _getTpPriceButton.Bottom + 5),
                Parent = this,
                Size = new(27),
                ClickAction = GetTpPriceButton,
                Enabled = false,
            };

            _effectivePrice = new()
            {
                Parent = this,
                Location = new(_effectivePriceButton.Right, _effectivePriceButton.Top),
                Size = new(200, 27),
            };
        }

        private void DeleteButton(MouseEventArgs args)
        {
            var parent = Parent;
            Dispose();
            Parent = null;

            parent?.Invalidate();
        }

        private async void GetTpPriceButton(MouseEventArgs args = null)
        {
            if (Item != null)
            {
                var price = await OverflowTradingAssist.ModuleInstance.Gw2ApiManager.Gw2ApiClient.V2.Commerce.Prices.GetAsync(Item.Id);

                if (price is not null)
                {
                    _effectivePrice.Value = price.Sells.UnitPrice;
                }
            }
        }

        public Item Item { get => _item; set => Common.SetProperty(ref _item, value, OnItemChanged); }

        private void OnItemChanged(object sender, ValueChangedEventArgs<Item> e)
        {
            _itemControl.Item = Item;
            _itemLabel.Text = Item?.Name;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);
        }
    }
}
