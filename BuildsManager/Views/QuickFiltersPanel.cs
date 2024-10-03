using Blish_HUD.Content;
using Microsoft.Xna.Framework;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Controls;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using Kenedia.Modules.BuildsManager.Utility;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class QuickFiltersPanel : AnchoredContainer
    {
        private readonly FlowPanel _tagPanel;
        private Dictionary<TagGroupPanel, List<TagToggle>> _tagControls = [];
        private TagGroupPanel _ungroupedPanel;

        public QuickFiltersPanel(TemplateTags templateTags, TagGroups tagGroups)
        {
            TemplateTags = templateTags;
            TagGroups = tagGroups;

            Parent = Graphics.SpriteScreen;
            Width = 250;
            Height = 600;

            //BackgroundColor = Color.Black * 0.4F;
            BackgroundImage = AsyncTexture2D.FromAssetId(155985);

            if (BackgroundImage is not null)
                TextureRectangle = new Rectangle(430, 30, 250, 600);

            BorderColor = Color.Black;
            BorderWidth = new(2);
            Visible = false;

            TemplateTags.TagAdded += TemplateTags_TagAdded;
            TemplateTags.TagRemoved += TemplateTags_TagRemoved;
            TemplateTags.TagChanged += TemplateTags_TagChanged;

            _tagPanel = new()
            {
                Parent = this,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom,
                ContentPadding = new(5),
                CanScroll = true,
            };

            CreateTagControls();
        }

        private void TemplateTags_TagRemoved(object sender, TemplateTag e)
        {
            RemoveTemplateTag(e);
        }

        private void TemplateTags_TagAdded(object sender, TemplateTag e)
        {
            AddTemplateTag(e);
        }

        private void TemplateTags_TagChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is TemplateTag tag)
            {
                switch (e.PropertyName)
                {
                    case nameof(TemplateTag.Priority):
                    case nameof(TemplateTag.Name):
                        {
                            var p = GetPanel(tag.Group);
                            p.SortChildren<TagToggle>(SortTagControls);
                            break;
                        }

                    case nameof(TemplateTag.Group):
                        List<TagGroupPanel> flowPanelsToDelete = [];

                        foreach (var t in _tagControls)
                        {
                            if (t.Value.FirstOrDefault(x => x.Tag == tag) is var control && control is not null)
                            {
                                var p = GetPanel(tag.Group);
                                control.Parent = p;
                                p.Children.Add(control);
                                p.SortChildren<TagToggle>(SortTagControls);
                                _tagControls[p].Add(control);

                                var panel = t.Key;
                                _tagControls[panel].Remove(control);
                                panel.Children.Remove(control);

                                if (panel.Children.Where(x => x != control).Count() <= 0)
                                {
                                    flowPanelsToDelete.Add(panel);
                                    panel.Dispose();
                                }

                                break;
                            }
                        }

                        if (flowPanelsToDelete.Count > 0)
                        {
                            foreach (var t in flowPanelsToDelete)
                            {
                                _tagControls.Remove(t);
                            }
                        }
                        break;
                }
            }
        }

        public TemplateTags TemplateTags { get; }

        public TagGroups TagGroups { get; }

        public TagGroupPanel GetPanel(string title)
        {
            TagGroupPanel panel = null;

            if (!string.IsNullOrEmpty(title))
            {
                if (_tagControls.Keys.FirstOrDefault(x => x.TagGroup.Name == title) is TagGroupPanel p)
                {
                    panel = p;
                }

                panel ??= new TagGroupPanel(TagGroups.First(x => x.Name == title))
                {
                    //Title = title,
                    Parent = _tagPanel,
                    //Width = _tagPanel.Width - 25,
                    WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                    HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
                    AutoSizePadding = new(0, 2),
                    //OuterControlPadding = new(25, 0),
                    //CanCollapse = true,
                };
            }

            panel ??= _ungroupedPanel ??= new TagGroupPanel(TagGroup.Empty)
            {
                //Title = TagGroup.DefaultName,
                Parent = _tagPanel,
                //Width = _tagPanel.Width - 25,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
                AutoSizePadding = new(0, 2),
                //OuterControlPadding = new(25, 0),
                //CanCollapse = true,
            };

            if (!_tagControls.ContainsKey(panel))
            {
                _tagControls.Add(panel, []);
                SortPanels();
            }

            return panel;
        }

        private void AddTemplateTag(TemplateTag e)
        {
            var panel = GetPanel(e.Group);

            _tagControls[panel].Add(new TagToggle()
            {
                Parent = panel,
                Tag = e,
                OnClicked = (selected) =>
                {

                }
            });

            SortPanels();
            var comparer = new TemplateTagComparer(TagGroups);

            panel.SortChildren<TagToggle>((x, y) =>
            {
                var a = x.Tag;
                var b = y.Tag;

                return comparer.Compare(a, b);
            });
        }

        private void SortPanels()
        {
            _tagPanel.SortChildren<TagGroupPanel>((x, y) =>
            {
                var a = TagGroups.FirstOrDefault(group => group.Name == x.Title);
                var b = TagGroups.FirstOrDefault(group => group.Name == y.Title);

                return TemplateTagComparer.CompareGroups(a, b);
            });
        }

        private int SortTagControls(TagToggle x, TagToggle y)
        {
            var comparer = new TemplateTagComparer(TagGroups);
            return comparer.Compare(x.Tag, y.Tag);
        }
        private void RemoveTemplateTag(TemplateTag e)
        {

            TagToggle tagControl = null;
            var p = _tagControls.FirstOrDefault(x => x.Value.Any(x => x.Tag == e));

            var panel = p.Key;
            tagControl = p.Value.FirstOrDefault(x => x.Tag == e);

            tagControl?.Dispose();
            _tagControls[panel].Remove(tagControl);

            if (panel.Children.Any())
            {
                panel.SortChildren<TagToggle>(SortTagControls);
            }

            RemoveEmptyPanels();
        }

        private void RemoveEmptyPanels(TagGroupPanel? fp = null)
        {
            var panels = _tagControls.Keys.ToList();

            foreach (var p in panels)
            {
                if (p == fp) continue;

                if (!p.Children.Any())
                {
                    _tagControls.Remove(p);
                    p.Dispose();
                }
            }
        }

        private void CreateTagControls()
        {
            var tags = TemplateTags.ToList();
            tags.Sort(new TemplateTagComparer(TagGroups));
            List<string> added = [];

            foreach (var tag in tags)
            {
                var panel = GetPanel(tag.Group);

                AddTemplateTag(tag);

                added.Add(tag.Name);
            }

            SortPanels();
        }
    }
}
