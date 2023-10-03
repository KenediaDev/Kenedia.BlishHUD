using Blish_HUD.Content;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Res;
using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Kenedia.Modules.BuildsManager.Controls.AboutPage
{
    public class AboutPage : Blish_HUD.Controls.Container
    {
        private readonly Blish_HUD.Controls.MultilineTextBox _noteField;
        private readonly FlowPanel _tagPanel;
        private readonly Label _notesLabel;
        private readonly Label _tagsLabel;
        private readonly ButtonImage _editTags;
        private readonly FilterBox  _tagFilter;

        private readonly List<(TemplateFlag tag, Image texture, Checkbox checkbox)> _tags = new();
        private readonly List<(EncounterFlag tag, Image texture, Checkbox checkbox)> _encounters = new();
        private readonly bool _created = false;
        private readonly TemplateTags _templateTags;
        private int tagSectionWidth;
        private bool _changeBuild = true;

        private Color _disabledColor = Color.Gray;

        public AboutPage(TemplatePresenter _templatePresenter, TemplateTags templateTags)
        {
            TemplatePresenter = _templatePresenter;
            _templateTags = templateTags;

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

            _editTags = new()
            {
                Parent = this,
                Size = new(35),
                Location = new(_tagsLabel.Right + 5, _tagsLabel.Top),
                Texture = AsyncTexture2D.FromAssetId(157109),
                HoveredTexture = AsyncTexture2D.FromAssetId(157110),
                SetLocalizedTooltip = () => strings.EditTags,
            };

            _tagFilter = new()
            {
                Parent = this,
                Location = new(0, _tagsLabel.Bottom + 10),
                Width = tagSectionWidth,
                SetLocalizedPlaceholder = () => strings_common.Search,
                FilteringOnTextChange = true,
                FilteringOnEnter = true,
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
            _templateTags.TagsChanged += TemplateTags_TagsChanged;

            CreateTagControls();

            _created = true;
        }

        private void TemplateTags_TagsChanged(object sender, TemplateTag e)
        {
            CreateTagControls();
        }

        private void CreateTagControls()
        {
            _tagPanel?.ClearChildren();

            foreach (var tag in _templateTags.Tags)
            {
                _ = new TagControl()
                {
                    Parent = _tagPanel,
                    Width = tagSectionWidth - _tagPanel.ContentPadding.Horizontal - 20,
                    Tag = tag,
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

            TemplatePresenter.TemplateChanged -= TemplatePresenter_TemplateChanged;
            foreach (var c in Children)
            {
                c?.Dispose();
            }
        }
    }
}
