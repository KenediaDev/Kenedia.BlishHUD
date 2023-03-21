using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Kenedia.Modules.Core.Extensions;
using Blish_HUD.Content;
using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Blish_HUD.Input;
using Blish_HUD;
using Kenedia.Modules.Core.Utility;
using Dropdown = Kenedia.Modules.Core.Controls.Dropdown;
using System;
using System.Threading.Tasks;
using Blish_HUD.Gw2Mumble;
using static System.Net.Mime.MediaTypeNames;
using Gw2Sharp.Models;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class ProfessionToggle : ImageToggle
    {
        public ProfessionType Profession { get; set; }
    }

    public class BuildSelection : BaseSelection
    {
        private readonly List<ProfessionToggle> _specIcons = new();
        private readonly ImageButton _addBuildsButton;
        private readonly List<TemplateSelectable> _templates = new();

        private readonly Dropdown _sortBehavior;
        private double _lastShown;

        public BuildSelection()
        {
            _sortBehavior = new()
            {
                Parent = this,
                Location = new(0, 30),
            };
            _sortBehavior.Items.Add("Sort by Profession");
            _sortBehavior.Items.Add("Sort by Name");

            Search.Location = new(2, 60);
            SelectionContent.Location = new(0, Search.Bottom + 5);

            Search.FilteringOnTextChange = true;
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
                    Location = new(start.X + (i * (size + 10)), start.Y + (j * size)),
                    Size = new(size, size),               
                    ImageColor = Color.Gray * 0.5F,
                    ActiveColor = Color.White,
                    Profession = prof.Id,
                    OnCheckChanged = (isChecked) => FilterTemplates(),
                    Checked = prof.Id == player?.Profession,
                });

                //foreach (var spec in prof.Specializations.Values)
                //{                    
                //    if (spec.Elite)
                //    {
                //        j++;
                //        _specIcons.Add(new DetailedTexture(spec.ProfessionIconBig.Texture.ToGrayScaledPalettable())
                //        {
                //            Bounds = new(i * 25, j * 25, 25, 25),
                //            DrawColor = ColorExtension.Guardian * 0.7F,
                //            HoverDrawColor = ColorExtension.Guardian,
                //        });
                //    }
                //}

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
                        Template t;
                        string name = string.IsNullOrEmpty(Search.Text) ? "New Template" : Search.Text;
                        if (BuildsManager.ModuleInstance.Templates.Where(e => e.Name == name).Count() == 0)
                        {
                            BuildsManager.ModuleInstance.Templates.Add(t = new() { Name = name });
                            BuildsManager.ModuleInstance.SelectedTemplate = t;
                            Search.Text = null;

                            try
                            {
                                var code = await ClipboardUtil.WindowsClipboardService.GetTextAsync();
                                t.BuildTemplate.LoadFromCode(code);
                            }
                            catch (Exception)
                            {

                            }
                        }
                    });
                },
            };

            BuildsManager.ModuleInstance.SelectedTemplateChanged += ModuleInstance_SelectedTemplateChanged;

            Search.TextChangedAction = (txt) =>
            {
                _addBuildsButton.BasicTooltipText = string.IsNullOrEmpty(txt) ? $"Create a new Template" : $"Create new Template '{txt}'";
            };

            BuildsManager.ModuleInstance.Templates.CollectionChanged += Templates_CollectionChanged;
            Templates_CollectionChanged(this, null);
        }

        private void FilterTemplates()
        {
            string lowerTxt = Search.Text?.Trim().ToLower();
            bool anyName = string.IsNullOrEmpty(lowerTxt);
            bool anyProfession = !_specIcons.Any(e => e.Checked);
            var professions = _specIcons.Where(e => e.Checked).Select(e => e.Profession);

            foreach (var template in _templates)
            {
                template.Visible = 
                    (anyProfession || professions.Contains(template.Template.Profession)) &&
                    (anyName || template.Template.Name.ToLower().Contains(lowerTxt));
            }

            if(_sortBehavior.SelectedItem == "Sort by Profession")
            {
                SelectionContent.SortChildren<TemplateSelectable>((a, b) =>
                {
                    int prof = a.Template.Profession.CompareTo(b.Template.Profession);
                    int name = a.Template.Name.CompareTo(b.Template.Name);

                    return prof == 0 ? prof + name : prof;
                });
            }
            if(_sortBehavior.SelectedItem == "Sort by Name")  SelectionContent.SortChildren<TemplateSelectable>((a,b ) => a.Template.Name.CompareTo(b.Template.Name));

            SelectionContent.Invalidate();
        }

        private void ModuleInstance_SelectedTemplateChanged(object sender, EventArgs e)
        {
            var activeTemplate = _templates.Find(e => e.Template == BuildsManager.ModuleInstance.SelectedTemplate);
            if (activeTemplate != null) SelectionPanel.SetTemplateAnchor(activeTemplate);
        }

        public SelectionPanel SelectionPanel { get; set; }

        private void Templates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var templates = _templates.Select(e => e.Template);
            var removedTemplates = templates.Except(BuildsManager.ModuleInstance.Templates);
            var addedTemplates = BuildsManager.ModuleInstance.Templates.Except(templates);
            bool firstLoad = _templates.Count == 0 && BuildsManager.ModuleInstance.Templates.Count != 0;

            foreach (var template in addedTemplates)
            {
                TemplateSelectable t = new()
                {
                    Parent = SelectionContent,
                    Template = template,
                    Width = SelectionContent.Width - 35,
                    OnNameChangedAction = FilterTemplates,
                };

                t.OnClickAction = () => SelectionPanel?.SetTemplateAnchor(t);
                if(!firstLoad) t.ToggleEditMode(true);
                
                _templates.Add(t);
            }

            for (int i = _templates.Count - 1; i >= 0; i--)
            {
                var template = _templates[i];
                if (removedTemplates.Contains(template.Template))
                {
                    _ = _templates.Remove(template);
                    template.Dispose();
                }
            }

            FilterTemplates();
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            foreach (var specIcon in _specIcons)
            {
                //specIcon.Draw(this, spriteBatch, RelativeMousePosition);
            }
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

            foreach (var template in _templates)
            {
                template.Width = SelectionContent.Width - 35;
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            if (Common.Now() - _lastShown >= 250)
            {
                base.OnClick(e);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            _lastShown = Common.Now();
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
                _sortBehavior.Enabled = _sortBehavior.Enabled || Common.Now() - _lastShown >= 5;
                foreach (var icon in _specIcons)
                {
                    icon.Enabled = _sortBehavior.Enabled || Common.Now() - _lastShown >= 5;
                }
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            _sortBehavior?.Dispose();

            BuildsManager.ModuleInstance.SelectedTemplateChanged -= ModuleInstance_SelectedTemplateChanged;
        }
    }
}
