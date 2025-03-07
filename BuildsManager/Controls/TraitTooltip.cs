﻿using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Blish_HUD.ContentService;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.Core.Interfaces;
using System;

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class TraitTooltip : Blish_HUD.Controls.Tooltip, ILocalizable
    {
        private readonly DetailedTexture _image = new() { TextureRegion = new(14, 14, 100, 100), };
        private readonly Label _title;
        private readonly Label _id;
        private readonly Label _description;

        private Trait _trait;

        public TraitTooltip()
        {
            WidthSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
            HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
            AutoSizePadding = new(5);

            Rectangle imageBounds;
            _image.Bounds = imageBounds = new(4, 4, 48, 48);

            _title = new()
            {
                Parent = this,
                Height = Content.DefaultFont16.LineHeight,
                AutoSizeWidth = true,
                Location = new(imageBounds.Right, imageBounds.Top),
                Font = Content.DefaultFont16,
            };

            _id = new()
            {
                Parent = this,
                Height = Content.DefaultFont12.LineHeight,
                AutoSizeWidth = true,
                Location = new(imageBounds.Right, _title.Bottom),
                Font = Content.DefaultFont12,
                TextColor = Color.White * 0.8F,
            };

            _description = new()
            {
                Parent = this,
                Width = 300,
                AutoSizeHeight = true,
                Location = new(imageBounds.Left, imageBounds.Bottom + 10),
                Font = Content.DefaultFont14,
                WrapText = true,
            };

            LocalizingService.LocaleChanged += UserLocale_SettingChanged;
        }

        public Trait Trait { get => _trait; set => Common.SetProperty(ref _trait, value, ApplyTrait); }

        public Func<string> SetLocalizedTooltip { get; set; }

        private void ApplyTrait(object sender, ValueChangedEventArgs<Trait> e)
        {
            _title.TextColor = Colors.Chardonnay;
            _title.Text = Trait?.Name;
            _id.Text = $"{strings.TraitId}: {Trait?.Id}";
            _description.Text = Trait?.Description.InterpretItemDescription() ?? strings.MissingInfoFromAPI;

            if (Trait is not null)
            {
                _image.Texture = TexturesService.GetAsyncTexture(Trait.IconAssetId);

                if (_image.Texture is not null)
                {
                    int padding = _image.Texture.Width / 16;
                    _image.TextureRegion = new(padding, padding, _image.Texture.Width - (padding * 2), _image.Texture.Height - (padding * 2));
                }
            }
        }

        public void UserLocale_SettingChanged(object sender, Blish_HUD.ValueChangedEventArgs<Locale> e)
        {
            ApplyTrait(this, null);
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle drawBounds, Rectangle scissor)
        {
            if (Trait == null) return;
            base.Draw(spriteBatch, drawBounds, scissor);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            _image.Draw(this, spriteBatch);
        }

        override protected void DisposeControl()
        {
            Trait = null;

            LocalizingService.LocaleChanged -= UserLocale_SettingChanged;
            _image?.Dispose();
            base.DisposeControl();
        }
    }
}
