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
    public class SettingsModel : BaseSettingsModel
    {
        private readonly ObservableCollection<SettingEntry> _appearanceSettings = new();

        public SettingsModel(SettingCollection settings)
        {
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

            FilterAsOne = internalSettings.DefineSetting(nameof(FilterAsOne), false);
            UseBetaGamestate = internalSettings.DefineSetting(nameof(UseBetaGamestate), true);

            EnableRadialMenu = internalSettings.DefineSetting(nameof(EnableRadialMenu), false);
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
            FocusSearchOnShow = internalSettings.DefineSetting(nameof(FocusSearchOnShow), true);

            ShowRandomButton = internalSettings.DefineSetting(nameof(ShowRandomButton), false);
            ShowLastButton = internalSettings.DefineSetting(nameof(ShowLastButton), false);
            ShowStatusWindow = internalSettings.DefineSetting(nameof(ShowStatusWindow), true);
            ShowChoyaSpinner = internalSettings.DefineSetting(nameof(ShowChoyaSpinner), true);
            EnterOnSwap = internalSettings.DefineSetting(nameof(EnterOnSwap), true);
            OnlyEnterOnExact = internalSettings.DefineSetting(nameof(OnlyEnterOnExact), true);
            OpenInventoryOnEnter = internalSettings.DefineSetting(nameof(OpenInventoryOnEnter), false);
            DoubleClickToEnter = internalSettings.DefineSetting(nameof(DoubleClickToEnter), false);
            EnterToLogin = internalSettings.DefineSetting(nameof(EnterToLogin), false);
            CheckDistance = internalSettings.DefineSetting(nameof(CheckDistance), 5);
            SwapDelay = internalSettings.DefineSetting(nameof(SwapDelay), 250);
            KeyDelay = internalSettings.DefineSetting(nameof(KeyDelay), 0);
            FilterDelay = internalSettings.DefineSetting(nameof(FilterDelay), 0);
            WindowSize = internalSettings.DefineSetting(nameof(CurrentWindowSize), new Point(385, 920));
            WindowOffset = internalSettings.DefineSetting(nameof(WindowOffset), new RectangleDimensions(8, 31, -8, -8));
            DisplayToggles = internalSettings.DefineSetting(nameof(DisplayToggles), new Dictionary<string, ShowCheckPair>());

            Point res = GameService.Graphics.Resolution;
            //WindowedMode = internalSettings.DefineSetting(nameof(WindowedMode), false);
            PinSideMenus = internalSettings.DefineSetting(nameof(PinSideMenus), true);
            UseOCR = internalSettings.DefineSetting(nameof(UseOCR), false);
            AutoSortCharacters = internalSettings.DefineSetting(nameof(AutoSortCharacters), false);
            OCRRegion = internalSettings.DefineSetting(nameof(OCRRegion), new Rectangle(50, 550, 530, 50));
            OCRRegions = internalSettings.DefineSetting(nameof(OCRRegions), new Dictionary<string, Rectangle>());
            OCRCustomOffset = internalSettings.DefineSetting(nameof(OCRCustomOffset), new RectangleDimensions(3, 3, 5, 5));
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

            SortType = internalSettings.DefineSetting(nameof(SortType), ESortType.SortByLastLogin);
            SortOrder = internalSettings.DefineSetting(nameof(SortOrder), ESortOrder.Ascending);

            foreach (SettingEntry setting in _appearanceSettings)
            {
                setting.PropertyChanged += OnAppearanceSettingChanged;
            }

            _appearanceSettings.ItemAdded += AppearanceSettings_ItemAdded;
            _appearanceSettings.ItemRemoved += AppearanceSettings_ItemRemoved;
        }

        public event EventHandler AppearanceSettingChanged;

        public enum ESortOrder
        {
            Ascending,
            Descending,
        }

        public enum ESortType
        {
            SortByName,
            SortByTag,
            SortByProfession,
            SortByLastLogin,
            SortByMap,
            Custom,
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

        public SettingEntry<Dictionary<string, ShowCheckPair>> DisplayToggles { get; set; }

        public SettingEntry<bool> PinSideMenus { get; set; }

        public SettingEntry<bool> CloseWindowOnSwap { get; set; }

        public SettingEntry<bool> FilterDiacriticsInsensitive { get; set; }

        public SettingEntry<bool> ShowRandomButton { get; set; }

        public SettingEntry<bool> ShowLastButton { get; set; }

        public SettingEntry<bool> ShowCornerIcon { get; set; }

        public Point CurrentWindowSize => WindowSize.Value;

        public SettingEntry<Point> WindowSize { get; set; }

        public SettingEntry<RectangleDimensions> WindowOffset { get; set; }

        public SettingEntry<bool> ShowStatusWindow { get; set; }

        public SettingEntry<bool> ShowChoyaSpinner { get; set; }

        public SettingEntry<bool> AutoSortCharacters { get; set; }

        public SettingEntry<bool> UseOCR { get; set; }

        public SettingEntry<bool> WindowedMode { get; set; }

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

                return regions.ContainsKey(OCRKey) ? regions[OCRKey] : new Rectangle(50, (int)(res.Y * 0.8413), 530, 50);
            }
        }

        public SettingEntry<Rectangle> OCRRegion { get; set; }

        public SettingEntry<Dictionary<string, Rectangle>> OCRRegions { get; set; }

        public SettingEntry<RectangleDimensions> OCRCustomOffset { get; set; }

        public SettingEntry<bool> EnableRadialMenu { get; set; }

        public SettingEntry<bool> Radial_UseProfessionIconsColor { get; set; }

        public SettingEntry<bool> Radial_UseProfessionIcons{ get; set; }

        public SettingEntry<bool> Radial_ShowAdvancedTooltip { get; set; }

        public SettingEntry<bool> Radial_UseProfessionColor { get; set; }
        
        public SettingEntry<bool> UseCharacterIconsOnRadial{ get; set; }

        public SettingEntry<float> Radial_Scale{ get; set; }

        public SettingEntry<Color> Radial_IdleColor{ get; set; }

        public SettingEntry<Color> Radial_IdleBorderColor{ get; set; }

        public SettingEntry<Color> Radial_HoveredBorderColor{ get; set; }

        public SettingEntry<Color> Radial_HoveredColor{ get; set; }

        public SettingEntry<bool> UseBetaGamestate { get; set; }

        public SettingEntry<bool> CharacterPanelFixedWidth { get; set; }

        public SettingEntry<int> CharacterPanelWidth { get; set; }

        public SettingEntry<int> OCRNoPixelColumns { get; set; }

        public SettingEntry<int> CustomCharacterIconSize { get; set; }

        public SettingEntry<int> CustomCharacterFontSize { get; set; }

        public SettingEntry<int> CustomCharacterNameFontSize { get; set; }

        public SettingEntry<PanelSizes> PanelSize { get; set; }

        public SettingEntry<CharacterPanelLayout> PanelLayout { get; set; }

        public SettingEntry<bool> ShowDetailedTooltip { get; set; }

        public SettingEntry<MatchingBehavior> ResultMatchingBehavior { get; set; }

        public SettingEntry<FilterBehavior> ResultFilterBehavior { get; set; }

        public SettingEntry<ESortType> SortType { get; set; }

        public SettingEntry<ESortOrder> SortOrder { get; set; }

        public SettingEntry<KeyBinding> LogoutKey { get; set; }

        public SettingEntry<KeyBinding> ShortcutKey { get; set; }

        public SettingEntry<KeyBinding> RadialKey { get; }

        public SettingEntry<KeyBinding> InventoryKey { get; }

        public SettingEntry<KeyBinding> MailKey { get; }

        public SettingEntry<bool> OnlyEnterOnExact { get; set; }

        public SettingEntry<bool> EnterOnSwap { get; set; }

        public SettingEntry<bool> OpenInventoryOnEnter { get; set; }

        public SettingEntry<bool> DoubleClickToEnter { get; set; }

        public SettingEntry<bool> EnterToLogin { get; set; }

        public SettingEntry<int> CheckDistance { get; set; }

        public SettingEntry<int> SwapDelay { get; set; }

        public SettingEntry<int> KeyDelay { get; set; }

        public SettingEntry<int> FilterDelay { get; set; }

        public SettingEntry<int> OCR_ColorThreshold { get; set; }

        public SettingEntry<bool> FilterAsOne { get; set; }

        public SettingEntry<bool> LoadCachedAccounts { get; set; }

        public SettingEntry<bool> OpenSidemenuOnSearch { get; set; }

        public SettingEntry<SemVer.Version> Version { get; set; }

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
    }
}
