using Blish_HUD;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using System;
using SizingMode = Blish_HUD.Controls.SizingMode;
using ControlFlowDirection = Blish_HUD.Controls.ControlFlowDirection;
using Kenedia.Modules.QoL.Res;
using Kenedia.Modules.Core.Extensions;

namespace Kenedia.Modules.QoL.SubModules.GameResets
{
    public enum DateDisplayType
    {
        Short,
        ShortDays,
        Long,
    }

    public class GameResets : SubModule
    {
        private readonly FlowPanel _container;
        private readonly IconLabel _serverTime;
        private readonly IconLabel _serverReset;
        private readonly IconLabel _weeklyReset;

        private SettingEntry<bool> _showTooltips;
        private SettingEntry<bool> _showServerTime;
        private SettingEntry<bool> _showDailyReset;
        private SettingEntry<bool> _showWeeklyReset;
        private SettingEntry<bool> _showIcons;
        private SettingEntry<bool> _autoPosition;
        private SettingEntry<DateDisplayType> _dateDisplay;
        private SettingEntry<Point> _resetPosition;
        private bool _editPosition;

        public GameResets(SettingCollection settings) : base(settings)
        {
            UI_Elements.Add(_container = new()
            {
                Parent = GameService.Graphics.SpriteScreen,
                Visible = Enabled,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.Standard,
                Height = (GameService.Content.DefaultFont14.LineHeight * 3) + (2 * 3),
                FlowDirection = ControlFlowDirection.SingleBottomToTop,
                ControlPadding = new Vector2(0, 2),
                Location = _resetPosition.Value,

                CaptureInput = !_autoPosition.Value,
                CanDrag = !_autoPosition.Value,
            });

            _weeklyReset = new IconLabel()
            {
                Parent = _container,
                Texture = new(156692) { Size = new(GameService.Content.DefaultFont14.LineHeight) },
                Text = "0 days 00:00:00",
                BasicTooltipText = "Weekly Reset",
                AutoSize = true,
            };

            _serverReset = new IconLabel()
            {
                Parent = _container,
                Texture = new(943979) { Size = new(GameService.Content.DefaultFont14.LineHeight) },
                Text = "00:00:00",
                BasicTooltipText = "Server Reset",
                AutoSize = true,
            };

            _serverTime = new IconLabel
            {
                Parent = _container,
                Texture = new(517180) { Size = new(GameService.Content.DefaultFont14.LineHeight), TextureRegion = new(4, 4, 24, 24) },
                Text = "00:00:00",
                BasicTooltipText = "Server Time",
                AutoSize = true,
                ShowIcon = _serverReset.ShowIcon = _weeklyReset.ShowIcon = _showIcons.Value
            };

            SetPositions();
            _container.Moved += Container_Moved;
        }

        private void Container_Moved(object sender, Blish_HUD.Controls.MovedEventArgs e)
        {
            if (!_autoPosition.Value)
            {
                _resetPosition.Value = _container.Location;
            }
        }

        public override SubModuleType SubModuleType => SubModuleType.GameResets;

        public override void Load()
        {
            base.Load();
            GameService.Gw2Mumble.UI.CompassSizeChanged += UI_CompassSizeChanged;
            GameService.Gw2Mumble.UI.IsCompassTopRightChanged += UI_IsCompassTopRightChanged;
            QoL.ModuleInstance.CoreServices.ClientWindowService.ResolutionChanged += ClientWindowService_ResolutionChanged;
        }

        private void ClientWindowService_ResolutionChanged(object sender, ValueChangedEventArgs<Point> e)
        {
            SetPositions();
        }

        public override void Unload()
        {
            base.Unload();
            GameService.Gw2Mumble.UI.CompassSizeChanged -= UI_CompassSizeChanged;
            GameService.Gw2Mumble.UI.IsCompassTopRightChanged -= UI_IsCompassTopRightChanged;

            if (QoL.ModuleInstance is not null)
                QoL.ModuleInstance.CoreServices.ClientWindowService.ResolutionChanged -= ClientWindowService_ResolutionChanged;

            _showServerTime.SettingChanged -= ChangeServerTimeVisibility;
            _showDailyReset.SettingChanged -= ChangeServerResetVisibility;
            _showWeeklyReset.SettingChanged -= ChangeWeeklyResetVisibility;
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

            _showTooltips = settings.DefineSetting(nameof(_showTooltips), true);
            _showServerTime = settings.DefineSetting(nameof(_showServerTime), true);
            _showDailyReset = settings.DefineSetting(nameof(_showDailyReset), true);
            _showWeeklyReset = settings.DefineSetting(nameof(_showWeeklyReset), true);
            _showIcons = settings.DefineSetting(nameof(_showIcons), true);
            _autoPosition = settings.DefineSetting(nameof(_autoPosition), true);
            _dateDisplay = settings.DefineSetting(nameof(_dateDisplay), DateDisplayType.Long);
            _resetPosition = settings.DefineSetting(nameof(_resetPosition), GameService.Graphics.SpriteScreen.LocalBounds.Center);

            _showServerTime.SettingChanged += ChangeServerTimeVisibility;
            _showDailyReset.SettingChanged += ChangeServerResetVisibility;
            _showWeeklyReset.SettingChanged += ChangeWeeklyResetVisibility;
            _showIcons.SettingChanged += ChangeShowIcons;
            _autoPosition.SettingChanged += AutoPosition_SettingChanged;
            _dateDisplay.SettingChanged += DateDisplay_SettingChanged;
        }

        private void AutoPosition_SettingChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            SetPositions();
        }

        private void DateDisplay_SettingChanged(object sender, ValueChangedEventArgs<DateDisplayType> e)
        {
            SetTexts();
        }

        private void ChangeShowIcons(object sender, ValueChangedEventArgs<bool> e)
        {
            _serverTime.ShowIcon = e.NewValue;
            _serverReset.ShowIcon = e.NewValue;
            _weeklyReset.ShowIcon = e.NewValue;
        }

        private void ChangeServerTimeVisibility(object sender, ValueChangedEventArgs<bool> e)
        {
            _serverTime.Visible = e.NewValue;
            _serverTime.Parent?.Invalidate();
        }

        private void ChangeServerResetVisibility(object sender, ValueChangedEventArgs<bool> e)
        {
            _serverReset.Visible = e.NewValue;
            _serverReset.Parent?.Invalidate();
        }

        private void ChangeWeeklyResetVisibility(object sender, ValueChangedEventArgs<bool> e)
        {
            _weeklyReset.Visible = e.NewValue;
            _weeklyReset.Parent?.Invalidate();
        }

        private void UI_IsCompassTopRightChanged(object sender, ValueEventArgs<bool> e)
        {
            SetPositions();
        }

        private void UI_CompassSizeChanged(object sender, ValueEventArgs<Gw2Sharp.Models.Size> e)
        {
            SetPositions();
        }

        protected override void Enable()
        {
            base.Enable();
            _container.Visible = true;
        }

        protected override void Disable()
        {
            base.Disable();
            _container.Visible = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (!Enabled) return;
            _container.Visible = Enabled && GameService.GameIntegration.Gw2Instance.IsInGame && !GameService.Gw2Mumble.UI.IsMapOpen;
            _serverTime.Capture = _serverReset.Capture = _weeklyReset.Capture = _showTooltips.Value ? _container.AbsoluteBounds.Contains(GameService.Input.Mouse.Position) ? Blish_HUD.Controls.CaptureType.DoNotBlock : Blish_HUD.Controls.CaptureType.None : Blish_HUD.Controls.CaptureType.None;

            _container.Capture = _autoPosition.Value ? _serverTime.Capture : null;
            SetTexts();
            SetPositions();
        }

        private void SetPositions()
        {
            if (!_autoPosition.Value) return;

            var s = GameService.Gw2Mumble.UI.CompassSize;
            float scale = s.Height / 362F;
            scale = scale < 0.5 ? scale - 0.3F : scale;

            int y = GameService.Gw2Mumble.UI.IsCompassTopRight ?
                s.Height - _container.Height + (int)(24 * scale) :
                GameService.Graphics.SpriteScreen.Height - _container.Height - 60;

            _container.Location = new(
                GameService.Graphics.SpriteScreen.Width - s.Width - (int)(GameService.Gw2Mumble.UI.CompassSize.Width * 0.1),
                y);
        }

        private void SetTexts()
        {
            var now = DateTime.UtcNow;
            var nextDay = DateTime.UtcNow.AddDays(1);
            var nextWeek = DateTime.UtcNow;
            for (int i = 0; i < 8; i++)
            {
                nextWeek = DateTime.UtcNow.AddDays(i);
                if (nextWeek.DayOfWeek == DayOfWeek.Monday && (nextWeek.Day != now.Day || now.Hour < 7 || (now.Hour == 7 && now.Minute < 30))) break;
            }
            var t = new DateTime(nextDay.Year, nextDay.Month, nextDay.Day, 0, 0, 0);
            var w = new DateTime(nextWeek.Year, nextWeek.Month, nextWeek.Day, 7, 30, 0);

            _serverTime.Text = string.Format("{0}:{1:00}", now.Hour, now.Minute);

            var weeklyReset = w.Subtract(now);
            _weeklyReset.Text =
                _dateDisplay.Value is DateDisplayType.Long ? string.Format("{1:0} {0} {2:00}:{3:00}:{4:00}", strings.Days, weeklyReset.Days, weeklyReset.Hours, weeklyReset.Minutes, weeklyReset.Seconds) :
                _dateDisplay.Value is DateDisplayType.ShortDays ? string.Format("{1:0}{0} {2:00}:{3:00}:{4:00}", strings.Days.Substring(0, 1), weeklyReset.Days, weeklyReset.Hours, weeklyReset.Minutes, weeklyReset.Seconds) :
                weeklyReset.Days > 0
                                    ? string.Format("{1:0} {0}", strings.Days, weeklyReset.Days)
                                    : string.Format("{0:00}:{1:00}:{2:00}", weeklyReset.Hours, weeklyReset.Minutes, weeklyReset.Seconds);

            var serverReset = t.Subtract(now);
            _serverReset.Text = string.Format("{0:00}:{1:00}:{2:00}", serverReset.Hours, serverReset.Minutes, serverReset.Seconds);
        }

        public override void CreateSettingsPanel(FlowPanel flowPanel, int width)
        {
            var headerPanel = new Panel()
            {
                Parent = flowPanel,
                Width = width,
                HeightSizingMode = SizingMode.AutoSize,
                ShowBorder = true,
                CanCollapse = true,
                TitleIcon = Icon.Texture,
                Title = SubModuleType.ToString(),
            };

            var contentFlowPanel = new FlowPanel()
            {
                Parent = headerPanel,
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ContentPadding = new(5, 2),
                ControlPadding = new(0, 2),
            };

            UI.WrapWithLabel(() => string.Format(strings.ShowInHotbar_Name, $"{SubModuleType}"), () => string.Format(strings.ShowInHotbar_Description, $"{SubModuleType}"), contentFlowPanel, width - 16, new Checkbox()
            {
                Height = 20,
                Checked = ShowInHotbar.Value,
                CheckedChangedAction = (b) => ShowInHotbar.Value = b,
            });

            _ = new KeybindingAssigner()
            {
                Parent = contentFlowPanel,
                Width = width - 16,
                KeyBinding = HotKey.Value,
                KeybindChangedAction = (kb) =>
                {
                    HotKey.Value = new()
                    {
                        ModifierKeys = kb.ModifierKeys,
                        PrimaryKey = kb.PrimaryKey,
                        Enabled = kb.Enabled,
                        IgnoreWhenInTextField = true,
                    };
                },
                SetLocalizedKeyBindingName = () => string.Format(strings.HotkeyEntry_Name, $"{SubModuleType}"),
                SetLocalizedTooltip = () => string.Format(strings.HotkeyEntry_Description, $"{SubModuleType}"),
            };

            UI.WrapWithLabel(() => strings.ShowServerTime_Name, () => strings.ShowServerTime_Tooltip, contentFlowPanel, width - 16, new Checkbox()
            {
                Height = 20,
                Checked = _showServerTime.Value,
                CheckedChangedAction = (b) => _showServerTime.Value = b,
            });

            UI.WrapWithLabel(() => strings.ShowDailyReset_Name, () => strings.ShowDailyReset_Tooltip, contentFlowPanel, width - 16, new Checkbox()
            {
                Height = 20,
                Checked = _showDailyReset.Value,
                CheckedChangedAction = (b) => _showDailyReset.Value = b,
            });

            UI.WrapWithLabel(() => strings.ShowWeeklyReset_Name, () => strings.ShowWeeklyReset_Tooltip, contentFlowPanel, width - 16, new Checkbox()
            {
                Height = 20,
                Checked = _showWeeklyReset.Value,
                CheckedChangedAction = (b) => _showWeeklyReset.Value = b,
            });

            UI.WrapWithLabel(() => strings.ShowIcons_Name, () => strings.ShowIcons_Tooltip, contentFlowPanel, width - 16, new Checkbox()
            {
                Height = 20,
                Checked = _showIcons.Value,
                CheckedChangedAction = (b) => _showIcons.Value = b,
            });

            Checkbox autoPosCheckbox = null;
            Checkbox editPosCheckbox = null;

            UI.WrapWithLabel(() => strings.AutoPosition_Name, () => strings.AutoPosition_Tooltip, contentFlowPanel, width - 16, autoPosCheckbox = new Checkbox()
            {
                Height = 20,
                Checked = _autoPosition.Value,
                CheckedChangedAction = (b) => { _autoPosition.Value = b; editPosCheckbox.Checked = !b && editPosCheckbox.Checked; },
            });

            UI.WrapWithLabel(() => strings.EditPosition_Name, () => strings.EditPosition_Tooltip, contentFlowPanel, width - 16, editPosCheckbox = new Checkbox()
            {
                Height = 20,
                Checked = _editPosition,
                CheckedChangedAction = (b) =>
                {
                    _container.CaptureInput = b;
                    _container.CanDrag = b;
                    _container.Location = b ? _resetPosition.Value : _container.Location;
                    autoPosCheckbox.Checked = !b && _autoPosition.Value;
                    _editPosition = b;
                },
            });

            UI.WrapWithLabel(() => strings.ShowTooltips_Name, () => strings.ShowTooltips_Tooltip, contentFlowPanel, width - 16, new Checkbox()
            {
                Height = 20,
                Checked = _showTooltips.Value,
                CheckedChangedAction = (b) => _showTooltips.Value = b,
            });

            UI.WrapWithLabel(() => strings.DateFormat_Name, () => strings.DateFormat_Tooltip, contentFlowPanel, width - 16, new Dropdown()
            {
                Location = new(250, 0),
                Parent = contentFlowPanel,
                SetLocalizedItems = () =>
                {
                    return
                    [
                        $"{DateDisplayType.Short}".SplitStringOnUppercase(),
                        $"{DateDisplayType.ShortDays}".SplitStringOnUppercase(),
                        $"{DateDisplayType.Long}".SplitStringOnUppercase(),
                    ];
                },
                SelectedItem = $"{_dateDisplay.Value}".SplitStringOnUppercase(),
                ValueChangedAction = (b) => _dateDisplay.Value = Enum.TryParse(b.RemoveSpaces(), out DateDisplayType dateType) ? dateType : _dateDisplay.Value,
            });
        }
    }
}
