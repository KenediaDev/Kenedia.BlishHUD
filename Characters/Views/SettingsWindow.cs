using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Settings;
using Characters.Res;
using Kenedia.Modules.Characters.Extensions;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using static Blish_HUD.ContentService;
using Checkbox = Kenedia.Modules.Core.Controls.Checkbox;
using Dropdown = Kenedia.Modules.Core.Controls.Dropdown;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
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
        private readonly SettingsModel _settings;
        private double _tick;

        public SettingsWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, SharedSettingsView sharedSettingsView, OCR ocr, SettingsModel settings) : base(background, windowRegion, contentRegion)
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

            CreateOCR();
            CreateAppearance();
            CreateBehavior();
            CreateDelays();
            CreateGeneral();

            CreateKeybinds();

            GameService.Overlay.UserLocale.SettingChanged += OnLanguageChanged;
            OnLanguageChanged();
        }

        public SemVer.Version Version { get; set; }

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
                        Enabled= kb.Enabled,
                        IgnoreWhenInTextField= true,         
                    };
                },
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
                Checked = _settings.EnterOnSwap.Value,
                SetLocalizedText = () => strings.EnterOnSwap,
                SetLocalizedTooltip = () => strings.EnterOnSwap_Tooltip,
                CheckedChangedAction = (b) => _settings.EnterOnSwap.Value = b,
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
                Checked = _settings.FilterAsOne.Value,
                SetLocalizedText = () => strings.FilterAsOne,
                SetLocalizedTooltip = () => strings.FilterAsOne_Tooltip,
                CheckedChangedAction = (b) => _settings.FilterAsOne.Value = b,
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
                MinValue = 0,
                MaxValue = 60000,
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
                Checked = _settings.EnableRadialMenu.Value,
                SetLocalizedText = () => strings.EnableRadialMenu,
                SetLocalizedTooltip = () => strings.EnableRadialMenu_Tooltip,
                CheckedChangedAction = (b) => _settings.EnableRadialMenu.Value = b,
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

            MonoGame.Extended.RectangleF titleBounds = _titleFont.GetStringRectangle(string.Format(strings.ItemSettings, $"{Characters.ModuleName}"));
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
                    string.Format(strings.ItemSettings, $"{Characters.ModuleName}"),
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

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if(Version != null) spriteBatch.DrawStringOnCtrl(this, $"v. {Version}", Content.DefaultFont16, new(bounds.Right - 150, bounds.Top + 10, 100, 30), Color.White, false, true, 1, HorizontalAlignment.Right, VerticalAlignment.Top);
        }
    }
}
