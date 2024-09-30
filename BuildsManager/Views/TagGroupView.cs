using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Controls;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Res;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Blish_HUD.ContentService;
using View = Blish_HUD.Graphics.UI.View;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class GroupSelectable : Selectable<TagGroup>
    {
        public TagGroup Group => Item;

        public TagGroups TagGroups { get; }

        public GroupSelectable(TagGroup tagGroup, Blish_HUD.Controls.Container parent, TagGroups tagGroups) : base(tagGroup, parent)
        {
            TagGroups = tagGroups;

            Menu = new();
            _ = Menu.AddMenuItem(new ContextMenuItem(() => strings.Delete, () => RemoveTag(Group)));
        }

        protected override void DrawItem(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Group.Icon.Texture is not null)
            {
                spriteBatch.DrawOnCtrl(this, Group.Icon.Texture, IconBounds);
            }

            spriteBatch.DrawStringOnCtrl(this, Group.Name, Content.DefaultFont14, TextBounds, Color.White, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Middle);
        }

        private void RemoveTag(TagGroup group)
        {
            TagGroups.Remove(group);
        }
    }

    public class GroupEditPanel : Panel
    {
        private TagGroup _group;
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

        public GroupEditPanel(TagGroups tagGroups)
        {
            ContentPadding = new(5);
            TagGroups = tagGroups;

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
                        if (!string.IsNullOrEmpty(txt))
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
                    SetLocalizedText = () => strings.Width,
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
                    SetLocalizedText = () => strings.Height,
                    HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Center,
                },
                new()
                {
                    Parent = this,
                    ShowButtons = false,
                    Height = 25,
                    ValueChangedAction = (n) => SetIcon(),
                });

            _resetButton = new()
            {
                Parent = this,
                Text = strings.Reset,
                Height = 25,
                ClickAction = () => SetTextureRegionToTextureBounds(Group.Icon.Texture),
            };

            _created = true;

            Menu = new();
            _ = Menu.AddMenuItem(new ContextMenuItem(() => strings.Delete, () => RemoveTag(Group)));
        }

        private void SetPriority()
        {
            Group.Priority = _priority.numberBox.Value;
        }

        private void RemoveTag(TagGroup group)
        {
            TagGroups.Remove(group);
        }

        public TagGroup Group { get => _group; set => Common.SetProperty(ref _group, value, OnTagChanged); }

        public TemplateTags TemplateTags { get; set; }

        public TagGroups TagGroups { get; }

        private void OnTagChanged(object sender, Core.Models.ValueChangedEventArgs<TagGroup> e)
        {
            TagGroup group = e.NewValue;

            if (group is not null)
            {
                group.PropertyChanged += Tag_TagChanged;
                ApplyTag(group);
            }
        }

        private void Tag_TagChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is TagGroup group)
            {
                ApplyTag(group);
            }
        }

        private void ApplyTag(TagGroup group)
        {
            var r = group?.TextureRegion ?? group?.Icon?.Bounds ?? Rectangle.Empty;
            _icon.SourceRectangle = r;

            _priority.numberBox.Value = group?.Priority ?? 1;
            _name.textBox.Text = group?.Name;
            _icon.Texture = group?.Icon?.Texture;
            _iconId.numberBox.Value = group?.AssetId ?? 0;

            _x.numberBox.Value = r.X;
            _y.numberBox.Value = r.Y;
            _width.numberBox.Value = r.Width;
            _height.numberBox.Value = r.Height;
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

        private void SetIcon()
        {
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

            _name.textBox.Width = x - (_iconId.numberBox.Width + _icon.Width + 5);
            _name.label.Location = new(_name.textBox.Left, 0);
            _name.label.Width = _name.textBox.Width;

            _icon.Location = new(_name.textBox.Right + 5, _name.textBox.Top);

            _iconId.numberBox.Location = new(_icon.Right + 5, _name.textBox.Top);
            _iconId.label.Location = new(_iconId.numberBox.Left, 0);
            _iconId.label.Width = _iconId.numberBox.Width;

            _priority.label.Location = new(_name.textBox.Left, _name.textBox.Bottom + 25);
            _priority.numberBox.Location = new(_name.textBox.Left, _priority.label.Bottom + 3);
            _priority.numberBox.Width = _priority.label.Width = _iconId.numberBox.Width + _icon.Width + 3;

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
    }

    public class TagGroupView : View
    {
        private FlowPanel _groupsPanel;
        private GroupEditPanel _editPanel;
        private FilterBox _filterBox;
        private ImageButton _addButton;
        private List<GroupSelectable> _groupSelectables = [];

        public TagGroupView(TagGroups tagGroups)
        {
            TagGroups = tagGroups;
        }

        public GroupSelectable SelectedGroup { get; set; }

        public TagGroups TagGroups { get; }

        override protected void Build(Blish_HUD.Controls.Container buildPanel)
        {
            base.Build(buildPanel);

            _filterBox = new()
            {
                Parent = buildPanel,
                Location = new(50, 0),
                Width = buildPanel.Width - 75 - 27,
                SetLocalizedPlaceholder = () => strings_common.Search,
                FilteringOnTextChange = true,
                PerformFiltering = FilterTags,
            };

            _addButton = new()
            {
                Parent = buildPanel,
                Location = new(_filterBox.Right + 2, _filterBox.Top),
                Texture = AsyncTexture2D.FromAssetId(155902),
                DisabledTexture = AsyncTexture2D.FromAssetId(155903),
                HoveredTexture = AsyncTexture2D.FromAssetId(155904),
                TextureRectangle = new(2, 2, 28, 28),
                Size = new Point(_filterBox.Height),
                ClickAction = (m) => TagGroups.Add(new(_filterBox.Text)),
                SetLocalizedTooltip = () => "Add Group",
            };

            _groupsPanel = new()
            {
                Parent = buildPanel,
                Location = new(50, _filterBox.Bottom + 5),
                ContentPadding = new(5, 5, 0, 0),
                Width = (buildPanel.Width - 75) / 3,
                BorderColor = Color.Black,
                BorderWidth = new Core.Structs.RectangleDimensions(2),
                BackgroundColor = Color.Black * 0.5F,
                CanScroll = true,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                ControlPadding = new(0, 2)
            };

            _editPanel = new GroupEditPanel(TagGroups)
            {
                Location = new(_groupsPanel.Right + 5, _filterBox.Bottom + 5),
                CanScroll = true,
                Parent = buildPanel,
                BorderColor = Color.Black,
                BorderWidth = new Core.Structs.RectangleDimensions(2),
                BackgroundColor = Color.Black * 0.5F,
                Width = buildPanel.Width - 75 - (_groupsPanel.Width + 5),
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
            };

            foreach (var g in TagGroups)
            {
                AddGroup(g);
            }

            TagGroups.TagAdded += TagGroups_TagAdded;
            TagGroups.TagRemoved += TagGroups_TagRemoved;
            TagGroups.TagChanged += TagGroups_TagChanged;

            buildPanel.Resized += BuildPanel_Resized;
        }

        private void BuildPanel_Resized(object sender, Blish_HUD.Controls.ResizedEventArgs e)
        {
            var b = e.CurrentSize;

            if (_filterBox is not null)
                _filterBox.Width = b.X - 75 - 27;

            if (_editPanel is not null)
                _editPanel.Width = b.X - 75 - (_groupsPanel.Width + 5);

            if (_addButton is not null)
                _addButton.Location = new(_filterBox.Right + 2, _filterBox.Top);
        }

        private void FilterTags(string obj)
        {
            _groupsPanel.SuspendLayout();

            obj = obj.ToLowerInvariant();

            foreach (var g in _groupSelectables)
            {
                g.Visible = g.Group.Name.ToLowerInvariant().Contains(obj);
            }

            _groupsPanel.ResumeLayout();
            _groupsPanel.Invalidate();
        }

        private void AddGroup(TagGroup g)
        {
            _groupSelectables.Add(new(g, _groupsPanel, TagGroups)
            {
                OnClickAction = SetGroupToEdit,                
            });
        }

        private void SetGroupToEdit(TagGroup group)
        {
            SelectedGroup = _groupSelectables.FirstOrDefault(x => x.Group == group);
            _editPanel.Group = group;

            foreach (var g in _groupSelectables)
            {
                g.Selected = g == SelectedGroup;
            }
        }

        private void TagGroups_TagChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }

        private void TagGroups_TagRemoved(object sender, TagGroup e)
        {
            var group = _groupSelectables.FirstOrDefault(x => x.Group == e);

            if (group is not null)
            {
                _groupSelectables.Remove(group);
                group.Dispose();
            }
        }

        private void TagGroups_TagAdded(object sender, TagGroup e)
        {
            AddGroup(e);
        }
    }
}
