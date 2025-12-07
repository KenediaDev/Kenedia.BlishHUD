using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kenedia.Modules.Core.Extensions;
using Blish_HUD;
using System;
using Blish_HUD.Input;
using Microsoft.Xna.Framework.Input;
using Kenedia.Modules.OverflowTradingAssist.DataModels;
using Kenedia.Modules.OverflowTradingAssist.Res;
using System.Diagnostics;

namespace Kenedia.Modules.OverflowTradingAssist.Controls.GearPage
{

    public class SelectableItemControl : ItemControl
    {
        private ItemSelection _itemSelection;

        public Action<Item> OnItemSelected { get; set; }

        protected override void ApplyItem(object sender, Core.Models.ValueChangedEventArgs<Item> e)
        {
            base.ApplyItem(sender, e);

            OnItemSelected?.Invoke(Item);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            _itemSelection ??= new()
            {
                OnItemSelected = (item) => Item = item,
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = ZIndex + 100,
            };

            _itemSelection?.SetLocation(AbsoluteBounds.Location.Add(new((int)(Height * 0.66))));
            _itemSelection?.Show();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            _itemSelection?.Dispose();
        }
    }

    public class ItemControl : Blish_HUD.Controls.Control
    {
        private readonly DetailedTexture _texture = new();
        private int _frameThickness = 2;
        private Color _frameColor = Color.White * 0.15F;
        private Rectangle _frameRectangle;
        private Rectangle _nameRectanlge;

        public ItemControl()
        {
            Tooltip = new ItemTooltip()
            {
                SetLocalizedComment = () => Environment.NewLine + strings.ItemControlClickToCopyItem,
            };
        }

        public ItemControl(DetailedTexture placeholder) : this()
        {
            Placeholder = placeholder;
        }

        public Action<Item> OnClicked { get; set; }

        public Item Item { get; set => Common.SetProperty(ref field, value, ApplyItem); }

        public Color TextureColor { get => _texture.DrawColor ?? Color.White; set => _texture.DrawColor = value; }

        public DetailedTexture Placeholder { get; set => Common.SetProperty(ref field, value, ApplyPlaceholder); } = new();

        private void ApplyPlaceholder(object sender, Core.Models.ValueChangedEventArgs<DetailedTexture> e)
        {

        }

        protected virtual void ApplyItem(object sender, Core.Models.ValueChangedEventArgs<Item> e)
        {
            _frameColor = Item?.Rarity.GetColor() ?? Color.White * 0.15F;
            _texture.Texture = Item?.Icon;

            if (Item is Item item && item.Icon != null)
            {
                int padding = item.Icon.Width / 16;
                _texture.TextureRegion = new(padding, padding, Item.Icon.Width - (padding * 2), Item.Icon.Height - (padding * 2));
            }

            if (Tooltip is ItemTooltip itemTooltip)
            {
                itemTooltip.Item = Item;
                itemTooltip.SetLocalizedComment = (Item?.Type) switch
                {
                    Core.DataModels.ItemType.Armor or Core.DataModels.ItemType.Back or Core.DataModels.ItemType.Trinket or Core.DataModels.ItemType.Weapon => () => Environment.NewLine + strings.ItemControlClickToCopyItem,
                    _ => () => Environment.NewLine + strings.ItemControlClickToCopyItem,
                };
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int size = Math.Min(Width, Height);
            int padding = 3;
            _frameThickness = CalculateFrameThickness();

            _texture.Bounds = new(_frameThickness, _frameThickness, Height - (_frameThickness * 2), Height - (_frameThickness * 2));
            Placeholder.Bounds = new(_frameThickness, _frameThickness, Height - (_frameThickness * 2), Height - (_frameThickness * 2));
            _frameRectangle = new(0, 0, Height, Height);
            _nameRectanlge = new(_texture.Bounds.Right + 15, 0, Width - _texture.Bounds.Right + 15, Height);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Item == null)
            {
                Placeholder.Draw(this, spriteBatch, RelativeMousePosition, TextureColor);
            }
            else
            {
                _texture.Draw(this, spriteBatch, RelativeMousePosition, TextureColor);
            }

            spriteBatch.DrawFrame(this, _frameRectangle, _frameColor, _frameThickness);
            spriteBatch.DrawStringOnCtrl(this, Item?.Name, Content.DefaultFont16, _nameRectanlge, ContentService.Colors.ColonialWhite);
        }

        private int CalculateFrameThickness()
        {
            int size = Math.Min(Width, Height);

            return Math.Max(2, size / 24);
        }

        protected override async void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (Item != null)
            {
                try
                {
                    Debug.WriteLine($"{nameof(OnClick)}");
                    if (Input.Keyboard.ActiveModifiers.HasFlag(ModifierKeys.Ctrl))
                    {
                        _ = await ClipboardUtil.WindowsClipboardService.SetTextAsync(Item.Name);
                        return;
                    }

                    OnClicked?.Invoke(Item);
                }
                catch
                {

                }
            }

        }

        override protected void DisposeControl()
        {
            Item = null;
            _texture?.Dispose();
            Placeholder?.Dispose();
            base.DisposeControl();
        }
    }
}
