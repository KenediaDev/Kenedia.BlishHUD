using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Settings;
using Characters.Res;
using Kenedia.Modules.Characters.Extensions;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Res;
using Kenedia.Modules.Core.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using static Blish_HUD.ContentService;
using Checkbox = Kenedia.Modules.Core.Controls.Checkbox;
using Dropdown = Kenedia.Modules.Core.Controls.Dropdown;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using Image = Blish_HUD.Controls.Image;
using KeybindingAssigner = Kenedia.Modules.Core.Controls.KeybindingAssigner;
using Label = Kenedia.Modules.Core.Controls.Label;
using Panel = Kenedia.Modules.Core.Controls.Panel;
using StandardWindow = Kenedia.Modules.Core.Views.StandardWindow;
using TrackBar = Kenedia.Modules.Core.Controls.TrackBar;

namespace Kenedia.Modules.Characters.Views
{
    public class SettingsWindow : StandardWindow
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

        private readonly FlowPanel _contentPanel;
        private readonly SharedSettingsView _sharedSettingsView;
        private readonly OCR _ocr;

        private double _tick;

        public SettingsWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, SharedSettingsView sharedSettingsView, OCR ocr) : base(background, windowRegion, contentRegion)
        {
            _sharedSettingsView = sharedSettingsView;
            _ocr = ocr;

            _contentPanel = new()
            {
                Parent = this,
                Width = ContentRegion.Width,
                Height = ContentRegion.Height,
                ControlPadding = new(0, 10),
                CanScroll = true,
            };

            CreateOCR();
            CreateAppearance();
            CreateBehavior();
            CreateDelays();
            CreateGeneral();

            CreateKeybinds();

            GameService.Overlay.UserLocale.SettingChanged += OnLanguageChanged;
            OnLanguageChanged();
        }

        private SettingsModel Settings => Characters.ModuleInstance.Settings;

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
                Checked = Settings.UseOCR.Value,
                SetLocalizedText = () => strings.UseOCR,
                SetLocalizedTooltip = () => strings.UseOCR_Tooltip,
                CheckedChangedAction = (b) => Settings.UseOCR.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = settingsFlowPanel,
                Checked = Settings.UseBetaGamestate.Value,
                SetLocalizedText = () => strings.UseBetaGameState_DisplayName,
                SetLocalizedTooltip = () => strings.UseBetaGameState_Description,
                CheckedChangedAction = (b) => Settings.UseBetaGamestate.Value = b,
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

            _ = new KeybindingAssigner()
            {
                Parent = cP,
                Width = ContentRegion.Width - 35,
                KeyBinding = Settings.RadialKey.Value,
                KeybindChangedAction = (kb) => Settings.RadialKey.Value = kb,
                SetLocalizedKeyBindingName = () => strings.RadialMenuKey,
                SetLocalizedTooltip = () => strings.RadialMenuKey_Tooltip,
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
                TitleIcon = AsyncTexture2D.FromAssetId(157094),
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
                Checked = Settings.EnterOnSwap.Value,
                SetLocalizedText = () => strings.EnterOnSwap_DisplayName,
                SetLocalizedTooltip = () => strings.EnterOnSwap_Description,
                CheckedChangedAction = (b) => Settings.EnterOnSwap.Value = b,
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
                SetLocalizedText = () => string.Format(strings.FontSize, Settings.CustomCharacterFontSize.Value),
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
                SetLocalizedText = () => string.Format(strings.NameFontSize, Settings.CustomCharacterNameFontSize.Value),
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
                SetLocalizedText = () => string.Format(strings.IconSize, Settings.CustomCharacterIconSize.Value),
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
                SetLocalizedText = () => string.Format(strings.CardWith, Settings.CharacterPanelWidth.Value),
                SetLocalizedTooltip = () => string.Format(strings.CardWidth_Tooltip, Settings.CharacterPanelWidth.Value),
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
                Checked = Settings.LoadCachedAccounts.Value,
                CheckedChangedAction = (b) => Settings.LoadCachedAccounts.Value = b,
                SetLocalizedText = () => strings.LoadCachedAccounts_DisplayName,
                SetLocalizedTooltip = () => strings.LoadCachedAccounts_Description,
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
                Checked = Settings.EnableRadialMenu.Value,
                SetLocalizedText = () => strings.EnableRadialMenu,
                SetLocalizedTooltip = () => strings.EnableRadialMenu_Tooltip,
                CheckedChangedAction = (b) => Settings.EnableRadialMenu.Value = b,
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

            GameService.Overlay.UserLocale.SettingChanged -= OnLanguageChanged;

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
                    _sharedSettingsView?.UpdateOffset();
                }
            }
        }
    }
}
