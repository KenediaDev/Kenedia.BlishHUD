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
using System.Diagnostics;
using System.Collections.Specialized;
using Kenedia.Modules.BuildsManager.Views;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class BuildSelection : BaseSelection
    {
        private readonly ImageButton _addBuildsButton;
        private readonly Dropdown _sortBehavior;
        private double _lastShown;

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
            AddTemplateSelectable(true, [.. Templates]);
        }

        private void AddNewTemplate()
        {
            //Get Clipboard text async then create a new template on main thread

            _ = Task.Run(async () =>
            {
                string code = await ClipboardUtil.WindowsClipboardService.GetTextAsync();

                GameService.Graphics.QueueMainThreadRender((graphicsDevice) =>
                {
                    string name = string.IsNullOrEmpty(Search.Text) ? strings.NewTemplate : Search.Text;
                    var t = CreateTemplate(Templates.GetNewName(name));

                    if (!string.IsNullOrEmpty(code))
                    {
                        try
                        {
                            BuildsManager.Logger.Debug($"Load template from clipboard code: {code}");
                            t.LoadFromCode(code);
                        }
                        catch (Exception)
                        {

                        }
                    }


                    TemplateSelectable ts = null;
                    SelectionPanel?.SetTemplateAnchor(ts = TemplateSelectables.FirstOrDefault(e => e.Template == t));
                    ts?.ToggleEditMode(true);

                    if (Settings.SetFilterOnTemplateCreate.Value)
                    {
                        Search.Text = t.Name;
                    }
                    else if (Settings.ResetFilterOnTemplateCreate.Value)
                    {
                        Search.Text = null;
                    }
                });
            });
        }

        private void Templates_TemplateChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            FilterTemplates();
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

            var current = TemplateSelectables.FirstOrDefault(x => x.Template == TemplatePresenter.Template);

            if ((current?.Visible is not true && Settings.RequireVisibleTemplate.Value) || current?.Template == Template.Empty)
            {
                var newTemplate = SelectionContent.OfType<TemplateSelectable>().FirstOrDefault(x => x.Visible) is TemplateSelectable t ? t : null;
                TemplatePresenter.SetTemplate(newTemplate?.Template);
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
                        int name = a.Template.Name.CompareTo(b.Template.Name);

                        return prof == 0 ? name : prof;
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
                    OnNameChangedAction = FilterTemplates,
                };

                template.ProfessionChanged += ProfessionChanged;

                t.OnClickAction = () => SelectionPanel?.SetTemplateAnchor(t);

                if (!firstLoad)
                {
                    SelectionPanel?.SetTemplateAnchor(t);
                    TemplatePresenter.SetTemplate(t.Template);
                    t.ToggleEditMode(true);
                }

                TemplateSelectables.Add(t);
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
            for (int i = 0; i < int.MaxValue; i++)
            {
                string newName = i == 0 ? name : $"{name} #{i}";

                if (Templates.Where(e => e.Name == newName)?.FirstOrDefault() is not Template template)
                {
                    TemplateSelectable ts = null;
                    Template t;
                    Templates.Add(t = TemplateFactory.CreateTemplate(name));
                    SelectionPanel?.SetTemplateAnchor(ts = TemplateSelectables.FirstOrDefault(e => e.Template == t));
                    ts?.ToggleEditMode(false);

                    TemplatePresenter.SetTemplate(t);
                    t.ProfessionChanged += ProfessionChanged;

                    if (ts is not null)
                    {
                        ts.DisposeAction = () =>
                        {
                            t.ProfessionChanged -= ProfessionChanged;
                        };
                    }

                    return t;
                }
            }

            return null;
        }

        private void SpecializationChanged(object sender, Core.Models.DictionaryItemChangedEventArgs<Models.Templates.SpecializationSlotType, DataModels.Professions.Specialization> e)
        {
            FilterTemplates();
        }

        private void ProfessionChanged(object sender, Core.Models.ValueChangedEventArgs<Gw2Sharp.Models.ProfessionType> e)
        {
            FilterTemplates();
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
