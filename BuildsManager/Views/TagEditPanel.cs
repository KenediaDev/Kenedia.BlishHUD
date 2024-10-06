using Blish_HUD;
using Blish_HUD.Content;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.ComponentModel;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class TagEditPanel : Panel
    {
        private TemplateTag _tag;
        private readonly (Label label, TextBox textBox) _name;
        private readonly (Label label, Dropdown textBox) _group;
        private readonly (Label label, NumberBox numberBox) _iconId;
        private readonly (Label label, NumberBox numberBox) _priority;

        private readonly (Label label, NumberBox numberBox) _x;
        private readonly bool _created;
        private readonly (Label label, NumberBox numberBox) _y;
        private readonly (Label label, NumberBox numberBox) _width;
        private readonly (Label label, NumberBox numberBox) _height;
        private readonly Button _resetButton;
        private readonly Button _deleteButton;

        private readonly Image _icon;

        private Blish_HUD.Controls.Container? _draggingStartParent;
        private Point _draggingStart;
        private bool _dragging;

        private bool _loading;

        public TagEditPanel(TagGroups tagGroups)
        {
            TagGroups = tagGroups;
            ContentPadding = new(5);

            _name = new(
                new()
                {
                    Parent = this,
                    SetLocalizedText = () => strings.TagName,
                },
                new()
                {
                    Parent = this,
                    Width = 200,
                    Height = 32,
                    SetLocalizedPlaceholder = () => strings.TagName,
                    Location = new(0, Content.DefaultFont14.LineHeight + 2),
                    TextChangedAction = (txt) =>
                    {
                        if (!string.IsNullOrEmpty(txt) && Tag is not null)
                        {
                            Tag.Name = txt;
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
                    SetLocalizedText = () => strings.AssetId,
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

            _priority = new(
                new()
                {
                    Parent = this,
                    SetLocalizedText = () => strings.Priority,
                },
                new()
                {
                    Parent = this,
                    Width = 100,
                    ShowButtons = true,
                    MinValue = 0,
                    Location = new(0, _icon.Bottom + 25 + Content.DefaultFont14.LineHeight + 2),
                    Height = 32,
                    ValueChangedAction = (n) => SetPriority(),
                });

            _group = new(
                new()
                {
                    Parent = this,
                    SetLocalizedText = () => strings.Group,
                },
                new()
                {
                    Parent = this,
                    Width = 200,
                    Height = 32,
                    Location = new(0, _icon.Bottom + 25 + Content.DefaultFont14.LineHeight + 2),
                    ValueChangedAction = SetGroup,
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
                    ValueChangedAction = (n) => SetTextureRegion(),
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
                    ValueChangedAction = (n) => SetTextureRegion(),
                });

            _width = new(
                new()
                {
                    Parent = this,
                    SetLocalizedText = () => strings.Width,
                    HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Center,
                },
                new()
                {
                    Parent = this,
                    ShowButtons = false,
                    Height = 25,
                    ValueChangedAction = (n) => SetTextureRegion(),
                });

            _height = new(
                new()
                {
                    Parent = this,
                    SetLocalizedText = () => strings.Height,
                    HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Center,
                },
                new()
                {
                    Parent = this,
                    ShowButtons = false,
                    Height = 25,
                    ValueChangedAction = (n) => SetTextureRegion(),
                });

            _resetButton = new()
            {
                Parent = this,
                Text = strings.Reset,
                Height = 25,
                ClickAction = () => SetTextureRegionToTextureBounds(Tag?.Icon?.Texture),
            };

            _deleteButton = new()
            {
                Parent = this,
                Text = strings.Delete,
                Height = 25,
                ClickAction = () => RemoveTag(Tag),
                Visible = false,
            };

            _created = true;

            Menu = new();
            _ = Menu.AddMenuItem(new ContextMenuItem(() => strings.Delete, () => RemoveTag(Tag)));

            SetGroupDropdownItems();

            TagGroups.GroupAdded += TagGroups_GroupAdded;
            TagGroups.GroupChanged += TagGroups_GroupChanged;
            TagGroups.GroupRemoved += TagGroups_GroupRemoved;

            ApplyTag();
        }

        private void SetGroupDropdownItems()
        {
            _group.textBox.Items.Clear();

            _group.textBox.Items.Add(string.Empty);

            foreach (var group in TagGroups)
            {
                _group.textBox.Items.Add(group.Name);
            }

            _group.textBox.SelectedItem = Tag?.Group;
        }

        private void TagGroups_GroupRemoved(object sender, TagGroup e)
        {
            SetGroupDropdownItems();
        }

        private void TagGroups_GroupChanged(object sender, PropertyChangedEventArgs e)
        {
            SetGroupDropdownItems();
        }

        private void TagGroups_GroupAdded(object sender, TagGroup e)
        {
            SetGroupDropdownItems();
        }

        private void SetGroup(string txt)
        {
            if (_loading || Tag is null) return;
            Tag.Group = txt;
        }

        private void SetTextureRegion()
        {
            if (_loading || Tag is null) return;
            Tag.TextureRegion = new(_x.numberBox.Value, _y.numberBox.Value, _width.numberBox.Value, _height.numberBox.Value);
        }

        private void SetPriority()
        {
            if (_loading || Tag is null) return;
            Tag.Priority = _priority.numberBox.Value;
        }

        private void RemoveTag(TemplateTag tag)
        {
            TemplateTags.Remove(Tag);
        }

        public TemplateTag Tag { get => _tag; set => Common.SetProperty(ref _tag, value, OnTagChanged); }

        public TemplateTags TemplateTags { get; set; }

        public TagGroups TagGroups { get; }

        private void OnTagChanged(object sender, Core.Models.ValueChangedEventArgs<TemplateTag> e)
        {
            TemplateTag tag;

            tag = e.OldValue;
            if (tag is not null)
            {
                tag.PropertyChanged -= Tag_TagChanged;
            }

            tag = e.NewValue;

            if (tag is not null)
            {
                tag.PropertyChanged += Tag_TagChanged;
            }

            ApplyTag(tag);
        }

        private void Tag_TagChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is TemplateTag tag)
            {
                ApplyTag(tag);
            }
        }

        private void ApplyTag(TemplateTag? tag = null)
        {
            _loading = true;

            bool hasTag = tag is not null;
            var r = tag?.TextureRegion ?? tag?.Icon?.Bounds ?? Rectangle.Empty;
            _icon.SourceRectangle = r;

            _priority.numberBox.Value = tag?.Priority ?? 1;
            _priority.numberBox.Enabled = hasTag;

            _group.textBox.SelectedItem = tag?.Group;
            _group.textBox.Enabled = hasTag;

            _name.textBox.Text = tag?.Name;
            _name.textBox.Enabled = hasTag;

            _icon.Texture = tag?.Icon?.Texture;

            _iconId.numberBox.Value = tag?.AssetId ?? 0;
            _iconId.numberBox.Enabled = hasTag;

            _x.numberBox.Value = r.X;
            _x.numberBox.Enabled = hasTag;

            _y.numberBox.Value = r.Y;
            _y.numberBox.Enabled = hasTag;

            _width.numberBox.Value = r.Width;
            _width.numberBox.Enabled = hasTag;

            _height.numberBox.Value = r.Height;
            _height.numberBox.Enabled = hasTag;

            _loading = false;
        }

        private void SetTextureRegionToTextureBounds(AsyncTexture2D icon)
        {
            if (_loading) return;

            if (icon is not null)
            {
                _x.numberBox.Value = 0;
                _y.numberBox.Value = 0;
                _width.numberBox.Value = icon.Width;
                _height.numberBox.Value = icon.Height;
            }
        }

        private void SetIcon()
        {
            if (_loading || Tag is null) return;

            if (AsyncTexture2D.FromAssetId(_iconId.numberBox.Value) is AsyncTexture2D icon)
            {
                _icon.SourceRectangle = icon != _icon.Texture
                    ? new(0, 0, icon.Width, icon.Height)
                    : new(_x.numberBox.Value, _y.numberBox.Value, _width.numberBox.Value, _height.numberBox.Value);

                if (icon != _icon.Texture)
                {
                    SetTextureRegionToTextureBounds(icon);
                }

                _icon.Texture = icon;

                Tag.AssetId = _iconId.numberBox.Value;
                Tag.Icon.Texture = icon;
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

            _group.textBox.Width = x - (_iconId.numberBox.Width + _icon.Width + 5);
            _group.label.Location = new(_group.textBox.Left, _name.textBox.Bottom + 25);

            _priority.label.Location = new(_icon.Left, _group.label.Top);
            _priority.numberBox.Location = new(_icon.Left, _group.textBox.Top);
            _priority.numberBox.Width = _priority.label.Width = _iconId.numberBox.Width + _icon.Width + 3;

            int amount = 5;
            int padding = 5;
            int w = (x - (padding * amount)) / amount;
            _x.label.Location = new(0, _group.textBox.Bottom + 25);
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

            _resetButton.Location = new(_height.numberBox.Right + 5, _height.numberBox.Top);
            _resetButton.Width = w;

            _deleteButton.Location = new(_height.numberBox.Right + 5, _group.textBox.Top);
            _deleteButton.Width = w;
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle drawBounds, Rectangle scissor)
        {

            if (Tag is null)
            {
                Rectangle scissorRectangle = Rectangle.Intersect(scissor, AbsoluteBounds.WithPadding(_padding)).ScaleBy(Graphics.UIScaleMultiplier);
                spriteBatch.GraphicsDevice.ScissorRectangle = scissorRectangle;
                EffectBehind?.Draw(spriteBatch, drawBounds);
                spriteBatch.Begin(SpriteBatchParameters);

                var r = drawBounds;
                spriteBatch.FillRectangle(AbsoluteBounds, BackgroundColor ?? Color.Black * 0.5F);
                spriteBatch.DrawStringOnCtrl(this, strings.SelectTagToEdit, Content.DefaultFont18, r, Color.White, false, Blish_HUD.Controls.HorizontalAlignment.Center, Blish_HUD.Controls.VerticalAlignment.Middle);
                spriteBatch.End();
            }
            else
            {
                base.Draw(spriteBatch, drawBounds, scissor);
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

        }
    }
}
