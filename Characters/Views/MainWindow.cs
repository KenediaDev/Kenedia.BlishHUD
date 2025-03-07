﻿using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Res;
using Kenedia.Modules.Characters.Controls;
using Kenedia.Modules.Characters.Controls.SideMenu;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static Kenedia.Modules.Characters.Services.Settings;
using static Kenedia.Modules.Core.Controls.AnchoredContainer;
using Color = Microsoft.Xna.Framework.Color;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using StandardWindow = Kenedia.Modules.Core.Views.StandardWindow;
using System.Threading.Tasks;
using Kenedia.Modules.Core.Utility;
using System.Collections.ObjectModel;

namespace Kenedia.Modules.Characters.Views
{
    public class MainWindow : StandardWindow
    {
        private readonly Settings _settings;
        private readonly TextureManager _textureManager;
        private readonly ObservableCollection<Character_Model> _rawCharacterModels;

        private readonly SearchFilterCollection _searchFilters;
        private readonly SearchFilterCollection _tagFilters;
        private readonly Action _toggleOCR;
        private readonly Func<Character_Model> _currentCharacter;
        private readonly Data _data;
        private readonly AsyncTexture2D _windowEmblem = AsyncTexture2D.FromAssetId(156015);

        private readonly ImageButton _toggleSideMenuButton;
        private readonly CollapseContainer _collapseWrapper;
        private readonly ImageButton _displaySettingsButton;
        private readonly ImageButton _randomButton;
        private readonly ImageButton _lastButton;
        private readonly ImageButton _clearButton;
        private NotificationPanel _notifications;
        private readonly FlowPanel _dropdownPanel;
        private readonly FlowPanel _buttonPanel;
        private readonly FilterBox _filterBox;

        private readonly bool _created;
        private bool _filterCharacters;
        private double _filterTick = 0;
        private APITimeoutNotification _apiTimeoutNotification;
        private APIPermissionNotification _apiPermissionNotification;
        private TesseractFailedNotification _tesseractFailedNotification;
        private double lastUniform;

        public MainWindow(Texture2D background, Rectangle windowRegion, Rectangle contentRegion,
            Settings settings, TextureManager textureManager, ObservableCollection<Character_Model> characterModels,
            SearchFilterCollection searchFilters, SearchFilterCollection tagFilters, Action toggleOCR, Action togglePotrait,
            Action refreshAPI, Func<string> accountImagePath, TagList tags, Func<Character_Model> currentCharacter, Data data, CharacterSorting characterSorting)
            : base(background, windowRegion, contentRegion)
        {
            _settings = settings;
            _textureManager = textureManager;
            _rawCharacterModels = characterModels;
            _searchFilters = searchFilters;
            _tagFilters = tagFilters;
            _toggleOCR = toggleOCR;
            _currentCharacter = currentCharacter;
            _data = data;
            ContentPanel = new FlowPanel()
            {
                Parent = this,
                Location = new Point(0, 0),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                AutoSizePadding = new(5, 5),
                ControlPadding = new(0, 5),
            };

            DraggingControl.LeftMouseButtonReleased += DraggingControl_LeftMouseButtonReleased;

            _collapseWrapper = new CollapseContainer()
            {
                Parent = ContentPanel,
                WidthSizingMode = SizingMode.Fill,
                SetLocalizedTitle = () => strings.Notifications,
                TitleIcon = AsyncTexture2D.FromAssetId(222246),
                TitleBarHeight = 24,
                Height = 24,
                ContentPadding = new(5),
                MaxSize = new(-1, 350),
                Collapsed = true,
                Visible = false,
            };
            _collapseWrapper.Hidden += CollapseWrapper_Hidden;

            _notifications = new NotificationPanel(_rawCharacterModels)
            {
                Parent = _collapseWrapper,
                WidthSizingMode = SizingMode.Fill,
                MaxSize = new(-1, 310),
            };
            _collapseWrapper.SizeDeterminingChild = _notifications;

            _dropdownPanel = new FlowPanel()
            {
                Parent = ContentPanel,
                Location = new Point(0, 2),
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(5, 0),
            };

            _filterBox = new FilterBox()
            {
                Parent = _dropdownPanel,
                PlaceholderText = strings.Search,
                Width = 100,
                FilteringDelay = _settings.FilterDelay.Value,
                EnterPressedAction = FilterBox_EnterPressed,
                ClickAction = FilterBox_Click,
                PerformFiltering = (t) => PerformFiltering(),
                FilteringOnTextChange = true,
            };
            _settings.FilterDelay.SettingChanged += FilterDelay_SettingChanged;

            _clearButton = new ImageButton()
            {
                Parent = this,
                Texture = AsyncTexture2D.FromAssetId(2175783),
                HoveredTexture = AsyncTexture2D.FromAssetId(2175782),
                ClickedTexture = AsyncTexture2D.FromAssetId(2175784),
                Size = new Point(20, 20),
                SetLocalizedTooltip = () => strings.ClearFilters,
                Visible = false,
                ClickAction = (m) =>
                {
                    _filterBox.Text = null;
                    _filterCharacters = true;
                    SideMenu.ResetToggles();
                }
            };

            _buttonPanel = new FlowPanel()
            {
                Parent = _dropdownPanel,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Padding = new(15),
            };
            _buttonPanel.Resized += ButtonPanel_Resized;

            _lastButton = new()
            {
                Parent = _buttonPanel,
                Texture = AsyncTexture2D.FromAssetId(1078535),
                Size = new Point(25, 25),
                ImageColor = ContentService.Colors.ColonialWhite,
                ColorHovered = Color.White,
                SetLocalizedTooltip = () => strings.LastButton_Tooltip,
                Visible = _settings.ShowLastButton.Value,
                ClickAction = (m) =>
                {
                    if (CharacterModels.Count > 1)
                    {
                        var character = CharacterModels.Aggregate((a, b) => b.LastLogin > a.LastLogin ? a : b);
                        character?.Swap(true);
                    }
                    else if(CharacterModels.Count == 1) 
                    {
                        CharacterModels?.FirstOrDefault()?.Swap(true);
                    }
                }
            };
            _lastButton.Texture.TextureSwapped += Texture_TextureSwapped;

            _randomButton = new()
            {
                Parent = _buttonPanel,
                Size = new Point(25, 25),
                SetLocalizedTooltip = () => strings.RandomButton_Tooltip,
                Texture = _textureManager.GetIcon(TextureManager.Icons.Dice),
                HoveredTexture = _textureManager.GetIcon(TextureManager.Icons.Dice_Hovered),
                Visible = _settings.ShowRandomButton.Value,
                ClickAction = (m) =>
                {
                    var selection = CharactersPanel.Children.Where(e => e.Visible).ToList();
                    int r = RandomService.Rnd.Next(selection.Count);
                    if (selection.Count > r)
                    {
                        var entry = (CharacterCard)selection[r];
                        entry?.Character.Swap();
                    }
                }
            };

            _displaySettingsButton = new()
            {
                Parent = _buttonPanel,
                Texture = AsyncTexture2D.FromAssetId(155052),
                HoveredTexture = AsyncTexture2D.FromAssetId(157110),
                Size = new Point(25, 25),
                SetLocalizedTooltip = () => string.Format(strings.ShowItem, string.Format(strings.ItemSettings, strings.Display)),
                ClickAction = (m) => SettingsWindow?.ToggleWindow(),
            };

            _toggleSideMenuButton = new()
            {
                Parent = _buttonPanel,
                Texture = AsyncTexture2D.FromAssetId(605018),
                Size = new Point(25, 25),
                ImageColor = ContentService.Colors.ColonialWhite,
                ColorHovered = Color.White,
                SetLocalizedTooltip = () => string.Format(strings.Toggle, "Side Menu"),
                ClickAction = (m) => ShowAttached(SideMenu.Visible ? null : SideMenu),
            };

            CharactersPanel = new FlowPanel()
            {
                Parent = ContentPanel,
                Size = Size,
                ControlPadding = new Vector2(2, 4),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                CanScroll = true,
            };

            CharacterEdit = new CharacterEdit(textureManager, togglePotrait, accountImagePath, tags, settings, PerformFiltering)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Anchor = this,
                AnchorPosition = AnchorPos.AutoHorizontal,
                RelativePosition = new(0, 45, 0, 0),
                Visible = false,
                FadeOut = !_settings.PinSideMenus.Value,
            };

            SideMenu = new(toggleOCR, togglePotrait, refreshAPI, textureManager, settings, characterSorting)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Anchor = this,
                AnchorPosition = AnchorPos.AutoHorizontal,
                RelativePosition = new(0, 45, 0, 0),
                Visible = false,
                FadeOut = !_settings.PinSideMenus.Value,
            };

            AttachContainer(CharacterEdit);
            AttachContainer(SideMenu);
            UpdateMissingNotification();
            CheckOCRRegion();

            _created = true;

            _settings.PinSideMenus.SettingChanged += PinSideMenus_SettingChanged;
            _settings.ShowRandomButton.SettingChanged += ShowRandomButton_SettingChanged;
            _settings.ShowLastButton.SettingChanged += ShowLastButton_SettingChanged;
            _settings.IncludeBetaCharacters.SettingChanged += IncludeBetaCharacters_SettingChanged;

            GameService.Gw2Mumble.CurrentMap.MapChanged += CurrentMap_MapChanged;
        }

        private void IncludeBetaCharacters_SettingChanged(object sender, ValueChangedEventArgs<bool> e)
        {
        }

        private void CollapseWrapper_Hidden(object sender, EventArgs e)
        {
            ContentPanel.Invalidate();
        }

        public List<Character_Model> LoadedModels { get; } = new();

        private List<Character_Model> CharacterModels => _rawCharacterModels.ToList();

        private void FilterDelay_SettingChanged(object sender, ValueChangedEventArgs<int> e)
        {
            _filterBox.FilteringDelay = e.NewValue;
        }

        private void CurrentMap_MapChanged(object sender, ValueEventArgs<int> e)
        {
            _currentCharacter?.Invoke()?.UpdateCharacter();
            PerformFiltering();
        }

        private void Texture_TextureSwapped(object sender, ValueChangedEventArgs<Texture2D> e)
        {
            _lastButton.Texture.TextureSwapped -= Texture_TextureSwapped;
            _lastButton.Texture = e.NewValue.ToGrayScaledPalettable();
        }

        private void ShowLastButton_SettingChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            _lastButton.Visible = e.NewValue;
            _buttonPanel.Invalidate();
        }

        public SettingsWindow SettingsWindow { get; set; }

        public SideMenu SideMenu { get; }

        private void PinSideMenus_SettingChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            CharacterEdit.FadeOut = !e.NewValue;
            SideMenu.FadeOut = !e.NewValue;
        }

        private void ShowRandomButton_SettingChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            _randomButton.Visible = e.NewValue;
            _buttonPanel.Invalidate();
        }

        private void ButtonPanel_Resized(object sender, ResizedEventArgs e)
        {
            _filterBox.Width = _dropdownPanel.Width - _buttonPanel.Width - 2;
            _clearButton.Location = new Point(_filterBox.LocalBounds.Right - 25, _filterBox.LocalBounds.Top + 8);
        }

        public List<CharacterCard> CharacterCards { get; } = new();

        public CharacterEdit CharacterEdit { get; set; }

        public DraggingControl DraggingControl { get; set; } = new DraggingControl();

        public FlowPanel CharactersPanel { get; private set; }

        public FlowPanel ContentPanel { get; private set; }

        public void FilterCharacters(object sender = null, EventArgs e = null)
        {
            _filterCharacters = true;
        }

        public void PerformFiltering()
        {
            Regex regex = _settings.FilterAsOne.Value ? new("^(?!\\s*$).+") : new(@"\w+|""[\w\s]*""");
            var strings = regex.Matches(_filterBox.Text.Trim().ToLower()).Cast<Match>().ToList();

            List<string> textStrings = new();

            var stringFilters = new List<KeyValuePair<string, SearchFilter<Character_Model>>>();

            string SearchableString(string s)
            {
                return (_settings.FilterDiacriticsInsensitive.Value ? s.RemoveDiacritics() : s).ToLower();
            }

            foreach (object match in strings)
            {
                string string_text = SearchableString(match.ToString().Replace("\"", ""));

                if (_settings.DisplayToggles.Value["Name"].Check) stringFilters.Add(new("Name", new((c) => SearchableString(c.Name).Contains(string_text), true)));
                if (_settings.DisplayToggles.Value["Profession"].Check) stringFilters.Add(new("Specialization", new((c) => SearchableString(c.SpecializationName).Contains(string_text), true)));
                if (_settings.DisplayToggles.Value["Profession"].Check) stringFilters.Add(new("Profession", new((c) => SearchableString(c.ProfessionName).Contains(string_text), true)));
                if (_settings.DisplayToggles.Value["Level"].Check) stringFilters.Add(new("Level", new((c) => SearchableString(c.Level.ToString()).Contains(string_text), true)));
                if (_settings.DisplayToggles.Value["Race"].Check) stringFilters.Add(new("Race", new((c) => SearchableString(c.RaceName).Contains(string_text), true)));
                if (_settings.DisplayToggles.Value["Map"].Check) stringFilters.Add(new("Map", new((c) => SearchableString(c.MapName).Contains(string_text), true)));
                if (_settings.DisplayToggles.Value["Gender"].Check) stringFilters.Add(new("Gender", new((c) => SearchableString(c.Gender.ToString()).Contains(string_text), true)));
                if (_settings.DisplayToggles.Value["CraftingProfession"].Check)
                {
                    stringFilters.Add(new("CraftingProfession", new((c) =>
                    {
                        foreach (KeyValuePair<int, Data.CraftingProfession> craft in c.CraftingDisciplines)
                        {
                            if (!_settings.DisplayToggles.Value["OnlyMaxCrafting"].Check || craft.Key == craft.Value.MaxRating)
                            {
                                if (SearchableString(craft.Value.Name).Contains(string_text)) return true;
                            }
                        }

                        return false;
                    }, true)));
                }

                if (_settings.DisplayToggles.Value["Tags"].Check)
                {
                    stringFilters.Add(new("Tags", new((c) => { foreach (string tag in c.Tags) { if (SearchableString(tag).Contains(string_text)) return true; } return false; }, true)));
                }
            }

            bool matchAny = _settings.ResultMatchingBehavior.Value == MatchingBehavior.MatchAny;
            bool matchAll = _settings.ResultMatchingBehavior.Value == MatchingBehavior.MatchAll;

            bool include = _settings.ResultFilterBehavior.Value == FilterBehavior.Include;

            var toggleFilters = _searchFilters.Where(e => e.Value.IsEnabled).ToList();
            var tagFilters = _tagFilters.Where(e => e.Value.IsEnabled).ToList();

            bool TagResult(Character_Model c)
            {
                var results = new List<bool>();
                foreach (KeyValuePair<string, SearchFilter<Character_Model>> filter in tagFilters)
                {
                    bool result = filter.Value.CheckForMatch(c);
                    results.Add(result);

                    if (result)
                    {
                        if (matchAny)
                        {
                            return true;
                        }
                    }
                }

                return matchAll && results.Count(e => e == true) == tagFilters.Count;
            }

            bool FilterResult(Character_Model c)
            {
                var results = new List<bool>();
                foreach (KeyValuePair<string, SearchFilter<Character_Model>> filter in toggleFilters)
                {
                    bool result = filter.Value.CheckForMatch(c);
                    results.Add(result);

                    if (result)
                    {
                        if (matchAny)
                        {
                            return true;
                        }
                    }
                }

                return matchAll && results.Count(e => e == true) == toggleFilters.Count;
            }

            bool StringFilterResult(Character_Model c)
            {
                var results = new List<bool>();

                foreach (KeyValuePair<string, SearchFilter<Character_Model>> filter in stringFilters)
                {
                    bool matched = filter.Value.CheckForMatch(c);

                    if (matched)
                    {
                        if (matchAny)
                        {
                            return true;
                        }
                    }

                    if (matched) results.Add(matched);
                }

                return matchAll && results.Count(e => e == true) >= strings.Count;
            }

            foreach ((CharacterCard ctrl, bool toggleResult, bool stringsResult, bool tagsResult, bool showResult, bool betaResult) in from CharacterCard ctrl in CharactersPanel.Children
                                                                                                     let c = ctrl.Character
                                                                                                     where c is not null
                                                                                                     let toggleResult = toggleFilters.Count == 0 || (include == FilterResult(c))
                                                                                                     let stringsResult = stringFilters.Count == 0 || (include == StringFilterResult(c))
                                                                                                     let tagsResult = tagFilters.Count == 0 || (include == TagResult(c))
                                                                                                     let showResult = toggleFilters.Where(e => e.Value.IsEnabled).Select(e => e.Key).Contains("Hidden") || c.Show
                                                                                                     let betaResult = (_data.StaticInfo.IsBeta && c.Beta) || toggleFilters.Where(e => e.Value.IsEnabled).Select(e => e.Key).Contains("Hidden") || !c.Beta
                                                                                                     select (ctrl, toggleResult, stringsResult, tagsResult, showResult, betaResult))
            {
                ctrl.Visible = toggleResult && stringsResult && tagsResult && showResult && betaResult;
            }

            SortCharacters();
            CharactersPanel.Invalidate();

            _clearButton.Visible = stringFilters.Count > 0 || toggleFilters.Count > 0 || tagFilters.Count > 0;
        }

        public void CreateCharacterControls()
        {
            if (SideMenu.Tabs.Count > 1 && CharacterCards is not null)
            {
                var newCharacters = CharacterModels.Except(LoadedModels);

                foreach (Character_Model c in newCharacters)
                {
                    LoadedModels.Add(c);
                    CharacterCards.Add(new CharacterCard(_currentCharacter, _textureManager, _data, this, _settings)
                    {
                        Character = c,
                        Parent = CharactersPanel,
                        AttachedCards = CharacterCards,
                    });
                }

                FilterCharacters();
                CharacterCards.FirstOrDefault()?.UniformWithAttached();
            }
        }

        public void SortCharacters()
        {
            var sortby = _settings.SortType.Value;
            bool asc = _settings.SortOrder.Value == SortDirection.Ascending;
            bool isNum = sortby is SortBy.Level or SortBy.TimeSinceLogin or SortBy.Map or SortBy.NextBirthday or SortBy.Age;

            if (_settings.SortType.Value != SortBy.Custom)
            {
                int getValue(Character_Model c)
                {
                    string sortType = _settings.SortType.Value is SortBy.NextBirthday ? "SecondsUntilNextBirthday" : _settings.SortType.Value.ToString();
                    return Common.GetPropertyValue<int>(c, sortType);
                }

                string getString(Character_Model c)
                {
                    string sortType = _settings.SortType.Value is SortBy.NextBirthday ? "SecondsUntilNextBirthday" : _settings.SortType.Value.ToString();

                    string s = Common.GetPropertyValueAsString(c, sortType);
                    return s;
                }

                CharactersPanel.SortChildren<CharacterCard>((a, b) =>
                {
                    if (isNum)
                    {
                        int r1 = asc ? getValue(a.Character).CompareTo(getValue(b.Character)) : getValue(b.Character).CompareTo(getValue(a.Character));
                        int r2 = asc ? a.Character.Position.CompareTo(b.Character.Position) : b.Character.Position.CompareTo(a.Character.Position);
                        return r1 == 0 ? r1 - r2 : r1;
                    }
                    else
                    {
                        int r1 = asc ? getString(a.Character).CompareTo(getString(b.Character)) : getString(b.Character).CompareTo(getString(a.Character));
                        int r2 = asc ? a.Character.Position.CompareTo(b.Character.Position) : b.Character.Position.CompareTo(a.Character.Position);
                        return r1 == 0 ? r1 - r2 : r1;
                    }
                });
            }
            else
            {
                CharactersPanel.SortChildren<CharacterCard>((a, b) => a.Index.CompareTo(b.Index));
            }
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            BasicTooltipText = MouseOverTitleBar && Version is not null ? $"v. {Version}" : null;

            if (_filterCharacters && gameTime.TotalGameTime.TotalMilliseconds - _filterTick > _settings.FilterDelay.Value)
            {
                _filterTick = gameTime.TotalGameTime.TotalMilliseconds;
                _filterCharacters = false;
                PerformFiltering();
            }

            if (gameTime.TotalGameTime.TotalMilliseconds - lastUniform > 1000)
            {
                lastUniform = gameTime.TotalGameTime.TotalMilliseconds;
                CharacterCards.FirstOrDefault()?.UniformWithAttached();
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            //spriteBatch.DrawOnCtrl(
            //    this,
            //    _windowEmblem,
            //    _emblemRectangle,
            //    _windowEmblem.Bounds,
            //    Color.White,
            //    0f,
            //    default);

            //if (_titleRectangle.Width < bounds.Width - (_emblemRectangle.Width - 20))
            //{
            //    spriteBatch.DrawStringOnCtrl(
            //        this,
            //        $"{Characters.ModuleName}",
            //        _titleFont,
            //        _titleRectangle,
            //        ContentService.Colors.ColonialWhite, // new Color(247, 231, 182, 97),
            //        false,
            //        HorizontalAlignment.Left,
            //        VerticalAlignment.Bottom);
            //}
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (_settings.FocusSearchOnShow.Value)
            {
                foreach (var k in GameService.Input.Keyboard.KeysDown)
                {
                    Blish_HUD.Controls.Intern.Keyboard.Release((Blish_HUD.Controls.Extern.VirtualKeyShort)k, false);
                    Blish_HUD.Controls.Intern.Keyboard.Release((Blish_HUD.Controls.Extern.VirtualKeyShort)k, true);
                }

                _filterBox.Focused = true;
            }

            CharacterCards.FirstOrDefault()?.UniformWithAttached();
        }

        protected override void OnHidden(EventArgs e)
        {
            base.OnHidden(e);

            CharacterCards?.ForEach(c => c.HideTooltips());
            _filterBox?.ResetText();
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            if (_created)
            {
                if (ContentPanel is not null)
                {
                    ContentPanel.Size = new Point(ContentRegion.Size.X, ContentRegion.Size.Y - 35);
                }

                if (_dropdownPanel is not null)
                {
                    //_dropdownPanel.Size = new Point(ContentRegion.Size.X, 31);
                    _filterBox.Width = _dropdownPanel.Width - _buttonPanel.Width - 2;
                    _clearButton.Location = new Point(_filterBox.LocalBounds.Right - 23, _filterBox.LocalBounds.Top + 8);
                }

                if (e.CurrentSize.Y < 135)
                {
                    Size = new Point(Size.X, 135);
                }

                _settings.WindowSize.Value = Size;
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            _settings.PinSideMenus.SettingChanged -= PinSideMenus_SettingChanged;
            _settings.ShowRandomButton.SettingChanged -= ShowRandomButton_SettingChanged;
            _settings.ShowLastButton.SettingChanged -= ShowLastButton_SettingChanged;
            _settings.FilterDelay.SettingChanged -= FilterDelay_SettingChanged;

            GameService.Gw2Mumble.CurrentMap.MapChanged -= CurrentMap_MapChanged;

            DraggingControl.LeftMouseButtonReleased -= DraggingControl_LeftMouseButtonReleased;
            _buttonPanel.Resized -= ButtonPanel_Resized;

            _collapseWrapper.Hidden -= CollapseWrapper_Hidden;

            _collapseWrapper?.Dispose();
            ContentPanel?.Dispose();
            CharactersPanel?.Dispose();
            DraggingControl?.Dispose();
            CharacterEdit?.Dispose();

            _dropdownPanel?.Dispose();
            _displaySettingsButton?.Dispose();
            _filterBox?.Dispose();
            SideMenu?.Dispose();
        }

        private async void FilterBox_EnterPressed(string t)
        {
            if (_settings.EnterToLogin.Value)
            {
                _filterBox.ForceFilter();

                var c = (CharacterCard)CharactersPanel.Children.Where(e => e.Visible).FirstOrDefault();
                _filterBox.UnsetFocus();
                await Task.Delay(5);

                if (await ExtendedInputService.WaitForNoKeyPressed())
                {
                    c?.Character.Swap();

                    if (c == null)
                    {
                        Characters.Logger.Debug("No character found to swap to.");
                    }
                }
            }
        }

        private void FilterBox_Click()
        {
            if (_settings.OpenSidemenuOnSearch.Value) ShowAttached(SideMenu);
        }

        private void DraggingControl_LeftMouseButtonReleased(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            SetNewIndex(DraggingControl.CharacterControl);
            DraggingControl.EndDragging();
        }

        private void SetNewIndex(CharacterCard characterControl)
        {
            Blish_HUD.Input.MouseHandler m = Input.Mouse;
            var cards = CharactersPanel.Children.Cast<CharacterCard>();

            int newIndex = -1;
            int index = 0;
            foreach (CharacterCard c in cards)
            {
                if (c.AbsoluteBounds.Contains(m.Position) && newIndex == -1)
                {
                    characterControl.Index = c.Index;
                    characterControl.Character.Index = c.Index;
                    newIndex = c.Index;
                }

                if (newIndex == index)
                {
                    index++;
                }

                if (c != characterControl)
                {
                    c.Character.SetIndex(index);
                    c.Index = index;
                    index++;
                }
            }

            SortCharacters();
        }

        public void SendTesseractFailedNotification(string pathToEngine)
        {
            _tesseractFailedNotification ??= new TesseractFailedNotification()
            {
                Parent = _notifications,
                Height = 25,
                PathToEngine = pathToEngine,
                ClickAction = _toggleOCR,
            };

            if (_settings.ShowNotifications.Value && _notifications.Children.Count > 0 && _notifications.Children.Any(e => e.Visible))
            {
                _collapseWrapper.Show();
                ContentPanel.Invalidate();
            }
        }

        public void SendAPITimeoutNotification()
        {

            _apiTimeoutNotification ??= new APITimeoutNotification()
            {
                Parent = _notifications,
                Height = 25,
            };

            if (_settings.ShowNotifications.Value && _notifications.Children.Count > 0 && _notifications.Children.Any(e => e.Visible))
            {
                _collapseWrapper.Show();
                ContentPanel.Invalidate();
            }
        }

        public void SendAPIPermissionNotification()
        {

            _apiPermissionNotification ??= new APIPermissionNotification()
            {
                Parent = _notifications,
                Height = 25,
            };

            if (_settings.ShowNotifications.Value && _notifications.Children.Count > 0 && _notifications.Children.Any(e => e.Visible))
            {
                _collapseWrapper.Show();
                ContentPanel.Invalidate();
            }
        }

        public void CheckOCRRegion()
        {
            var defaultRect = new Rectangle(50, (int)((GameService.Graphics.Resolution.Y - 350) / 2), 530, 50);
            if (_settings.UseOCR.Value && _settings.ActiveOCRRegion.Equals(defaultRect))
            {
                _ = new OCRSetupNotification()
                {
                    Resolution = _settings.OCRKey,
                    Parent = _notifications,
                    Height = 25,
                    ClickAction = () =>
                    {
                        _toggleOCR?.Invoke();
                        SettingsWindow?.Show();
                    }
                };
            }

            if (_settings.ShowNotifications.Value && _notifications.Children.Count > 0 && _notifications.Children.Any(e => e.Visible))
            {
                _collapseWrapper.Show();
                ContentPanel.Invalidate();
            }

            if (_collapseWrapper.Collapsed)
            {
                _collapseWrapper.Height = _collapseWrapper.TitleBarHeight;
            }
        }

        public void UpdateMissingNotification()
        {
            _notifications.UpdateCharacters();

            if (_settings.ShowNotifications.Value && _notifications.Children.Count > 0 && _notifications.Children.Any(e => e.Visible))
            {
                _collapseWrapper.Show();
                ContentPanel.Invalidate();
            }

            if (_collapseWrapper.Collapsed)
            {
                _collapseWrapper.Height = _collapseWrapper.TitleBarHeight;
            }
        }

        public void AdjustForBeta()
        {
            LoadedModels?.Clear();
            CharactersPanel?.ClearChildren();
            CharacterCards?.Clear();

            CreateCharacterControls();
        }
    }
}
