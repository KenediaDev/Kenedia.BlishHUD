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

namespace Kenedia.Modules.BuildsManager.Views
{
    public class GroupEditPanel : Panel
    {
        private readonly (Label label, TextBox textBox) _name;
        private readonly (Label label, NumberBox numberBox) _iconId;
        private readonly (Label label, NumberBox numberBox) _priority;

        private readonly (Label label, NumberBox numberBox) _x;
        private readonly bool _created;
        private readonly (Label label, NumberBox numberBox) _y;
        private readonly (Label label, NumberBox numberBox) _width;
        private readonly (Label label, NumberBox numberBox) _height;
        private readonly Button _resetButton;

        private readonly Image _icon;

        private Blish_HUD.Controls.Container? _draggingStartParent;
        private Point _draggingStart;
        private bool _dragging;

        private bool _loading;

        public GroupEditPanel(TagGroups tagGroups)
        {
            ContentPadding = new(5);
            TagGroups = tagGroups;

            _name = new(
                new()
                {
                    Parent = this,
                    SetLocalizedText = () => strings.GroupName,
                },
                new()
                {
                    Parent = this,
                    Width = 200,
                    Height = 32,
                    SetLocalizedPlaceholder = () => strings.GroupName,
                    Location = new(0, Content.DefaultFont14.LineHeight + 2),
                    TextChangedAction = (txt) =>
                    {
                        if (!string.IsNullOrEmpty(txt) && Group is not null)
                        {
                            Group.Name = txt;
                        };
                    },
                });

            _icon = new()
            {
                Parent = this,
                Size = new(32),
                Location = new(0, Content.DefaultFont14.LineHeight + 2),
                BackgroundColor = Color.Black * 0.4F,
                Visible = false,
            };

            _iconId = new(
                new()
                {
                    Parent = this,
                    SetLocalizedText = () => strings.AssetId,
                    Visible = false,
                },
                new()
                {
                    Parent = this,
                    Width = 100,
                    ShowButtons = false,
                    Location = new(0, Content.DefaultFont14.LineHeight + 2),
                    Height = 32,
                    ValueChangedAction = (n) => SetIcon(),
                    Visible = false,
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
                    Location = new(0, _icon.Bottom + 25 + Content.DefaultFont14.LineHeight + 2),
                    Height = 32,
                    ValueChangedAction = (n) => SetPriority(),
                });

            _x = new(
                new()
                {
                    Parent = this,
                    SetLocalizedText = () => "X",
                    HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Center,
                    Visible = false,
                },
                new()
                {
                    Parent = this,
                    ShowButtons = false,
                    Height = 25,
                    ValueChangedAction = (n) => SetTextureRegion(),
                    Visible = false,
                });

            _y = new(
                new()
                {
                    Parent = this,
                    SetLocalizedText = () => "Y",
                    HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Center,
                    Visible = false,
                },
                new()
                {
                    Parent = this,
                    ShowButtons = false,
                    Height = 25,
                    ValueChangedAction = (n) => SetTextureRegion(),
                    Visible = false,
                });

            _width = new(
                new()
                {
                    Parent = this,
                    SetLocalizedText = () => strings.Width,
                    HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Center,
                    Visible = false,
                },
                new()
                {
                    Parent = this,
                    ShowButtons = false,
                    Height = 25,
                    ValueChangedAction = (n) => SetTextureRegion(),
                    Visible = false,
                });

            _height = new(
                new()
                {
                    Parent = this,
                    SetLocalizedText = () => strings.Height,
                    HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Center,
                    Visible = false,
                },
                new()
                {
                    Parent = this,
                    ShowButtons = false,
                    Height = 25,
                    ValueChangedAction = (n) => SetTextureRegion(),
                    Visible = false,
                });

            _resetButton = new()
            {
                Parent = this,
                Text = strings.Reset,
                Height = 25,
                ClickAction = () => SetTextureRegionToTextureBounds(Group?.Icon?.Texture),
                Visible = false,
            };

            _created = true;

            Menu = new();
            _ = Menu.AddMenuItem(new ContextMenuItem(() => strings.Delete, () => RemoveTag(Group)));

            ApplyGroup();
        }

        private void SetTextureRegion()
        {
            if (_loading || Group is null) return;
            Group.TextureRegion = new(_x.numberBox.Value, _y.numberBox.Value, _width.numberBox.Value, _height.numberBox.Value);
        }

        private void RemoveTag(TagGroup group)
        {
            TagGroups.Remove(group);
        }

        public TagGroup Group { get; set => Common.SetProperty(ref field, value, OnTagChanged); }

        public TemplateTags TemplateTags { get; set; }

        public TagGroups TagGroups { get; }

        private void OnTagChanged(object sender, Core.Models.ValueChangedEventArgs<TagGroup> e)
        {
            TagGroup group;

            group = e.OldValue;
            if (group is not null)
            {
                group.PropertyChanged -= Group_TagChanged;
            }

            group = e.NewValue;
            if (group is not null)
            {
                group.PropertyChanged += Group_TagChanged;
            }

            ApplyGroup(group);
        }

        private void Group_TagChanged(object sender, PropertyAndValueChangedEventArgs e)
        {
            if (sender is TagGroup group)
            {
                ApplyGroup(group);
            }
        }

        private void ApplyGroup(TagGroup? group = null)
        {
            _loading = true;

            bool hasGroup = group is not null;
            var r = group?.TextureRegion ?? group?.Icon?.Bounds ?? Rectangle.Empty;
            _icon.SourceRectangle = r;

            _priority.numberBox.Value = group?.Priority ?? 1;
            _priority.numberBox.Enabled = hasGroup;

            _name.textBox.Text = group?.Name;
            _name.textBox.Enabled = hasGroup;

            _icon.Texture = group?.Icon?.Texture;
            _iconId.numberBox.Value = group?.AssetId ?? 0;
            _iconId.numberBox.Enabled = hasGroup;

            _x.numberBox.Value = r.X;
            _x.numberBox.Enabled = hasGroup;

            _y.numberBox.Value = r.Y;
            _y.numberBox.Enabled = hasGroup;

            _width.numberBox.Value = r.Width;
            _width.numberBox.Enabled = hasGroup;

            _height.numberBox.Value = r.Height;
            _height.numberBox.Enabled = hasGroup;

            _loading = false;
        }

        private void SetTextureRegionToTextureBounds(AsyncTexture2D icon)
        {
            if (icon is not null)
            {
                _x.numberBox.Value = 0;
                _y.numberBox.Value = 0;
                _width.numberBox.Value = icon.Width;
                _height.numberBox.Value = icon.Height;
            }
        }

        private void SetPriority()
        {
            if (_loading || Group is null) return;

            Group.Priority = _priority.numberBox.Value;
        }

        private void SetIcon()
        {
            if (_loading || Group is null) return;

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

                Group.AssetId = _iconId.numberBox.Value;
                Group.Icon.Texture = icon;
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (!_created) return;

            int x = ContentRegion.Width;
            int y = ContentRegion.Height;

            _name.textBox.Width = x;
            _name.label.Location = new(_name.textBox.Left, 0);
            _name.label.Width = _name.textBox.Width;

            _icon.Location = new(_name.textBox.Right + 5, _name.textBox.Top);

            _iconId.numberBox.Location = new(_icon.Right + 5, _name.textBox.Top);
            _iconId.label.Location = new(_iconId.numberBox.Left, 0);
            _iconId.label.Width = _iconId.numberBox.Width;

            _priority.label.Location = new(_name.textBox.Left, _name.textBox.Bottom + 25);
            _priority.numberBox.Location = new(_name.textBox.Left, _priority.label.Bottom + 3);
            _priority.numberBox.Width = _priority.label.Width = _name.textBox.Width;

            int amount = 5;
            int padding = 5;
            int w = (x - (padding * amount)) / amount;
            _x.label.Location = new(0, _priority.numberBox.Bottom + 25);
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
        }
        
        public override void Draw(SpriteBatch spriteBatch, Rectangle drawBounds, Rectangle scissor)
        {
            if (Group is null)
            {
                Rectangle scissorRectangle = Rectangle.Intersect(scissor, AbsoluteBounds.WithPadding(_padding)).ScaleBy(Graphics.UIScaleMultiplier);
                spriteBatch.GraphicsDevice.ScissorRectangle = scissorRectangle;
                EffectBehind?.Draw(spriteBatch, drawBounds);
                spriteBatch.Begin(SpriteBatchParameters);

                var r = drawBounds;
                spriteBatch.FillRectangle(AbsoluteBounds, BackgroundColor ?? Color.Black * 0.5F);
                spriteBatch.DrawStringOnCtrl(this, strings.SelectGroupToEdit, Content.DefaultFont18, r, Color.White, false, Blish_HUD.Controls.HorizontalAlignment.Center, Blish_HUD.Controls.VerticalAlignment.Middle);
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
