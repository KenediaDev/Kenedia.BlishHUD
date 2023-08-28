using Blish_HUD;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using System;
using SizingMode = Blish_HUD.Controls.SizingMode;
using ControlFlowDirection = Blish_HUD.Controls.ControlFlowDirection;
using Kenedia.Modules.QoL.Res;
using System.Linq;

namespace Kenedia.Modules.QoL.SubModules.GameResets
{
    public class GameResets : SubModule
    {
        private readonly FlowPanel _container;
        private readonly IconLabel _serverTime;
        private readonly IconLabel _serverReset;
        private readonly IconLabel _weeklyReset;

        private SettingEntry<bool> _showServerTime;
        private SettingEntry<bool> _showDailyReset;
        private SettingEntry<bool> _showWeeklyReset;

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
                Location = GameService.Graphics.SpriteScreen.LocalBounds.Center,
                Enabled = false,
                CaptureInput = false,
            });

            _weeklyReset = new IconLabel()
            {
                Parent = _container,
                Texture = new(156692) { Size = new(GameService.Content.DefaultFont14.LineHeight) },
                Text = "0 days 00:00:00",
                BasicTooltipText = "Weekly Reset",
                AutoSize = true,
                Enabled = false,
                CaptureInput = false,
            };

            _serverReset = new IconLabel()
            {
                Parent = _container,
                Texture = new(943979) { Size = new(GameService.Content.DefaultFont14.LineHeight) },
                Text = "00:00:00",
                BasicTooltipText = "Server Reset",
                AutoSize = true,
                Enabled = false,
                CaptureInput = false,
            };

            _serverTime = new IconLabel()
            {
                Parent = _container,
                Texture = new(517180) { Size = new(GameService.Content.DefaultFont14.LineHeight), TextureRegion = new(4, 4, 24, 24) },
                Text = "00:00:00",
                BasicTooltipText = "Server Time",
                AutoSize = true,
                Enabled = false,
                CaptureInput = false,
            };

            SetPositions();
        }

        public override SubModuleType SubModuleType => SubModuleType.GameResets;
        public override void Load()
        {
            base.Load();
            GameService.Gw2Mumble.UI.CompassSizeChanged += UI_CompassSizeChanged;
            GameService.Gw2Mumble.UI.IsCompassTopRightChanged += UI_IsCompassTopRightChanged;
            QoL.ModuleInstance.Services.ClientWindowService.ResolutionChanged += ClientWindowService_ResolutionChanged;
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
                QoL.ModuleInstance.Services.ClientWindowService.ResolutionChanged -= ClientWindowService_ResolutionChanged;

            _showServerTime.SettingChanged -= ChangeServerTimeVisibility;
            _showDailyReset.SettingChanged -= ChangeServerResetVisibility;
            _showWeeklyReset.SettingChanged -= ChangeWeeklyResetVisibility;
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

            _showServerTime = settings.DefineSetting(nameof(_showServerTime), true);
            _showDailyReset = settings.DefineSetting(nameof(_showDailyReset), true);
            _showWeeklyReset = settings.DefineSetting(nameof(_showWeeklyReset), true);

            _showServerTime.SettingChanged += ChangeServerTimeVisibility;
            _showDailyReset.SettingChanged += ChangeServerResetVisibility;
            _showWeeklyReset.SettingChanged += ChangeWeeklyResetVisibility;
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
            SetTexts();
            SetPositions();
        }

        private void SetPositions()
        {
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
            _weeklyReset.Text = string.Format("{1:0} {0} {2:00}:{3:00}:{4:00}", strings.Days, weeklyReset.Days, weeklyReset.Hours, weeklyReset.Minutes, weeklyReset.Seconds);

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
        }
    }
}
