using Blish_HUD.Content;
using Blish_HUD.Graphics.UI;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using View = Blish_HUD.Graphics.UI.View;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class TagGroupView : View
    {
        public class GroupEditControl : Panel
        {
            private readonly Image _icon;
            private readonly (Label label, TextBox textBox) _name;
            private readonly (Label label, NumberBox numberBox) _iconId;
            private readonly (Label label, NumberBox numberBox) _priority;
            private readonly (Label label, NumberBox numberBox) _x;
            private readonly (Label label, NumberBox numberBox) _y;
            private readonly (Label label, NumberBox numberBox) _width;
            private readonly (Label label, NumberBox numberBox) _height;
            private readonly Button _resetButton;
            private readonly bool _created;

            public TagGroup Group { get; }

            public GroupEditControl(TagGroup tagGroup, Blish_HUD.Controls.Container parent)
            {
                Parent = parent;
                parent.Resized += Parent_Resized;
                Group = tagGroup;

                Height = 200;
                Width = Parent.Width - 25;

                CanCollapse = true;
                Collapsed = true;
                ContentPadding = new(5);

                //WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill;

                //HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;

                BorderColor = Color.Black;
                BorderWidth = new Core.Structs.RectangleDimensions(2);

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
                                Title = txt;
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
                        Value = Group.AssetId,
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
                        Location = new(0, _icon.Bottom + 5 + Content.DefaultFont14.LineHeight + 2),
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

                SetGroup();
            }

            private void SetGroup()
            {
                if (AsyncTexture2D.FromAssetId(Group.AssetId) is AsyncTexture2D icon)
                {
                    _name.textBox.Text = Group.Name;
                    _iconId.numberBox.Value = Group.AssetId;
                    _priority.numberBox.Value = Group.Priority;
                    _x.numberBox.Value = Group.Icon.TextureRegion.X;
                    _y.numberBox.Value = Group.Icon.TextureRegion.Y;
                    _width.numberBox.Value = Group.Icon.TextureRegion.Width;
                    _height.numberBox.Value = Group.Icon.TextureRegion.Height;

                    _icon.Texture = icon;
                    Title = Group.Name;
                    TitleIcon = icon;
                }
            }

            private void Parent_Resized(object sender, Blish_HUD.Controls.ResizedEventArgs e)
            {
                Width = (Parent?.Width ?? 0) - 25;
            }

            private void SetPriority()
            {
                Group.Priority = _priority.numberBox.Value;
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
                    TitleIcon = icon;

                    Group.AssetId = _iconId.numberBox.Value;
                    Group.Icon.Texture = icon;
                    TitleTextureRegion = Group.TextureRegion = _icon.SourceRectangle;
                }
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

        }

        public TagGroupView(TagGroups tagGroups)
        {
            TagGroups = tagGroups;
        }

        public TagGroups TagGroups { get; }

        override protected void Build(Blish_HUD.Controls.Container buildPanel)
        {
            base.Build(buildPanel);

            var flowPanel = new FlowPanel()
            {
                Location = new(50, 0),
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                CanScroll = true,
                Parent = buildPanel,
                ControlPadding = new(0, 5),
            };

            Debug.WriteLine($"BUILD");
            foreach (var g in TagGroups)
            {

                Debug.WriteLine($"Create Group: {g?.Name}");
                _ = new GroupEditControl(g, flowPanel);
            }
        }
    }
}
