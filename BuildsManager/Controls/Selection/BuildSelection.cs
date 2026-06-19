using Kenedia.Modules.Core.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Kenedia.Modules.Core.Extensions;
using Blish_HUD.Content;
using System.Linq;
using Blish_HUD.Input;
using Blish_HUD;
using Kenedia.Modules.Core.Utility;
using Dropdown = Kenedia.Modules.Core.Controls.Dropdown;
using System;
using System.Threading.Tasks;
using Blish_HUD.Gw2Mumble;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.Core.Services;
using Gw2Sharp.WebApi;
using Kenedia.Modules.BuildsManager.Services;
using System.Collections.Specialized;
using Gw2BuildTemplates;
using Blish_HUD.Controls;
using System.Reflection;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class BuildSelection : BaseSelection
    {
        private readonly ImageButton _addBuildsButton;
        private readonly Dropdown _sortBehavior;
        private double _lastShown;
        private Template? _pendingFocusedTemplate;
        private bool _pendingRename;
        private int _pendingFocusDelayFrames;
        private int _pendingFocusFramesRemaining;

        public BuildSelection(TemplateCollection templates, TemplateTags templateTags, Data data, TemplatePresenter templatePresenter, TemplateFactory templateFactory, Settings settings)
        {
            Data = data;
            Templates = templates;
            TemplateTags = templateTags;
            TemplateFactory = templateFactory;
            Settings = settings;
            TemplatePresenter = templatePresenter;

            _sortBehavior = new()
            {
                Parent = this,
                Location = new(0, 0),
                ValueChangedAction = (s) =>
                {
                    if (_sortBehavior is null) return;

                    Settings.SortBehavior.Value = GetSortBehaviorFromString(s);
                    FilterTemplates();
                },
                SetLocalizedItems = () =>
                {
                    if (_sortBehavior is not null)
                    {
                        _sortBehavior.SelectedItem = GetSortBehaviorString(Settings.SortBehavior.Value);
                    }

                    return
                    [
                        GetSortBehaviorString(TemplateSortBehavior.ByProfession),
                        GetSortBehaviorString(TemplateSortBehavior.ByName),
                        GetSortBehaviorString(TemplateSortBehavior.ByModified),
                    ];
                },
                SelectedItem = GetSortBehaviorString(Settings.SortBehavior.Value),
            };

            Search.Location = new(2, _sortBehavior.Bottom + 5);
            SelectionContent.Location = new(0, Search.Bottom + 5);

            Search.PerformFiltering = (txt) => FilterTemplates();

            int i = 0;
            int size = 25;
            Point start = new(0, 0);

            PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;

            _addBuildsButton = new()
            {
                Parent = this,
                Location = new(0, 30),
                Texture = AsyncTexture2D.FromAssetId(155902),
                DisabledTexture = AsyncTexture2D.FromAssetId(155903),
                HoveredTexture = AsyncTexture2D.FromAssetId(155904),
                TextureRectangle = new(2, 2, 28, 28),
                SetLocalizedTooltip = () => strings.AddNewTemplateWithClipboard,
                ClickAction = (m) =>
                {
                    AddNewTemplate();
                },
            };

            Search.TextChangedAction = (txt) => _addBuildsButton.BasicTooltipText = string.IsNullOrEmpty(txt) ? strings.CreateNewTemplate : string.Format(strings.CreateNewTemplateName, txt);

            LocalizingService.LocaleChanged += LocalizingService_LocaleChanged;

            Templates.CollectionChanged += Templates_CollectionChanged;
            Templates.TemplateChanged += Templates_TemplateChanged;

            Templates.Loaded += Templates_Loaded;
            if (Templates.IsLoaded)
            {
                AddTemplateSelectable(true, [.. Templates]);
            }
        }

        private void Templates_Loaded(object sender, EventArgs e)
        {
            TemplateSelectables?.DisposeAll();
            TemplateSelectables?.Clear();

            AddTemplateSelectable(true, [.. Templates]);
        }

        private void AddNewTemplate()
        {
            _ = Task.Run(async () =>
            {
                string? code = null;

                try
                {
                    code = await ClipboardUtil.WindowsClipboardService.GetTextAsync();
                }
                catch (Exception ex)
                {
                    BuildsManager.Logger.Warn(ex, "Failed to read clipboard while creating a template.");
                }

                string? trimmedCode = string.IsNullOrWhiteSpace(code) ? null : code.Trim();
                bool hasClipboardCode = !string.IsNullOrEmpty(trimmedCode);
                bool hasValidBuildCode = hasClipboardCode && Gw2BuildCodec.TryDecode(trimmedCode, out _);

                GameService.Graphics.QueueMainThreadRender((graphicsDevice) =>
                {
                    string name = string.IsNullOrEmpty(Search.Text) ? strings.NewTemplate : Search.Text;
                    var t = CreateTemplate(name);

                    if (hasValidBuildCode)
                    {
                        try
                        {
                            BuildsManager.Logger.Debug($"Load template from clipboard code: {trimmedCode}");
                            t.LoadFromCode(trimmedCode);
                        }
                        catch (Exception ex)
                        {
                            BuildsManager.Logger.Warn(ex, $"Failed to load clipboard build code '{trimmedCode}'.");
                            ScreenNotification.ShowNotification("Clipboard build code could not be loaded. Created blank template instead.");
                        }
                    }

                    if (Settings.SetFilterOnTemplateCreate.Value)
                    {
                        Search.Text = t.Name;
                        Search.ForceFilter();
                    }
                    else if (Settings.ResetFilterOnTemplateCreate.Value)
                    {
                        Search.Text = null;
                        Search.ForceFilter();
                    }

                    QueueFocusTemplate(t, true);
                });
            });
        }

        private void Templates_TemplateChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RefreshTemplateSelection(sender as Template);
        }

        private TemplateSortBehavior GetSortBehaviorFromString(string s)
        {
            return s == strings.SortyByProfession ? TemplateSortBehavior.ByProfession
                : s == strings.SortByName ? TemplateSortBehavior.ByName
                : s == strings.SortByModified ? TemplateSortBehavior.ByModified
                : TemplateSortBehavior.ByProfession;
        }

        private string GetSortBehaviorString(TemplateSortBehavior templateSortBehavior)
        {
            return templateSortBehavior switch
            {
                TemplateSortBehavior.ByProfession => strings.SortyByProfession,
                TemplateSortBehavior.ByName => strings.SortByName,
                TemplateSortBehavior.ByModified => strings.SortByModified,
                _ => string.Empty,
            };
        }

        public List<TemplateSelectable> TemplateSelectables { get; } = [];

        public SelectionPanel SelectionPanel { get; set; }

        public TemplateCollection Templates { get; }

        public TemplateTags TemplateTags { get; }

        public Data Data { get; }

        public TemplatePresenter TemplatePresenter { get; }

        public TemplateFactory TemplateFactory { get; }

        public Settings Settings { get; }

        public List<KeyValuePair<string, List<Func<Template, bool>>>> FilterQueries { get; } = [];

        public List<Func<Template, bool>> SpecializationFilterQueries { get; } = [];

        private void SortBehavior_ValueChanged(object sender, Blish_HUD.Controls.ValueChangedEventArgs e)
        {
            FilterTemplates();
        }

        private void LocalizingService_LocaleChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            _sortBehavior.Items[0] = strings.SortyByProfession;
            _sortBehavior.Items[1] = strings.SortByName;
        }

        public void FilterTemplates()
        {
            try
            {
                string lowerTxt = Search.Text?.Trim().ToLower();
                bool anyName = string.IsNullOrEmpty(lowerTxt);

                foreach (var template in TemplateSelectables)
                {
                    bool filterQueriesMatches = FilterQueries.Count == 0 || FilterQueries.All(x => x.Value.Count == 0 || x.Value.Any(x => x(template.Template)));
                    bool specMatches = SpecializationFilterQueries.Count == 0 || SpecializationFilterQueries.Any(x => x(template.Template));
                    bool nameMatches = anyName || template.Template.Name.ToLower().Contains(lowerTxt);
                    bool lastModifiedMatch = template.Template.LastModified.ToLower().Contains(lowerTxt);

                    template.Visible = filterQueriesMatches && specMatches && (nameMatches || lastModifiedMatch);
                }

                SortTemplates();
                SelectionContent.Invalidate();

                var current = TemplateSelectables.FirstOrDefault(x => x.Template == TemplatePresenter.Template);

                if ((current?.Visible is not true && Settings.RequireVisibleTemplate.Value) || current?.Template == Template.Empty)
                {
                    var newTemplate = SelectionContent.OfType<TemplateSelectable>().FirstOrDefault(x => x.Visible) is TemplateSelectable t ? t : null;
                    TemplatePresenter.SetTemplate(newTemplate?.Template);
                }
            }
            catch (Exception ex)
            {
                BuildsManager.Logger.Debug(ex, "Error while filtering templates");
            }
        }

        private void SortTemplates()
        {
            switch (Settings.SortBehavior.Value)
            {
                case TemplateSortBehavior.ByProfession:
                    SelectionContent.SortChildren<TemplateSelectable>((a, b) =>
                    {
                        int prof = a.Template.Profession.CompareTo(b.Template.Profession);
                        int eliteSpec = a.Template.EliteSpecializationId.CompareTo(b.Template.EliteSpecializationId);
                        int name = a.Template.Name.CompareTo(b.Template.Name);

                        if (prof != 0)
                        {
                            return prof;
                        }

                        return eliteSpec != 0 ? eliteSpec : name;
                    });
                    break;

                case TemplateSortBehavior.ByName:
                    SelectionContent.SortChildren<TemplateSelectable>((a, b) => a.Template.Name.CompareTo(b.Template.Name));
                    break;

                case TemplateSortBehavior.ByModified:
                    SelectionContent.SortChildren<TemplateSelectable>((a, b) =>
                    {
                        int lastModified = a.Template.LastModified.CompareTo(b.Template.LastModified);
                        int name = a.Template.Name.CompareTo(b.Template.Name);

                        return lastModified == 0 ? name : lastModified;
                    });

                    break;
            }
        }

        private void Templates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var templates = TemplateSelectables.Select(e => e.Template);

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var removedTemplates = e.OldItems?.OfType<Template>()?.ToList();
                RemoveTemplateSelectable(removedTemplates);

                if (Templates.Count > 0)
                {
                    FilterTemplates();
                    return;
                }
                else
                {
                    CreateTemplate(strings.NewTemplate);
                    return;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                bool firstLoad = TemplateSelectables.Count == 0 && (Templates?.Count ?? 0) != 0;
                var addedTemplates = e.NewItems?.OfType<Template>()?.ToList();

                AddTemplateSelectable(firstLoad, addedTemplates);

                FilterTemplates();
                return;
            }
        }

        private void RemoveTemplateSelectable(List<Template> removedTemplates)
        {
            if (removedTemplates is null || !removedTemplates.Any()) return;

            for (int i = TemplateSelectables.Count - 1; i >= 0; i--)
            {
                var template = TemplateSelectables[i];

                if (removedTemplates.Contains(template.Template))
                {
                    _ = TemplateSelectables.Remove(template);
                    template.Dispose();
                }
            }
        }

        private void AddTemplateSelectable(bool firstLoad, List<Template> addedTemplates)
        {
            if (addedTemplates == null || !addedTemplates.Any()) return;

            foreach (var template in addedTemplates)
            {
                TemplateSelectable t = new(TemplatePresenter, Templates, Data, TemplateTags, TemplateFactory)
                {
                    Parent = SelectionContent,
                    Template = template,
                    Width = SelectionContent.Width - 35,
                };

                t.OnNameChangedAction = () => RefreshTemplateSelection(t.Template);

                template.ProfessionChanged += ProfessionChanged;

                t.OnClickAction = () => SelectionPanel?.SetTemplateAnchor(t);

                TemplateSelectables.Add(t);

                if (!firstLoad)
                {
                    QueueFocusTemplate(t.Template, true);
                }
            }

            if (firstLoad)
            {
                FilterTemplates();
                var tt = GetFirstTemplateSelectable();
                TemplatePresenter.SetTemplate(tt?.Template);
            }
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);
        }

        public Template CreateTemplate(string name)
        {
            string uniqueName = Templates.GetNewName(name);
            var template = TemplateFactory.CreateTemplate(uniqueName);
            Templates.Add(template);

            return template;
        }

        private void QueueFocusTemplate(Template template, bool rename)
        {
            if (template is null) return;

            _pendingFocusedTemplate = template;
            _pendingRename = rename;
            _pendingFocusDelayFrames = 2;
            _pendingFocusFramesRemaining = 30;
        }

        private void RefreshTemplateSelection(Template? template)
        {
            FilterTemplates();

            if (template is not null && template == TemplatePresenter.Template)
            {
                QueueFocusTemplate(template, false);
            }
        }

        private void FocusTemplate(Template template, bool rename)
        {
            if (template is null) return;

            var selectable = TemplateSelectables.FirstOrDefault(e => e.Template == template);
            if (selectable is null) return;

            TemplatePresenter.SetTemplate(template);
            SelectionPanel?.SetTemplateAnchor(selectable);
            BringTemplateIntoView(selectable);

            if (rename)
            {
                selectable.ToggleEditMode(true);
            }
        }

        private static readonly BindingFlags s_instanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private Kenedia.Modules.Core.Controls.Scrollbar? GetSelectionScrollbar()
        {
            return Parent?.Children?.OfType<Kenedia.Modules.Core.Controls.Scrollbar>().FirstOrDefault(s => s.AssociatedContainer == SelectionContent)
                ?? SelectionContent.Parent?.Children?.OfType<Kenedia.Modules.Core.Controls.Scrollbar>().FirstOrDefault(s => s.AssociatedContainer == SelectionContent);
        }

        private object? GetNativeSelectionScrollbar()
        {
            Type? type = SelectionContent.GetType();

            while (type is not null)
            {
                var field = type.GetField("_panelScrollbar", s_instanceFlags);
                if (field?.GetValue(SelectionContent) is object scrollbar)
                {
                    return scrollbar;
                }

                type = type.BaseType;
            }

            return null;
        }

        private void SetSelectionScrollState(int targetOffset, int maxOffset)
        {
            targetOffset = Math.Max(0, Math.Min(targetOffset, maxOffset));
            SelectionContent.VerticalScrollOffset = targetOffset;

            float scrollDistance = maxOffset == 0 ? 0f : Math.Max(0f, Math.Min(targetOffset / (float)maxOffset, 1f));

            if (GetSelectionScrollbar() is Kenedia.Modules.Core.Controls.Scrollbar customScrollbar)
            {
                customScrollbar.ScrollDistance = scrollDistance;
            }

            if (GetNativeSelectionScrollbar() is object nativeScrollbar)
            {
                var nativeType = nativeScrollbar.GetType();
                nativeType.GetProperty("ScrollDistance", s_instanceFlags)?.SetValue(nativeScrollbar, scrollDistance);
                nativeType.GetProperty("TargetScrollDistance", s_instanceFlags)?.SetValue(nativeScrollbar, scrollDistance);
            }
        }

        private bool TryGetTemplateBounds(TemplateSelectable selectable, out int top, out int bottom, out int contentHeight)
        {
            top = 0;
            bottom = 0;
            contentHeight = SelectionContent.ContentRegion.Height;

            if (selectable is null || !selectable.Visible)
            {
                return false;
            }

            int y = SelectionContent.ContentPadding.Top;
            int spacing = (int)SelectionContent.ControlPadding.Y;

            foreach (var child in SelectionContent.Children.OfType<TemplateSelectable>())
            {
                if (!child.Visible)
                {
                    continue;
                }

                if (child == selectable)
                {
                    top = y;
                    bottom = y + child.Height;
                }

                y += child.Height + spacing;
            }

            contentHeight = Math.Max(y + SelectionContent.ContentPadding.Bottom - spacing, SelectionContent.ContentRegion.Height);
            return bottom > top;
        }

        private void BringTemplateIntoView(TemplateSelectable selectable)
        {
            if (!TryGetTemplateBounds(selectable, out int childTop, out int childBottom, out int contentHeight))
            {
                return;
            }

            int maxOffset = Math.Max(contentHeight - SelectionContent.ContentRegion.Height, 0);
            if (maxOffset == 0)
            {
                SetSelectionScrollState(0, 0);
                return;
            }

            int margin = 10;
            int currentOffset = SelectionContent.VerticalScrollOffset;
            int viewportTop = currentOffset;
            int viewportBottom = currentOffset + SelectionContent.ContentRegion.Height;
            int targetOffset = currentOffset;

            if (childTop < viewportTop + margin)
            {
                targetOffset = Math.Max(childTop - margin, 0);
            }
            else if (childBottom > viewportBottom - margin)
            {
                targetOffset = Math.Max(childBottom - SelectionContent.ContentRegion.Height + margin, 0);
            }

            targetOffset = Math.Max(0, Math.Min(targetOffset, maxOffset));
            SetSelectionScrollState(targetOffset, maxOffset);
        }

        private bool IsTemplateInView(TemplateSelectable selectable)
        {
            if (!TryGetTemplateBounds(selectable, out int childTop, out int childBottom, out _))
            {
                return false;
            }

            int margin = 10;
            int viewportTop = SelectionContent.VerticalScrollOffset;
            int viewportBottom = viewportTop + SelectionContent.ContentRegion.Height;

            return childTop >= viewportTop + margin
                && childBottom <= viewportBottom - margin;
        }

        private void SpecializationChanged(object sender, Core.Models.DictionaryItemChangedEventArgs<Models.Templates.SpecializationSlotType, DataModels.Professions.Specialization> e)
        {
            RefreshTemplateSelection(sender as Template);
        }

        private void ProfessionChanged(object sender, Core.Models.ValueChangedEventArgs<Gw2Sharp.Models.ProfessionType> e)
        {
            RefreshTemplateSelection(sender as Template);
        }

        public TemplateSelectable? GetFirstTemplateSelectable()
        {
            FilterTemplates();
            return SelectionContent.GetChildrenOfType<TemplateSelectable>().FirstOrDefault(e => e.Visible);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            Search?.SetSize(Width - Search.Left - Search.Height - 2);

            _addBuildsButton?.SetLocation(Search.Right, Search.Top);
            _addBuildsButton?.SetSize(Search.Height, Search.Height);

            _sortBehavior?.SetLocation(Search.Left);
            _sortBehavior?.SetSize((_addBuildsButton?.Right ?? 0) - Search.Left);
        }

        protected override void OnSelectionContent_Resized(object sender, Blish_HUD.Controls.ResizedEventArgs e)
        {
            base.OnSelectionContent_Resized(sender, e);

            foreach (var template in TemplateSelectables)
            {
                template.Width = SelectionContent.Width - 35;
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            if (Common.Now - _lastShown >= 250)
            {
                base.OnClick(e);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            _lastShown = Common.Now;
        }

        protected override void OnHidden(EventArgs e)
        {
            base.OnHidden(e);

            _sortBehavior.Enabled = false;
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (!_sortBehavior.Enabled)
            {
                _sortBehavior.Enabled = _sortBehavior.Enabled || Common.Now - _lastShown >= 5;
            }

            if (_pendingFocusedTemplate is not null)
            {
                var pendingTemplate = _pendingFocusedTemplate;
                var pendingRename = _pendingRename;

                if (_pendingFocusDelayFrames > 0)
                {
                    _pendingFocusDelayFrames--;
                }
                else if (TemplateSelectables.FirstOrDefault(e => e.Template == pendingTemplate) is TemplateSelectable selectable
                    && selectable.Visible
                    && selectable.Parent == SelectionContent
                    && selectable.Height > 0)
                {
                    FocusTemplate(pendingTemplate, pendingRename);
                    _pendingRename = false;
                    _pendingFocusFramesRemaining--;

                    if (IsTemplateInView(selectable) || _pendingFocusFramesRemaining <= 0)
                    {
                        _pendingFocusedTemplate = null;
                        _pendingRename = false;
                    }
                }
                else if (_pendingFocusFramesRemaining <= 0)
                {
                    _pendingFocusedTemplate = null;
                    _pendingRename = false;
                }
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            _sortBehavior?.Dispose();

            Templates.CollectionChanged -= Templates_CollectionChanged;

            LocalizingService.LocaleChanged -= LocalizingService_LocaleChanged;

            TemplateSelectables.Clear();
        }
    }
}
