using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Settings;
using Kenedia.Modules.Characters.Res;
using Kenedia.Modules.Characters.Extensions;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Views;
using Microsoft.Xna.Framework;
using System;
using static Blish_HUD.ContentService;
using Checkbox = Kenedia.Modules.Core.Controls.Checkbox;
using Dropdown = Kenedia.Modules.Core.Controls.Dropdown;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using KeybindingAssigner = Kenedia.Modules.Core.Controls.KeybindingAssigner;
using Label = Kenedia.Modules.Core.Controls.Label;
using Panel = Kenedia.Modules.Core.Controls.Panel;
using TextBox = Kenedia.Modules.Core.Controls.TextBox;
using TrackBar = Kenedia.Modules.Core.Controls.TrackBar;
using ColorPicker = Kenedia.Modules.Core.Controls.ColorPicker;

namespace Kenedia.Modules.Characters.Views
{
    public class SettingsWindow : BaseSettingsWindow
    {
        private Label _customFontSizeLabel;
        private Dropdown _customFontSize;

        private Label _customNameFontSizeLabel;
        private Dropdown _customNameFontSize;

        private readonly FlowPanel _contentPanel;
        private readonly SharedSettingsView _sharedSettingsView;
        private readonly OCR _ocr;
        private readonly Settings _settings;
        private double _tick;

        public SettingsWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, SharedSettingsView sharedSettingsView, OCR ocr, Settings settings) : base(background, windowRegion, contentRegion)
        {
            _sharedSettingsView = sharedSettingsView;
            _ocr = ocr;
            _settings = settings;
            _contentPanel = new()
            {
                Parent = this,
                Width = ContentRegion.Width,
                Height = ContentRegion.Height,
                ControlPadding = new(0, 10),
                CanScroll = true,
            };

            SubWindowEmblem = AsyncTexture2D.FromAssetId(156027);
            MainWindowEmblem = AsyncTexture2D.FromAssetId(156015);
            Name = string.Format(strings.ItemSettings, $"{Characters.ModuleName}");

            CreateOCR();
            CreateAppearance();
            CreateBehavior();
            CreateRadial();
            CreateDelays();
            CreateGeneral();

            CreateKeybinds();

            GameService.Overlay.UserLocale.SettingChanged += OnLanguageChanged;
            OnLanguageChanged();
        }

        private void CreateRadial()
        {
            var headerPanel = new Panel()
            {
                Parent = _contentPanel,
                Width = ContentRegion.Width - 20,
                HeightSizingMode = SizingMode.AutoSize,
                ShowBorder = true,
                CanCollapse = true,
                TitleIcon = AsyncTexture2D.FromAssetId(157122),
                SetLocalizedTitle = () => strings.RadialMenuSettings,
                SetLocalizedTitleTooltip = () => strings.RadialMenuSettings_Tooltip,
            };

            var contentFlowPanel = new FlowPanel()
            {
                Parent = headerPanel,
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(10),
            };

            var settingsFlowPanel = new FlowPanel()
            {
                Parent = contentFlowPanel,
                HeightSizingMode = SizingMode.AutoSize,
                Width = ContentRegion.Width - 20,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
                OuterControlPadding = new(5),
            };

            _ = new Checkbox()
            {
                Parent = settingsFlowPanel,
                Checked = _settings.EnableRadialMenu.Value,
                SetLocalizedText = () => strings.EnableRadialMenu,
                SetLocalizedTooltip = () => strings.EnableRadialMenu_Tooltip,
                CheckedChangedAction = (b) => _settings.EnableRadialMenu.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = settingsFlowPanel,
                SetLocalizedText = () => strings.Radial_ShowAdvancedTooltip,
                SetLocalizedTooltip = () => strings.Radial_ShowAdvancedTooltip_Tooltip,
                Checked = _settings.Radial_ShowAdvancedTooltip.Value,
                CheckedChangedAction = (b) => _settings.Radial_ShowAdvancedTooltip.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = settingsFlowPanel,
                SetLocalizedText = () => strings.Radial_UseProfessionColor,
                SetLocalizedTooltip = () => strings.Radial_UseProfessionColor_Tooltip,
                Checked = _settings.Radial_UseProfessionColor.Value,
                CheckedChangedAction = (b) => _settings.Radial_UseProfessionColor.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = settingsFlowPanel,
                SetLocalizedText = () => strings.Radial_UseProfessionIcons,
                SetLocalizedTooltip = () => strings.Radial_UseProfessionIcons_Tooltip,
                Checked = _settings.Radial_UseProfessionIcons.Value,
                CheckedChangedAction = (b) => _settings.Radial_UseProfessionIcons.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = settingsFlowPanel,
                SetLocalizedText = () => strings.Radial_UseProfessionIconsColor,
                SetLocalizedTooltip = () => strings.Radial_UseProfessionIconsColor_Tooltip,
                Checked = _settings.Radial_UseProfessionIconsColor.Value,
                CheckedChangedAction = (b) => _settings.Radial_UseProfessionIconsColor.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = settingsFlowPanel,
                SetLocalizedText = () => strings.Radial_CenterScreen,
                SetLocalizedTooltip = () => strings.Radial_CenterScreen_Tooltip,
                Checked = _settings.Radial_CenterScreen.Value,
                CheckedChangedAction = (b) => _settings.Radial_CenterScreen.Value = b,
            };

            var subP = new Panel()
            {
                Parent = settingsFlowPanel,
                Width = ContentRegion.Width - 20,
                HeightSizingMode = SizingMode.AutoSize,
            };

            var scaleLabel = new Label()
            {
                Parent = subP,
                AutoSizeWidth = true,
                SetLocalizedText = () => string.Format(strings.Radial_Scale + " {0}%", _settings.Radial_Scale.Value * 100),
                SetLocalizedTooltip = () => strings.Radial_Scale_Tooltip,
            };
            _ = new TrackBar()
            {
                Parent = subP,
                SetLocalizedTooltip = () => strings.Radial_Scale_Tooltip,
                Value = _settings.Radial_Scale.Value * 100,
                ValueChangedAction = (v) =>
                {
                    _settings.Radial_Scale.Value = (float)v / (float)100;
                    scaleLabel.UserLocale_SettingChanged(_settings.Radial_Scale.Value, null);
                },
                MinValue = 0,
                MaxValue = 100,
                Location = new Point(250, 0),
            };

            int color_box_numbers = 4;
            int color_box_padding = 5;
            int color_box_width = (ContentRegion.Width - 37 - 250);

            subP = new Panel()
            {
                Parent = settingsFlowPanel,
                Width = ContentRegion.Width - 20,
                HeightSizingMode = SizingMode.AutoSize,
            };

            var idleBackgroundLabel = new Label()
            {
                Parent = subP,
                AutoSizeWidth = true,
                Location = new(0, 0),
                SetLocalizedText = () => strings.Radial_IdleBackgroundColor,
            };

            var radial_SliceBackgroundStart = new ColorPicker()
            {
                Parent = subP,
                Location = new(250, 0),
                Width = color_box_width,
                SelectedColor = _settings.Radial_SliceBackground.Value.Start,
                OnColorChangedAction = (color) => _settings.Radial_SliceBackground.Value = new(color, _settings.Radial_SliceBackground.Value.End)
            };

            var radial_SliceBackgroundEnd = new ColorPicker()
            {
                Parent = subP,
                Location = new(250, 25),
                Width = color_box_width,
                SelectedColor = _settings.Radial_SliceBackground.Value.End,
                OnColorChangedAction = (color) => _settings.Radial_SliceBackground.Value = new(_settings.Radial_SliceBackground.Value.Start, color)
            };

            _ = new Dummy()
            {
                Parent = settingsFlowPanel,
                Height = 10,
            };

            subP = new Panel()
            {
                Parent = settingsFlowPanel,
                Width = ContentRegion.Width - 20,
                HeightSizingMode = SizingMode.AutoSize,
            };

            var activeBackgroundLabel = new Label()
            {
                Parent = subP,
                AutoSizeWidth = true,
                Location = new(0, 0),
                SetLocalizedText = () => strings.Radial_HoveredBackgroundColor,
            };

            var radial_SliceHighlightStart = new ColorPicker()
            {
                Parent = subP,
                Location = new(250, 0),
                Width = color_box_width,
                SelectedColor = _settings.Radial_SliceHighlight.Value.Start,
                OnColorChangedAction = (color) => _settings.Radial_SliceHighlight.Value = new(color, _settings.Radial_SliceHighlight.Value.End)
            };

            var radial_SliceHighlightEnd = new ColorPicker()
            {
                Parent = subP,
                Location = new(250, 25),
                Width = color_box_width,
                SelectedColor = _settings.Radial_SliceHighlight.Value.End,
                OnColorChangedAction = (color) => _settings.Radial_SliceHighlight.Value = new(_settings.Radial_SliceHighlight.Value.Start, color)
            };
        }

        private void CreateOCR()
        {
            var headerPanel = new Panel()
            {
                Parent = _contentPanel,
                Width = ContentRegion.Width - 20,
                HeightSizingMode = SizingMode.AutoSize,
                ShowBorder = true,
                CanCollapse = true,
                TitleIcon = AsyncTexture2D.FromAssetId(759447),
                SetLocalizedTitle = () => strings.OCRAndImageRecognition,
                SetLocalizedTitleTooltip = () => strings.OCRAndImageRecognition_Tooltip,
            };

            var contentFlowPanel = new FlowPanel()
            {
                Parent = headerPanel,
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(10),
            };

            var settingsFlowPanel = new FlowPanel()
            {
                Parent = contentFlowPanel,
                HeightSizingMode = SizingMode.AutoSize,
                Width = (ContentRegion.Width - 20) / 2,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
                OuterControlPadding = new(5),
            };

            _ = new Checkbox()
            {
                Parent = settingsFlowPanel,
                Checked = _settings.UseOCR.Value,
                SetLocalizedText = () => strings.UseOCR,
                SetLocalizedTooltip = () => strings.UseOCR_Tooltip,
                CheckedChangedAction = (b) => _settings.UseOCR.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = settingsFlowPanel,
                Checked = _settings.UseBetaGamestate.Value,
                SetLocalizedText = () => strings.UseBetaGameState,
                SetLocalizedTooltip = () => strings.UseBetaGameState_Tooltip,
                CheckedChangedAction = (b) => _settings.UseBetaGamestate.Value = b,
            };

            var buttonFlowPanel = new FlowPanel()
            {
                Parent = headerPanel,
                Location = new(settingsFlowPanel.Right, 0),
                HeightSizingMode = SizingMode.AutoSize,
                Width = (ContentRegion.Width - 20) / 2,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
                OuterControlPadding = new(5),
            };
            _ = new Button()
            {
                Parent = buttonFlowPanel,
                SetLocalizedText = () => strings.EditOCR,
                SetLocalizedTooltip = () => strings.EditOCR_Tooltip,
                Width = buttonFlowPanel.Width - 15,
                Height = 40,
                ClickAction = _ocr.ToggleContainer,
            };

            _sharedSettingsView.CreateLayout(contentFlowPanel, ContentRegion.Width - 20);

        }

        private void CreateKeybinds()
        {
            #region Keybinds
            var p = new Panel()
            {
                Parent = _contentPanel,
                Width = ContentRegion.Width - 20,
                HeightSizingMode = SizingMode.AutoSize,
                ShowBorder = true,
                CanCollapse = true,
                SetLocalizedTitle = () => strings.Keybinds,
                TitleIcon = AsyncTexture2D.FromAssetId(156734),
            };

            var cP = new FlowPanel()
            {
                Parent = p,
                Location = new(5),
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
            };

            _ = new KeybindingAssigner()
            {
                Parent = cP,
                Width = ContentRegion.Width - 35,
                KeyBinding = _settings.LogoutKey.Value,
                KeybindChangedAction = (kb) =>
                {
                    _settings.LogoutKey.Value = new()
                    {
                        ModifierKeys = kb.ModifierKeys,
                        PrimaryKey = kb.PrimaryKey,
                        Enabled = kb.Enabled,
                        IgnoreWhenInTextField = true,
                    };
                },
                SetLocalizedKeyBindingName = () => strings.Logout,
                SetLocalizedTooltip = () => strings.LogoutDescription,
            };

            _ = new KeybindingAssigner()
            {
                Parent = cP,
                Width = ContentRegion.Width - 35,
                KeyBinding = _settings.ShortcutKey.Value,
                KeybindChangedAction = (kb) =>
                {
                    _settings.ShortcutKey.Value = new()
                    {
                        ModifierKeys = kb.ModifierKeys,
                        PrimaryKey = kb.PrimaryKey,
                        Enabled = kb.Enabled,
                        IgnoreWhenInTextField = true,
                    };
                },
                SetLocalizedKeyBindingName = () => strings.ShortcutToggle,
                SetLocalizedTooltip = () => strings.ShortcutToggle_Tooltip,
            };

            _ = new KeybindingAssigner()
            {
                Parent = cP,
                Width = ContentRegion.Width - 35,
                KeyBinding = _settings.RadialKey.Value,
                KeybindChangedAction = (kb) =>
                {
                    _settings.RadialKey.Value = new()
                    {
                        ModifierKeys = kb.ModifierKeys,
                        PrimaryKey = kb.PrimaryKey,
                        Enabled = kb.Enabled,
                        IgnoreWhenInTextField = true,
                    };
                },
                SetLocalizedKeyBindingName = () => strings.RadialMenuKey,
                SetLocalizedTooltip = () => strings.RadialMenuKey_Tooltip,
            };

            _ = new KeybindingAssigner()
            {
                Parent = cP,
                Width = ContentRegion.Width - 35,
                KeyBinding = _settings.InventoryKey.Value,
                KeybindChangedAction = (kb) =>
                {
                    _settings.InventoryKey.Value = new()
                    {
                        ModifierKeys = kb.ModifierKeys,
                        PrimaryKey = kb.PrimaryKey,
                        Enabled = kb.Enabled,
                        IgnoreWhenInTextField = true,
                    };
                },
                SetLocalizedKeyBindingName = () => strings.InventoryKey,
                SetLocalizedTooltip = () => strings.InventoryKey_Tooltip,
            };

            _ = new KeybindingAssigner()
            {
                Parent = cP,
                Width = ContentRegion.Width - 35,
                KeyBinding = _settings.MailKey.Value,
                KeybindChangedAction = (kb) =>
                {
                    _settings.MailKey.Value = new()
                    {
                        ModifierKeys = kb.ModifierKeys,
                        PrimaryKey = kb.PrimaryKey,
                        Enabled = kb.Enabled,
                        IgnoreWhenInTextField = true,
                    };
                },
                SetLocalizedKeyBindingName = () => strings.MailKey,
                SetLocalizedTooltip = () => strings.MailKey_Tooltip,
            };
            #endregion Keybinds
        }

        private void CreateBehavior()
        {
            #region Behavior
            var p = new Panel()
            {
                Parent = _contentPanel,
                Width = ContentRegion.Width - 20,
                HeightSizingMode = SizingMode.AutoSize,
                ShowBorder = true,
                CanCollapse = true,
                SetLocalizedTitle = () => strings.ModuleBehavior,
                SetLocalizedTitleTooltip = () => strings.ModuleBehavior_Tooltip,
                TitleIcon = AsyncTexture2D.FromAssetId(60968),
            };

            var cP = new FlowPanel()
            {
                Parent = p,
                Location = new(5, 5),
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = _settings.OnlyEnterOnExact.Value,
                SetLocalizedText = () => strings.OnlyEnterOnExact,
                SetLocalizedTooltip = () => strings.OnlyEnterOnExact_Tooltip,
                CheckedChangedAction = (b) => _settings.OnlyEnterOnExact.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = _settings.EnterOnSwap.Value,
                SetLocalizedText = () => strings.EnterOnSwap,
                SetLocalizedTooltip = () => strings.EnterOnSwap_Tooltip,
                CheckedChangedAction = (b) => _settings.EnterOnSwap.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = _settings.OpenInventoryOnEnter.Value,
                SetLocalizedText = () => strings.OpenInventoryOnEnter,
                SetLocalizedTooltip = () => strings.OpenInventoryOnEnter_Tooltip,
                CheckedChangedAction = (b) => _settings.OpenInventoryOnEnter.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = _settings.CloseWindowOnSwap.Value,
                SetLocalizedText = () => strings.CloseWindowOnSwap,
                SetLocalizedTooltip = () => strings.CloseWindowOnSwap_Tooltip,
                CheckedChangedAction = (b) => _settings.CloseWindowOnSwap.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = _settings.DoubleClickToEnter.Value,
                SetLocalizedText = () => strings.DoubleClickToEnter,
                SetLocalizedTooltip = () => strings.DoubleClickToEnter_Tooltip,
                CheckedChangedAction = (b) => _settings.DoubleClickToEnter.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = _settings.EnterToLogin.Value,
                SetLocalizedText = () => strings.EnterToLogin,
                SetLocalizedTooltip = () => strings.EnterToLogin_Tooltip,
                CheckedChangedAction = (b) => _settings.EnterToLogin.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = _settings.AutoSortCharacters.Value,
                SetLocalizedText = () => strings.AutoFix,
                SetLocalizedTooltip = () => strings.AutoFix_Tooltip,
                CheckedChangedAction = (b) => _settings.AutoSortCharacters.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = _settings.FilterDiacriticsInsensitive.Value,
                SetLocalizedText = () => strings.FilterDiacriticsInsensitive,
                SetLocalizedTooltip = () => strings.FilterDiacriticsInsensitive_Tooltip,
                CheckedChangedAction = (b) => _settings.FilterDiacriticsInsensitive.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = _settings.AutomaticCharacterDelete.Value,
                SetLocalizedText = () => strings.AutomaticCharacterDelete,
                SetLocalizedTooltip = () => strings.AutomaticCharacterDelete_Tooltip,
                CheckedChangedAction = (b) => _settings.AutomaticCharacterDelete.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = _settings.CancelOnlyOnESC.Value,
                SetLocalizedText = () => strings.CancelOnlyOnESC,
                SetLocalizedTooltip = () => strings.CancelOnlyOnESC_Tooltip,
                CheckedChangedAction = (b) => _settings.CancelOnlyOnESC.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = _settings.FilterAsOne.Value,
                SetLocalizedText = () => strings.FilterAsOne,
                SetLocalizedTooltip = () => strings.FilterAsOne_Tooltip,
                CheckedChangedAction = (b) => _settings.FilterAsOne.Value = b,
            };

            var subP = new Panel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
            };

            var checkDistanceLabel = new Label()
            {
                Parent = subP,
                AutoSizeWidth = true,
                Height = 20,
                SetLocalizedText = () => string.Format(strings.CheckDistance, _settings.CheckDistance.Value),
                SetLocalizedTooltip = () => strings.CheckDistance_Tooltip,
            };

            _ = new TrackBar()
            {
                Location = new(225, 2),
                Parent = subP,
                MinValue = 0,
                MaxValue = 72,
                Value = _settings.CheckDistance.Value,
                Width = ContentRegion.Width - 35 - 225,
                SetLocalizedTooltip = () => strings.CheckDistance_Tooltip,
                ValueChangedAction = (num) =>
                {
                    _settings.CheckDistance.Value = num;
                    checkDistanceLabel.UserLocale_SettingChanged(null, null);
                },
            };
            #endregion Behavior
        }

        private void CreateAppearance()
        {
            #region Appearance
            var p = new Panel()
            {
                Parent = _contentPanel,
                Width = ContentRegion.Width - 20,
                HeightSizingMode = SizingMode.AutoSize,
                ShowBorder = true,
                CanCollapse = true,
                TitleIcon = AsyncTexture2D.FromAssetId(156740),
                SetLocalizedTitle = () => strings.Appearance,
            };

            var cP = new FlowPanel()
            {
                Parent = p,
                Location = new(5, 5),
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = _settings.ShowCornerIcon.Value,
                SetLocalizedText = () => strings.ShowCorner_Name,
                SetLocalizedTooltip = () => string.Format(strings.ShowCorner_Tooltip, Characters.ModuleName),
                CheckedChangedAction = (b) => _settings.ShowCornerIcon.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = _settings.ShowRandomButton.Value,
                SetLocalizedText = () => strings.ShowRandomButton_Name,
                SetLocalizedTooltip = () => strings.ShowRandomButton_Tooltip,
                CheckedChangedAction = (b) => _settings.ShowRandomButton.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = _settings.ShowLastButton.Value,
                SetLocalizedText = () => strings.ShowLastButton,
                SetLocalizedTooltip = () => strings.ShowLastButton_Tooltip,
                CheckedChangedAction = (b) => _settings.ShowLastButton.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = _settings.ShowDetailedTooltip.Value,
                SetLocalizedText = () => string.Format(strings.ShowItem, strings.DetailedTooltip),
                SetLocalizedTooltip = () => strings.DetailedTooltip_Tooltip,
                CheckedChangedAction = (b) => _settings.ShowDetailedTooltip.Value = b,
            };

            var subP = new Panel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                Height = 30,
            };

            _ = new Label()
            {
                Parent = subP,
                AutoSizeWidth = true,
                Height = 30,
                SetLocalizedText = () => strings.CharacterDisplaySize,
            };

            _ = new Dropdown()
            {
                Location = new(250, 0),
                Parent = subP,
                SetLocalizedItems = () =>
                {
                    return new()
                    {
                        strings.Small,
                        strings.Normal,
                        strings.Large,
                        strings.Custom,
                    };
                },
                SelectedItem = _settings.PanelSize.Value.GetPanelSize(),
                ValueChangedAction = (b) => _settings.PanelSize.Value = b.GetPanelSize(),
            };

            subP = new Panel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                Height = 30,
            };

            _ = new Label()
            {
                Parent = subP,
                AutoSizeWidth = true,
                Height = 30,
                SetLocalizedText = () => strings.CharacterDisplayOption,
            };

            _ = new Dropdown()
            {
                Parent = subP,
                Location = new(250, 0),
                SetLocalizedItems = () =>
                {
                    return new()
                    {
                        strings.OnlyText,
                        strings.OnlyIcons,
                        strings.TextAndIcon,
                    };
                },
                SelectedItem = _settings.PanelLayout.Value.GetPanelLayout(),
                ValueChangedAction = (b) => _settings.PanelLayout.Value = b.GetPanelLayout(),
            };

            subP = new Panel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                Height = 30,
            };
            _customFontSizeLabel = new Label()
            {
                Parent = subP,
                AutoSizeWidth = true,
                SetLocalizedText = () => string.Format(strings.FontSize, _settings.CustomCharacterFontSize.Value),
            };

            _customFontSize = new Dropdown()
            {
                Parent = subP,
                Location = new(250, 0),
                SelectedItem = _settings.CustomCharacterFontSize.Value.ToString(),
                ValueChangedAction = (str) => { GetFontSize(_settings.CustomCharacterFontSize, str); _customFontSizeLabel.UserLocale_SettingChanged(null, null); },
            };

            subP = new Panel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                Height = 30,
            };
            _customNameFontSizeLabel = new Label()
            {
                Parent = subP,
                AutoSizeWidth = true,
                SetLocalizedText = () => string.Format(strings.NameFontSize, _settings.CustomCharacterNameFontSize.Value),
            };

            _customNameFontSize = new()
            {
                Parent = subP,
                Location = new(250, 0),
                SelectedItem = _settings.CustomCharacterNameFontSize.Value.ToString(),
                ValueChangedAction = (str) => { GetFontSize(_settings.CustomCharacterNameFontSize, str); _customNameFontSizeLabel.UserLocale_SettingChanged(null, null); },
            };

            foreach (object i in Enum.GetValues(typeof(FontSize)))
            {
                _customFontSize.Items.Add($"{(int)i}");
                _customNameFontSize.Items.Add($"{(int)i}");
            }

            subP = new Panel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                Height = 30,
            };
            var iconSizeLabel = new Label()
            {
                Parent = subP,
                AutoSizeWidth = true,
                SetLocalizedText = () => string.Format(strings.IconSize, _settings.CustomCharacterIconSize.Value),
            };
            _ = new TrackBar()
            {
                Parent = subP,
                Location = new(250, 0),
                MinValue = 8,
                MaxValue = 256,
                Width = ContentRegion.Width - 35 - 250,
                Value = _settings.CustomCharacterIconSize.Value,
                ValueChangedAction = (num) =>
                {
                    _settings.CustomCharacterIconSize.Value = num;
                    iconSizeLabel.UserLocale_SettingChanged(num, null);
                },
            };

            subP = new Panel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                Height = 30,
            };

            var cardWidthCheckbox = new Checkbox()
            {
                Parent = subP,
                Checked = _settings.CharacterPanelFixedWidth.Value,
                CheckedChangedAction = (b) => _settings.CharacterPanelFixedWidth.Value = b,
                SetLocalizedText = () => string.Format(strings.CardWith, _settings.CharacterPanelWidth.Value),
                SetLocalizedTooltip = () => string.Format(strings.CardWidth_Tooltip, _settings.CharacterPanelWidth.Value),
            };

            _ = new TrackBar()
            {
                Parent = subP,
                Location = new(250, 0),
                MinValue = 25,
                MaxValue = 750,
                Width = ContentRegion.Width - 35 - 250,
                Value = _settings.CharacterPanelWidth.Value,
                ValueChangedAction = (num) =>
                {
                    _settings.CharacterPanelWidth.Value = num;
                    cardWidthCheckbox.UserLocale_SettingChanged(null, null);
                },
            };

            #endregion Appearance
        }

        private void CreateDelays()
        {
            #region Delays
            var p = new Panel()
            {
                Parent = _contentPanel,
                Width = ContentRegion.Width - 20,
                HeightSizingMode = SizingMode.AutoSize,
                ShowBorder = true,
                CanCollapse = true,
                SetLocalizedTitle = () => strings.Delays,
                TitleIcon = AsyncTexture2D.FromAssetId(155035),
            };

            var cP = new FlowPanel()
            {
                Parent = p,
                Location = new(5),
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
            };

            var subP = new Panel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
            };

            var keyDelayLabel = new Label()
            {
                Parent = subP,
                AutoSizeWidth = true,
                Height = 20,
                SetLocalizedText = () => string.Format(strings.KeyDelay, _settings.KeyDelay.Value),
                SetLocalizedTooltip = () => strings.KeyDelay_Tooltip,
            };

            _ = new TrackBar()
            {
                Location = new(225, 2),
                Parent = subP,
                MinValue = 0,
                MaxValue = 500,
                Value = _settings.KeyDelay.Value,
                Width = ContentRegion.Width - 35 - 225,
                ValueChangedAction = (num) =>
                {
                    _settings.KeyDelay.Value = num;
                    keyDelayLabel.UserLocale_SettingChanged(null, null);
                },
            };

            subP = new Panel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
            };

            var filterDelayLabel = new Label()
            {
                Parent = subP,
                AutoSizeWidth = true,
                Height = 20,
                SetLocalizedText = () => string.Format(strings.FilterDelay, _settings.FilterDelay.Value),
                SetLocalizedTooltip = () => strings.FilterDelay_Tooltip,
            };

            _ = new TrackBar()
            {
                Location = new(225, 2),
                Parent = subP,
                MinValue = 0,
                MaxValue = 500,
                Value = _settings.FilterDelay.Value,
                Width = ContentRegion.Width - 35 - 225,
                ValueChangedAction = (num) =>
                {
                    _settings.FilterDelay.Value = num;
                    filterDelayLabel.UserLocale_SettingChanged(num, null);
                },
            };

            subP = new Panel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
            };

            var swapDelayLabel = new Label()
            {
                Parent = subP,
                AutoSizeWidth = true,
                Height = 20,
                SetLocalizedText = () => string.Format(strings.SwapDelay, _settings.SwapDelay.Value),
                SetLocalizedTooltip = () => strings.SwapDelay_Tooltip,
            };

            _ = new TrackBar()
            {
                Location = new(225, 2),
                Parent = subP,
                MinValue = 500,
                MaxValue = 30000,
                Value = _settings.SwapDelay.Value,
                Width = ContentRegion.Width - 35 - 225,
                ValueChangedAction = (num) =>
                {
                    _settings.SwapDelay.Value = num;
                    swapDelayLabel.UserLocale_SettingChanged(null, null);
                },
            };
            #endregion Delays
        }

        private void CreateGeneral()
        {
            #region General
            var p = new Panel()
            {
                Parent = _contentPanel,
                Width = ContentRegion.Width - 20,
                HeightSizingMode = SizingMode.AutoSize,
                ShowBorder = true,
                CanCollapse = true,
                SetLocalizedTitle = () => strings.GeneralAndWindows,
                TitleIcon = AsyncTexture2D.FromAssetId(157109),
            };

            var cP = new FlowPanel()
            {
                Parent = p,
                Location = new(5),
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = _settings.LoadCachedAccounts.Value,
                CheckedChangedAction = (b) => _settings.LoadCachedAccounts.Value = b,
                SetLocalizedText = () => strings.LoadCachedAccounts,
                SetLocalizedTooltip = () => strings.LoadCachedAccounts_Tooltip,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = _settings.ShowStatusWindow.Value,
                SetLocalizedText = () => strings.ShowStatusWindow_Name,
                SetLocalizedTooltip = () => strings.ShowStatusWindow_Tooltip,
                CheckedChangedAction = (b) => _settings.ShowStatusWindow.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = _settings.ShowChoyaSpinner.Value,
                SetLocalizedText = () => strings.ShowChoyaSpinner,
                CheckedChangedAction = (b) => _settings.ShowChoyaSpinner.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = _settings.OpenSidemenuOnSearch.Value,
                CheckedChangedAction = (b) => _settings.OpenSidemenuOnSearch.Value = b,
                SetLocalizedText = () => strings.OpenSidemenuOnSearch,
                SetLocalizedTooltip = () => strings.OpenSidemenuOnSearch_Tooltip,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = _settings.FocusSearchOnShow.Value,
                CheckedChangedAction = (b) => _settings.FocusSearchOnShow.Value = b,
                SetLocalizedText = () => strings.FocusSearchOnShow,
                SetLocalizedTooltip = () => strings.FocusSearchOnShow_Tooltip,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = _settings.ShowNotifications.Value,
                CheckedChangedAction = (b) => _settings.ShowNotifications.Value = b,
                SetLocalizedText = () => strings.ShowNotifications,
                SetLocalizedTooltip = () => strings.ShowNotifications_Tooltip,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = _settings.DebugMode.Value,
                CheckedChangedAction = (b) => _settings.DebugMode.Value = b,
                SetLocalizedText = () => strings.DebugMode_Name,
                SetLocalizedTooltip = () => strings.DebugMode_Tooltip,
            };
            #endregion
        }

        private void GetFontSize(SettingEntry<int> setting, string item)
        {
            if (int.TryParse(item, out int fontSize))
            {
                setting.Value = fontSize;
            }
        }

        public void OnLanguageChanged(object s = null, EventArgs e = null)
        {
            Name = string.Format(strings.ItemSettings, $"{Characters.ModuleName}");
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (gameTime.TotalGameTime.TotalMilliseconds - _tick >= 1000)
            {
                _tick = gameTime.TotalGameTime.TotalMilliseconds;

                if (GameService.GameIntegration.Gw2Instance.Gw2HasFocus)
                {
                    _sharedSettingsView?.SetWindowOffsetImages();
                }
            }
        }
    }
}
