using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Blish_HUD;
using System;
using ItemType = Gw2Sharp.WebApi.V2.Models.ItemType;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using System.Linq;
using Kenedia.Modules.Core.Services;
using Gw2Sharp.WebApi;
using Kenedia.Modules.BuildsManager.Res;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage
{
    public class ItemTooltip : Blish_HUD.Controls.Tooltip
    {
        private readonly Image _image;
        private readonly Label _title;
        private readonly Label _id;
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
                Height = Content.DefaultFont16.LineHeight,
                AutoSizeWidth = true,
                Location = new(_image.Right + 10, _image.Top),
                Font = Content.DefaultFont16,
            };

            _id = new()
            {
                Parent = this,
                Height = Content.DefaultFont12.LineHeight,
                AutoSizeWidth = true,
                Location = new(_image.Right + 10, _title.Bottom),
                Font = Content.DefaultFont12,
                TextColor = Color.White * 0.8F,
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

            LocalizingService.LocaleChanged += UserLocale_SettingChanged;
        }

        public BaseItem Item { get => _item; set => Common.SetProperty(ref _item, value, ApplyItem); }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, ApplyStat); }

        private void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            ApplyItem(this, null);
        }

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
                        _description.Text = string.Join(Environment.NewLine, Stat?.Attributes.Values.Where(e => e is not null).Select(e => $"+ {Math.Round(e.Value + (e.Multiplier * (armor?.AttributeAdjustment ?? 0)))} {e.Id.GetDisplayName()}"));
                        _description.TextColor = Color.Lime;
                    }
                    break;

                case ItemType.Weapon:
                    if (Item is Weapon weapon)
                    {
                        _description.Text = string.Join(Environment.NewLine, Stat?.Attributes.Values.Where(e => e is not null).Select(e => $"+ {Math.Round(e.Value + (e.Multiplier * (weapon?.AttributeAdjustment ?? 0)))} {e.Id.GetDisplayName()}"));
                        _description.TextColor = Color.Lime;
                    }
                    break;

                case ItemType.Trinket:
                case ItemType.Back:
                    if (Item is Trinket trinket)
                    {
                        _description.Text = string.Join(Environment.NewLine, Stat?.Attributes.Values.Where(e => e is not null).Select(e => $"+ {Math.Round(e.Value + (e.Multiplier * (trinket?.AttributeAdjustment ?? 0)))} {e.Id.GetDisplayName()}"));
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
            _id.Text = $"{strings.ItemId}: {Item?.Id}";
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

                case ItemType.UpgradeComponent:
                    if (Item is Rune rune)
                    {
                        _description.Text = rune.BonusDescriptions.Bonuses.Select(e => e.InterpretItemDescription()).ToList().Enumerate(Environment.NewLine, "({0}): ");
                    }
                    else if (Item is Sigil sigil)
                    {
                        _description.Text = sigil.Buff.InterpretItemDescription();
                    }
                    else if (Item is Infusion infusion)
                    {
                        _description.Text = infusion.DisplayText;
                    }
                    else if (Item is Enrichment enrichment)
                    {
                        _description.Text = enrichment.DisplayText;
                    }
                    break;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle drawBounds, Rectangle scissor)
        {
            if (Item == null) return;
            base.Draw(spriteBatch, drawBounds, scissor);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if (_image?.Texture is not null)
            {
                spriteBatch.DrawFrame(this, _image.LocalBounds.Add(2, 2, 6, 4), _frameColor, 2);
            }
        }
    }
}
