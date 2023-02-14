using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Res;
using Kenedia.Modules.Characters.Enums;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Interfaces;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;

namespace Kenedia.Modules.Characters.Controls.SideMenu
{
    public class SideMenuToggles : FlowTab, ILocalizable
    {
        private readonly List<Tag> _tags = new();
        private readonly FlowPanel _toggleFlowPanel;
        private readonly TagFlowPanel _tagFlowPanel;
        private readonly List<KeyValuePair<ImageColorToggle, Action>> _toggles = new();
        private readonly TextureManager _textureManager;
        private readonly SearchFilterCollection _tagFilters;
        private readonly SearchFilterCollection _searchFilters;
        private readonly Action _onFilterChanged;
        private readonly TagList _allTags;
        private readonly Data _data;
        private Rectangle _contentRectangle;

        public event EventHandler TogglesChanged;

        public SideMenuToggles(TextureManager textureManager, SearchFilterCollection tagFilters, SearchFilterCollection searchFilters, Action onFilterChanged, TagList allTags, Data data)
        {
            _textureManager = textureManager;
            _tagFilters = tagFilters;
            _searchFilters = searchFilters;
            _onFilterChanged = onFilterChanged;
            _allTags = allTags;
            _data = data;
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

            GameService.Overlay.UserLocale.SettingChanged += OnLanguageChanged;
            _allTags.CollectionChanged += Tags_CollectionChanged;
            OnLanguageChanged();
        }

        public void ResetToggles()
        {
            _tags.ForEach(t => t.SetActive(false));
            _toggles.ForEach(t => t.Key.Active = false);

            foreach (KeyValuePair<string, SearchFilter<Character_Model>> t in _searchFilters)
            {
                t.Value.IsEnabled = false;
            }

            foreach (KeyValuePair<string, SearchFilter<Character_Model>> t in _tagFilters)
            {
                t.Value.IsEnabled = false;
            }
        }

        private void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CreateTags();
            Invalidate();
        }

        private void CreateTags()
        {
            var tagLlist = _tags.Select(e => e.Text);

            var deleteTags = tagLlist.Except(_allTags);
            var addTags = _allTags.Except(tagLlist);

            bool tagChanged = deleteTags.Any() || addTags.Any();

            if (tagChanged)
            {
                var deleteList = new List<Tag>();
                foreach (string tag in deleteTags)
                {
                    var t = _tags.FirstOrDefault(e => e.Text == tag);
                    if (t != null) deleteList.Add(t);
                }

                foreach (var t in deleteList)
                {
                    t.Dispose();
                    _ = _tags.Remove(t);
                }

                foreach (string tag in addTags)
                {
                    Tag t;
                    _tags.Add(t = new Tag()
                    {
                        Parent = _tagFlowPanel,
                        Text = tag,
                        ShowDelete = true,
                        CanInteract = true,
                    });

                    t.OnDeleteAction = () =>
                    {
                        _ = _tags.Remove(t);
                        _ = _allTags.Remove(t.Text);
                        _tagFlowPanel.Invalidate();
                    };

                    t.OnClickAction = () =>
                    {
                        _tagFilters[t.Text].IsEnabled = t.Active;
                        _onFilterChanged?.Invoke();
                    };

                    _tagFilters.Add(tag, new((c) => c.Tags.Contains(tag), false));
                    t.SetActive(false);
                }

                _tagFlowPanel.FitWidestTag(ContentRegion.Width);
            }
        }

        private void CreateToggles()
        {
            void action(bool active, string entry)
            {
                _searchFilters[entry].IsEnabled = active;
                _onFilterChanged?.Invoke();
            }

            var profs = _data.Professions.ToDictionary(entry => entry.Key, entry => entry.Value);
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
                    Active = _searchFilters[$"Core {profession.Value.Name}"].IsEnabled,
                    BasicTooltipText = $"Core {profession.Value.Name}",
                    Alpha = 0.7f,
                };

                KeyValuePair<ImageColorToggle, Action> tt = new (t, () => t.BasicTooltipText = $"Core {profession.Value.Name}");
                _toggles.Add(tt);
            }

            foreach (KeyValuePair<Gw2Sharp.Models.ProfessionType, Data.Profession> profession in profs)
            {
                var t = new ImageColorToggle((b) => action(b, profession.Value.Name))
                {
                    Texture = profession.Value.IconBig,
                    Active = _searchFilters[profession.Value.Name].IsEnabled,
                    BasicTooltipText = profession.Value.Name,                    
                };
                _toggles.Add(new(t, () => t.BasicTooltipText = profession.Value.Name));
            }

            List<KeyValuePair<ImageColorToggle, Action>> specToggles = new();
            foreach (KeyValuePair<SpecializationType, Data.Specialization> specialization in _data.Specializations)
            {
                var t = new ImageColorToggle((b) => action(b, specialization.Value.Name))
                {
                    Texture = specialization.Value.IconBig,
                    Profession = specialization.Value.Profession,
                    Active = _searchFilters[specialization.Value.Name].IsEnabled,
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
            foreach (KeyValuePair<int, Data.CraftingProfession> crafting in _data.CrafingProfessions)
            {

                if (crafting.Key > 0)
                {
                    ImageColorToggle img = new((b) => action(b, crafting.Value.Name))
                    {
                        Texture = crafting.Value.Icon,
                        UseGrayScale = false,
                        TextureRectangle = crafting.Key > 0 ? new Rectangle(8, 7, 17, 19) : new Rectangle(4, 4, 24, 24),
                        SizeRectangle = new Rectangle(4, 4, 20, 20),
                        Active = _searchFilters[crafting.Value.Name].IsEnabled,
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

            foreach (KeyValuePair<Gw2Sharp.Models.RaceType, Data.Race> race in _data.Races)
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
                Texture = _textureManager.GetIcon(TextureManager.Icons.Male),
                UseGrayScale = true,
                TextureRectangle = new Rectangle(1, 0, 30, 32),
                BasicTooltipText = strings.Show_Birthday_Tooltip,
            };
            _toggles.Add(new(male, () => male.BasicTooltipText = strings.Male));

            var female = new ImageColorToggle((b) => action(b, "Female"))
            {
                Texture = _textureManager.GetIcon(TextureManager.Icons.Female),
                UseGrayScale = true,
                TextureRectangle = new Rectangle(1, 0, 30, 32),
                BasicTooltipText = strings.Show_Birthday_Tooltip,
            };
            _toggles.Add(new(female, () => female.BasicTooltipText = strings.Female));

            int j = 0;
            foreach (KeyValuePair<ImageColorToggle, Action> t in _toggles)
            {
                j++;
                t.Key.Parent = _toggleFlowPanel;
                t.Key.Size = new Point(29, 29);
            }
        }

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
            _allTags.CollectionChanged -= Tags_CollectionChanged;

            _tagFilters.Clear();
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            _contentRectangle = new Rectangle((int)OuterControlPadding.X, (int)OuterControlPadding.Y, Width - ((int)OuterControlPadding.X * 2), Height - ((int)OuterControlPadding.Y * 2));
            _toggleFlowPanel.Width = _contentRectangle.Width;

            _tagFlowPanel.FitWidestTag(_contentRectangle.Width);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            _tagFlowPanel.FitWidestTag(ContentRegion.Width);
            Invalidate();
        }
    }
}
