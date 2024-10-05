﻿using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Controls;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Res;
using Kenedia.Modules.Core.Views;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class TagEditWindowX : TabbedWindow
    {
        private FilterBox _tagFilter;
        private FlowPanel _tagPanel;
        private List<TagEditControl> _tagEditControls = [];
        private readonly ButtonImage _addTag;

        private Dictionary<FlowPanel, List<TagEditControl>> _tagControls = [];
        private FlowPanel _ungroupedPanel;

        private FlowPanel _startPanel = null;
        private TagEditControl _draggingTagEditControl = null;

        public TagEditWindowX(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, TemplateTags templateTags) : base(background, windowRegion, contentRegion)
        {
            TemplateTags = templateTags;

            _tagFilter = new()
            {
                Parent = this,
                Location = new(0, 0),
                Width = 530,
                SetLocalizedPlaceholder = () => strings_common.Search,
                FilteringOnTextChange = true,
                FilteringOnEnter = true,
                EnterPressedAction = TagFilter_EnterPressed(),
                TextChangedAction = TagFilter_TextChanged(),
                PerformFiltering = TagFilter_PerformFilter()
            };

            _addTag = new()
            {
                Parent = this,
                Size = new(_tagFilter.Height),
                Location = new(_tagFilter.Right + 2, _tagFilter.Top),
                Texture = AsyncTexture2D.FromAssetId(255443),
                HoveredTexture = AsyncTexture2D.FromAssetId(255297),
                DisabledTexture = AsyncTexture2D.FromAssetId(255296),
                SetLocalizedTooltip = () => strings.AddTag,
                Enabled = true,
                ClickAction = AddTag_Click()
            };

            _tagPanel = new()
            {
                Parent = this,
                Location = new(0, _tagFilter.Bottom + 2),
                Width = _addTag.Right,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                ShowBorder = false,
                BorderColor = Color.Black,
                BorderWidth = new(2),
                BackgroundColor = Color.Black * 0.4F,
                ShowRightBorder = true,
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom,
                ContentPadding = new(5),
                ControlPadding = new(0, 5),
                CanScroll = true,
            };

            TemplateTags.TagAdded += TemplateTags_TagAdded;
            TemplateTags.TagRemoved += TemplateTags_TagRemoved;

            _tagPanel.ChildAdded += TagPanel_ChildsChanged;
            _tagPanel.ChildRemoved += TagPanel_ChildsChanged;

            CreateTagControls();
        }

        private Action<MouseEventArgs> AddTag_Click()
        {
            return (b) => TemplateTags.Add(new TemplateTag() { Name = string.IsNullOrEmpty(_tagFilter.Text) ? TemplateTag.DefaultName : _tagFilter.Text });
        }

        private Action<string> TagFilter_PerformFilter()
        {
            return (txt) =>
            {
                string t = txt.ToLower();
                bool any = string.IsNullOrEmpty(t);

                foreach (var tag in _tagPanel.GetChildrenOfType<TagControl>())
                {
                    tag.Visible = any || tag.Tag.Name.ToLower().Contains(t);
                }

                _tagPanel.Invalidate();
            };
        }

        private Action<string> TagFilter_TextChanged()
        {
            return (txt) => _addTag.Enabled = !string.IsNullOrEmpty(txt.Trim()) && TemplateTags.FirstOrDefault(e => e.Name.ToLower() == txt.ToLower()) is null;
        }

        private Action<string> TagFilter_EnterPressed()
        {
            return (txt) =>
            {
                if (!string.IsNullOrEmpty(txt.Trim()))
                {
                    var templateTag = TemplateTags.FirstOrDefault(e => e.Name.ToLower() == txt.ToLower());

                    if (templateTag is null)
                    {
                        TemplateTags.Add(new TemplateTag() { Name = txt });
                    }
                    else
                    {

                    }
                }
            };
        }

        public TemplateTags TemplateTags { get; }

        public FlowPanel GetPanel(string title)
        {
            FlowPanel panel = null;

            if (!string.IsNullOrEmpty(title))
            {
                if (_tagControls.Keys.FirstOrDefault(x => x.Title == title) is FlowPanel p)
                {
                    panel = p;
                }

                panel ??= new FlowPanel()
                {
                    Title = title,
                    Parent = _tagPanel,
                    Width = _tagPanel.Width - 25,
                    WidthSizingMode = Blish_HUD.Controls.SizingMode.Standard,
                    HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
                    AutoSizePadding = new(0, 2),
                    OuterControlPadding = new(25, 0),
                    CanCollapse = true,
                };
            }

            panel ??= _ungroupedPanel ??= new FlowPanel()
            {
                Title = TagGroup.DefaultName,
                Parent = _tagPanel,
                Width = _tagPanel.Width - 25,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Standard,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
                AutoSizePadding = new(0, 2),
                OuterControlPadding = new(25, 0),
                CanCollapse = true,
            };

            if (!_tagControls.ContainsKey(panel))
            {
                _tagControls.Add(panel, []);
                _tagPanel.SortChildren<FlowPanel>((x, y) => x.Title == TagGroup.DefaultName ? -1 : x.Title.CompareTo(y.Title));
            }

            return panel;
        }

        private void TagPanel_ChildsChanged(object sender, Blish_HUD.Controls.ChildChangedEventArgs e)
        {
            if (sender is FlowPanel p)
            {
                p.SortChildren<FlowPanel>((x, y) => x.Title == TagGroup.DefaultName ? -1 : x.Title.CompareTo(y.Title));
            }
        }

        private void TemplateTags_TagRemoved(object sender, TemplateTag e)
        {
            RemoveTemplateTag(e);
        }

        private void TemplateTags_TagAdded(object sender, TemplateTag e)
        {
            AddTemplateTag(e);
        }

        private int SortTagControls(TagEditControl x, TagEditControl y)
        {
            return x.Tag.Priority.CompareTo(y.Tag.Priority) == 0 ? x.Tag.Name.CompareTo(y.Tag.Name) : x.Tag.Priority.CompareTo(y.Tag.Priority);
        }

        private void RemoveTemplateTag(TemplateTag e)
        {
            TagEditControl tagControl = null;
            var p = _tagControls.FirstOrDefault(x => x.Value.Any(x => x.Tag == e));

            var panel = p.Key;
            tagControl = p.Value.FirstOrDefault(x => x.Tag == e);

            tagControl?.Dispose();
            _tagControls[panel].Remove(tagControl);

            if (!_tagControls[panel].Any())
            {
                _tagControls.Remove(panel);
                panel.Dispose();

                _tagPanel.SortChildren<FlowPanel>((x, y) => x.Title.CompareTo(y.Title));
            }
            else
            {
                panel.SortChildren<TagEditControl>(SortTagControls);
            }
        }

        private void AddTemplateTag(TemplateTag e)
        {
            var panel = _tagControls.Keys.FirstOrDefault(x => x.Title == e.Group);

            panel ??= !string.IsNullOrEmpty(e.Group)
                    ? new FlowPanel()
                    {
                        Title = e.Group,
                        Parent = _tagPanel,
                        Width = _tagPanel.Width - 25,
                        WidthSizingMode = Blish_HUD.Controls.SizingMode.Standard,
                        HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
                        AutoSizePadding = new(0, 2),
                        OuterControlPadding = new(25, 0),
                        CanCollapse = true,
                    }
                    : (_ungroupedPanel ??= new FlowPanel()
                    {
                        Title = TagGroup.DefaultName,
                        Parent = _tagPanel,
                        Width = _tagPanel.Width - 25,
                        WidthSizingMode = Blish_HUD.Controls.SizingMode.Standard,
                        HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
                        AutoSizePadding = new(0, 2),
                        OuterControlPadding = new(25, 0),
                        CanCollapse = true,
                    });

            if (!_tagControls.ContainsKey(panel))
            {
                _tagControls.Add(panel, []);
                _tagPanel.SortChildren<FlowPanel>((x, y) => x.Title.CompareTo(y.Title));
            }
            TagEditControl t;
            _tagControls[panel].Add(t = new TagEditControl()
            {
                Parent = panel,
                Tag = e,
                Width = panel.Width - 25,
                TemplateTags = TemplateTags,
            });

            panel.SortChildren<TagEditControl>(SortTagControls);
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);

            if (GameService.Input.Mouse.State.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                //_draggingTagEditControl.Location = GameService.Input.Mouse.Position;
            }
        }

        protected override void OnResized(Blish_HUD.Controls.ResizedEventArgs e)
        {
            base.OnResized(e);

            _tagFilter?.SetSize(Width - 50);
            _addTag?.SetLocation(new(_tagFilter.Right + 2, _tagFilter.Top));
            _tagPanel?.SetLocation(new(0, _tagFilter.Bottom + 2));
            _tagPanel?.SetSize(_addTag.Right);

            ResizeTagControls();
        }

        public void Show(TemplateTag tag)
        {
            Show();

            var tagControls = new List<TagEditControl>(_tagPanel.GetChildrenOfType<TagEditControl>());
            tagControls.AddRange(_tagPanel.GetChildrenOfType<FlowPanel>().SelectMany(x => x.GetChildrenOfType<TagEditControl>()));

            foreach (var t in tagControls)
            {
                t.Collapsed = t.Tag != tag;
            }
        }

        private void ResizeTagControls()
        {
            if (_tagPanel is null) return;

            var tagControls = new List<TagEditControl>(_tagPanel.GetChildrenOfType<TagEditControl>());
            tagControls.AddRange(_tagPanel.GetChildrenOfType<FlowPanel>().SelectMany(x => x.GetChildrenOfType<TagEditControl>()));

            foreach (var tag in tagControls)
            {
                if (tag.Parent is FlowPanel fp)
                {
                    fp.Width = _tagPanel.Width - 25;
                    tag.Width = fp.Width - 25;
                }
            }
        }

        private void CreateTagControls()
        {
            foreach (var tag in TemplateTags)
            {
                AddTemplateTag(tag);
            }
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            //SetNewGroupFromDragging();
        }

        private void SetNewGroupFromDragging()
        {
            if (_draggingTagEditControl is not null)
            {
                if (Input.Mouse.State.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
                {

                    FlowPanel p = null;
                    foreach (var s in _tagControls.Keys)
                    {
                        if (s.AbsoluteBounds.Contains(_draggingTagEditControl.Location)) ;
                        {
                            p = s;
                            break;
                        }
                    }

                    if (p is not null)
                    {
                        _draggingTagEditControl.Tag.Group = p.Title;
                    }
                    else if (_startPanel is not null)
                    {
                        _draggingTagEditControl.Tag.Group = _startPanel.Title != TagGroup.DefaultName ? _startPanel.Title : string.Empty;
                    }

                    _draggingTagEditControl = null;
                    _startPanel = null;
                }
            }
        }
    }
}