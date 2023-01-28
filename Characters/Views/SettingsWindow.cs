using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Settings;
using Characters.Res;
using Kenedia.Modules.Characters.Extensions;
using Kenedia.Modules.Characters.Interfaces;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.IO;
using static Blish_HUD.ContentService;
using static Kenedia.Modules.Characters.Utility.WindowsUtil.WindowsUtil;
using Bitmap = System.Drawing.Bitmap;
using Checkbox = Kenedia.Modules.Core.Controls.Checkbox;
using Dropdown = Kenedia.Modules.Core.Controls.Dropdown;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using Image = Blish_HUD.Controls.Image;
using KeybindingAssigner = Kenedia.Modules.Core.Controls.KeybindingAssigner;
using Label = Kenedia.Modules.Core.Controls.Label;
using Panel = Kenedia.Modules.Core.Controls.Panel;
using TrackBar = Kenedia.Modules.Core.Controls.TrackBar;

namespace Kenedia.Modules.Characters.Views
{
    public class SettingsWindow : StandardWindow, ILocalizable
    {
        private readonly AsyncTexture2D _subWindowEmblem = AsyncTexture2D.FromAssetId(156027);
        private readonly AsyncTexture2D _mainWindowEmblem = AsyncTexture2D.FromAssetId(156015);
        private readonly BitmapFont _titleFont = GameService.Content.DefaultFont32;

        private Rectangle _mainEmblemRectangle;
        private Rectangle _subEmblemRectangle;
        private Rectangle _titleRectangle;

        private Label _customFontSizeLabel;
        private Dropdown _customFontSize;

        private Label _customNameFontSizeLabel;
        private Dropdown _customNameFontSize;

        private NumberBox _topOffsetBox;
        private NumberBox _leftOffsetBox;
        private NumberBox _bottomOffsetBox;
        private NumberBox _rightOffsetBox;

        private Image _topLeftImage;
        private Image _topRightImage;
        private Image _bottomLeftImage;
        private Image _bottomRightImage;

        private readonly FlowPanel _contentPanel;

        private double _tick;

        public SettingsWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion)
        {
            _contentPanel = new()
            {
                Parent = this,
                Width = ContentRegion.Width,
                Height = ContentRegion.Height,
                ControlPadding = new(0, 10),
                CanScroll = true,
            };

            CreateAppearance();
            CreateBehavior();
            CreateDelays();
            CreateGeneral();
            CreateLayout();
            CreateKeybinds();

            Characters.ModuleInstance.LanguageChanged += OnLanguageChanged;
            OnLanguageChanged();
        }

        private SettingsModel Settings => Characters.ModuleInstance.Settings;

        private void CreateKeybinds()
        {
            #region Keybinds
            var p = new Panel()
            {
                Parent = _contentPanel,
                Width = ContentRegion.Width - 20,
                HeightSizingMode = SizingMode.AutoSize,
                ShowBorder = true,
            };

            _ = new Image()
            {
                Texture = AsyncTexture2D.FromAssetId(156734),
                Parent = p,
                Size = new(30, 30),
            };

            _ = new Label()
            {
                Parent = p,
                AutoSizeWidth = true,
                Location = new(35, 0),
                Height = 30,
                SetLocalizedText = () => strings.Keybinds,
            };

            var cP = new FlowPanel()
            {
                Parent = p,
                Location = new(5, 35),
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
            };

            _ = new KeybindingAssigner()
            {
                Parent = cP,
                Width = ContentRegion.Width - 35,
                KeyBinding = Settings.LogoutKey.Value,
                KeybindChangedAction = (kb) => Settings.LogoutKey.Value = kb,
                SetLocalizedKeyBindingName = () => strings.Logout,
                SetLocalizedTooltip = () => strings.LogoutDescription,
            };

            _ = new KeybindingAssigner()
            {
                Parent = cP,
                Width = ContentRegion.Width - 35,
                KeyBinding = Settings.ShortcutKey.Value,
                KeybindChangedAction = (kb) => Settings.ShortcutKey.Value = kb,
                SetLocalizedKeyBindingName = () => strings.ShortcutToggle_DisplayName,
                SetLocalizedTooltip = () => strings.ShortcutToggle_Description,
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
            };

            _ = new Image()
            {
                Texture = AsyncTexture2D.FromAssetId(157092),
                Parent = p,
                Size = new(30, 30),
            };

            _ = new Label()
            {
                Parent = p,
                AutoSizeWidth = true,
                Location = new(35, 0),
                Height = 30,
                SetLocalizedText = () => strings.SwapBehavior,
            };

            var cP = new FlowPanel()
            {
                Parent = p,
                Location = new(5, 35),
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = Settings.CloseWindowOnSwap.Value,
                SetLocalizedText = () => strings.CloseWindowOnSwap_DisplayName,
                SetLocalizedTooltip = () => strings.CloseWindowOnSwap_Description,
                CheckedChangedAction = (b) => Settings.CloseWindowOnSwap.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = Settings.EnterOnSwap.Value,
                SetLocalizedText = () => strings.EnterOnSwap_DisplayName,
                SetLocalizedTooltip = () => strings.EnterOnSwap_Description,
                CheckedChangedAction = (b) => Settings.EnterOnSwap.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = Settings.DoubleClickToEnter.Value,
                SetLocalizedText = () => strings.DoubleClickToEnter_DisplayName,
                SetLocalizedTooltip = () => strings.DoubleClickToEnter_Description,
                CheckedChangedAction = (b) => Settings.DoubleClickToEnter.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = Settings.EnterToLogin.Value,
                SetLocalizedText = () => strings.EnterToLogin_DisplayName,
                SetLocalizedTooltip = () => strings.EnterToLogin_Description,
                CheckedChangedAction = (b) => Settings.EnterToLogin.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = Settings.UseOCR.Value,
                SetLocalizedText = () => strings.UseOCR,
                SetLocalizedTooltip = () => strings.UseOCR_Tooltip,
                CheckedChangedAction = (b) => Settings.UseOCR.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = Settings.AutoSortCharacters.Value,
                SetLocalizedText = () => strings.AutoFix,
                SetLocalizedTooltip = () => strings.AutoFix_Tooltip,
                CheckedChangedAction = (b) => Settings.AutoSortCharacters.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = Settings.FilterDiacriticsInsensitive.Value,
                SetLocalizedText = () => strings.FilterDiacriticsInsensitive_DisplayName,
                SetLocalizedTooltip = () => strings.FilterDiacriticsInsensitive_Description,
                CheckedChangedAction = (b) => Settings.FilterDiacriticsInsensitive.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = Settings.FilterAsOne.Value,
                SetLocalizedText = () => strings.FilterAsOne_DisplayName,
                SetLocalizedTooltip = () => strings.FilterAsOne_Description,
                CheckedChangedAction = (b) => Settings.FilterAsOne.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = Settings.UseBetaGamestate.Value,
                SetLocalizedText = () => strings.UseBetaGameState_DisplayName,
                SetLocalizedTooltip = () => strings.UseBetaGameState_Description,
                CheckedChangedAction = (b) => Settings.UseBetaGamestate.Value = b,
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
            };

            _ = new Image()
            {
                Texture = AsyncTexture2D.FromAssetId(156740),
                Parent = p,
                Size = new(30, 30),
            };

            _ = new Label()
            {
                Parent = p,
                AutoSizeWidth = true,
                Location = new(35, 0),
                Height = 30,
                SetLocalizedText = () => strings.Appearance,
            };

            var cP = new FlowPanel()
            {
                Parent = p,
                Location = new(5, 35),
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = Settings.ShowCornerIcon.Value,
                SetLocalizedText = () => strings.ShowCorner_Name,
                SetLocalizedTooltip = () => string.Format(strings.ShowCorner_Tooltip, Characters.ModuleInstance.Name),
                CheckedChangedAction = (b) => Settings.ShowCornerIcon.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = Settings.ShowRandomButton.Value,
                SetLocalizedText = () => strings.ShowRandomButton_Name,
                SetLocalizedTooltip = () => strings.ShowRandomButton_Description,
                CheckedChangedAction = (b) => Settings.ShowRandomButton.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = Settings.ShowStatusWindow.Value,
                SetLocalizedText = () => strings.ShowStatusWindow_Name,
                SetLocalizedTooltip = () => strings.ShowStatusWindow_Description,
                CheckedChangedAction = (b) => Settings.ShowStatusWindow.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = Settings.ShowDetailedTooltip.Value,
                SetLocalizedText = () => string.Format(strings.ShowItem, strings.DetailedTooltip),
                SetLocalizedTooltip = () => strings.DetailedTooltip_Description,
                CheckedChangedAction = (b) => Settings.ShowDetailedTooltip.Value = b,
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
                SelectedItem = Settings.PanelSize.Value.GetPanelSize(),
                ValueChangedAction = (b) => Settings.PanelSize.Value = b.GetPanelSize(),
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
                SelectedItem = Settings.PanelLayout.Value.GetPanelLayout(),
                ValueChangedAction = (b) => Settings.PanelLayout.Value = b.GetPanelLayout(),
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
                SetLocalizedText = () => string.Format("Font Size: {0}", Settings.CustomCharacterFontSize.Value),
            };

            _customFontSize = new Dropdown()
            {
                Parent = subP,
                Location = new(250, 0),
                SelectedItem = Settings.CustomCharacterFontSize.Value.ToString(),
                ValueChangedAction = (str) => { GetFontSize(Settings.CustomCharacterFontSize, str); _customFontSizeLabel.UserLocale_SettingChanged(null, null); },
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
                SetLocalizedText = () => string.Format("Name Font Size: {0}", Settings.CustomCharacterNameFontSize.Value),
            };

            _customNameFontSize = new()
            {
                Parent = subP,
                Location = new(250, 0),
                SelectedItem = Settings.CustomCharacterNameFontSize.Value.ToString(),
                ValueChangedAction = (str) => { GetFontSize(Settings.CustomCharacterNameFontSize, str); _customNameFontSizeLabel.UserLocale_SettingChanged(null, null); },
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
                SetLocalizedText = () => string.Format("Icon Size {0}x{0} px", Settings.CustomCharacterIconSize.Value),
            };
            _ = new TrackBar()
            {
                Parent = subP,
                Location = new(250, 0),
                MinValue = 8,
                MaxValue = 256,
                Width = ContentRegion.Width - 35 - 250,
                Value = Settings.CustomCharacterIconSize.Value,
                ValueChangedAction = (num) =>
                {
                    Settings.CustomCharacterIconSize.Value = num;
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
                Checked = Settings.CharacterPanelFixedWidth.Value,
                CheckedChangedAction = (b) => Settings.CharacterPanelFixedWidth.Value = b,
                SetLocalizedText = () => string.Format("Card Width {0} px", Settings.CharacterPanelWidth.Value),
                SetLocalizedTooltip = () => string.Format("Sets the character card width to {0} px", Settings.CharacterPanelWidth.Value),
            };

            _ = new TrackBar()
            {
                Parent = subP,
                Location = new(250, 0),
                MinValue = 25,
                MaxValue = 750,
                Width = ContentRegion.Width - 35 - 250,
                Value = Settings.CharacterPanelWidth.Value,
                ValueChangedAction = (num) =>
                {
                    Settings.CharacterPanelWidth.Value = num;
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
            };

            _ = new Image()
            {
                Texture = AsyncTexture2D.FromAssetId(155035),
                Parent = p,
                Size = new(30, 30),
            };

            _ = new Label()
            {
                Parent = p,
                AutoSizeWidth = true,
                Location = new(35, 0),
                Height = 30,
                SetLocalizedText = () => strings.Delays,
            };

            var cP = new FlowPanel()
            {
                Parent = p,
                Location = new(5, 35),
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
                SetLocalizedText = () => string.Format(strings.KeyDelay_DisplayName, Settings.KeyDelay.Value),
                SetLocalizedTooltip = () => strings.KeyDelay_Description,
            };

            _ = new TrackBar()
            {
                Location = new(225, 2),
                Parent = subP,
                MinValue = 0,
                MaxValue = 500,
                Value = Settings.KeyDelay.Value,
                Width = ContentRegion.Width - 35 - 225,
                ValueChangedAction = (num) =>
                {
                    Settings.KeyDelay.Value = num;
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
                SetLocalizedText = () => string.Format(strings.FilterDelay_DisplayName, Settings.FilterDelay.Value),
                SetLocalizedTooltip = () => strings.FilterDelay_Description,
            };

            _ = new TrackBar()
            {
                Location = new(225, 2),
                Parent = subP,
                MinValue = 0,
                MaxValue = 500,
                Value = Settings.FilterDelay.Value,
                Width = ContentRegion.Width - 35 - 225,
                ValueChangedAction = (num) =>
                {
                    Settings.FilterDelay.Value = num;
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
                SetLocalizedText = () => string.Format(strings.SwapDelay_DisplayName, Settings.SwapDelay.Value),
                SetLocalizedTooltip = () => strings.SwapDelay_Description,
            };

            _ = new TrackBar()
            {
                Location = new(225, 2),
                Parent = subP,
                MinValue = 0,
                MaxValue = 60000,
                Value = Settings.SwapDelay.Value,
                Width = ContentRegion.Width - 35 - 225,
                ValueChangedAction = (num) =>
                {
                    Settings.SwapDelay.Value = num;
                    swapDelayLabel.UserLocale_SettingChanged(null, null);
                },
            };
            #endregion Delays
        }

        private void CreateLayout()
        {
            #region Layout
            var p = new Panel()
            {
                Parent = _contentPanel,
                Width = ContentRegion.Width - 20,
                HeightSizingMode = SizingMode.AutoSize,
                ShowBorder = true,
            };

            _ = new Image()
            {
                Texture = AsyncTexture2D.FromAssetId(156736),
                Parent = p,
                Size = new(30, 30),
            };

            _ = new Label()
            {
                Parent = p,
                AutoSizeWidth = true,
                Location = new(35, 0),
                Height = 30,
                SetLocalizedText = () => string.Format(strings.ItemSettings, "Layout"),
            };

            var cFP = new FlowPanel()
            {
                Parent = p,
                Location = new(5, 35),
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new(3, 3),
            };

            var cP = new FlowPanel()
            {
                Parent = cFP,
                HeightSizingMode = SizingMode.AutoSize,
                Width = (ContentRegion.Width - 20) / 2,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
            };

            var pp = new FlowPanel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
            };

            _ = new Label()
            {
                Parent = pp,
                Width = 150,
                Location = new(35, 0),
                Height = 20,
                SetLocalizedText = () => "Top Offset",
            };
            _topOffsetBox = new()
            {
                Parent = pp,
                MinValue = -50,
                MaxValue = 50,
                Value = Settings.WindowOffset.Value.Top,
                SetLocalizedTooltip = () => "Top",
                ValueChangedAction = (num) => UpdateOffset(null, num),
            };

            pp = new FlowPanel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
            };
            _ = new Label()
            {
                Parent = pp,
                Width = 150,
                Location = new(35, 0),
                Height = 20,
                SetLocalizedText = () => "Left Offset",
            };
            _leftOffsetBox = new()
            {
                Parent = pp,
                MinValue = -50,
                MaxValue = 50,
                Value = Settings.WindowOffset.Value.Left,
                SetLocalizedTooltip = () => "Left",
                ValueChangedAction = (num) => UpdateOffset(null, num),
            };

            pp = new FlowPanel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
            };
            _ = new Label()
            {
                Parent = pp,
                Width = 150,
                Location = new(35, 0),
                Height = 20,
                SetLocalizedText = () => "Bottom Offset",
            };
            _bottomOffsetBox = new()
            {
                Parent = pp,
                MinValue = -50,
                MaxValue = 50,
                Value = Settings.WindowOffset.Value.Bottom,
                SetLocalizedTooltip = () => "Bottom",
                ValueChangedAction = (num) => UpdateOffset(null, num),
            };

            pp = new FlowPanel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
            };
            _ = new Label()
            {
                Parent = pp,
                Width = 150,
                Location = new(35, 0),
                Height = 20,
                SetLocalizedText = () => "Right Offset",
            };
            _rightOffsetBox = new()
            {
                Parent = pp,
                MinValue = -50,
                MaxValue = 50,
                Value = Settings.WindowOffset.Value.Right,
                SetLocalizedTooltip = () => "Right",
                ValueChangedAction = (num) => UpdateOffset(null, num),
            };

            var subCP = new FlowPanel()
            {
                Parent = cFP,
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new(5, 5),
            };

            cP = new FlowPanel()
            {
                Parent = subCP,
                HeightSizingMode = SizingMode.AutoSize,
                Width = 125,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(5, 5),
            };
            _ = new Label()
            {
                Parent = cP,
                SetLocalizedText = () => "Left Top",
                AutoSizeWidth = true,
                Visible = false,
            };
            _topLeftImage = new()
            {
                Parent = cP,
                BackgroundColor = Color.White,
                Size = new(100, _rightOffsetBox.Height * 2),
            };
            _ = new Label()
            {
                Parent = cP,
                SetLocalizedText = () => "Left Bottom",
                AutoSizeWidth = true,
                Visible = false,
            };
            _bottomLeftImage = new()
            {
                Parent = cP,
                BackgroundColor = Color.White,
                Size = new(100, _rightOffsetBox.Height * 2),
            };

            cP = new FlowPanel()
            {
                Parent = subCP,
                HeightSizingMode = SizingMode.AutoSize,
                Width = 125,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(5, 5),
            };
            _ = new Label()
            {
                Parent = cP,
                SetLocalizedText = () => "Right Top",
                AutoSizeWidth = true,
                Visible = false,
            };
            _topRightImage = new()
            {
                Parent = cP,
                BackgroundColor = Color.White,
                Size = new(100, _rightOffsetBox.Height * 2),
            };
            _ = new Label()
            {
                Parent = cP,
                SetLocalizedText = () => "Right Bottom",
                AutoSizeWidth = true,
                Visible = false,
            };
            _bottomRightImage = new()
            {
                Parent = cP,
                BackgroundColor = Color.White,
                Size = new(100, _rightOffsetBox.Height * 2),
            };

            #endregion
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
            };

            _ = new Image()
            {
                Texture = AsyncTexture2D.FromAssetId(157109), //Arenanet 156710 //Globe 517180
                Parent = p,
                Size = new(30, 30),
            };

            _ = new Label()
            {
                Parent = p,
                AutoSizeWidth = true,
                Location = new(35, 0),
                Height = 30,
                SetLocalizedText = () => string.Format(strings.ItemSettings, strings.GeneralAndWindows),
            };

            var cP = new FlowPanel()
            {
                Parent = p,
                Location = new(5, 35),
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = Settings.LoadCachedAccounts.Value,
                CheckedChangedAction = (b) => Settings.LoadCachedAccounts.Value = b,
                SetLocalizedText = () => strings.LoadCachedAccounts_DisplayName,
                SetLocalizedTooltip = () => strings.LoadCachedAccounts_Description,
            };

            _ = new Checkbox()
            {
                Parent = cP,
                Checked = Settings.OpenSidemenuOnSearch.Value,
                CheckedChangedAction = (b) => Settings.OpenSidemenuOnSearch.Value = b,
                SetLocalizedText = () => strings.OpenSidemenuOnSearch_DisplayName,
                SetLocalizedTooltip = () => strings.OpenSidemenuOnSearch_Description,
            };
            #endregion
        }

        private void UpdateOffset(object sender, int e)
        {
            Settings.WindowOffset.Value = new(_leftOffsetBox.Value, _topOffsetBox.Value, _rightOffsetBox.Value, _bottomOffsetBox.Value);

            SetTopLeftImage();
            SetTopRightImage();

            SetBottomLeftImage();
            SetBottomRightImage();
        }

        private void SetTopLeftImage()
        {
            RECT wndBounds = Characters.ModuleInstance.WindowRectangle;

            bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
            Point p = windowed ? new(Settings.WindowOffset.Value.Left, Settings.WindowOffset.Value.Top) : Point.Zero;

            using Bitmap bitmap = new(_topLeftImage.Width, _topLeftImage.Height);
            using var g = System.Drawing.Graphics.FromImage(bitmap);
            using MemoryStream s = new();
            g.CopyFromScreen(new System.Drawing.Point(wndBounds.Left + p.X, wndBounds.Top + p.Y), System.Drawing.Point.Empty, new(_topLeftImage.Width, _topLeftImage.Height));
            bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Bmp);
            _topLeftImage.Texture = s.CreateTexture2D();
        }

        private void SetBottomLeftImage()
        {
            RECT wndBounds = Characters.ModuleInstance.WindowRectangle;

            bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
            Point p = windowed ? new(Settings.WindowOffset.Value.Left, Settings.WindowOffset.Value.Bottom) : Point.Zero;

            using Bitmap bitmap = new(_bottomLeftImage.Width, _bottomLeftImage.Height);
            using var g = System.Drawing.Graphics.FromImage(bitmap);
            using MemoryStream s = new();
            g.CopyFromScreen(new System.Drawing.Point(wndBounds.Left + p.X, wndBounds.Bottom - _bottomLeftImage.Height + p.Y), System.Drawing.Point.Empty, new(_bottomLeftImage.Width, _bottomLeftImage.Height));
            bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Bmp);
            _bottomLeftImage.Texture = s.CreateTexture2D();
        }

        private void SetTopRightImage()
        {
            RECT wndBounds = Characters.ModuleInstance.WindowRectangle;

            bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
            Point p = windowed ? new(Settings.WindowOffset.Value.Right, Settings.WindowOffset.Value.Top) : Point.Zero;

            using Bitmap bitmap = new(_topRightImage.Width, _topRightImage.Height);
            using var g = System.Drawing.Graphics.FromImage(bitmap);
            using MemoryStream s = new();
            g.CopyFromScreen(new System.Drawing.Point(wndBounds.Right - _topRightImage.Width + p.X, wndBounds.Top + p.Y), System.Drawing.Point.Empty, new(_topRightImage.Width, _topRightImage.Height));
            bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Bmp);
            _topRightImage.Texture = s.CreateTexture2D();

        }

        private void SetBottomRightImage()
        {
            RECT wndBounds = Characters.ModuleInstance.WindowRectangle;

            bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
            Point p = windowed ? new(Settings.WindowOffset.Value.Right, Settings.WindowOffset.Value.Bottom) : Point.Zero;

            using Bitmap bitmap = new(_bottomLeftImage.Width, _bottomLeftImage.Height);
            using var g = System.Drawing.Graphics.FromImage(bitmap);
            using MemoryStream s = new();
            g.CopyFromScreen(new System.Drawing.Point(wndBounds.Right - _bottomRightImage.Width + p.X, wndBounds.Bottom - _bottomRightImage.Height + p.Y), System.Drawing.Point.Empty, new(_bottomRightImage.Width, _bottomRightImage.Height));
            bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Bmp);
            _bottomRightImage.Texture = s.CreateTexture2D();

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
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _subEmblemRectangle = new(-43 + 64, -58 + 64, 64, 64);
            _mainEmblemRectangle = new(-43, -58, 128, 128);

            MonoGame.Extended.RectangleF titleBounds = _titleFont.GetStringRectangle(string.Format(strings.ItemSettings, $"{Characters.ModuleInstance.Name}"));
            _titleRectangle = new(80, 5, (int)titleBounds.Width, Math.Max(30, (int)titleBounds.Height));
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            spriteBatch.DrawOnCtrl(
                this,
                _mainWindowEmblem,
                _mainEmblemRectangle,
                _mainWindowEmblem.Bounds,
                Color.White,
                0f,
                default);

            spriteBatch.DrawOnCtrl(
                this,
                _subWindowEmblem,
                _subEmblemRectangle,
                _subWindowEmblem.Bounds,
                Color.White,
                0f,
                default);

            if (_titleRectangle.Width < bounds.Width - (_subEmblemRectangle.Width - 20))
            {
                spriteBatch.DrawStringOnCtrl(
                    this,
                    string.Format(strings.ItemSettings, $"{Characters.ModuleInstance.Name}"),
                    _titleFont,
                    _titleRectangle,
                    Colors.ColonialWhite, // new Color(247, 231, 182, 97),
                    false,
                    HorizontalAlignment.Left,
                    VerticalAlignment.Bottom);
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Characters.ModuleInstance.LanguageChanged -= OnLanguageChanged;

            Children.DisposeAll();
            _contentPanel.Children.DisposeAll();

            _subWindowEmblem.Dispose();
            _mainWindowEmblem.Dispose();
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (gameTime.TotalGameTime.TotalMilliseconds - _tick >= 1000)
            {
                _tick = gameTime.TotalGameTime.TotalMilliseconds;

                if (GameService.GameIntegration.Gw2Instance.Gw2HasFocus)
                {
                    SetTopLeftImage();
                    SetTopRightImage();

                    SetBottomLeftImage();
                    SetBottomRightImage();
                }
            }
        }
    }
}
