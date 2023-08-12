using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Blish_HUD;
using System;
using Kenedia.Modules.BuildsManager.TemplateEntries;
using ItemType = Gw2Sharp.WebApi.V2.Models.ItemType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using System.Linq;
using static Blish_HUD.ContentService;
using System.Diagnostics;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage
{
    public class ItemTooltip : Blish_HUD.Controls.Tooltip
    {
        private readonly Image _image;
        private readonly Label _title;
        private readonly Label _description;

        private BaseItem _item;
        private Stat _stat;

        private Color _frameColor = Color.Transparent;

        public ItemTooltip()
        {
            WidthSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
            HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
            AutoSizePadding = new(5);

            _image = new()
            {
                Parent = this,
                Size = new(48),
                Location = new(2),
            };

            _title = new()
            {
                Parent = this,
                Height = 48,
                AutoSizeWidth = true,
                Location = new(_image.Right + 10, _image.Top),
                Font = Content.DefaultFont16,
                WrapText = true,
            };

            _description = new()
            {
                Parent = this,
                Width = 300,
                AutoSizeHeight = true,
                Location = new(_image.Left, _image.Bottom + 10),
                Font = Content.DefaultFont14,
                WrapText = true,
            };
        }

        public BaseItem Item { get => _item; set => Common.SetProperty(ref _item, value, ApplyItem); }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, ApplyStat); }

        private void ApplyStat(object sender, Core.Models.ValueChangedEventArgs<Stat> e)
        {
            if (Stat == null)
            {
                _description.Text = string.Empty;
                return;
            }

            switch (Item?.Type)
            {
                case ItemType.Armor:
                    if (Item is Armor armor)
                    {
                        _description.Text = string.Join(Environment.NewLine, Stat?.Attributes.Values.Where(e => e != null).Select(e => $"+ {Math.Round(e.Value + (e.Multiplier * (armor?.AttributeAdjustment ?? 0)))} {e.Id.GetDisplayName()}"));
                        _description.TextColor = Color.Lime;
                    }
                    break;

                case ItemType.Weapon:
                    if (Item is Weapon weapon)
                    {
                        _description.Text = string.Join(Environment.NewLine, Stat?.Attributes.Values.Where(e => e != null).Select(e => $"+ {Math.Round(e.Value + (e.Multiplier * (weapon?.AttributeAdjustment ?? 0)))} {e.Id.GetDisplayName()}"));
                        _description.TextColor = Color.Lime;
                    }
                    break;

                case ItemType.Trinket:
                case ItemType.Back:
                    if (Item is Trinket trinket)
                    {
                        _description.Text = string.Join(Environment.NewLine, Stat?.Attributes.Values.Where(e => e != null).Select(e => $"+ {Math.Round(e.Value + (e.Multiplier * (trinket?.AttributeAdjustment ?? 0)))} {e.Id.GetDisplayName()}"));
                        _description.TextColor = Color.Lime;
                    }
                    break;
            }
        }

        private void ApplyItem(object sender, Core.Models.ValueChangedEventArgs<BaseItem> e)
        {
            _image.Texture = Item?.Icon;
            _frameColor = Item?.Rarity.GetColor() ?? Color.Transparent;
            _title.Text = Item?.Name;
            _title.TextColor = Item?.Rarity.GetColor() ?? Color.White;

            switch (Item?.Type)
            {
                case ItemType.Consumable:
                    if (Item is Nourishment nourishment)
                    {
                        _description.Text = nourishment.Details.Description;
                    }
                    else if (Item is DataModels.Items.Utility utility)
                    {
                        _description.Text = utility.Details.Description;
                    }
                    break;

                case ItemType.Armor:
                case ItemType.Weapon:
                case ItemType.Trinket:
                case ItemType.Back:
                    ApplyStat(sender, null);
                    break;
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if (_image?.Texture != null)
            {
                spriteBatch.DrawFrame(this, _image.LocalBounds.Add(2, 2, 6, 4), _frameColor, 2);
            }
        }
    }

    public class ItemControl : Blish_HUD.Controls.Control
    {
        private readonly DetailedTexture _texture = new();

        private BaseItem _item;
        private Stat _stat;
        private Color _frameColor = Color.Transparent;

        public ItemControl()
        {
            Tooltip = new ItemTooltip();
        }

        public BaseItem Item { get => _item; set => Common.SetProperty(ref _item, value, ApplyItem); }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, ApplyStat); }

        public Color TextureColor { get => _texture.DrawColor ?? Color.White; set => _texture.DrawColor = value; }

        private void ApplyStat(object sender, Core.Models.ValueChangedEventArgs<Stat> e)
        {
            Debug.WriteLine($"ApplyStat: {e.NewValue?.Name}");

            if (Tooltip is ItemTooltip itemTooltip)
                itemTooltip.Stat = Stat;
        }

        private void ApplyItem(object sender, Core.Models.ValueChangedEventArgs<BaseItem> e)
        {
            Debug.WriteLine($"ApplyItem: {e.NewValue?.Name}");

            _frameColor = Item?.Rarity.GetColor() ?? Color.Transparent;
            _texture.Texture = Item?.Icon;

            if (Tooltip is ItemTooltip itemTooltip)
                itemTooltip.Item = Item;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _texture.Bounds = LocalBounds.Add(2, 2, -4, -4);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            _texture.Draw(this, spriteBatch, RelativeMousePosition, TextureColor);

            spriteBatch.DrawFrame(this, bounds, _frameColor, 2);
        }
    }
}
