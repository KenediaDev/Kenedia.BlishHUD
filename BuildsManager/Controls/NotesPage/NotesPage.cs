using Kenedia.Modules.BuildsManager.Controls.Selection;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.Controls.NotesPage
{
    public class AboutPage : Blish_HUD.Controls.Container
    {
        private readonly Blish_HUD.Controls.MultilineTextBox _noteField;
        private readonly TexturesService _texturesService;
        private readonly FlowPanel _tagPanel;
        private readonly FlowPanel _encountersPanel;
        private readonly Label _notesLabel;
        private readonly Label _encountersLabel;
        private readonly Label _tagsLabel;
        private readonly Button _clearAll;
        private readonly Button _setAll;
        private readonly Button _deleteTemplate;
        private readonly List<(TemplateFlag tag, Image texture, Checkbox checkbox)> _tags = new();
        private readonly List<(EncounterFlag tag, Image texture, Checkbox checkbox)> _encounters = new();
        private readonly bool _created = false;
        private bool _changeBuild = true;

        private Template _template;
        public AboutPage(TexturesService texturesService)
        {
            _texturesService = texturesService;

            _tagPanel = new()
            {
                Parent = this,
                Location = new(0, 75),
                Width = 250,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                ShowBorder = true,
                ShowRightBorder = true,
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom,
                ContentPadding = new(5),
                CanScroll = true,
            };

            _setAll = new()
            {
                Text = "All",
                Parent = this,
                Width = (_tagPanel.Width - 5) / 2,
                Location = new(0, _tagPanel.Top - 25),
                ClickAction = () =>
                {
                    if(Template !=null)
                    {
                        Template.Encounters = (EncounterFlag)Enum.GetValues(typeof(EncounterFlag)).Cast<long>().Sum();
                        Template.Tags = (TemplateFlag)Enum.GetValues(typeof(TemplateFlag)).Cast<int>().Sum();
                    }
                },
            };

            _clearAll = new()
            {
                Text = "Clear",
                Parent = this,
                Width = (_tagPanel.Width - 5) / 2,
                Location = new(_setAll.Right + 5, _setAll.Top),
                ClickAction = () =>
                {
                    if (Template != null)
                    {
                        Template.Encounters = EncounterFlag.None;
                        Template.Tags = TemplateFlag.None;
                    }
                },
            };

            _tagsLabel = new()
            {
                Parent = this,
                Text = "Tags",
                Font = Content.DefaultFont32,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                Location = new(_tagPanel.Left + 3, _clearAll.Top - Content.DefaultFont32.LineHeight - 2),
            };

            _notesLabel = new()
            {
                Parent = this,
                Text = "Notes",
                Font = Content.DefaultFont32,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };

            _deleteTemplate = new()
            {
                Text = "Delete Template",
                Parent = this,
                Width = 150,
                Location = new(ContentRegion.Right -  155, _tagsLabel.Bottom - 25),
                ClickAction = () =>
                {
                    _ = BuildsManager.ModuleInstance.Templates.Remove(Template);
                    _ = Template.Delete();
                    BuildsManager.ModuleInstance.SelectedTemplate = null;
                },
            };

            _noteField = new()
            {
                Parent = this,
                HideBackground = false,
            };

            _noteField.TextChanged += _noteField_TextChanged;

            foreach (TemplateFlag tag in Enum.GetValues(typeof(TemplateFlag)))
            {
                if (tag != TemplateFlag.None)
                {
                    FlowPanel p = new()
                    {
                        Parent = _tagPanel,
                        WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                        HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
                        FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleLeftToRight,
                        Height = 30,
                        ControlPadding = new(5),
                    };

                    (TemplateFlag tag, Image texture, Checkbox checkbox) t;

                    _tags.Add(t = new(
                        tag,
                        new() { Texture = TemplateTagTextures.GetTexture(tag), Parent = p, Size = new(p.Height) },
                        new()
                        {
                            Parent = p,
                            Text = tag.ToString(),
                            Height = p.Height,
                        }
                        ));

                    t.checkbox.CheckedChangedAction = async (isChecked) =>
                    {
                        t.checkbox.TextColor = isChecked ? Color.White : Color.Gray * 0.5F;
                        t.texture.Tint = isChecked ? Color.White : Color.Gray * 0.5F;

                        if (_changeBuild)
                        {
                            Template.Tags = isChecked ? Template.Tags | tag : Template.Tags & ~tag;
                        }
                    };
                }
            }

            foreach (EncounterFlag tag in Enum.GetValues(typeof(EncounterFlag)))
            {
                if (tag != EncounterFlag.None)
                {
                    FlowPanel p = new()
                    {
                        Parent = _tagPanel,
                        WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                        HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
                        FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleLeftToRight,
                        Height = 30,
                        ControlPadding = new(5),
                    };

                    (EncounterFlag tag, Image texture, Checkbox checkbox) t;

                    _encounters.Add(t = new(
                        tag,
                        new()
                        {
                            Texture = TemplateTagTextures.GetTexture(tag),
                            Parent = p,
                            Size = new(p.Height)
                        },
                        new()
                        {
                            Parent = p,
                            Text = tag.ToString(),
                            Height = p.Height,
                        }
                        ));

                    t.checkbox.CheckedChangedAction = async (isChecked) =>
                    {
                        t.checkbox.TextColor = isChecked ? Color.White : Color.Gray * 0.5F;
                        t.texture.Tint = isChecked ? Color.White : Color.Gray * 0.5F;

                        if (_changeBuild)
                        {
                            Template.Encounters = isChecked ? Template.Encounters | tag : Template.Encounters & ~tag;
                        }
                    };
                }
            }

            _created = true;
        }

        private void _noteField_TextChanged(object sender, EventArgs e)
        {
            if(_changeBuild)
            {
                Template.Description = _noteField.Text;
            }
        }

        public Template Template
        {
            get => _template; set
            {
                var temp = _template;
                if (Common.SetProperty(ref _template, value, ApplyTemplate))
                {
                    if (temp != null) temp.Changed -= TemplateChanged;
                    if (_template != null) _template.Changed += TemplateChanged;
                }
            }
        }

        private void ApplyTemplate()
        {
            _changeBuild = false;
            foreach (var tag in _tags)
            {
                tag.checkbox.Checked = Template?.Tags.HasFlag(tag.tag) == true;
                tag.checkbox.TextColor = tag.checkbox.Checked ? Color.White : Color.Gray * 0.5F;
                tag.texture.Tint = tag.checkbox.Checked ? Color.White : Color.Gray * 0.5F;
            }

            foreach (var tag in _encounters)
            {
                tag.checkbox.Checked = Template?.Encounters.HasFlag(tag.tag) == true;
                tag.checkbox.TextColor = tag.checkbox.Checked ? Color.White : Color.Gray * 0.5F;
                tag.texture.Tint = tag.checkbox.Checked ? Color.White : Color.Gray * 0.5F;
            }

            _noteField.Text = Template?.Description;
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

            if (_noteField != null)
            {
                _notesLabel.Location = new(_tagPanel.Right + 18, 50 - _notesLabel.Font.LineHeight - 2);
                _noteField.Location = new(_tagPanel.Right + 15, 50);
                _noteField.Size = new(Width - _tagPanel.Right - 15, Height - 50);
            }

            _deleteTemplate.Location = new(ContentRegion.Right - 150, _tagsLabel.Bottom - 25);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            if (_template != null) _template.Changed -= TemplateChanged;
        }
    }

    public class RotationPage : Blish_HUD.Controls.Container
    {
        private readonly Blish_HUD.Controls.MultilineTextBox _noteField;
        private readonly bool created = false;
        private TexturesService _texturesService;

        private Template _template;

        public RotationPage(TexturesService texturesService)
        {
            _texturesService = texturesService;

            _noteField = new()
            {
                Parent = this,
                HideBackground = false,
            };

            created = true;
        }

        public Template Template
        {
            get => _template; set
            {
                var temp = _template;
                if (Common.SetProperty(ref _template, value, ApplyTemplate))
                {
                    if (temp != null) temp.Changed -= TemplateChanged;
                    if (_template != null) _template.Changed += TemplateChanged;
                }
            }
        }

        private void ApplyTemplate()
        {

        }

        private void TemplateChanged(object sender, PropertyChangedEventArgs e)
        {
            ApplyTemplate();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            if (!created) return;

            if (_noteField != null) _noteField.Size = Size;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            if (_template != null) _template.Changed -= TemplateChanged;
        }
    }
}
