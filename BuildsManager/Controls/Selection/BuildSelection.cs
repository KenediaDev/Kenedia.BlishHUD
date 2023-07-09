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
using System.ComponentModel;
using Kenedia.Modules.BuildsManager.Controls.GearPage;
using static Blish_HUD.ContentService;
using Gw2Sharp.Models;
using Kenedia.Modules.Core.DataModels;
using System.Diagnostics;
using static Blish_HUD.ArcDps.Common.CommonFields;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class ProfessionRaceSelectable : Panel
    {
        private readonly bool _created;
        private readonly Image _icon;
        private readonly Label _name;

        private Enum _value = ProfessionType.Guardian;
        private ProfessionRaceSelection.SelectionType _selectionType = ProfessionRaceSelection.SelectionType.Profession;

        public ProfessionRaceSelectable()
        {
            HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
            BorderWidth = new(2);
            BorderColor = Color.Black;
            BackgroundColor = Color.Black * 0.4F;
            HoveredBorderColor = Colors.ColonialWhite;
            ContentPadding = new(5);
            ClipInputToBounds = false;

            _name = new()
            {
                Parent = this,
                Font = Content.DefaultFont18,
                TextColor = Color.White,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Middle,
            };

            _icon = new()
            {
                Parent = this,
                Size = new(36),
                Location = new(2, 2),
            };

            _created = true;
            SetValue(this, null);

            Input.Mouse.LeftMouseButtonPressed += Mouse_LeftMouseButtonPressed;
        }

        private void Mouse_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (Hovered && this.IsVisible())
                OnClickAction?.Invoke(Value);
        }

        public Enum Value { get => _value; set => Common.SetProperty(ref _value, value, SetValue); }

        public Action<Enum> OnClickAction { get; set; }

        public ProfessionRaceSelection.SelectionType SelectionType
        {
            get => _selectionType;
            set => Common.SetProperty(ref _selectionType, value, () => Value = null);
        }

        private void SetValue(object sender, PropertyChangedEventArgs e)
        {
            if(Value == null)
            {
                _name.Text = null;
                _icon.Texture = null;
                return;
            }

            switch (SelectionType)
            {
                case ProfessionRaceSelection.SelectionType.Profession:
                    if (BuildsManager.Data.Professions.TryGetValue((ProfessionType)Value, out var profession))
                    {
                        _name.Text = profession.Name;
                        _icon.Texture = profession.IconBig;
                    }
                    break;

                case ProfessionRaceSelection.SelectionType.Race:
                    if (BuildsManager.Data.Races.TryGetValue((Races)Value, out var race))
                    {
                        _name.Text = race.Name;
                        _icon.Texture = race.Icon;
                    }
                    break;
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            if (!_created) return;

            _name.SetLocation(_icon.Right + 10, _icon.Top - 2);
            _name.SetSize(Right - _icon.Right, _icon.Height);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Input.Mouse.LeftMouseButtonPressed -= Mouse_LeftMouseButtonPressed;
        }
    }

    public class ProfessionRaceSelection : BaseSelection
    {
        private SelectionType _type = SelectionType.Race;
        private List<ProfessionRaceSelectable> _races = new();
        private List<ProfessionRaceSelectable> _professions = new();

        public ProfessionRaceSelection()
        {
            ProfessionRaceSelectable ctrl;
            Search.Dispose();

            BackgroundImage = AsyncTexture2D.FromAssetId(155963);
            SelectionContent.Location = Point.Zero;
            SelectionContent.HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill;
            SelectionContent.ShowBorder = false;
            SelectionContent.ContentPadding = new(0);

            HeightSizingMode = Blish_HUD.Controls.SizingMode.Standard;
            WidthSizingMode = Blish_HUD.Controls.SizingMode.Standard;
            Width = 225;

            BorderColor = Color.Black;
            BorderWidth = new(2);
            ContentPadding = new(5);

            foreach(Races race in Enum.GetValues(typeof(Races)))
            {
                _races.Add(ctrl = new()
                {
                    Parent = SelectionContent,
                    SelectionType = SelectionType.Race,
                    Value = race,
                    OnClickAction = (v) => OnClickAction?.Invoke(v),
                });
            }

            foreach(ProfessionType profession in Enum.GetValues(typeof(ProfessionType)))
            {
                _professions.Add(ctrl = new()
                {
                    Parent = SelectionContent,
                    SelectionType = SelectionType.Profession,
                    Value = profession,
                    OnClickAction = (v) => OnClickAction?.Invoke(v),
                });
            }

            OnTypeChanged(this, null);
            Input.Mouse.LeftMouseButtonPressed += Mouse_LeftMouseButtonPressed;
        }

        private void Mouse_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (!Hovered) Hide();
        }

        public enum SelectionType
        {
            None,
            Profession,
            Race,
        }

        public SelectionType Type
        {
            get => _type;
            set => Common.SetProperty(ref _type, value, OnTypeChanged);
        }

        public Action<Enum> OnClickAction { get; set; }

        private void OnTypeChanged(object sender, PropertyChangedEventArgs e)
        {
            SelectionContent.FilterChildren<ProfessionRaceSelectable>(e => e.SelectionType == Type);

            var ctrl = SelectionContent.Children.FirstOrDefault();
            Height = ContentPadding.Vertical + (SelectionContent.Children.Where(e => e.Visible).Count() * (Math.Max(ctrl.Height, 48) + (int)SelectionContent.ControlPadding.Y));

            SelectionContent.Invalidate();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            TextureRectangle = new(25, 25, Width, Height);
        }

        protected override void OnSelectionContent_Resized(object sender, Blish_HUD.Controls.ResizedEventArgs e)
        {
            base.OnSelectionContent_Resized(sender, e);

            foreach (var child in SelectionContent.Children)
            {
               child.Width = SelectionContent.Width;
            }
        }
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
                        Template t;
                        string name = string.IsNullOrEmpty(Search.Text) ? "New Template" : Search.Text;
                        if (BuildsManager.ModuleInstance.Templates.Where(e => e.Name == name).Count() == 0)
                        {
                            BuildsManager.ModuleInstance.Templates.Add(t = new() { Name = name });
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

            Search.TextChangedAction = (txt) => _addBuildsButton.BasicTooltipText = string.IsNullOrEmpty(txt) ? $"Create a new Template" : $"Create new Template '{txt}'";

            BuildsManager.ModuleInstance.Templates.CollectionChanged += Templates_CollectionChanged;
            Templates_CollectionChanged(this, null);

            GameService.Gw2Mumble.PlayerCharacter.SpecializationChanged += PlayerCharacter_SpecializationChanged;
        }

        private void PlayerCharacter_SpecializationChanged(object sender, ValueEventArgs<int> e)
        {
            //_specIcons.ForEach(c => c.Checked = c.Profession == GameService.Gw2Mumble.PlayerCharacter.Profession);
            FilterTemplates();
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

            if (_sortBehavior.SelectedItem == "Sort by Profession")
            {
                SelectionContent.SortChildren<TemplateSelectable>((a, b) =>
                {
                    int prof = a.Template.Profession.CompareTo(b.Template.Profession);
                    int name = a.Template.Name.CompareTo(b.Template.Name);

                    return prof == 0 ? prof + name : prof;
                });
            }
            if (_sortBehavior.SelectedItem == "Sort by Name") SelectionContent.SortChildren<TemplateSelectable>((a, b) => a.Template.Name.CompareTo(b.Template.Name));

            SelectionContent.Invalidate();
        }

        private void ModuleInstance_SelectedTemplateChanged(object sender, EventArgs e)
        {
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
                if (!firstLoad) t.ToggleEditMode(true);

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

        }
    }
}
