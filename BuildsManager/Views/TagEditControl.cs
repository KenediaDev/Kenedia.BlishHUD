﻿using Blish_HUD.Content;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using System;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class TagEditControl : Panel
    {
        private TemplateTag _tag;
        private readonly (Label label, TextBox textBox) _name;
        private readonly (Label label, NumberBox numberBox) _iconId;

        private readonly (Label label, NumberBox numberBox) _x;
        private readonly bool _created;
        private readonly (Label label, NumberBox numberBox) _y;
        private readonly (Label label, NumberBox numberBox) _width;
        private readonly (Label label, NumberBox numberBox) _height;

        private readonly Image _icon;

        public TagEditControl()
        {
            Height = 150;
            CanCollapse = true;
            Collapsed = true;
            ContentPadding = new(5);

            _name = new(
                new()
                {
                    Parent = this,
                    SetLocalizedText = () => "Tag Text",
                },
                new()
                {
                    Parent = this,
                    Width = 200,
                    Height = 32,
                    SetLocalizedPlaceholder = () => "Name",
                    Location = new(0, Content.DefaultFont14.LineHeight + 2),
                    TextChangedAction = (txt) =>
                    {
                        if (!string.IsNullOrEmpty(txt))
                        {
                            Tag.Name = txt;
                            Title = txt;
                            _ = (TemplateTags?.Save());
                        };
                    },
                });

            _icon = new()
            {
                Parent = this,
                Size = new(32),
                Location = new(0, Content.DefaultFont14.LineHeight + 2),
                BackgroundColor = Color.Black * 0.4F,
            };

            _iconId = new(
                new()
                {
                    Parent = this,
                    SetLocalizedText = () => "Asset Id",
                },
                new()
                {
                    Parent = this,
                    Width = 100,
                    ShowButtons = false,
                    Location = new(0, Content.DefaultFont14.LineHeight + 2),
                    Height = 32,
                    ValueChangedAction = (n) => SetIcon(),
                });

            _x = new(
                new()
                {
                    Parent = this,
                    SetLocalizedText = () => "X",
                    HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Center,
                },
                new()
                {
                    Parent = this,
                    ShowButtons = false,
                    Height = 25,
                    ValueChangedAction = (n) => SetIcon(),
                });

            _y = new(
                new()
                {
                    Parent = this,
                    SetLocalizedText = () => "Y",
                    HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Center,
                },
                new()
                {
                    Parent = this,
                    ShowButtons = false,
                    Height = 25,
                    ValueChangedAction = (n) => SetIcon(),
                });

            _width = new(
                new()
                {
                    Parent = this,
                    SetLocalizedText = () => "Width",
                    HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Center,
                },
                new()
                {
                    Parent = this,
                    ShowButtons = false,
                    Height = 25,
                    ValueChangedAction = (n) => SetIcon(),
                });

            _height = new(
                new()
                {
                    Parent = this,
                    SetLocalizedText = () => "Height",
                    HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Center,
                },
                new()
                {
                    Parent = this,
                    ShowButtons = false,
                    Height = 25,
                    ValueChangedAction = (n) => SetIcon(),
                });

            _created = true;

            Menu = new();
            _ = Menu.AddMenuItem(new ContextMenuItem(() => strings.Delete, () => RemoveTag(Tag)));

        }

        private void RemoveTag(TemplateTag tag)
        {
            BuildsManager.ModuleInstance.TemplateTags.Remove(Tag);
            Dispose();
        }

        public TemplateTag Tag { get => _tag; set => Common.SetProperty(ref _tag, value, OnTagChanged); }

        public TemplateTags TemplateTags { get; set; }

        private void OnTagChanged(object sender, ValueChangedEventArgs<TemplateTag> e)
        {
            Title = e.NewValue?.Name;
            TitleIcon = e.NewValue?.Icon?.Texture;

            var r = e.NewValue?.TextureRegion ?? e.NewValue?.Icon?.Bounds ?? Rectangle.Empty;
            _icon.SourceRectangle = r;
            TitleTextureRegion = r;

            _name.textBox.Text = e.NewValue?.Name;
            _icon.Texture = e.NewValue?.Icon?.Texture;
            _iconId.numberBox.Value = e.NewValue?.AssetId ?? 0;

            _x.numberBox.Value = r.X;
            _y.numberBox.Value = r.Y;
            _width.numberBox.Value = r.Width;
            _height.numberBox.Value = r.Height;
        }

        private void SetIcon()
        {
            if (AsyncTexture2D.FromAssetId(_iconId.numberBox.Value) is AsyncTexture2D icon)
            {
                _icon.SourceRectangle = icon != _icon.Texture
                    ? new(0, 0, icon.Width, icon.Height)
                    : new(_x.numberBox.Value, _y.numberBox.Value, _width.numberBox.Value, _height.numberBox.Value);

                if (icon != _icon.Texture)
                {
                    _x.numberBox.Value = 0;
                    _y.numberBox.Value = 0;
                    _width.numberBox.Value = icon.Width;
                    _height.numberBox.Value = icon.Height;
                }

                _icon.Texture = icon;
                TitleIcon = icon;

                Tag.AssetId = _iconId.numberBox.Value;
                Tag.Icon.Texture = icon;
                TitleTextureRegion = Tag.TextureRegion = _icon.SourceRectangle;

                _ = (TemplateTags?.Save());
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (!_created) return;

            int x = ContentRegion.Width;
            int y = ContentRegion.Height;

            _name.textBox.Width = x - (_iconId.numberBox.Width + _icon.Width + 5);
            _name.label.Location = new(_name.textBox.Left, 0);
            _name.label.Width = _name.textBox.Width;

            _icon.Location = new(_name.textBox.Right + 5, _name.textBox.Top);

            _iconId.numberBox.Location = new(_icon.Right + 5, _name.textBox.Top);
            _iconId.label.Location = new(_iconId.numberBox.Left, 0);
            _iconId.label.Width = _iconId.numberBox.Width;

            int w = (x - (5 * 4)) / 4;
            _x.label.Location = new(0, _name.textBox.Bottom + 5);
            _x.label.Width = w;

            _x.numberBox.Location = new(_x.label.Left, _x.label.Bottom);
            _x.numberBox.Width = w;

            _y.label.Location = new(_x.numberBox.Right + 5, _x.label.Top);
            _y.label.Width = w;

            _y.numberBox.Location = new(_y.label.Left, _y.label.Bottom);
            _y.numberBox.Width = w;

            _width.label.Location = new(_y.numberBox.Right + 5, _y.label.Top);
            _width.label.Width = w;

            _width.numberBox.Location = new(_width.label.Left, _width.label.Bottom);
            _width.numberBox.Width = w;

            _height.label.Location = new(_width.numberBox.Right + 5, _width.label.Top);
            _height.label.Width = w;

            _height.numberBox.Location = new(_height.label.Left, _height.label.Bottom);
            _height.numberBox.Width = w;
        }
    }
}