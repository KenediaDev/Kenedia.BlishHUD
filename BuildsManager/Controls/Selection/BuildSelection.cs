﻿using Kenedia.Modules.Core.Controls;
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
using System.Diagnostics;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class BuildSelection : BaseSelection
    {
        private readonly List<ProfessionToggle> _specIcons = new();
        private readonly ImageButton _addBuildsButton;
        private readonly LoadingSpinner _spinner;
        private readonly Dropdown _sortBehavior;
        private double _lastShown;

        public BuildSelection()
        {
            _spinner = new()
            {
                Parent = this,
                Location = SelectionContent.LocalBounds.Center,
                Size = new(64),
            };

            _sortBehavior = new()
            {
                Parent = this,
                Location = new(0, 30),
            };
            _sortBehavior.Items.Add(strings.SortyByProfession);
            _sortBehavior.Items.Add(strings.SortByName);
            _sortBehavior.ValueChanged += SortBehavior_ValueChanged;

            Search.Location = new(2, 60);
            SelectionContent.Location = new(0, Search.Bottom + 5);

            Search.PerformFiltering = (txt) => FilterTemplates();

            int i = 0;
            int size = 25;
            Point start = new(0, 0);

            PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;

            foreach (var prof in BuildsManager.Data.Professions.Values)
            {
                int j = 0;
                _specIcons.Add(new()
                {
                    Parent = this,
                    Texture = prof.Icon,
                    Location = new(start.X + (i * (size + 16)), start.Y + (j * size)),
                    Size = new(size, size),
                    ImageColor = Color.Gray * 0.5F,
                    ActiveColor = Color.White,
                    Profession = prof.Id,
                    OnCheckChanged = (isChecked) => FilterTemplates(),
                    Checked = false,
                    //Checked = prof.Id == player?.Profession,
                    TextureRectangle = new(4, 4, 24, 24),
                });

                i++;
            }

            _addBuildsButton = new()
            {
                Parent = this,
                Location = new(0, 30),
                Texture = AsyncTexture2D.FromAssetId(155902),
                DisabledTexture = AsyncTexture2D.FromAssetId(155903),
                HoveredTexture = AsyncTexture2D.FromAssetId(155904),
                TextureRectangle = new(2, 2, 28, 28),
                ClickAction = (m) =>
                {
                    _ = Task.Run(async () =>
                    {
                        string name = string.IsNullOrEmpty(Search.Text) ? strings.NewTemplate : Search.Text;
                        var t = CreateTemplate(name);

                        if (t is not null)
                        {
                            Search.Text = null;

                            try
                            {
                                string code = await ClipboardUtil.WindowsClipboardService.GetTextAsync();
                                t.LoadFromCode(code);
                                BuildsManager.ModuleInstance.MainWindow.Template = t;
                            }
                            catch (Exception)
                            {

                            }

                            TemplateSelectable ts = null;
                            SelectionPanel?.SetTemplateAnchor(ts = Templates.FirstOrDefault(e => e.Template == t));
                            ts?.ToggleEditMode(true);
                            FilterTemplates();
                        }
                    });
                },
            };

            Search.TextChangedAction = (txt) => _addBuildsButton.BasicTooltipText = string.IsNullOrEmpty(txt) ? strings.CreateNewTemplate : string.Format(strings.CreateNewTemplateName, txt);

            BuildsManager.ModuleInstance.TemplatesLoadedDone += ModuleInstance_TemplatesLoadedDone; ;
            BuildsManager.ModuleInstance.Templates.CollectionChanged += Templates_CollectionChanged;

            LocalizingService.LocaleChanged += LocalizingService_LocaleChanged;

            Templates_CollectionChanged(this, null);
        }

        public List<TemplateSelectable> Templates { get; } = new();

        public SelectionPanel SelectionPanel { get; set; }

        private void SortBehavior_ValueChanged(object sender, Blish_HUD.Controls.ValueChangedEventArgs e)
        {
            FilterTemplates();
        }

        private void LocalizingService_LocaleChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            _sortBehavior.Items[0] = strings.SortyByProfession;
            _sortBehavior.Items[1] = strings.SortByName;
        }

        private void ModuleInstance_TemplatesLoadedDone(object sender, Core.Models.ValueChangedEventArgs<bool> e)
        {
            Templates_CollectionChanged(sender, null);
        }

        public void SetTogglesToPlayerProfession()
        {
            _specIcons.ForEach(c => c.Checked = c.Profession == GameService.Gw2Mumble.PlayerCharacter.Profession);
            FilterTemplates();
        }

        private void FilterTemplates()
        {
            string lowerTxt = Search.Text?.Trim().ToLower();
            bool anyName = string.IsNullOrEmpty(lowerTxt);
            bool anyProfession = !_specIcons.Any(e => e.Checked);
            var professions = _specIcons.Where(e => e.Checked).Select(e => e.Profession);

            foreach (var template in Templates)
            {
                template.Visible =
                    (anyProfession || professions.Contains(template.Template.Profession)) &&
                    (anyName || template.Template.Name.ToLower().Contains(lowerTxt));
            }

            if (_sortBehavior.SelectedItem == strings.SortyByProfession)
            {
                SelectionContent.SortChildren<TemplateSelectable>((a, b) =>
                {
                    int prof = a.Template.Profession.CompareTo(b.Template.Profession);
                    int name = a.Template.Name.CompareTo(b.Template.Name);

                    return prof == 0 ? prof + name : prof;
                });
            }
            if (_sortBehavior.SelectedItem == strings.SortByName) SelectionContent.SortChildren<TemplateSelectable>((a, b) => a.Template.Name.CompareTo(b.Template.Name));

            //SelectionContent.Invalidate();
        }

        private void ModuleInstance_SelectedTemplateChanged(object sender, EventArgs e)
        {
        }

        private void Templates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!BuildsManager.ModuleInstance?.TemplatesLoaded == true)
            {
                if (!_spinner.Visible)
                {
                    _spinner.Show();
                    _spinner.Location = SelectionPanel.LocalBounds.Center.Add(_spinner.Size.Scale(-0.5));
                    Templates.DisposeAll();
                    Templates.Clear();
                }

                return;
            }
            else
            {
                _spinner.Hide();
            }

            bool firstLoad = Templates.Count == 0 && (BuildsManager.ModuleInstance?.Templates?.Count ?? 0) != 0;
            var templates = Templates.Select(e => e.Template);
            var removedTemplates = templates.Except(BuildsManager.ModuleInstance?.Templates ?? new());
            var addedTemplates = BuildsManager.ModuleInstance?.Templates?.Except(templates);
            TemplateSelectable targetTemplate = null;

            if (addedTemplates == null) return;

            foreach (var template in addedTemplates)
            {
                TemplateSelectable t = new()
                {
                    Parent = SelectionContent,
                    Template = template,
                    Width = SelectionContent.Width - 35,
                    OnNameChangedAction = FilterTemplates,
                };

                template.ProfessionChanged += ProfessionChanged;
                template.SpecializationChanged += SpecializationChanged;

                if (t is not null)
                {
                    t.DisposeAction = () =>
                    {
                        template.ProfessionChanged -= ProfessionChanged;
                        template.SpecializationChanged -= SpecializationChanged;
                    };
                }

                t.OnClickAction = () => SelectionPanel?.SetTemplateAnchor(t);
                if (!firstLoad)
                {
                    SelectionPanel?.SetTemplateAnchor(t);
                    t.ToggleEditMode(true);
                    targetTemplate = t;
                }

                Templates.Add(t);
            }

            for (int i = Templates.Count - 1; i >= 0; i--)
            {
                var template = Templates[i];
                if (removedTemplates.Contains(template.Template))
                {
                    _ = Templates.Remove(template);
                    template.Dispose();
                }
            }

            FilterTemplates();
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

                if (BuildsManager.ModuleInstance.Templates.Where(e => e.Name == newName)?.FirstOrDefault() is not Template template)
                {
                    TemplateSelectable ts = null;
                    Template t;
                    BuildsManager.ModuleInstance.Templates.Add(t = new() { Name = newName });
                    SelectionPanel?.SetTemplateAnchor(ts = Templates.FirstOrDefault(e => e.Template == t));
                    ts?.ToggleEditMode(false);

                    t.ProfessionChanged += ProfessionChanged;
                    t.SpecializationChanged += SpecializationChanged;

                    if (ts is not null)
                    {
                        ts.DisposeAction = () =>
                        {
                            t.ProfessionChanged -= ProfessionChanged;
                            t.SpecializationChanged -= SpecializationChanged;
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

        public TemplateSelectable GetFirstTemplateSelectable()
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

            foreach (var template in Templates)
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

            foreach (var icon in _specIcons)
            {
                icon.Enabled = false;
            }
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (!_sortBehavior.Enabled)
            {
                _sortBehavior.Enabled = _sortBehavior.Enabled || Common.Now - _lastShown >= 5;
                foreach (var icon in _specIcons)
                {
                    icon.Enabled = _sortBehavior.Enabled || Common.Now - _lastShown >= 5;
                }
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            _sortBehavior?.Dispose();

            BuildsManager.ModuleInstance.TemplatesLoadedDone -= ModuleInstance_TemplatesLoadedDone; ;
            BuildsManager.ModuleInstance.Templates.CollectionChanged -= Templates_CollectionChanged;
            Templates_CollectionChanged(this, null);

            LocalizingService.LocaleChanged -= LocalizingService_LocaleChanged;

            Templates.Clear();
        }
    }
}
