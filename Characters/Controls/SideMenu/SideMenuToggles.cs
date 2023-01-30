using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Characters.Res;
using Kenedia.Modules.Characters.Enums;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Characters.Views;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Interfaces;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;

namespace Kenedia.Modules.Characters.Controls.SideMenu
{
    public class SideMenuToggles : FlowTab, ILocalizable
    {
        private List<Tag> _tags = new();
        private readonly FlowPanel _toggleFlowPanel;
        private readonly FlowPanel _tagFlowPanel;
        private readonly List<KeyValuePair<ImageColorToggle, Action>> _toggles = new();
        private Rectangle _contentRectangle;

        public event EventHandler TogglesChanged;

        public SideMenuToggles()
        {
            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            AutoSizePadding = new Point(5, 5);
            HeightSizingMode = SizingMode.AutoSize;
            OuterControlPadding = new Vector2(5, 5);
            ControlPadding = new Vector2(5, 3);
            Location = new Point(0, 25);

            _toggleFlowPanel = new()
            {
                Parent = this,
                FlowDirection = ControlFlowDirection.TopToBottom,
                ControlPadding = new Vector2(5, 3),
                Height = 286,
                Width = Width,
            };

            _tagFlowPanel = new()
            {
                Parent = this,
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(5, 3),
                Width = Width,
            };

            CreateToggles();
            CreateTags();

            _ = Task.Run(async () => { await Task.Delay(250); CalculateTagPanelSize(); });
            GameService.Overlay.UserLocale.SettingChanged += OnLanguageChanged;
            Characters.ModuleInstance.Tags.CollectionChanged += Tags_CollectionChanged;
            OnLanguageChanged();
        }

        public void ResetToggles()
        {
            _tags.ForEach(t => t.SetActive(false));
            _toggles.ForEach(t => t.Key.Active = false);

            foreach (KeyValuePair<string, SearchFilter<Character_Model>> t in SearchFilters)
            {
                t.Value.IsEnabled = false;
            }

            foreach (KeyValuePair<string, SearchFilter<Character_Model>> t in TagFilters)
            {
                t.Value.IsEnabled = false;
            }
        }

        private void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CreateTags();
            CalculateTagPanelSize();
        }

        private void CalculateTagPanelSize()
        {
            if (Visible)
            {
                int width = _tagFlowPanel.Width - (int)(_tagFlowPanel.OuterControlPadding.X * 2);
                int? height = null;

                int curWidth = 0;
                foreach (Tag tag in _tags)
                {
                    height ??= tag.Height + (int)_tagFlowPanel.ControlPadding.Y + (int)(_tagFlowPanel.OuterControlPadding.Y * 2);

                    int newWidth = curWidth + tag.Width + (int)_tagFlowPanel.ControlPadding.X;

                    if (newWidth >= width)
                    {
                        height += tag.Height + (int)_tagFlowPanel.ControlPadding.Y;
                        curWidth = 0;
                    }

                    curWidth += tag.Width + (int)_tagFlowPanel.ControlPadding.X;
                }

                _tagFlowPanel.Height = (height ?? 0) + (int)(_tagFlowPanel.OuterControlPadding.Y * 2);
            }
        }

        private void CreateTags()
        {
            _tags.ForEach(t => t.ActiveChanged -= Tag_ActiveChanged);
            _tags.DisposeAll();
            _tags.Clear();

            _tagFlowPanel.Children.Clear();
            TagFilters.Clear();

            foreach (string tag in Characters.ModuleInstance.Tags)
            {
                if (!TagFilters.ContainsKey(tag))
                {
                    Tag t;
                    _tags.Add(t = new Tag()
                    {
                        Parent = _tagFlowPanel,
                        Text = tag,
                        ShowDelete = false,
                    });

                    TagFilters.Add(tag, new((c) => c.Tags.Contains(tag), false));

                    t.SetActive(false);
                    t.ActiveChanged += Tag_ActiveChanged;
                }
            }
        }

        private void Tag_ActiveChanged(object sender, EventArgs e)
        {
            var t = (Tag)sender;
            TagFilters[t.Text].IsEnabled = t.Active;
            MainWindow?.PerformFiltering();
        }

        private Dictionary<string, SearchFilter<Character_Model>> TagFilters => Characters.ModuleInstance.TagFilters;

        private TextureManager TM => Characters.ModuleInstance.TextureManager;

        private Dictionary<string, SearchFilter<Character_Model>> SearchFilters => Characters.ModuleInstance.SearchFilters;

        private void CreateToggles()
        {
            void action(bool active, string entry)
            {
                SearchFilters[entry].IsEnabled = active;
                MainWindow?.PerformFiltering();
            }

            var profs = Characters.ModuleInstance.Data.Professions.ToDictionary(entry => entry.Key, entry => entry.Value);
            profs = profs.OrderBy(e => e.Value.WeightClass).ThenBy(e => e.Value.APIId).ToDictionary(e => e.Key, e => e.Value);

            // Profession All Specs
            foreach (KeyValuePair<Gw2Sharp.Models.ProfessionType, Data.Profession> profession in profs)
            {
                var t = new ImageColorToggle((b) => action(b, $"Core {profession.Value.Name}"))
                {
                    Texture = profession.Value.IconBig,
                    UseGrayScale = false,
                    ColorActive = profession.Value.Color,
                    ColorHovered = profession.Value.Color,
                    ColorInActive = profession.Value.Color * 0.5f,
                    Active = SearchFilters[$"Core {profession.Value.Name}"].IsEnabled,
                    BasicTooltipText = $"Core {profession.Value.Name}",
                    Alpha = 0.7f,
                };

                _toggles.Add(new(t, () => t.BasicTooltipText = $"Core {profession.Value.Name}"));
            }

            foreach (KeyValuePair<Gw2Sharp.Models.ProfessionType, Data.Profession> profession in profs)
            {
                var t = new ImageColorToggle((b) => action(b, profession.Value.Name))
                {
                    Texture = profession.Value.IconBig,
                    Active = SearchFilters[profession.Value.Name].IsEnabled,
                    BasicTooltipText = profession.Value.Name,
                };
                _toggles.Add(new(t, () => t.BasicTooltipText = profession.Value.Name));
            }

            List<KeyValuePair<ImageColorToggle, Action>> specToggles = new();
            foreach (KeyValuePair<SpecializationType, Data.Specialization> specialization in Characters.ModuleInstance.Data.Specializations)
            {
                var t = new ImageColorToggle((b) => action(b, specialization.Value.Name))
                {
                    Texture = specialization.Value.IconBig,
                    Profession = specialization.Value.Profession,
                    Active = SearchFilters[specialization.Value.Name].IsEnabled,
                    BasicTooltipText = specialization.Value.Name,
                };
                specToggles.Add(new(t, () => t.BasicTooltipText = specialization.Value.Name));
            }

            for (int i = 0; i < 3; i++)
            {
                foreach (KeyValuePair<Gw2Sharp.Models.ProfessionType, Data.Profession> p in profs)
                {
                    KeyValuePair<ImageColorToggle, Action> t = specToggles.Find(e => p.Key == e.Key.Profession && !_toggles.Contains(e));
                    if (t.Key != null)
                    {
                        _toggles.Add(t);
                    }
                }
            }

            // Crafting Professions
            foreach (KeyValuePair<int, Data.CrafingProfession> crafting in Characters.ModuleInstance.Data.CrafingProfessions)
            {

                if (crafting.Key > 0)
                {
                    ImageColorToggle img = new((b) => action(b, crafting.Value.Name))
                    {
                        Texture = crafting.Value.Icon,
                        UseGrayScale = false,
                        TextureRectangle = crafting.Key > 0 ? new Rectangle(8, 7, 17, 19) : new Rectangle(4, 4, 24, 24),
                        SizeRectangle = new Rectangle(4, 4, 20, 20),
                        Active = SearchFilters[crafting.Value.Name].IsEnabled,
                        BasicTooltipText = crafting.Value.Name,
                    };
                    _toggles.Add(new(img, () => img.BasicTooltipText = crafting.Value.Name));
                }
            }

            var hidden = new ImageColorToggle((b) => action(b, "Hidden"))
            {
                Texture = AsyncTexture2D.FromAssetId(605021),
                UseGrayScale = true,
                TextureRectangle = new Rectangle(4, 4, 24, 24),
                BasicTooltipText = strings.ShowHidden_Tooltip,
            };
            _toggles.Add(new(hidden, () => hidden.BasicTooltipText = strings.ShowHidden_Tooltip));

            var birthday = new ImageColorToggle((b) => action(b, "Birthday"))
            {
                Texture = AsyncTexture2D.FromAssetId(593864),
                UseGrayScale = true,
                TextureRectangle = new Rectangle(1, 0, 30, 32),
                BasicTooltipText = strings.Show_Birthday_Tooltip,
            };
            _toggles.Add(new(birthday, () => birthday.BasicTooltipText = strings.Show_Birthday_Tooltip));

            foreach (KeyValuePair<Gw2Sharp.Models.RaceType, Data.Race> race in Characters.ModuleInstance.Data.Races)
            {
                var t = new ImageColorToggle((b) => action(b, race.Value.Name))
                {
                    Texture = race.Value.Icon,
                    UseGrayScale = true,
                    BasicTooltipText = race.Value.Name 
                };

                _toggles.Add(new(t, () => t.BasicTooltipText = race.Value.Name));
            }

            var male = new ImageColorToggle((b) => action(b, "Male"))
            {
                Texture = TM.GetIcon(TextureManager.Icons.Male),
                UseGrayScale = true,
                TextureRectangle = new Rectangle(1, 0, 30, 32),
                BasicTooltipText = strings.Show_Birthday_Tooltip,
            };
            _toggles.Add(new(male, () => male.BasicTooltipText = strings.Male));

            var female = new ImageColorToggle((b) => action(b, "Female"))
            {
                Texture = TM.GetIcon(TextureManager.Icons.Female),
                UseGrayScale = true,
                TextureRectangle = new Rectangle(1, 0, 30, 32),
                BasicTooltipText = strings.Show_Birthday_Tooltip,
            };
            _toggles.Add(new(female, () => female.BasicTooltipText = strings.Female));

            foreach (KeyValuePair<ImageColorToggle, Action> t in _toggles)
            {
                t.Key.Parent = _toggleFlowPanel;
                t.Key.Size = new Point(29, 29);
            }
        }

        private MainWindow MainWindow => Characters.ModuleInstance.MainWindow;

        public void OnLanguageChanged(object s = null, EventArgs e = null)
        {
            _toggles.ForEach(t => t.Value.Invoke());
        }

        public void OnTogglesChanged(object s = null, EventArgs e = null)
        {
            TogglesChanged?.Invoke(this, e);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            GameService.Overlay.UserLocale.SettingChanged -= OnLanguageChanged;
            Characters.ModuleInstance.Tags.CollectionChanged -= Tags_CollectionChanged;
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            _contentRectangle = new Rectangle((int)OuterControlPadding.X, (int)OuterControlPadding.Y, Width - ((int)OuterControlPadding.X * 2), Height - ((int)OuterControlPadding.Y * 2));
            _toggleFlowPanel.Width = _contentRectangle.Width;

            _tagFlowPanel.Width = _contentRectangle.Width;
            CalculateTagPanelSize();
        }
    }
}
