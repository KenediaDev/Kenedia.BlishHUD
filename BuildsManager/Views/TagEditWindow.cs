using Blish_HUD.Content;
using Kenedia.Modules.BuildsManager.Controls;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Res;
using Kenedia.Modules.Core.Views;
using Microsoft.Extensions.Options;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class TagEditWindow : StandardWindow
    {
        private FilterBox _tagFilter;
        private FlowPanel _tagPanel;
        private List<TagEditControl> _tagEditControls = [];
        private readonly ButtonImage _editTags;

        private Dictionary<FlowPanel, List<TagEditControl>> _tagControls = [];
        private FlowPanel _ungroupedPanel;

        public TagEditWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, TemplateTags templateTags) : base(background, windowRegion, contentRegion)
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
                EnterPressedAction = (txt) =>
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
                },
                TextChangedAction = (txt) => _editTags.Enabled = !string.IsNullOrEmpty(txt.Trim()) && TemplateTags.FirstOrDefault(e => e.Name.ToLower() == txt.ToLower()) is null,
                PerformFiltering = (txt) =>
                {
                    string t = txt.ToLower();
                    bool any = string.IsNullOrEmpty(t);

                    foreach (var tag in _tagPanel.GetChildrenOfType<TagControl>())
                    {
                        tag.Visible = any || tag.Tag.Name.ToLower().Contains(t);
                    }

                    _tagPanel.Invalidate();
                }
            };

            _editTags = new()
            {
                Parent = this,
                Size = new(_tagFilter.Height),
                Location = new(_tagFilter.Right + 2, _tagFilter.Top),
                Texture = AsyncTexture2D.FromAssetId(255443),
                HoveredTexture = AsyncTexture2D.FromAssetId(255297),
                DisabledTexture = AsyncTexture2D.FromAssetId(255296),
                SetLocalizedTooltip = () => "Add Tag",
                Enabled = true,
                ClickAction = (b) => TemplateTags.Add(new TemplateTag() { Name = string.IsNullOrEmpty(_tagFilter.Text) ? "New Tag" : _tagFilter.Text })
            };

            _tagPanel = new()
            {
                Parent = this,
                Location = new(0, _tagFilter.Bottom + 2),
                Width = _editTags.Right,
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

            CreateTagControls();

            TemplateTags.TagAdded += TemplateTags_TagAdded;
            TemplateTags.TagAdded += TemplateTags_TagRemoved;
            TemplateTags = templateTags;
        }
        public TemplateTags TemplateTags { get; }

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
                        Title = "Not Grouped",
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

            _tagControls[panel].Add(new TagEditControl()
            {
                Parent = panel,
                Tag = e,
                Width = panel.Width - 25,
                TemplateTags = TemplateTags,
            });

            panel.SortChildren<TagEditControl>(SortTagControls);
        }

        protected override void OnResized(Blish_HUD.Controls.ResizedEventArgs e)
        {
            base.OnResized(e);

            _tagFilter?.SetSize(Width - 50);
            _editTags?.SetLocation(new(_tagFilter.Right + 2, _tagFilter.Top));
            _tagPanel?.SetLocation(new(0, _tagFilter.Bottom + 2));
            _tagPanel?.SetSize(_editTags.Right);

            ResizeTagControls();
        }

        public void ToggleWindow(TemplateTag tag)
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
    }
}
