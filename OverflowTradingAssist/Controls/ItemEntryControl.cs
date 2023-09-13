using Blish_HUD.Content;
using Blish_HUD.Input;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.OverflowTradingAssist.Controls.GearPage;
using Kenedia.Modules.OverflowTradingAssist.DataModels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kenedia.Modules.OverflowTradingAssist.Controls
{
    public class ItemEntryControl : Panel
    {
        private ItemControl _itemControl;
        private Label _itemLabel;

        private ButtonImage _deleteButton;
        private ButtonImage _getTpPriceButton;
        private CoinControl _coinControl;
        private NumberBox _rateBox;
        private Item _item;

        public ItemEntryControl()
        {
            HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;

            _itemControl = new()
            {
                Parent = this,
                Size = new(30, 30),
                Location = new(0, 0),
            };

            _itemLabel = new()
            {
                Parent = this,
                Location = new(_itemControl.Right, 0),
                AutoSizeHeight = true,
                Text = "Item Name",
                Width = 150,
                WrapText = true,
            };

            _getTpPriceButton = new()
            {
                Texture = AsyncTexture2D.FromAssetId(255379),
                HoveredTexture = AsyncTexture2D.FromAssetId(255378),
                BasicTooltipText = "Get TP Price",
                Location = new(_itemLabel.Right, 0),
                Parent = this,
                Size = new(25),
                ClickAction = GetTpPriceButton
            };

            _coinControl = new()
            {
                Parent = this,
                Location = new(_getTpPriceButton.Right, 0),
                Width = 150,
            };

            _rateBox = new()
            {
                Parent = this,
                Location = new(_coinControl.Right, 0),
                Width = 50,                
                Value = 85,
                MinValue = 1,
                MaxValue = 100,
            };

            _deleteButton = new()
            {
                Texture = AsyncTexture2D.FromAssetId(156674),
                HoveredTexture = AsyncTexture2D.FromAssetId(156675),
                BasicTooltipText = "Delete",
                Size = new(25),
                Parent = this,
                Location = new(_rateBox.Right, 5),
                ClickAction = DeleteButton
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
                    _coinControl.Value = price.Sells.UnitPrice;
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
