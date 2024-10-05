using Blish_HUD;
using Blish_HUD.Content;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.BuildsManager.Views;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Res;
using Kenedia.Modules.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.Controls.Tabs
{
    public class AboutTab : Blish_HUD.Controls.Container
    {
        private readonly TextBox _modifiedField;
        private readonly Blish_HUD.Controls.MultilineTextBox _noteField;
        private readonly FlowPanel _tagPanel;
        private readonly Label _modifiedLabel;
        private readonly Label _notesLabel;
        private readonly Label _tagsLabel;
        private readonly ButtonImage _editTags;
        private readonly FilterBox _tagFilter;

        private readonly List<(TemplateFlag tag, Image texture, Checkbox checkbox)> _tags = [];
        private readonly List<(EncounterFlag tag, Image texture, Checkbox checkbox)> _encounters = [];
        private readonly TemplateTagComparer _comparer;
        private readonly bool _created = false;
        private int tagSectionWidth;
        private bool _changeBuild = true;

        private Color _disabledColor = Color.Gray;

        private Dictionary<FlowPanel, List<TagControl>> _tagControls = [];
        private FlowPanel _ungroupedPanel;

        public AboutTab(TemplatePresenter templatePresenter, TemplateTags templateTags, TagGroups tagGroups)
        {
            TemplatePresenter = templatePresenter;
            TemplateTags = templateTags;
            TagGroups = tagGroups;

            _comparer = new TemplateTagComparer(TagGroups);

            HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill;
            WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill;
            tagSectionWidth = 300;

            _tagsLabel = new()
            {
                Parent = this,
                SetLocalizedText = () => strings.Tags,
                Font = Content.DefaultFont32,
                Height = 35,
                Width = tagSectionWidth - 35 - 5,
                Location = new(0, 10),
            };

            _tagFilter = new()
            {
                Parent = this,
                Location = new(0, _tagsLabel.Bottom + 10),
                Width = tagSectionWidth - 30,
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
                            var tag = _tagPanel.GetChildrenOfType<TagControl>().FirstOrDefault(e => e.Tag == templateTag);
                            tag?.SetSelected(!tag.Selected);
                            _tagFilter.Focused = true;
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
                SetLocalizedTooltip = () => strings.AddTag,
                Enabled = false,
                ClickAction = (b) => TemplateTags.Add(new TemplateTag() { Name = string.IsNullOrEmpty(_tagFilter.Text) ? TemplateTag.DefaultName : _tagFilter.Text })
            };

            _tagPanel = new()
            {
                Parent = this,
                Location = new(0, _tagFilter.Bottom + 2),
                Width = tagSectionWidth,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                ShowBorder = false,
                BorderColor = Color.Black,
                BorderWidth = new(2),
                BackgroundColor = Color.Black * 0.4F,
                ShowRightBorder = true,
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom,
                ContentPadding = new(5),
                CanScroll = true,
            };

            _notesLabel = new()
            {
                Parent = this,
                SetLocalizedText = () => strings.Notes,
                Font = Content.DefaultFont32,
                Location = new(_tagPanel.Right + 18, _tagsLabel.Top),
                Size = _tagsLabel.Size,
            };

            _noteField = new()
            {
                Parent = this,
                Location = new(_tagPanel.Right + 10, _notesLabel.Bottom + 10),
                HideBackground = false,
            };

            _noteField.TextChanged += NoteField_TextChanged;

            _modifiedLabel = new()
            {
                Parent = this,
                SetLocalizedText = () => string.Format(strings.LastModified, string.Empty).Substring(0, strings.LastModified.Length - 5),
                Font = Content.DefaultFont16,
                Location = new(_tagPanel.Right + 18, _noteField.Bottom + 5),
                Size = _notesLabel.Size,
                HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Right,
            };

            _modifiedField = new()
            {
                Parent = this,
                Location = new(_modifiedLabel.Right + 10, _modifiedLabel.Top),
                HideBackground = false,
                TextChangedAction = (s) =>
                {
                    if (TemplatePresenter.Template is not null)
                    {
                        TemplatePresenter.Template.LastModified = s;
                    }
                },
            };

            TemplatePresenter.TemplateChanged += TemplatePresenter_TemplateChanged;

            TemplateTags.TagAdded += TemplateTags_TagAdded;
            TemplateTags.TagRemoved += TemplateTags_TagRemoved;
            TemplateTags.TagChanged += TemplateTags_TagChanged;

            _tagPanel.ChildAdded += TagPanel_ChildsChanged;
            _tagPanel.ChildRemoved += TagPanel_ChildsChanged;

            CreateTagControls();

            _created = true;
        }

        public TemplateTags TemplateTags { get; }

        public TagGroups TagGroups { get; }

        public override void Draw(SpriteBatch spriteBatch, Rectangle drawBounds, Rectangle scissor)
        {
            base.Draw(spriteBatch, drawBounds, scissor);
        }

        private void TagPanel_ChildsChanged(object sender, Blish_HUD.Controls.ChildChangedEventArgs e)
        {
            SortPanels();
        }

        private void SortPanels()
        {
            _tagPanel.SortChildren<FlowPanel>((x, y) =>
            {
                var a = TagGroups.FirstOrDefault(group => group.Name == x.Title);
                var b = TagGroups.FirstOrDefault(group => group.Name == y.Title);

                return TemplateTagComparer.CompareGroups(a, b);
            });
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
                            var comparer = new TemplateTagComparer(TagGroups);

                            p.SortChildren<TagControl>(SortTagControls);
                            break;
                        }

                    case nameof(TemplateTag.Group):
                        List<FlowPanel> flowPanelsToDelete = [];

                        foreach (var t in _tagControls)
                        {
                            if (t.Value.FirstOrDefault(x => x.Tag == tag) is var control && control is not null)
                            {
                                var p = GetPanel(tag.Group);
                                control.Parent = p;
                                p.Children.Add(control);
                                p.SortChildren<TagControl>(SortTagControls);
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
                SortPanels();
            }

            return panel;
        }

        private void RemoveEmptyPanels(FlowPanel? fp = null)
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

        private void TemplateTags_TagRemoved(object sender, TemplateTag e)
        {
            RemoveTemplateTag(e);
        }

        private void TemplateTags_TagAdded(object sender, TemplateTag e)
        {
            AddTemplateTag(e);
        }

        private void RemoveTemplateTag(TemplateTag e)
        {

            TagControl tagControl = null;
            var p = _tagControls.FirstOrDefault(x => x.Value.Any(x => x.Tag == e));

            var panel = p.Key;
            tagControl = p.Value.FirstOrDefault(x => x.Tag == e);

            tagControl?.Dispose();
            _tagControls[panel].Remove(tagControl);

            if (panel.Children.Any())
            {
                panel.SortChildren<TagControl>(SortTagControls);
            }

            RemoveEmptyPanels();
        }

        private void AddTemplateTag(TemplateTag e)
        {
            var panel = GetPanel(e.Group);

            _tagControls[panel].Add(new TagControl()
            {
                Parent = panel,
                Tag = e,
                Width = panel.Width - 25,
                OnEditClicked = () =>
                {
                    if (MainWindow is null) return;

                    MainWindow.SelectedTab = MainWindow.TagEditViewTab;
                    MainWindow.TagEditView.SetTagToEdit(e);
                },
                OnClicked = (selected) =>
                {
                    if (TemplatePresenter.Template is Template template)
                    {
                        if (selected)
                        {
                            template.Tags.Add(e.Name);
                        }
                        else
                        {
                            _ = template.Tags.Remove(e.Name);
                        }
                    }
                }
            });

            SortPanels();
            panel.SortChildren<TagControl>(SortTagControls);
        }

        private int SortTagControls(TagControl x, TagControl y)
        {
            return _comparer.Compare(x.Tag, y.Tag);
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

        private void TemplatePresenter_TemplateChanged(object sender, Core.Models.ValueChangedEventArgs<Template> e)
        {
            ApplyTemplate();
        }

        public TemplatePresenter TemplatePresenter { get; private set; }

        public MainWindow MainWindow { get; internal set; }

        private void NoteField_TextChanged(object sender, EventArgs e)
        {
            if (_changeBuild && TemplatePresenter.Template is not null)
            {
                TemplatePresenter.Template.Description = _noteField.Text;
            }
        }

        private void ApplyTemplate()
        {
            _changeBuild = false;

            _modifiedField.Text = TemplatePresenter?.Template?.LastModified;
            _noteField.Text = TemplatePresenter?.Template?.Description;

            var tagControls = new List<TagControl>(_tagPanel.GetChildrenOfType<TagControl>());
            tagControls.AddRange(_tagPanel.GetChildrenOfType<FlowPanel>().SelectMany(x => x.GetChildrenOfType<TagControl>()));

            foreach (var tag in tagControls)
            {
                tag.Selected = TemplatePresenter?.Template?.Tags.Contains(tag.Tag.Name) ?? false;
            }

            _changeBuild = true;
        }

        private void TemplateChanged(object sender, PropertyChangedEventArgs e)
        {
            ApplyTemplate();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            if (!_created) return;

            if (_noteField is not null)
            {
                //_notesLabel.Location = new(_tagPanel.Right + 18, 50 - _notesLabel.Font.LineHeight - 2);
                //_noteField.Location = new(_tagPanel.Right + 15, 50);
                _noteField.Size = new(Width - _tagPanel.Right - 15, Height - _noteField.Top - _modifiedField.Height - 5);

                _modifiedLabel.Location = new(_tagPanel.Right + 18, _noteField.Bottom);
                _modifiedField.Location = new(_modifiedLabel.Right + 10, _modifiedLabel.Top + 5);
                _modifiedField.Size = new(Width - _modifiedField.Left - 5, _modifiedField.Font.LineHeight + 5);
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            TemplatePresenter.TemplateChanged -= TemplatePresenter_TemplateChanged;
            foreach (var c in Children)
            {
                c?.Dispose();
            }

            //TagEditWindow?.Dispose();
        }
    }
}
