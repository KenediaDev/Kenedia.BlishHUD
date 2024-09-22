using Blish_HUD;
using Blish_HUD.Content;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.BuildsManager.Views;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Res;
using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.Controls.Tabs
{
    public class AboutTab : Blish_HUD.Controls.Container
    {
        private readonly Blish_HUD.Controls.MultilineTextBox _noteField;
        private readonly FlowPanel _tagPanel;
        private readonly Label _notesLabel;
        private readonly Label _tagsLabel;
        private readonly ButtonImage _editTags;
        private readonly FilterBox _tagFilter;

        private readonly List<(TemplateFlag tag, Image texture, Checkbox checkbox)> _tags = new();
        private readonly List<(EncounterFlag tag, Image texture, Checkbox checkbox)> _encounters = new();
        private readonly bool _created = false;
        private int tagSectionWidth;
        private bool _changeBuild = true;
        private readonly TagEditWindow _tagEditWindow;

        private Color _disabledColor = Color.Gray;

        public AboutTab(TemplatePresenter templatePresenter, TemplateTags templateTags)
        {
            TemplatePresenter = templatePresenter;
            TemplateTags = templateTags;

            HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill;
            WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill;
            tagSectionWidth = 300;

            int Height = 670;
            int Width = 915;

            _tagEditWindow = new(
                TexturesService.GetTextureFromRef(@"textures\mainwindow_background.png", "mainwindow_background"),
                new Rectangle(30, 30, Width, Height + 30),
                new Rectangle(40, 40, Width - 3, Height),
                templateTags)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "❤",
                Subtitle = "❤",
                SavesPosition = true,
                Id = $"{BuildsManager.ModuleInstance.Name} TagWindow",
                MainWindowEmblem = AsyncTexture2D.FromAssetId(536043),
                SubWindowEmblem = AsyncTexture2D.FromAssetId(156031),
                Name = "Tag Editing",
                Width = 580,
                Height = 800,
                CanResize = true,
            };

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
                        var templateTag = TemplateTags.Tags.FirstOrDefault(e => e.Name.ToLower() == txt.ToLower());

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
                TextChangedAction = (txt) => _editTags.Enabled = !string.IsNullOrEmpty(txt.Trim()) && TemplateTags.Tags.FirstOrDefault(e => e.Name.ToLower() == txt.ToLower()) is null,
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
                Enabled = false,
                ClickAction = (b) => TemplateTags.Add(new TemplateTag() { Name = string.IsNullOrEmpty(_tagFilter.Text) ? "New Tag" : _tagFilter.Text })
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

            TemplatePresenter.TemplateChanged += TemplatePresenter_TemplateChanged;
            TemplateTags.TagsChanged += TemplateTags_TagsChanged;

            CreateTagControls();

            _created = true;
        }

        public TemplateTags TemplateTags { get; }

        private void TemplateTags_TagsChanged(object sender, TemplateTag e)
        {
            CreateTagControls();
        }

        private void CreateTagControls()
        {
            _tagPanel?.ClearChildren();

            foreach (var tag in TemplateTags.Tags)
            {
                _ = new TagControl()
                {
                    Parent = _tagPanel,
                    Width = tagSectionWidth - _tagPanel.ContentPadding.Horizontal - 20,
                    Tag = tag,
                    OnEditClicked = () => _tagEditWindow?.ToggleWindow(),
                    OnClicked = (selected) =>
                    {
                        if (TemplatePresenter.Template is Template template)
                        {
                            if (selected)
                            {
                                template.Tags.Add(tag.Name);
                            }
                            else
                            {
                                _ = template.Tags.Remove(tag.Name);
                            }
                        }
                    }
                };
            }
        }

        private void TemplatePresenter_TemplateChanged(object sender, Core.Models.ValueChangedEventArgs<Template> e)
        {
            ApplyTemplate();
        }

        public TemplatePresenter TemplatePresenter { get; private set; }

        private void NoteField_TextChanged(object sender, EventArgs e)
        {
            if (_changeBuild)
            {
                TemplatePresenter.Template.Description = _noteField.Text;
            }
        }

        private void ApplyTemplate()
        {
            _changeBuild = false;

            _noteField.Text = TemplatePresenter?.Template?.Description;

            foreach (var tag in _tagPanel.GetChildrenOfType<TagControl>())
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
                _noteField.Size = new(Width - _tagPanel.Right - 15, Height - _noteField.Top);
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            _tagEditWindow?.Dispose();
            TemplatePresenter.TemplateChanged -= TemplatePresenter_TemplateChanged;
            foreach (var c in Children)
            {
                c?.Dispose();
            }
        }
    }
}
