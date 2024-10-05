using Blish_HUD.Content;
using Microsoft.Xna.Framework;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Controls;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework.Graphics;
using Blish_HUD;
using Kenedia.Modules.Core.Extensions;
using SharpDX.Direct3D9;
using System;
using Kenedia.Modules.BuildsManager.Controls.Selection;
using Kenedia.Modules.Core.Services;
using Gw2Sharp.WebApi;
using Kenedia.Modules.BuildsManager.Controls;
using System.Diagnostics;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class QuickFiltersPanel : AnchoredContainer
    {
        private int _specializationHeight = 270;
        private Rectangle _textBounds = Rectangle.Empty;
        private readonly DetailedTexture _headerSeparator = new(605022);
        private readonly FlowPanel _tagPanel;
        private readonly Button _resetButton;
        private Dictionary<TagGroupPanel, List<TagToggle>> _tagControls = [];
        private TagGroupPanel _ungroupedPanel;
        private List<TagToggle> _specToggles = [];

        public QuickFiltersPanel(TemplateTags templateTags, TagGroups tagGroups, SelectionPanel selectionPanel, Settings settings)
        {
            TemplateTags = templateTags;
            TagGroups = tagGroups;
            SelectionPanel = selectionPanel;
            Settings = settings;

            Parent = Graphics.SpriteScreen;
            Width = 205;
            Height = 640;

            ContentPadding = new(5);

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

            var fp = new FlowPanel()
            {
                Parent = this,
                Location = new(0, 30),
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
            };

            _tagPanel = new()
            {
                Parent = fp,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Standard,
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom,
                ContentPadding = new(5),
                ControlPadding = new(5),
                CanScroll = true,
            };

            _resetButton = new Button()
            {
                SetLocalizedText = () => string.Format(strings.ResetAll, strings.Filters),
                Width = 192,
                Parent = fp,
                ClickAction = () =>
                {
                    _specToggles.ForEach(x => x.Selected = false);
                    _tagControls.SelectMany(x => x.Value).ForEach(x => x.Selected = false);
                },
            };

            CreateTagControls();
            FadeSteps = 150;

            GameService.Gw2Mumble.PlayerCharacter.SpecializationChanged += PlayerCharacter_SpecializationChanged;
            Settings.QuickFiltersPanelFade.SettingChanged += QuickFiltersPanelFade_SettingChanged;
            Settings.QuickFiltersPanelFadeDelay.SettingChanged += QuickFiltersPanelFadeDelay_SettingChanged;
            Settings.QuickFiltersPanelFadeDuration.SettingChanged += QuickFiltersPanelFadeDuration_SettingChanged;

            ApplySettings();
        }

        private void QuickFiltersPanelFadeDelay_SettingChanged(object sender, Blish_HUD.ValueChangedEventArgs<double> e)
        {
            ApplySettings();
        }

        private void QuickFiltersPanelFade_SettingChanged(object sender, Blish_HUD.ValueChangedEventArgs<bool> e)
        {
            ApplySettings();
        }

        private void QuickFiltersPanelFadeDuration_SettingChanged(object sender, Blish_HUD.ValueChangedEventArgs<double> e)
        {
            ApplySettings();
        }

        private void ApplySettings()
        {
            FadeOut = Settings.QuickFiltersPanelFade.Value;
            FadeDelay = Settings.QuickFiltersPanelFadeDelay.Value * 1000;
            FadeDuration = Settings.QuickFiltersPanelFadeDuration.Value;
        }

        private void PlayerCharacter_SpecializationChanged(object sender, ValueEventArgs<int> e)
        {
            var professionType = GameService.Gw2Mumble.PlayerCharacter.Profession;

            if (BuildsManager.Data.Professions.TryGetValue(professionType, out var profession))
            {
                foreach (var t in _specToggles)
                {
                    t.Selected = false;
                }

                if (Settings.AutoSetFilterProfession?.Value is true)
                {

                    if (_specToggles.FirstOrDefault(x => x.Tag?.Name == profession.Name) is TagToggle toggle)
                    {
                        toggle.Selected = true;
                    }
                }

                if (Settings.AutoSetFilterSpecialization?.Value is true)
                {

                    if (profession.Specializations.Values.FirstOrDefault(x => x.Id == e.Value) is var spec && spec is not null && _specToggles.FirstOrDefault(x => x.Tag?.Name == spec.Name) is TagToggle specToggle)
                    {
                        specToggle.Selected = true;
                    }
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

        public SelectionPanel SelectionPanel { get; }

        public Settings Settings { get; }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int width = Width - 50;
            int w = width / 2;
            int scale = width / 512;

            int padding = (Width - width) / 2;

            _headerSeparator.Bounds = new(padding + w, -w + 30, 16, width);
            _textBounds = new(0, ContentPadding.Top, Width, Content.DefaultFont18.LineHeight);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            spriteBatch.DrawStringOnCtrl(this, "Filter Templates", Content.DefaultFont18, _textBounds, Color.White, false, Blish_HUD.Controls.HorizontalAlignment.Center);
            spriteBatch.DrawCenteredRotationOnCtrl(this, _headerSeparator.Texture, _headerSeparator.Bounds, _headerSeparator.TextureRegion, Color.White, 1.56F, false, false);
        }

        public TagGroupPanel GetPanel(string title)
        {
            TagGroupPanel panel = null;

            if (!string.IsNullOrEmpty(title))
            {
                if (_tagControls.Keys.FirstOrDefault(x => x.TagGroup.Name == title) is TagGroupPanel p)
                {
                    panel = p;
                }

                if (TagGroups.FirstOrDefault(x => x.Name == title) is var groupName && groupName is not null)
                {
                    panel ??= new TagGroupPanel(groupName, _tagPanel)
                    {
                        //Title = title,
                        //Width = _tagPanel.Width - 25,
                        WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                        HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
                        AutoSizePadding = new(0, 2),
                        //OuterControlPadding = new(25, 0),
                        //CanCollapse = true,
                    };
                }
            }

            panel ??= _ungroupedPanel ??= new TagGroupPanel(TagGroup.Empty, _tagPanel)
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

        private TagToggle AddTemplateTag(TemplateTag e, TagGroupPanel? parent = null, Action<TemplateTag>? action = null)
        {
            var panel = parent ?? GetPanel(e.Group);

            bool hasTag(Template t)
            {
                return t.Tags.Contains(e.Name);
            }

            Action<TemplateTag> a = action ?? new Action<TemplateTag>((x) =>
            {
                if (!SelectionPanel.BuildSelection.FilterQueries.Any(x => x.Key == e.Group))
                {
                    SelectionPanel.BuildSelection.FilterQueries.Add(new KeyValuePair<string, List<Func<Template, bool>>>(e.Group, []));
                }

                if (SelectionPanel.BuildSelection.FilterQueries.FirstOrDefault(x => x.Key == e.Group) is var q && q.Value is not null)
                {
                    if (q.Value.Contains(hasTag))
                    {
                        q.Value.Remove(hasTag);
                    }
                    else
                    {
                        q.Value.Add(hasTag);
                    }
                }

                SelectionPanel.BuildSelection.FilterTemplates();
            });

            var t = new TagToggle(e)
            {
                Parent = panel,
                OnSelectedChanged = a,
            };

            if (_tagControls.ContainsKey(panel))
            {
                _tagControls[panel].Add(t);
            }
            else
            {
                _tagControls.Add(panel, [t]);
            }

            SortPanels();
            var comparer = new TemplateTagComparer(TagGroups);

            panel.SortChildren<TagToggle>((x, y) =>
            {
                var a = x.Tag;
                var b = y.Tag;

                return comparer.Compare(a, b);
            });

            return t;
        }

        private void SortPanels()
        {
            _tagPanel.SortChildren<TagGroupPanel>((x, y) =>
            {
                var a = TagGroups.FirstOrDefault(group => group.Name == x.Title);
                var b = TagGroups.FirstOrDefault(group => group.Name == y.Title);

                return TemplateTagComparer.CompareGroups(a, b);
            });

            SetHeightToTags();
        }

        private void SetHeightToTags()
        {
            int height = _specializationHeight;

            foreach (var t in _tagControls)
            {
                if (t.Key.TagGroup.Name == strings.Specializations)
                    continue;

                int rows = (int)Math.Ceiling(t.Value.Count / 6d);
                height += (rows * TagToggle.TagHeight) + ((rows - 1) * TagGroupPanel.ControlPaddingY) + TagGroupPanel.OuterControlPaddingY + (int)_tagPanel.ControlPadding.Y;
            }

            _tagPanel.Height = height - (int)_tagPanel.ControlPadding.Y - 5;
            Height = _tagPanel.Height + 30 + 30 + _tagPanel.ContentPadding.Vertical;
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

            TagGroupPanel specs = new(new TagGroup(strings.Specializations), _tagPanel)
            {
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.LeftToRight,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Standard,
                Height = _specializationHeight,
                AutoSizePadding = new(0, 2),
                ControlPadding = new(21, 2),
            };

            TagToggle toggle;
            _specToggles.Clear();

            int prio;
            foreach (var p in BuildsManager.Data.Professions.Values)
            {
                prio = ((int)p.Id) * 100;

                bool isProfession(Template t)
                {
                    return t.Profession == p.Id;
                }

                _specToggles.Add(toggle = AddTemplateTag(new TemplateTag()
                {
                    Name = p.Name,
                    Group = strings.Specializations,
                    AssetId = p.IconAssetId,
                    Priority = prio,
                }, specs, (x) =>
                {
                    if (SelectionPanel.BuildSelection.SpecializationFilterQueries.Contains(isProfession))
                    {
                        SelectionPanel.BuildSelection.SpecializationFilterQueries.Remove(isProfession);
                    }
                    else
                    {
                        SelectionPanel.BuildSelection.SpecializationFilterQueries.Add(isProfession);
                    }

                    SelectionPanel.BuildSelection.FilterTemplates();
                }));

                toggle.SetLocalizedTooltip = () => p.Name;

                foreach (var s in p.Specializations.Values)
                {
                    if (s.Elite)
                    {

                        bool isSpecialization(Template t)
                        {
                            return t.EliteSpecializationId == s.Id;
                        }

                        _specToggles.Add(toggle = AddTemplateTag(new TemplateTag()
                        {
                            Name = s.Name,
                            Group = strings.Specializations,
                            AssetId = s.ProfessionIconAssetId ?? 0,
                            Priority = prio + s.Id,
                        }, specs, (x) =>
                        {
                            if (SelectionPanel.BuildSelection.SpecializationFilterQueries.Contains(isSpecialization))
                            {
                                SelectionPanel.BuildSelection.SpecializationFilterQueries.Remove(isSpecialization);
                            }
                            else
                            {
                                SelectionPanel.BuildSelection.SpecializationFilterQueries.Add(isSpecialization);
                            }

                            SelectionPanel.BuildSelection.FilterTemplates();
                        }));

                        toggle.SetLocalizedTooltip = () => s.Name;
                    }
                }
            }

            SortPanels();
        }

        public override void UserLocale_SettingChanged(object sender, Blish_HUD.ValueChangedEventArgs<Locale> e)
        {
            base.UserLocale_SettingChanged(sender, e);

            //_tagControls.Clear();
            //_tagPanel.ClearChildren();
        }
    }
}
