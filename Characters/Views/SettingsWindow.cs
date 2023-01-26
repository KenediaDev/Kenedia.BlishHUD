using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Settings;
using Kenedia.Modules.Characters.Extensions;
using Kenedia.Modules.Characters.Interfaces;
using Kenedia.Modules.Characters.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using static Blish_HUD.ContentService;
using static Kenedia.Modules.Characters.Utility.WindowsUtil.WindowsUtil;
using static System.Net.Mime.MediaTypeNames;
using Image = Blish_HUD.Controls.Image;
using Bitmap = System.Drawing.Bitmap;
using Graphics = System.Drawing.Graphics;
using Characters.Res;

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

        private readonly KeybindingAssigner _logoutButton;
        private readonly KeybindingAssigner _toggleWindowButton;
        private readonly Checkbox _showCornerIcon;
        private readonly Checkbox _closeOnSwap;
        private readonly Checkbox _ignoreDiacritics;
        private readonly Checkbox _filterAsOne;
        private readonly Checkbox _openSidemenuOnSearch;
        private readonly Checkbox _loadCachedAccounts;
        private readonly Checkbox _showStatusPopup;
        private readonly Checkbox _loginAfterSelect;
        private readonly Checkbox _doubleClickLogin;
        private readonly Checkbox _enterLogin;
        private readonly Checkbox _autoFix;
        private readonly Checkbox _useOCR;
        private readonly Checkbox _showRandomButton;
        private readonly Checkbox _showTooltip;
        private readonly Checkbox _useBetaGamestate;

        private readonly Dropdown _panelSize;
        private readonly Dropdown _panelAppearance;

        private readonly Checkbox _characterPanelFixedWidth;
        private readonly TrackBar _characterPanelWidth;

        private readonly Label _customIconSizeLabel;
        private readonly TrackBar _customIconSize;

        private readonly Label _customFontSizeLabel;
        private readonly Dropdown _customFontSize;

        private readonly Label _customNameFontSizeLabel;
        private readonly Dropdown _customNameFontSize;

        private readonly Label _keyDelayLabel;
        private readonly TrackBar _keyDelay;
        private readonly Label _filterDelayLabel;
        private readonly TrackBar _filterDelay;
        private readonly Label _loadingDelayLabel;
        private readonly TrackBar _loadingDelay;

        private readonly CounterBox _leftOffsetBox;
        private readonly CounterBox _topOffsetBox;
        private readonly CounterBox _bottomOffsetBox;
        private readonly CounterBox _rightOffsetBox;
        private readonly StandardButton _saveOffsetButton;

        private readonly Image _bottomLeftImage;
        private readonly Image _bottomRightImage;

        private readonly Panel _contentPanel;
        private readonly List<Action> _languageActions = new();

        public SettingsWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion)
        {
            Label label;
            Panel p;
            Panel subP;

            _contentPanel = new()
            {
                Parent = this,
                Width = ContentRegion.Width,
                Height = ContentRegion.Height,
                CanScroll = true,
            };

            #region Keybinds
            p = new Panel()
            {
                Parent = _contentPanel,
                Width = ContentRegion.Width - 20,
                Height = 100,
                ShowBorder = true,
            };

            _ = new Image()
            {
                Texture = AsyncTexture2D.FromAssetId(156734),
                Parent = p,
                Size = new(30, 30),
            };

            var label1 = new Label()
            {
                Parent = p,
                AutoSizeWidth = true,
                Location = new(35, 0),
                Height = 30,
            };
            _languageActions.Add(() => label1.Text = strings.Keybinds);

            var cP = new FlowPanel()
            {
                Parent = p,
                Location = new(5, 35),
                HeightSizingMode = SizingMode.Fill,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
            };

            _logoutButton = new()
            {
                Parent = cP,
                Width = ContentRegion.Width - 35,
                KeyBinding = Settings.LogoutKey.Value,
            };
            _logoutButton.BindingChanged += LogoutButton_BindingChanged;
            _languageActions.Add(() =>
            {
                _logoutButton.KeyBindingName = strings.Logout;
                _logoutButton.BasicTooltipText = strings.LogoutDescription;
            });

            _toggleWindowButton = new()
            {
                Parent = cP,
                Width = ContentRegion.Width - 35,
                KeyBinding = Settings.ShortcutKey.Value,
            };
            _toggleWindowButton.BindingChanged += ToggleWindowButton_BindingChanged;
            _languageActions.Add(() =>
            {
                _toggleWindowButton.KeyBindingName = strings.ShortcutToggle_DisplayName;
                _toggleWindowButton.BasicTooltipText = strings.ShortcutToggle_Description;
            });
            #endregion Keybinds

            #region Behaviour
            p = new Panel()
            {
                Parent = _contentPanel,
                Location = new(0, p.LocalBounds.Bottom + 5),
                Width = ContentRegion.Width - 20,
                Height = 225,
                ShowBorder = true,
            };

            _ = new Image()
            {
                Texture = AsyncTexture2D.FromAssetId(157092),
                Parent = p,
                Size = new(30, 30),
            };

            var label2 = new Label()
            {
                Parent = p,
                AutoSizeWidth = true,
                Location = new(35, 0),
                Height = 30,
            };
            _languageActions.Add(() => label2.Text = strings.SwapBehaviour);

            cP = new FlowPanel()
            {
                Parent = p,
                Location = new(5, 35),
                HeightSizingMode = SizingMode.Fill,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
            };

            _closeOnSwap = new()
            {
                Parent = cP,
                Checked = Settings.CloseWindowOnSwap.Value,
            };
            _closeOnSwap.CheckedChanged += CloseOnSwap_CheckedChanged;
            _languageActions.Add(() =>
            {
                _closeOnSwap.Text = strings.CloseWindowOnSwap_DisplayName;
                _closeOnSwap.BasicTooltipText = strings.CloseWindowOnSwap_Description;
            });

            _loginAfterSelect = new()
            {
                Parent = cP,
                Checked = Settings.EnterOnSwap.Value,
            };
            _loginAfterSelect.CheckedChanged += LoginAfterSelect_CheckedChanged;
            _languageActions.Add(() =>
            {
                _loginAfterSelect.Text = strings.EnterOnSwap_DisplayName;
                _loginAfterSelect.BasicTooltipText = strings.EnterOnSwap_Description;
            });

            _doubleClickLogin = new()
            {
                Parent = cP,
                Checked = Settings.DoubleClickToEnter.Value,
            };
            _doubleClickLogin.CheckedChanged += DoubleClickLogin_CheckedChanged;
            _languageActions.Add(() =>
            {
                _doubleClickLogin.Text = strings.DoubleClickToEnter_DisplayName;
                _doubleClickLogin.BasicTooltipText = strings.DoubleClickToEnter_Description;
            });

            _enterLogin = new()
            {
                Parent = cP,
                Checked = Settings.EnterToLogin.Value,
            };
            _enterLogin.CheckedChanged += EnterLogin_CheckedChanged;
            _languageActions.Add(() =>
            {
                _enterLogin.Text = strings.EnterToLogin_DisplayName;
                _enterLogin.BasicTooltipText = strings.EnterToLogin_Description;
            });

            _useOCR = new()
            {
                Parent = cP,
                Checked = Settings.UseOCR.Value
            };
            _useOCR.CheckedChanged += UseOCR_CheckedChanged;
            _languageActions.Add(() =>
            {
                _useOCR.Text = strings.UseOCR;
                _useOCR.BasicTooltipText = strings.UseOCR_Tooltip;
            });

            _autoFix = new()
            {
                Parent = cP,
                Checked = Settings.AutoSortCharacters.Value
            };
            _autoFix.CheckedChanged += AutoFix_CheckedChanged;
            _languageActions.Add(() =>
            {
                _autoFix.Text = strings.AutoFix;
                _autoFix.BasicTooltipText = strings.AutoFix_Tooltip;
            });

            _ignoreDiacritics = new()
            {
                Parent = cP,
                Checked = Settings.FilterDiacriticsInsensitive.Value
            };
            _ignoreDiacritics.CheckedChanged += IgnoreDiacritics_CheckedChanged;
            _languageActions.Add(() =>
            {
                _ignoreDiacritics.Text = strings.FilterDiacriticsInsensitive_DisplayName;
                _ignoreDiacritics.BasicTooltipText = strings.FilterDiacriticsInsensitive_Description;
            });

            _filterAsOne = new()
            {
                Parent = cP,
                Checked = Settings.FilterAsOne.Value
            };
            _filterAsOne.CheckedChanged += FilterAsOne_CheckedChanged;
            _languageActions.Add(() =>
            {
                _filterAsOne.Text = strings.FilterAsOne_DisplayName;
                _filterAsOne.BasicTooltipText = strings.FilterAsOne_Description;
            });

            _useBetaGamestate = new()
            {
                Parent = cP,
                Checked = Settings.UseBetaGamestate.Value
            };
            _useBetaGamestate.CheckedChanged += UseBetaGamestate_CheckedChanged;
            _languageActions.Add(() =>
            {
                _useBetaGamestate.Text = strings.UseBetaGameState_DisplayName;
                _useBetaGamestate.BasicTooltipText = strings.UseBetaGameState_Description;
            });
            #endregion Behaviour

            #region Appearance
            p = new Panel()
            {
                Parent = _contentPanel,
                Location = new(0, p.LocalBounds.Bottom + 5),
                Width = ContentRegion.Width - 20,
                Height = 355,
                ShowBorder = true,
            };

            _ = new Image()
            {
                Texture = AsyncTexture2D.FromAssetId(156740),
                Parent = p,
                Size = new(30, 30),
            };

            var label3 = new Label()
            {
                Parent = p,
                AutoSizeWidth = true,
                Location = new(35, 0),
                Height = 30,
            };
            _languageActions.Add(() => label3.Text = strings.Appearance);

            cP = new FlowPanel()
            {
                Parent = p,
                Location = new(5, 35),
                HeightSizingMode = SizingMode.Fill,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
            };

            _showCornerIcon = new()
            {
                Parent = cP,
                Checked = Settings.ShowCornerIcon.Value,
            };
            _showCornerIcon.CheckedChanged += ShowCornerIcon_CheckedChanged;
            _languageActions.Add(() =>
            {
                _showCornerIcon.Text = strings.ShowCorner_Name;
                _showCornerIcon.BasicTooltipText = string.Format(strings.ShowCorner_Tooltip, Characters.ModuleInstance.Name);
            });

            _showRandomButton = new()
            {
                Parent = cP,
                Checked = Settings.ShowRandomButton.Value,
            };
            _showRandomButton.CheckedChanged += ShowRandomButton_CheckedChanged;
            _languageActions.Add(() =>
            {
                _showRandomButton.Text = strings.ShowRandomButton_Name;
                _showRandomButton.BasicTooltipText = strings.ShowRandomButton_Description;
            });

            _showStatusPopup = new()
            {
                Parent = cP,
                Checked = Settings.ShowStatusWindow.Value
            };
            _showStatusPopup.CheckedChanged += ShowStatusPopup_CheckedChanged;
            _languageActions.Add(() =>
            {
                _showStatusPopup.Text = strings.ShowStatusWindow_Name;
                _showStatusPopup.BasicTooltipText = strings.ShowStatusWindow_Description;
            });

            _showTooltip = new()
            {
                Parent = cP,
                Checked = Settings.ShowStatusWindow.Value
            };
            _showTooltip.CheckedChanged += ShowTooltip_CheckedChanged;
            _languageActions.Add(() =>
            {
                _showTooltip.Text = string.Format(strings.ShowItem, strings.DetailedTooltip);
                _showTooltip.BasicTooltipText = strings.DetailedTooltip_Description;
            });

            subP = new Panel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                Height = 30,
            };

            var label4 = new Label()
            {
                Parent = subP,
                AutoSizeWidth = true,
                Height = 30
            };
            _languageActions.Add(() => label4.Text = strings.CharacterDisplaySize);

            _panelSize = new()
            {
                Location = new(250, 0),
                Parent = subP,
            };
            _panelSize.ValueChanged += PanelSize_ValueChanged;
            _languageActions.Add(() =>
            {
                _panelSize.Items.Clear();
                _panelSize.SelectedItem = Settings.PanelSize.Value.GetPanelSize();
                _panelSize.Items.Add(strings.Small);
                _panelSize.Items.Add(strings.Normal);
                _panelSize.Items.Add(strings.Large);
                _panelSize.Items.Add(strings.Custom);
            });

            subP = new Panel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                Height = 30,
            };

            var label5 = new Label()
            {
                Parent = subP,
                AutoSizeWidth = true,
                Height = 30
            };
            _languageActions.Add(() => label5.Text = strings.CharacterDisplayOption);

            _panelAppearance = new()
            {
                Parent = subP,
                Location = new(250, 0),
            };
            _panelAppearance.ValueChanged += PanelAppearance_ValueChanged;
            _languageActions.Add(() =>
            {
                _panelAppearance.Items.Clear();
                _panelAppearance.SelectedItem = Settings.PanelLayout.Value.GetPanelLayout();
                _panelAppearance.Items.Add(strings.OnlyText);
                _panelAppearance.Items.Add(strings.OnlyIcons);
                _panelAppearance.Items.Add(strings.TextAndIcon);
            });

            subP = new Panel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                Height = 30,
            };
            _customFontSizeLabel = new()
            {
                Parent = subP,
                AutoSizeWidth = true,
            };
            _languageActions.Add(() => _customFontSizeLabel.Text = string.Format("Font Size: {0}", Settings.CustomCharacterFontSize.Value));

            _customFontSize = new()
            {
                Parent = subP,
                Location = new(250, 0),
                SelectedItem = Settings.CustomCharacterFontSize.Value.ToString(),
            };
            _customFontSize.ValueChanged += CustomFontSize_ValueChanged;

            subP = new Panel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                Height = 30,
            };
            _customNameFontSizeLabel = new()
            {
                Parent = subP,
                AutoSizeWidth = true,
            };
            _languageActions.Add(() => _customNameFontSizeLabel.Text = string.Format("Name Font Size: {0}", Settings.CustomCharacterNameFontSize.Value));

            _customNameFontSize = new()
            {
                Parent = subP,
                SelectedItem = Settings.CustomCharacterNameFontSize.Value.ToString(),
                Location = new(250, 0),
            };
            _customNameFontSize.ValueChanged += CustomNameFontSize_ValueChanged;

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
            _customIconSizeLabel = new()
            {
                Parent = subP,
                AutoSizeWidth = true,
            };
            _customIconSize = new()
            {
                Parent = subP,
                Location = new(250, 0),
                MinValue = 8,
                MaxValue = 256,
                Width = ContentRegion.Width - 35 - 250,
                Value = Settings.CustomCharacterIconSize.Value,
            };
            _languageActions.Add(() => _customIconSizeLabel.Text = string.Format("Icon Size {0}x{0} px", Settings.CustomCharacterIconSize.Value));
            _customIconSize.ValueChanged += CustomIconSize_ValueChanged;

            subP = new Panel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                Height = 30,
            };

            _characterPanelFixedWidth = new()
            {
                Parent = subP,
                Checked = Settings.CharacterPanelFixedWidth.Value,
            };
            _characterPanelFixedWidth.CheckedChanged += CharacterPanelFixedWidth_CheckedChanged;
            _languageActions.Add(() =>
            {
                _characterPanelFixedWidth.Text = string.Format("Card Width {0} px", Settings.CharacterPanelWidth.Value);
                _characterPanelFixedWidth.BasicTooltipText = string.Format("Sets the character card width to {0} px", Settings.CharacterPanelWidth.Value);
            });

            _characterPanelWidth = new()
            {
                Parent = subP,
                Location = new(250, 0),
                MinValue = 25,
                MaxValue = 750,
                Width = ContentRegion.Width - 35 - 250,
                Value = Settings.CharacterPanelWidth.Value,
            };
            _characterPanelWidth.ValueChanged += CharacterPanelWidth_ValueChanged;

            #endregion Appearance

            #region Delays
            p = new Panel()
            {
                Parent = _contentPanel,
                Location = new(0, p.LocalBounds.Bottom + 5),
                Width = ContentRegion.Width - 20,
                Height = 125,
                ShowBorder = true,
            };

            _ = new Image()
            {
                Texture = AsyncTexture2D.FromAssetId(155035),
                Parent = p,
                Size = new(30, 30),
            };

            var label6 = new Label()
            {
                Parent = p,
                AutoSizeWidth = true,
                Location = new(35, 0),
                Height = 30,
            };
            _languageActions.Add(() => label6.Text = strings.Delays);

            cP = new FlowPanel()
            {
                Parent = p,
                Location = new(5, 35),
                HeightSizingMode = SizingMode.Fill,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
            };

            subP = new Panel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
            };

            _keyDelayLabel = new Label()
            {
                Parent = subP,
                AutoSizeWidth = true,
                Height = 20,
            };
            _languageActions.Add(() =>
            {
                _keyDelayLabel.Text = string.Format(strings.KeyDelay_DisplayName, Settings.KeyDelay.Value);
                _keyDelayLabel.BasicTooltipText = strings.KeyDelay_Description;
            });
            _keyDelay = new()
            {
                Location = new(225, 2),
                Parent = subP,
                MinValue = 0,
                MaxValue = 500,
                Value = Settings.KeyDelay.Value,
                Width = ContentRegion.Width - 35 - 225,
            };
            _keyDelay.ValueChanged += KeyDelay_ValueChanged;

            subP = new Panel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
            };

            _filterDelayLabel = new Label()
            {
                Parent = subP,
                AutoSizeWidth = true,
                Height = 20,
            };
            _languageActions.Add(() =>
            {
                _filterDelayLabel.Text = string.Format(strings.FilterDelay_DisplayName, Settings.FilterDelay.Value);
                _filterDelayLabel.BasicTooltipText = strings.FilterDelay_Description;
            });
            _filterDelay = new()
            {
                Location = new(225, 2),
                Parent = subP,
                MinValue = 0,
                MaxValue = 500,
                Value = Settings.FilterDelay.Value,
                Width = ContentRegion.Width - 35 - 225,
            };
            _filterDelay.ValueChanged += FilterDelay_ValueChanged;

            subP = new Panel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
            };

            _loadingDelayLabel = new Label()
            {
                Parent = subP,
                AutoSizeWidth = true,
                Height = 20,
            };
            _languageActions.Add(() =>
            {
                _loadingDelayLabel.Text = string.Format(strings.SwapDelay_DisplayName, Settings.SwapDelay.Value);
                _loadingDelayLabel.BasicTooltipText = strings.SwapDelay_Description;
            });
            _loadingDelay = new()
            {
                Location = new(225, 2),
                Parent = subP,
                MinValue = 0,
                MaxValue = 60000,
                Value = Settings.SwapDelay.Value,
                Width = ContentRegion.Width - 35 - 225,
            };
            _loadingDelay.ValueChanged += LoadingDelay_ValueChanged;
            #endregion Delays

            #region Layout
            p = new Panel()
            {
                Parent = _contentPanel,
                Location = new(0, p.LocalBounds.Bottom + 5),
                Width = ContentRegion.Width - 20,
                Height = 150,
                ShowBorder = true,
            };

            _ = new Image()
            {
                Texture = AsyncTexture2D.FromAssetId(156736),
                Parent = p,
                Size = new(30, 30),
            };

            var label8 = new Label()
            {
                Parent = p,
                AutoSizeWidth = true,
                Location = new(35, 0),
                Height = 30,
            };
            _languageActions.Add(() => label8.Text = string.Format(strings.ItemSettings, "Layout"));

            var cFP = new FlowPanel()
            {
                Parent = p,
                Location = new(5, 35),
                HeightSizingMode = SizingMode.Fill,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new(3, 3),
            };

            cP = new FlowPanel()
            {
                Parent = cFP,
                HeightSizingMode = SizingMode.Fill,
                Width = (ContentRegion.Width - 20) / 3,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
            };

            _leftOffsetBox = new()
            {
                Parent = cP,
                MinValue = -50,
                MaxValue = 50,
                Value = Settings.WindowOffset.Value.Left,
                BasicTooltipText = "Left",
            };
            _leftOffsetBox.PropertyChanged += OffsetChanged;

            _bottomOffsetBox = new()
            {
                Parent = cP,
                MinValue = -50,
                MaxValue = 50,
                Value = Settings.WindowOffset.Value.Bottom,
                BasicTooltipText = "Bottom",
            };
            _bottomOffsetBox.PropertyChanged += OffsetChanged;

            _rightOffsetBox = new()
            {
                Parent = cP,
                MinValue = -50,
                MaxValue = 50,
                Value = Settings.WindowOffset.Value.Right,
                BasicTooltipText = "Right",
            };
            _rightOffsetBox.PropertyChanged += OffsetChanged;
            _saveOffsetButton = new()
            {
                Parent = cP,
                Text = "Save",
                Width = _rightOffsetBox.Width
            };
            _saveOffsetButton.Click += OffsetChanged;

            cP = new FlowPanel()
            {
                Parent = cFP,
                HeightSizingMode = SizingMode.Fill,
                Width = (ContentRegion.Width - 20) / 3 * 2,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(5, 5),
            };

            _bottomLeftImage = new()
            {
                Parent = cP,
                BackgroundColor = Color.White,
                Size = new(100, _rightOffsetBox.Height * 2),
            };

            _bottomRightImage = new()
            {
                Parent = cP,
                BackgroundColor = Color.White,
                Size = new(100, _rightOffsetBox.Height * 2),
            };

            #endregion

            #region General
            p = new Panel()
            {
                Parent = _contentPanel,
                Location = new(0, p.LocalBounds.Bottom + 5),
                Width = ContentRegion.Width - 20,
                Height = 105,
                ShowBorder = true,
            };

            _ = new Image()
            {
                Texture = AsyncTexture2D.FromAssetId(157109), //Arenanet 156710 //Globe 517180
                Parent = p,
                Size = new(30, 30),
            };

            var label7 = new Label()
            {
                Parent = p,
                AutoSizeWidth = true,
                Location = new(35, 0),
                Height = 30,
            };
            _languageActions.Add(() => label7.Text = string.Format(strings.ItemSettings, strings.GeneralAndWindows));

            cP = new FlowPanel()
            {
                Parent = p,
                Location = new(5, 35),
                HeightSizingMode = SizingMode.Fill,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
            };

            _loadCachedAccounts = new()
            {
                Parent = cP,
                Checked = Settings.LoadCachedAccounts.Value
            };
            _loadCachedAccounts.CheckedChanged += LoadCachedAccounts_CheckedChanged;
            _languageActions.Add(() =>
            {
                _loadCachedAccounts.Text = strings.LoadCachedAccounts_DisplayName;
                _loadCachedAccounts.BasicTooltipText = strings.LoadCachedAccounts_Description;
            });

            _openSidemenuOnSearch = new()
            {
                Parent = cP,
                Checked = Settings.OpenSidemenuOnSearch.Value
            };
            _openSidemenuOnSearch.CheckedChanged += OpenSidemenuOnSearch_CheckedChanged;
            _languageActions.Add(() =>
            {
                _openSidemenuOnSearch.Text = strings.OpenSidemenuOnSearch_DisplayName;
                _openSidemenuOnSearch.BasicTooltipText = strings.OpenSidemenuOnSearch_Description;
            });
            #endregion

            Characters.ModuleInstance.LanguageChanged += OnLanguageChanged;
            OnLanguageChanged();
        }

        private void OffsetChanged(object sender, EventArgs e)
        {
            Settings.WindowOffset.Value = new(_leftOffsetBox.Value, 0, _rightOffsetBox.Value, _bottomOffsetBox.Value);

            SetLeftImage();
            SetRightImage();
        }

        private void SetLeftImage()
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

        private void SetRightImage()
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

        private void UseBetaGamestate_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.UseBetaGamestate.Value = e.Checked;
        }

        private void FilterAsOne_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.FilterAsOne.Value = e.Checked;
        }

        private SettingsModel Settings => Characters.ModuleInstance.Settings;

        private void CharacterPanelWidth_ValueChanged(object sender, ValueEventArgs<float> e)
        {
            int? size = Convert.ToInt32(_characterPanelWidth.Value);

            if (size is not null and > 0)
            {
                Settings.CharacterPanelWidth.Value = (int)size;
                _characterPanelFixedWidth.Text = string.Format("Card Width {0} px", Settings.CharacterPanelWidth.Value);
                _characterPanelFixedWidth.BasicTooltipText = string.Format("Sets the character card width to {0} px", Settings.CharacterPanelWidth.Value);
            }
        }

        private void CharacterPanelFixedWidth_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.CharacterPanelFixedWidth.Value = e.Checked;
        }

        private void CustomNameFontSize_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (int.TryParse(_customNameFontSize.SelectedItem, out int fontSize))
            {
                _customNameFontSizeLabel.Text = string.Format("Name Font Size: {0}", fontSize);
                Settings.CustomCharacterNameFontSize.Value = fontSize;
            }
        }

        private void CustomIconSize_ValueChanged(object sender, ValueEventArgs<float> e)
        {
            int? size = Convert.ToInt32(_customIconSize.Value);

            if (size is not null and > 0)
            {
                Settings.CustomCharacterIconSize.Value = (int)size;
                _customIconSizeLabel.Text = string.Format("Icon Size {0}x{0} px", Settings.CustomCharacterIconSize.Value);
            }
        }

        private void CustomFontSize_ValueChanged(object sender, EventArgs e)
        {
            if (int.TryParse(_customFontSize.SelectedItem, out int fontSize))
            {
                _customFontSizeLabel.Text = string.Format("Font Size: {0}", fontSize);
                Settings.CustomCharacterFontSize.Value = fontSize;
            }
        }

        private void ShowTooltip_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.ShowDetailedTooltip.Value = e.Checked;
        }

        private void WindowMode_CheckedChanged(object sender, CheckChangedEvent e)
        {
            //Settings.WindowedMode.Value = e.Checked;
            //OffsetChanged(null, null);
        }

        private void LoadingDelay_ValueChanged(object sender, ValueEventArgs<float> e)
        {
            Settings.SwapDelay.Value = (int)_loadingDelay.Value;
            _loadingDelayLabel.Text = string.Format(strings.SwapDelay_DisplayName, Settings.SwapDelay.Value);
        }

        private void FilterDelay_ValueChanged(object sender, ValueEventArgs<float> e)
        {
            Settings.FilterDelay.Value = (int)_filterDelay.Value;
            _filterDelayLabel.Text = string.Format(strings.FilterDelay_DisplayName, Settings.FilterDelay.Value);
        }

        private void KeyDelay_ValueChanged(object sender, ValueEventArgs<float> e)
        {
            Settings.KeyDelay.Value = (int)_keyDelay.Value;
            _keyDelayLabel.Text = string.Format(strings.KeyDelay_DisplayName, Settings.KeyDelay.Value);
        }

        private void PanelAppearance_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Settings.PanelLayout.Value = _panelAppearance.SelectedItem.GetPanelLayout();
        }

        private void PanelSize_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Settings.PanelSize.Value = _panelSize.SelectedItem.GetPanelSize();
        }

        private void ShowStatusPopup_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.ShowStatusWindow.Value = e.Checked;
        }

        private void ShowRandomButton_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.ShowRandomButton.Value = e.Checked;
        }

        private void ShowCornerIcon_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.ShowCornerIcon.Value = e.Checked;
        }

        private void IgnoreDiacritics_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.FilterDiacriticsInsensitive.Value = e.Checked;
        }

        private void OpenSidemenuOnSearch_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.OpenSidemenuOnSearch.Value = e.Checked;
        }
        private void LoadCachedAccounts_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.LoadCachedAccounts.Value = e.Checked;
        }

        private void AutoFix_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.AutoSortCharacters.Value = e.Checked;
        }

        private void UseOCR_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.UseOCR.Value = e.Checked;
        }

        private void EnterLogin_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.EnterToLogin.Value = e.Checked;
        }

        private void DoubleClickLogin_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.DoubleClickToEnter.Value = e.Checked;
        }

        private void LoginAfterSelect_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.EnterOnSwap.Value = e.Checked;
        }

        private void CloseOnSwap_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.CloseWindowOnSwap.Value = e.Checked;
        }

        private void LogoutButton_BindingChanged(object sender, EventArgs e)
        {
            Settings.LogoutKey.Value = _logoutButton.KeyBinding;
        }

        private void ToggleWindowButton_BindingChanged(object sender, EventArgs e)
        {
            Settings.ShortcutKey.Value = _toggleWindowButton.KeyBinding;
        }

        public void OnLanguageChanged(object s = null, EventArgs e = null)
        {
            foreach (Action action in _languageActions)
            {
                action.Invoke();
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _subEmblemRectangle = new(-43 + 64, -58 + 64, 64, 64);
            _mainEmblemRectangle = new(-43, -58, 128, 128);

            MonoGame.Extended.RectangleF titleBounds = _titleFont.GetStringRectangle(string.Format(strings.ItemSettings, $"{Characters.ModuleInstance.Name}"));
            _titleRectangle = new(80, 5, (int)titleBounds.Width, Math.Max(30, (int)titleBounds.Height));
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

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

            _logoutButton.BindingChanged -= LogoutButton_BindingChanged;
            _toggleWindowButton.BindingChanged -= ToggleWindowButton_BindingChanged;
            _closeOnSwap.CheckedChanged -= CloseOnSwap_CheckedChanged;
            _loginAfterSelect.CheckedChanged -= LoginAfterSelect_CheckedChanged;
            _doubleClickLogin.CheckedChanged -= DoubleClickLogin_CheckedChanged;
            _enterLogin.CheckedChanged -= EnterLogin_CheckedChanged;
            _useOCR.CheckedChanged -= UseOCR_CheckedChanged;
            _autoFix.CheckedChanged -= AutoFix_CheckedChanged;
            _ignoreDiacritics.CheckedChanged -= IgnoreDiacritics_CheckedChanged;
            _showCornerIcon.CheckedChanged -= ShowCornerIcon_CheckedChanged;
            _showRandomButton.CheckedChanged -= ShowRandomButton_CheckedChanged;
            _showStatusPopup.CheckedChanged -= ShowStatusPopup_CheckedChanged;
            _showTooltip.CheckedChanged -= ShowTooltip_CheckedChanged;
            _panelSize.ValueChanged -= PanelSize_ValueChanged;
            _panelAppearance.ValueChanged -= PanelAppearance_ValueChanged;
            _customFontSize.ValueChanged -= CustomFontSize_ValueChanged;
            _customNameFontSize.ValueChanged -= CustomNameFontSize_ValueChanged;
            _customIconSize.ValueChanged -= CustomIconSize_ValueChanged;
            _characterPanelFixedWidth.CheckedChanged -= CharacterPanelFixedWidth_CheckedChanged;
            _characterPanelWidth.ValueChanged -= CharacterPanelWidth_ValueChanged;
            _keyDelay.ValueChanged -= KeyDelay_ValueChanged;
            _filterDelay.ValueChanged -= FilterDelay_ValueChanged;
            _loadingDelay.ValueChanged -= LoadingDelay_ValueChanged;
            _loadCachedAccounts.CheckedChanged -= LoadCachedAccounts_CheckedChanged;
            _openSidemenuOnSearch.CheckedChanged -= OpenSidemenuOnSearch_CheckedChanged;
            _filterAsOne.CheckedChanged -= FilterAsOne_CheckedChanged;
            Characters.ModuleInstance.LanguageChanged -= OnLanguageChanged;

            _languageActions.Clear();
            Children.DisposeAll();
            _contentPanel.Children.DisposeAll();

            _subWindowEmblem.Dispose();
            _mainWindowEmblem.Dispose();
        }
    }
}
