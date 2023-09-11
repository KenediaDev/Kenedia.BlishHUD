using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.Controls.AboutPage
{
    public class AboutPage : Blish_HUD.Controls.Container
    {
        private readonly Blish_HUD.Controls.MultilineTextBox _noteField;
        private readonly FlowPanel _tagPanel;
        private readonly Label _notesLabel;
        private readonly Label _tagsLabel;
        private readonly Button _clearAll;
        private readonly Button _setAll;
        private readonly Button _deleteTemplate;
        private readonly List<(TemplateFlag tag, Image texture, Checkbox checkbox)> _tags = new();
        private readonly List<(EncounterFlag tag, Image texture, Checkbox checkbox)> _encounters = new();
        private readonly bool _created = false;
        private bool _changeBuild = true;

        private Color _disabledColor = Color.Gray;

        public AboutPage(TemplatePresenter _templatePresenter)
        {
            TemplatePresenter = _templatePresenter;

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
                SetLocalizedText = () => strings.All,
                Parent = this,
                Width = (_tagPanel.Width - 5) / 2,
                Location = new(0, _tagPanel.Top - 25),
                ClickAction = () =>
                {
                    if (TemplatePresenter?.Template is not null)
                    {
                        TemplatePresenter.Template.Encounters = (EncounterFlag)Enum.GetValues(typeof(EncounterFlag)).Cast<long>().Sum();
                        TemplatePresenter.Template.Tags = (TemplateFlag)Enum.GetValues(typeof(TemplateFlag)).Cast<int>().Sum();
                        ApplyTemplate();
                    }
                },
            };

            _clearAll = new()
            {
                SetLocalizedText = () => strings.None,
                Parent = this,
                Width = (_tagPanel.Width - 5) / 2,
                Location = new(_setAll.Right + 5, _setAll.Top),
                ClickAction = () =>
                {
                    if (TemplatePresenter?.Template is not null)
                    {
                        TemplatePresenter.Template.Encounters = EncounterFlag.None;
                        TemplatePresenter.Template.Tags = TemplateFlag.None;
                        ApplyTemplate();
                    }
                },
            };

            _tagsLabel = new()
            {
                Parent = this,
                SetLocalizedText = () => strings.Tags,
                Font = Content.DefaultFont32,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                Location = new(_tagPanel.Left + 3, _clearAll.Top - Content.DefaultFont32.LineHeight - 2),
            };

            _notesLabel = new()
            {
                Parent = this,
                SetLocalizedText = () => strings.Notes,
                Font = Content.DefaultFont32,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };

            _deleteTemplate = new()
            {
                Parent = this,
                SetLocalizedText = () => strings.DeleteTemplate,
                Width = 150,
                Location = new(ContentRegion.Right - 155, _tagsLabel.Bottom - 25),
                ClickAction = () =>
                {
                    _ = BuildsManager.ModuleInstance.Templates.Remove(TemplatePresenter?.Template);
                    _ = TemplatePresenter?.Template.Delete();
                },
            };

            _noteField = new()
            {
                Parent = this,
                HideBackground = false,
            };

            _noteField.TextChanged += NoteField_TextChanged;

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

                    t.checkbox.CheckedChangedAction = (isChecked) =>
                    {
                        t.checkbox.TextColor = isChecked ? Color.White : _disabledColor;
                        t.texture.Tint = isChecked ? Color.White : _disabledColor;

                        if (_changeBuild && TemplatePresenter?.Template is not null)
                        {
                            TemplatePresenter.Template.Tags = isChecked ? TemplatePresenter.Template.Tags | tag : TemplatePresenter.Template.Tags & ~tag;
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
                        t.checkbox.TextColor = isChecked ? Color.White : _disabledColor;
                        t.texture.Tint = isChecked ? Color.White : _disabledColor;

                        if (_changeBuild && TemplatePresenter?.Template is not null)
                        {
                            TemplatePresenter.Template.Encounters = isChecked ? TemplatePresenter.Template.Encounters | tag : TemplatePresenter.Template.Encounters & ~tag;
                        }
                    };
                }
            }

            TemplatePresenter.TemplateChanged += TemplatePresenter_TemplateChanged;
            _created = true;
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
            foreach (var tag in _tags)
            {
                tag.checkbox.Checked = TemplatePresenter?.Template?.Tags.HasFlag(tag.tag) == true;
                tag.checkbox.TextColor = tag.checkbox.Checked ? Color.White : _disabledColor;
                tag.texture.Tint = tag.checkbox.Checked ? Color.White : _disabledColor;
            }

            foreach (var tag in _encounters)
            {
                tag.checkbox.Checked = TemplatePresenter?.Template?.Encounters.HasFlag(tag.tag) == true;
                tag.checkbox.TextColor = tag.checkbox.Checked ? Color.White : _disabledColor;
                tag.texture.Tint = tag.checkbox.Checked ? Color.White : _disabledColor;
            }

            _noteField.Text = TemplatePresenter?.Template?.Description;
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
                _notesLabel.Location = new(_tagPanel.Right + 18, 50 - _notesLabel.Font.LineHeight - 2);
                _noteField.Location = new(_tagPanel.Right + 15, 50);
                _noteField.Size = new(Width - _tagPanel.Right - 15, Height - 50);
            }

            _deleteTemplate.Location = new(ContentRegion.Right - 150, _tagsLabel.Bottom - 25);
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
