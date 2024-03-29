﻿using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Blish_HUD;
using System;
using ItemType = Kenedia.Modules.Core.DataModels.ItemType;
using System.Linq;
using Kenedia.Modules.Core.Services;
using Gw2Sharp.WebApi;
using Kenedia.Modules.OverflowTradingAssist.DataModels;
using Kenedia.Modules.OverflowTradingAssist.Res;

namespace Kenedia.Modules.OverflowTradingAssist.Controls.GearPage
{
    public class ItemTooltip : Blish_HUD.Controls.Tooltip
    {
        private readonly Image _image;
        private readonly Label _title;
        private readonly Label _id;
        private readonly Label _description;
        private readonly Label _commentLabel;

        private Item _item;
        private string _comment;

        private Color _frameColor = Color.Transparent;
        private Func<string> _setLocalizedComment;

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

            _commentLabel = new()
            {
                Parent = this,
                Width = 300,
                AutoSizeHeight = true,
                Location = new(_description.Left, _description.Bottom + 10),
                Font = Content.DefaultFont12,
                TextColor = Color.DarkGray,
                WrapText = true,
                Visible = false,
            };

            LocalizingService.LocaleChanged += UserLocale_SettingChanged;
        }

        public Item Item { get => _item; set => Common.SetProperty(ref _item, value, ApplyItem); }

        public string Comment { get => _comment; set => Common.SetProperty(ref _comment, value, ApplyComment); }

        public Func<string> SetLocalizedComment { get => _setLocalizedComment; set => Common.SetProperty(ref _setLocalizedComment, value, ApplyLocalizedComment); }

        private void ApplyLocalizedComment(object sender, Core.Models.ValueChangedEventArgs<Func<string>> e)
        {
            Comment = SetLocalizedComment?.Invoke();
        }

        private void ApplyComment(object sender, Core.Models.ValueChangedEventArgs<string> e)
        {
            _commentLabel.Text = Comment;
            _commentLabel.Visible = !string.IsNullOrEmpty(Comment);
        }

        private void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            ApplyItem(this, null);
            Comment = SetLocalizedComment?.Invoke();
        }

        private void ApplyItem(object sender, Core.Models.ValueChangedEventArgs<Item> e)
        {
            _image.Texture = Item?.Icon;
            _frameColor = Item?.Rarity.GetColor() ?? Color.Transparent;
            _title.Text = Item?.Name;
            _id.Text = $"{strings.ItemId}: {Item?.Id}";
            _title.TextColor = Item?.Rarity.GetColor() ?? Color.White;

            if (Item is Item item && item.Icon != null)
            {
                int padding = item.Icon.Width / 16;
                _image.SourceRectangle = new(padding, padding, Item.Icon.Width - (padding * 2), Item.Icon.Height - (padding * 2));
            }

            switch (Item?.Type)
            {
                default:
                    _description.Text = Item?.Description?.InterpretItemDescription() ?? strings.MissingInfoFromAPI;
                    break;
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _commentLabel?.SetLocation(new(_description?.Left ?? 0, (_description?.Bottom ?? 0) + 2));
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

        protected override void DisposeControl()
        {
            base.DisposeControl();

            LocalizingService.LocaleChanged -= UserLocale_SettingChanged;
        }
    }
}
