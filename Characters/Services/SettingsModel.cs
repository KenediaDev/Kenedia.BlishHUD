using Blish_HUD;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Structs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Collections;
using System;
using System.Collections.Generic;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Services
{
    public class Settings : BaseSettingsModel
    {
        private readonly ObservableCollection<SettingEntry> _appearanceSettings = new();

        public Settings(SettingCollection settings) : base(settings)        
        {
        }

        protected override void InitializeSettings(SettingCollection settings)
        {
            base.InitializeSettings(settings);

            SettingCollection internalSettings = settings.AddSubCollection("Internal", false, false);
            Version = internalSettings.DefineSetting(nameof(Version), new SemVer.Version("0.0.0"));
            LogoutKey = internalSettings.DefineSetting(nameof(LogoutKey), new KeyBinding(Keys.F12));
            ShortcutKey = internalSettings.DefineSetting(nameof(ShortcutKey), new KeyBinding(ModifierKeys.Shift, Keys.C));
            RadialKey = internalSettings.DefineSetting(nameof(RadialKey), new KeyBinding(Keys.None));
            InventoryKey = internalSettings.DefineSetting(nameof(InventoryKey), new KeyBinding(Keys.I));
            MailKey = internalSettings.DefineSetting(nameof(MailKey), new KeyBinding(Keys.None));
            ShowCornerIcon = internalSettings.DefineSetting(nameof(ShowCornerIcon), true);
            CloseWindowOnSwap = internalSettings.DefineSetting(nameof(CloseWindowOnSwap), false);
            FilterDiacriticsInsensitive = internalSettings.DefineSetting(nameof(FilterDiacriticsInsensitive), false);
            CancelOnlyOnESC = internalSettings.DefineSetting(nameof(CancelOnlyOnESC), false);
            AutomaticCharacterDelete = internalSettings.DefineSetting(nameof(AutomaticCharacterDelete), false);
            ShowNotifications = internalSettings.DefineSetting(nameof(ShowNotifications), true);

            IncludeBetaCharacters = internalSettings.DefineSetting(nameof(IncludeBetaCharacters), true);

            FilterAsOne = internalSettings.DefineSetting(nameof(FilterAsOne), false);
            UseBetaGamestate = internalSettings.DefineSetting(nameof(UseBetaGamestate), false);
            DebugMode = internalSettings.DefineSetting(nameof(DebugMode), true);

            EnableRadialMenu = internalSettings.DefineSetting(nameof(EnableRadialMenu), true);
            Radial_Scale = internalSettings.DefineSetting(nameof(Radial_Scale), 0.66F);
            Radial_HoveredBorderColor = internalSettings.DefineSetting(nameof(Radial_HoveredBorderColor), ContentService.Colors.ColonialWhite);
            Radial_HoveredColor = internalSettings.DefineSetting(nameof(Radial_HoveredBorderColor), ContentService.Colors.ColonialWhite * 0.8F);
            Radial_IdleColor = internalSettings.DefineSetting(nameof(Radial_IdleColor), Color.Black * 0.8F);
            Radial_IdleBorderColor = internalSettings.DefineSetting(nameof(Radial_IdleBorderColor), ContentService.Colors.ColonialWhite);
            Radial_UseProfessionColor = internalSettings.DefineSetting(nameof(Radial_UseProfessionColor), false);
            Radial_UseProfessionIcons = internalSettings.DefineSetting(nameof(Radial_UseProfessionIcons), false);
            Radial_UseProfessionIconsColor = internalSettings.DefineSetting(nameof(Radial_UseProfessionIconsColor), false);
            Radial_ShowAdvancedTooltip = internalSettings.DefineSetting(nameof(Radial_ShowAdvancedTooltip), true);

            LoadCachedAccounts = internalSettings.DefineSetting(nameof(LoadCachedAccounts), true);
            OpenSidemenuOnSearch = internalSettings.DefineSetting(nameof(OpenSidemenuOnSearch), true);
            FocusSearchOnShow = internalSettings.DefineSetting(nameof(FocusSearchOnShow), false);

            ShowRandomButton = internalSettings.DefineSetting(nameof(ShowRandomButton), true);
            ShowLastButton = internalSettings.DefineSetting(nameof(ShowLastButton), false);
            ShowStatusWindow = internalSettings.DefineSetting(nameof(ShowStatusWindow), true);
            ShowChoyaSpinner = internalSettings.DefineSetting(nameof(ShowChoyaSpinner), true);
            EnterOnSwap = internalSettings.DefineSetting(nameof(EnterOnSwap), true);
            OnlyEnterOnExact = internalSettings.DefineSetting(nameof(OnlyEnterOnExact), false);
            OpenInventoryOnEnter = internalSettings.DefineSetting(nameof(OpenInventoryOnEnter), false);
            DoubleClickToEnter = internalSettings.DefineSetting(nameof(DoubleClickToEnter), false);
            EnterToLogin = internalSettings.DefineSetting(nameof(EnterToLogin), false);
            CheckDistance = internalSettings.DefineSetting(nameof(CheckDistance), 5);
            SwapDelay = internalSettings.DefineSetting(nameof(SwapDelay), 500);
            KeyDelay = internalSettings.DefineSetting(nameof(KeyDelay), 10);
            FilterDelay = internalSettings.DefineSetting(nameof(FilterDelay), 0);
            WindowSize = internalSettings.DefineSetting(nameof(CurrentWindowSize), new Point(385, 920));
            WindowOffset = internalSettings.DefineSetting(nameof(WindowOffset), new RectangleDimensions(8, 31, -8, -8));
            DisplayToggles = internalSettings.DefineSetting(nameof(DisplayToggles), new Dictionary<string, ShowCheckPair>());

            Point res = GameService.Graphics.Resolution;
            PinSideMenus = internalSettings.DefineSetting(nameof(PinSideMenus), false);
            UseOCR = internalSettings.DefineSetting(nameof(UseOCR), false);
            AutoSortCharacters = internalSettings.DefineSetting(nameof(AutoSortCharacters), false);
            OCRRegion = internalSettings.DefineSetting(nameof(OCRRegion), new Rectangle(50, 550, 530, 50));
            OCRRegions = internalSettings.DefineSetting(nameof(OCRRegions), new Dictionary<string, Rectangle>());
            OCRNoPixelColumns = internalSettings.DefineSetting(nameof(OCRNoPixelColumns), 20);
            OCR_ColorThreshold = internalSettings.DefineSetting(nameof(OCR_ColorThreshold), 181);

            PanelSize = internalSettings.DefineSetting(nameof(PanelSize), PanelSizes.Normal);
            CustomCharacterIconSize = internalSettings.DefineSetting(nameof(CustomCharacterIconSize), 128);
            CustomCharacterFontSize = internalSettings.DefineSetting(nameof(CustomCharacterFontSize), 16);
            CustomCharacterNameFontSize = internalSettings.DefineSetting(nameof(CustomCharacterNameFontSize), 18);
            PanelLayout = internalSettings.DefineSetting(nameof(PanelLayout), CharacterPanelLayout.IconAndText);
            CharacterPanelFixedWidth = internalSettings.DefineSetting(nameof(CharacterPanelFixedWidth), false);
            CharacterPanelWidth = internalSettings.DefineSetting(nameof(CharacterPanelWidth), 300);

            _appearanceSettings.Add(DisplayToggles);
            _appearanceSettings.Add(PanelSize);
            _appearanceSettings.Add(CustomCharacterIconSize);
            _appearanceSettings.Add(CustomCharacterFontSize);
            _appearanceSettings.Add(CustomCharacterNameFontSize);
            _appearanceSettings.Add(PanelLayout);
            _appearanceSettings.Add(CharacterPanelFixedWidth);
            _appearanceSettings.Add(CharacterPanelWidth);

            ShowDetailedTooltip = internalSettings.DefineSetting(nameof(ShowDetailedTooltip), true);

            ResultMatchingBehavior = internalSettings.DefineSetting(nameof(ResultMatchingBehavior), MatchingBehavior.MatchAny);
            ResultFilterBehavior = internalSettings.DefineSetting(nameof(ResultFilterBehavior), FilterBehavior.Include);

            SortType = internalSettings.DefineSetting(nameof(SortType), SortBy.TimeSinceLogin);
            SortOrder = internalSettings.DefineSetting(nameof(SortOrder), SortDirection.Ascending);

            foreach (SettingEntry setting in _appearanceSettings)
            {
                setting.PropertyChanged += OnAppearanceSettingChanged;
            }

            _appearanceSettings.ItemAdded += AppearanceSettings_ItemAdded;
            _appearanceSettings.ItemRemoved += AppearanceSettings_ItemRemoved;
        }

        public event EventHandler AppearanceSettingChanged;

        public enum SortDirection
        {
            Ascending,
            Descending,
        }

        public enum SortBy
        {
            Name,
            Level,
            Tag,
            Profession,
            TimeSinceLogin,
            Map,
            Race,
            Gender,
            Specialization,
            Custom,
            NextBirthday,
            Age,
        }

        public enum FilterBehavior
        {
            Include,
            Exclude,
        }

        public enum MatchingBehavior
        {
            MatchAny,
            MatchAll,
        }

        public enum CharacterPanelLayout
        {
            OnlyIcons,
            OnlyText,
            IconAndText,
        }

        public enum PanelSizes
        {
            Small,
            Normal,
            Large,
            Custom,
        }

        public SettingCollection AccountSettings { get; private set; }

        public SettingEntry<Dictionary<string, ShowCheckPair>> DisplayToggles { get; private set; }

        public SettingEntry<bool> DebugMode { get; private set; }

        public SettingEntry<bool> PinSideMenus { get; private set; }

        public SettingEntry<bool> IncludeBetaCharacters { get; private set; }

        public SettingEntry<bool> CloseWindowOnSwap { get; private set; }

        public SettingEntry<bool> FilterDiacriticsInsensitive { get; private set; }

        public SettingEntry<bool> ShowRandomButton { get; private set; }

        public SettingEntry<bool> ShowLastButton { get; private set; }

        public SettingEntry<bool> ShowCornerIcon { get; private set; }

        public Point CurrentWindowSize => WindowSize.Value;

        public SettingEntry<Point> WindowSize { get; private set; }

        public SettingEntry<RectangleDimensions> WindowOffset { get; private set; }

        public SettingEntry<bool> ShowStatusWindow { get; private set; }

        public SettingEntry<bool> ShowChoyaSpinner { get; private set; }

        public SettingEntry<bool> AutoSortCharacters { get; private set; }

        public SettingEntry<bool> CancelOnlyOnESC { get; private set; }

        public SettingEntry<bool> AutomaticCharacterDelete { get; private set; }

        public SettingEntry<bool> ShowNotifications { get; private set; }

        public SettingEntry<bool> UseOCR { get; private set; }

        public SettingEntry<bool> WindowedMode { get; private set; }

        public string OCRKey
        {
            get
            {
                Point res = GameService.Graphics.Resolution;
                string key = "{" + $"X:{Math.Floor((double)res.X / 10) * 10} Y:{Math.Floor((double)res.Y / 10) * 10}" + "}";
                return key;
            }
        }

        public Rectangle ActiveOCRRegion
        {
            get
            {
                Point res = GameService.Graphics.Resolution;
                Dictionary<string, Rectangle> regions = OCRRegions.Value;

                return regions.ContainsKey(OCRKey) ? regions[OCRKey] : new Rectangle(50, (int)((res.Y - 350) / 2), 530, 50);
            }
        }

        public SettingEntry<Rectangle> OCRRegion { get; private set; }

        public SettingEntry<Dictionary<string, Rectangle>> OCRRegions { get; private set; }

        public SettingEntry<bool> EnableRadialMenu { get; private set; }

        public SettingEntry<bool> Radial_UseProfessionIconsColor { get; private set; }

        public SettingEntry<bool> Radial_UseProfessionIcons { get; private set; }

        public SettingEntry<bool> Radial_ShowAdvancedTooltip { get; private set; }

        public SettingEntry<bool> Radial_UseProfessionColor { get; private set; }

        public SettingEntry<bool> UseCharacterIconsOnRadial { get; private set; }

        public SettingEntry<float> Radial_Scale { get; private set; }

        public SettingEntry<Color> Radial_IdleColor { get; private set; }

        public SettingEntry<Color> Radial_IdleBorderColor { get; private set; }

        public SettingEntry<Color> Radial_HoveredBorderColor { get; private set; }

        public SettingEntry<Color> Radial_HoveredColor { get; private set; }

        public SettingEntry<bool> UseBetaGamestate { get; private set; }

        public SettingEntry<bool> CharacterPanelFixedWidth { get; private set; }

        public SettingEntry<int> CharacterPanelWidth { get; private set; }

        public SettingEntry<int> OCRNoPixelColumns { get; private set; }

        public SettingEntry<int> CustomCharacterIconSize { get; private set; }

        public SettingEntry<int> CustomCharacterFontSize { get; private set; }

        public SettingEntry<int> CustomCharacterNameFontSize { get; private set; }

        public SettingEntry<PanelSizes> PanelSize { get; private set; }

        public SettingEntry<CharacterPanelLayout> PanelLayout { get; private set; }

        public SettingEntry<bool> ShowDetailedTooltip { get; private set; }

        public SettingEntry<MatchingBehavior> ResultMatchingBehavior { get; private set; }

        public SettingEntry<FilterBehavior> ResultFilterBehavior { get; private set; }

        public SettingEntry<SortBy> SortType { get; private set; }

        public SettingEntry<SortDirection> SortOrder { get; private set; }

        public SettingEntry<KeyBinding> LogoutKey { get; private set; }

        public SettingEntry<KeyBinding> ShortcutKey { get; private set; }

        public SettingEntry<KeyBinding> RadialKey { get; private set; }

        public SettingEntry<KeyBinding> InventoryKey { get; private set; }

        public SettingEntry<KeyBinding> MailKey { get; private set; }

        public SettingEntry<bool> OnlyEnterOnExact { get; private set; }

        public SettingEntry<bool> EnterOnSwap { get; private set; }

        public SettingEntry<bool> OpenInventoryOnEnter { get; private set; }

        public SettingEntry<bool> DoubleClickToEnter { get; private set; }

        public SettingEntry<bool> EnterToLogin { get; private set; }

        public SettingEntry<int> CheckDistance { get; private set; }

        public SettingEntry<int> SwapDelay { get; private set; }

        public SettingEntry<int> KeyDelay { get; private set; }

        public SettingEntry<int> FilterDelay { get; private set; }

        public SettingEntry<int> OCR_ColorThreshold { get; private set; }

        public SettingEntry<bool> FilterAsOne { get; private set; }

        public SettingEntry<bool> LoadCachedAccounts { get; private set; }

        public SettingEntry<bool> OpenSidemenuOnSearch { get; private set; }

        public SettingEntry<SemVer.Version> Version { get; private set; }

        public SettingEntry<SemVer.Version> ImportVersion { get; private set; }

        public SettingEntry<bool> FocusSearchOnShow { get; private set; }

        protected override void OnDispose()
        {
            base.OnDispose();

            foreach (SettingEntry setting in _appearanceSettings)
            {
                setting.PropertyChanged -= OnAppearanceSettingChanged;
            }

            _appearanceSettings.ItemAdded -= AppearanceSettings_ItemAdded;
            _appearanceSettings.ItemRemoved -= AppearanceSettings_ItemRemoved;
        }

        private void AppearanceSettings_ItemRemoved(object sender, ItemEventArgs<SettingEntry> e)
        {
            e.Item.PropertyChanged -= OnAppearanceSettingChanged;
        }

        private void AppearanceSettings_ItemAdded(object sender, ItemEventArgs<SettingEntry> e)
        {
            e.Item.PropertyChanged += OnAppearanceSettingChanged;
        }

        private void OnAppearanceSettingChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            AppearanceSettingChanged?.Invoke(sender, e);
        }

        public void LoadAccountSettings(string accountName)
        {
            AccountSettings = SettingCollection.AddSubCollection(accountName, false, false);
            ImportVersion = AccountSettings.DefineSetting(nameof(ImportVersion), new SemVer.Version("0.0.0"));
        }
    }
}
