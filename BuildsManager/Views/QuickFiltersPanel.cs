using Blish_HUD;
using Blish_HUD.Content;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi;
using Kenedia.Modules.BuildsManager.Controls.Selection;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

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
        private TagGroupPanel _specPanel;
        private List<TagToggle> _specToggles = [];

        public QuickFiltersPanel(TemplateCollection templates, TemplateTags templateTags, TagGroups tagGroups, SelectionPanel selectionPanel, Settings settings, Data data)
        {
            Templates = templates;
            TemplateTags = templateTags;
            TagGroups = tagGroups;
            SelectionPanel = selectionPanel;
            Settings = settings;
            Data = data;

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
                ClickAction = ResetAllToggles,
            };

            FadeSteps = 150;

            GameService.Gw2Mumble.PlayerCharacter.SpecializationChanged += PlayerCharacter_SpecializationChanged;
            Settings.QuickFiltersPanelFade.SettingChanged += QuickFiltersPanelFade_SettingChanged;
            Settings.QuickFiltersPanelFadeDelay.SettingChanged += QuickFiltersPanelFadeDelay_SettingChanged;
            Settings.QuickFiltersPanelFadeDuration.SettingChanged += QuickFiltersPanelFadeDuration_SettingChanged;

            ApplySettings();
            SetAutoFilters(GameService.Gw2Mumble.PlayerCharacter?.Specialization ?? 0);

            TagGroups.GroupAdded += TagGroups_GroupAdded;
            TagGroups.GroupRemoved += TagGroups_GroupRemoved;
            TagGroups.GroupChanged += TagGroups_GroupChanged;

            TemplateTags.TagAdded += TemplateTags_TagAdded;
            TemplateTags.TagRemoved += TemplateTags_TagRemoved;
            TemplateTags.TagChanged += TemplateTags_TagChanged;

            Templates.CollectionChanged += Templates_CollectionChanged;

            GameService.Graphics.QueueMainThreadRender((graphicsDevice) => SortPanels());
            SetHeightToTags();

            TemplateTags.Loaded += TemplateTags_Loaded;

            if (TemplateTags.IsLoaded)
            {
                CreateTagControls();
            }

            Data.Loaded += Data_Loaded;

            if (Data.IsLoaded)
            {
                CreateSpecToggles();
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
        }

        private void Data_Loaded(object sender, EventArgs e)
        {
            CreateSpecToggles();
        }

        private void TemplateTags_Loaded(object sender, EventArgs e)
        {
            CreateTagControls();
        }

        private void ResetAllToggles()
        {
            _specToggles.ForEach(x => x.Selected = false);
            _tagControls.SelectMany(x => x.Value).ForEach(x => x.Selected = false);
        }

        private void Templates_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e?.Action is NotifyCollectionChangedAction.Add)
            {
                if (Settings.ResetFilterOnTemplateCreate.Value)
                {
                    ResetAllToggles();
                }
                else
                {

                }
            }
        }

        private void TagGroups_GroupChanged(object sender, PropertyAndValueChangedEventArgs e)
        {
            SortPanels();
        }

        private void TagGroups_GroupRemoved(object sender, TagGroup e)
        {
            SortPanels();
        }

        private void TagGroups_GroupAdded(object sender, TagGroup e)
        {
            SortPanels();
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
            SetAutoFilters(e.Value);
        }

        private void SetAutoFilters(int e)
        {
            var professionType = GameService.Gw2Mumble.PlayerCharacter.Profession;

            if (Data.Professions.TryGetValue(professionType, out var profession))
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

                    if (profession.Specializations.Values.FirstOrDefault(x => x.Id == e) is var spec && spec is not null && _specToggles.FirstOrDefault(x => x.Tag?.Name == spec.Name) is TagToggle specToggle)
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
                                var panel = t.Key;
                                var p = GetPanel(tag.Group);

                                if (panel == p)
                                {
                                    SortPanels();
                                    return;
                                }

                                control.Parent = p;
                                p.Children.Add(control);
                                p.SortChildren<TagToggle>(SortTagControls);
                                _tagControls[p].Add(control);

                                _tagControls[panel].Remove(control);
                                panel.Children.Remove(control);

                                if (panel != _ungroupedPanel && panel.Children.Where(x => x != control).Count() <= 0)
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

                        SortPanels();
                        break;
                }

            }
        }

        public TemplateCollection Templates { get; }

        public TemplateTags TemplateTags { get; }

        public TagGroups TagGroups { get; }

        public SelectionPanel SelectionPanel { get; }

        public Settings Settings { get; }

        public Data Data { get; }

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

        public TagGroupPanel GetPanel(string groupName)
        {
            TagGroupPanel panel = null;

            if (!string.IsNullOrEmpty(groupName))
            {
                var tagGroup = TagGroups.FirstOrDefault(x => x.Name == groupName);

                if (tagGroup is not null)
                {
                    if (_tagControls.Keys.FirstOrDefault(x => x.TagGroup == tagGroup) is TagGroupPanel p)
                    {
                        panel = p;
                    }

                    panel ??= new TagGroupPanel(tagGroup, _tagPanel)
                    {
                        //Title = groupName,
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
            if (e is null || string.IsNullOrEmpty(e.Name) || _tagControls.SelectMany(x => x.Value).Any(x => x.Tag == e))
            {
                return null;
            }

            if (e?.Icon?.Texture is null)
            {
                BuildsManager.Logger.Warn($"Tag '{e.Name}' in Group {e.Group} has no icon.");
                return null;
            }

            var panel = parent ?? GetPanel(e.Group);

            bool hasTag(Template t)
            {
                return t.Tags.Contains(e.Name);
            }

            Action<TemplateTag> a = action ?? new Action<TemplateTag>((x) =>
            {
                if (e?.Icon?.Texture is null)
                {
                    BuildsManager.Logger.Warn($"[OnSelectedChanged]: Tag '{e.Name}' in Group '{e.Group}' has no icon.");
                    BuildsManager.Logger.Warn($"{e?.ToJson()}");
                    return;
                }

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
                // --- Handle spec panel: always comes first ---
                if (x == _specPanel && y != _specPanel) return -1; // x before y
                if (y == _specPanel && x != _specPanel) return 1;  // y before x

                // --- Handle ungrouped panel: always comes last ---
                if (x == _ungroupedPanel && y != _ungroupedPanel) return 1;  // x after y
                if (y == _ungroupedPanel && x != _ungroupedPanel) return -1; // y after x

                // --- Normal comparison for all other groups ---
                var a = TagGroups.FirstOrDefault(group => group == x.TagGroup);
                var b = TagGroups.FirstOrDefault(group => group == y.TagGroup);

                return TemplateTagComparer.CompareGroups(a, b);
            });

            SetHeightToTags();

            GameService.Graphics.QueueMainThreadRender((graphicsDevice) => SetHeightToTags());
        }

        private void SetHeightToTags()
        {
            int height = _specializationHeight + 15;

            foreach (var t in _tagControls)
            {
                if (t.Key.TagGroup.Name == strings.Specializations)
                    continue;

                int rows = (int)Math.Ceiling(t.Value.Count / 6d);
                int rowHeight = t.Key.Height <= 20 ? (rows * TagToggle.TagHeight) + ((rows - 1) * TagGroupPanel.ControlPaddingY) + TagGroupPanel.OuterControlPaddingY + (int)_tagPanel.ControlPadding.Y : t.Key.Height + (int)_tagPanel.ControlPadding.Y;
                //height += (rows * TagToggle.TagHeight) + ((rows - 1) * TagGroupPanel.ControlPaddingY) + TagGroupPanel.OuterControlPaddingY + (int)_tagPanel.ControlPadding.Y;
                //height += t.Key.Height + (int)_tagPanel.ControlPadding.Y;
                height += rowHeight;
            }

            _tagPanel.Height = height;
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

            SortPanels();
        }

        private void CreateSpecToggles()
        {
            _specPanel?.ClearChildren();
            _specPanel?.Dispose();

            _specPanel = new(new TagGroup(strings.Specializations), _tagPanel)
            {
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.LeftToRight,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Standard,
                Height = _specializationHeight,
                AutoSizePadding = new(0, 2),
                ControlPadding = new(15, 2),
            };

            TagToggle toggle;
            _specToggles.Clear();

            int eliteSpecializations = Data.Professions[ProfessionType.Guardian].Specializations.Count(e => e.Value.Elite);
            int specializations = Data.Professions[ProfessionType.Guardian].Specializations.Count - eliteSpecializations;

            Width = 10 + ((eliteSpecializations + 1) * 42);
            _resetButton.Width = Width - 13;

            int prio;
            foreach (var p in Data.Professions.Values)
            {
                prio = ((int)p.Id) * 100;

                if (p is null)
                {
                    continue;
                }

                bool isProfession(Template t)
                {
                    return t.Profession == p.Id;
                }

                toggle = AddTemplateTag(new TemplateTag()
                {
                    Name = p.Name,
                    Group = strings.Specializations,
                    AssetId = p.IconAssetId,
                    Priority = prio,
                }, _specPanel, (x) =>
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
                });

                if (toggle is not null)
                {
                    _specToggles.Add(toggle);
                    toggle.SetLocalizedTooltip = () => p.Name;
                }

                foreach (var s in p.Specializations.Values)
                {
                    if (p is null)
                    {
                        continue;
                    }

                    if (s.Elite)
                    {

                        bool isSpecialization(Template t)
                        {
                            return t.EliteSpecializationId == s.Id;
                        }

                        toggle = AddTemplateTag(new TemplateTag()
                        {
                            Name = s.Name,
                            Group = strings.Specializations,
                            AssetId = s.ProfessionIconAssetId ?? 0,
                            Priority = prio + s.Id,
                        }, _specPanel, (x) =>
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
                        });

                        if (toggle is not null)
                        {
                            _specToggles.Add(toggle);
                            toggle.SetLocalizedTooltip = () => s.Name;
                        }
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
