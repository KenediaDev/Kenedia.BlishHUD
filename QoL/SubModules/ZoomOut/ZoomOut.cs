using Blish_HUD;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Kenedia.Modules.QoL.Res;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using Kenedia.Modules.Core.Extensions;
using SizingMode = Blish_HUD.Controls.SizingMode;
using ControlFlowDirection = Blish_HUD.Controls.ControlFlowDirection;

namespace Kenedia.Modules.QoL.SubModules.ZoomOut
{
    public class ZoomOut : SubModule
    {
        private const double TickIntervalMs = 25;
        private const float CameraDistanceThreshold = 1f;

        public bool MouseScrolled { get; private set; }

        private float _distance;
        private float _fieldOfView;
        private double _ticks;
        private double _saveDistanceTicks;

        private SettingEntry<KeyBinding> _zoomOutKey;
        private SettingEntry<KeyBinding> _manualMaxZoom;
        private SettingEntry<bool> _zoomOnCameraChange;
        private SettingEntry<bool> _allowManualZoom;
        private SettingEntry<bool> _zoomOnFoVChange;

        public ZoomOut(SettingCollection settings) : base(settings)
        {
        }

        public override SubModuleType SubModuleType => SubModuleType.ZoomOut;

        public override void Update(GameTime gameTime)
        {
            if (!Enabled) return;
            var Mumble = GameService.Gw2Mumble;
            double now = gameTime.TotalGameTime.TotalMilliseconds;

            if (Mumble.UI.IsMapOpen || !Mumble.Info.IsGameFocused || now - _ticks < TickIntervalMs)
            {
                return;
            }

            if (IsActionCameraActive())
            {
                _distance = ComputeCameraDistance();
                _fieldOfView = Mumble.PlayerCamera.FieldOfView;
                _ticks = now;
                return;
            }

            float distance = ComputeCameraDistance();
            float fieldOfView = Mumble.PlayerCamera.FieldOfView;
            bool recentManualZoom = now < _saveDistanceTicks;
            bool fieldOfViewChanged = Math.Abs(fieldOfView - _fieldOfView) > float.Epsilon;

            _ticks = now;

            if (recentManualZoom)
            {
                _distance = distance;
                _fieldOfView = fieldOfView;
                return;
            }

            if (_zoomOnCameraChange.Value && distance < _distance - CameraDistanceThreshold)
            {
                ZoomCameraOut(distance, _distance);
                return;
            }

            // Action camera can constantly vary FoV without an actual zoom state change.
            // Treat those as baseline updates instead of forcing max zoom repeatedly.
            if (_zoomOnFoVChange.Value && fieldOfViewChanged)
            {
                if (distance < _distance - CameraDistanceThreshold)
                {
                    ZoomCameraOut(distance, _distance);
                    return;
                }

                _fieldOfView = fieldOfView;
                return;
            }

            _fieldOfView = fieldOfView;
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

            _zoomOutKey = Settings.DefineSetting(nameof(_zoomOutKey), new KeyBinding(Keys.PageDown));
            _manualMaxZoom = Settings.DefineSetting(nameof(_manualMaxZoom), new KeyBinding(Keys.None));

            _zoomOnCameraChange = settings.DefineSetting(nameof(_zoomOnCameraChange),
                true);

            _allowManualZoom = settings.DefineSetting(nameof(_allowManualZoom),
                true);

            _zoomOnFoVChange = settings.DefineSetting(nameof(_zoomOnFoVChange),
                true);

            _manualMaxZoom.Value.Enabled = true;
            _manualMaxZoom.Value.Activated += ManualMaxZoomOut;
        }

        private void ManualMaxZoomOut(object sender, EventArgs e)
        {
            float distance = ComputeCameraDistance();
            _saveDistanceTicks = Common.Now + 1000;

            ZoomCameraOut(distance, 25f);
        }

        private void ZoomCameraOut(float distance, float targetDistance)
        {
            _distance = Math.Max(_distance, targetDistance);

            int zoomSteps = (int)Math.Max(0, (targetDistance - distance) * 2);

            for (int i = 0; i < zoomSteps; i++)
            {
                _ = _zoomOutKey.Press();
            }
        }

        protected override void Enable()
        {
            base.Enable();
        }

        override protected void Disable()
        {
            base.Disable();
        }

        public override void Load()
        {
            base.Load();

            GameService.Input.Mouse.MouseWheelScrolled += Mouse_MouseWheelScrolled;

            var Mumble = GameService.Gw2Mumble;
            Mumble.CurrentMap.MapChanged += CurrentMap_MapChanged;
            Mumble.PlayerCharacter.NameChanged += PlayerCharacter_NameChanged;

            _distance = ComputeCameraDistance();
            _fieldOfView = Mumble.PlayerCamera.FieldOfView;
        }

        private void PlayerCharacter_NameChanged(object sender, ValueEventArgs<string> e)
        {
            _ticks = 0;
        }

        private void CurrentMap_MapChanged(object sender, ValueEventArgs<int> e)
        {
            _ticks = 0;
        }

        private void Mouse_MouseWheelScrolled(object sender, MouseEventArgs e)
        {
            if (!_allowManualZoom.Value) return;

            _saveDistanceTicks = Common.Now + 1000;
        }

        private float ComputeCameraDistance()
        {
            var Mumble = GameService.Gw2Mumble;
            var camera = GameService.Gw2Mumble.PlayerCamera;
            var ppos = Mumble.PlayerCharacter.Position;
            return camera.Position.Distance3D(ppos);
        }

        private static bool IsActionCameraActive()
        {
            return !GameService.Input.Mouse.CursorIsVisible && !GameService.Gw2Mumble.UI.IsTextInputFocused;
        }

        public override void Unload()
        {
            base.Unload();

            GameService.Input.Mouse.MouseWheelScrolled -= Mouse_MouseWheelScrolled;

            var Mumble = GameService.Gw2Mumble;
            Mumble.CurrentMap.MapChanged -= CurrentMap_MapChanged;
            Mumble.PlayerCharacter.NameChanged -= PlayerCharacter_NameChanged;
        }

        protected override void SwitchLanguage()
        {
            base.SwitchLanguage();
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

            _ = new KeybindingAssigner()
            {
                Parent = contentFlowPanel,
                Width = width - 16,
                KeyBinding = _zoomOutKey.Value,
                KeybindChangedAction = (kb) =>
                {
                    _zoomOutKey.Value = new()
                    {
                        ModifierKeys = kb.ModifierKeys,
                        PrimaryKey = kb.PrimaryKey,
                        Enabled = kb.Enabled,
                        IgnoreWhenInTextField = true,
                    };
                },
                SetLocalizedKeyBindingName = () => strings.ZoomOutKey_Name,
                SetLocalizedTooltip = () => strings.ZoomOutKey_Tooltip,
            };

            _ = new KeybindingAssigner()
            {
                Parent = contentFlowPanel,
                Width = width - 16,
                KeyBinding = _manualMaxZoom.Value,
                KeybindChangedAction = (kb) =>
                {
                    _manualMaxZoom.Value = new()
                    {
                        ModifierKeys = kb.ModifierKeys,
                        PrimaryKey = kb.PrimaryKey,
                        Enabled = kb.Enabled,
                        IgnoreWhenInTextField = true,
                    };
                },
                SetLocalizedKeyBindingName = () => strings.ManualZoom_Name,
                SetLocalizedTooltip = () => strings.ManualZoom_Tooltip,
            };

            UI.WrapWithLabel(() => strings.ZoomOnCameraChange_Name, () => strings.ZoomOnCameraChange_Tooltip, contentFlowPanel, width - 16, new Checkbox()
            {
                Height = 20,
                Checked = _zoomOnCameraChange.Value,
                CheckedChangedAction = (b) => _zoomOnCameraChange.Value = b,
            });

            UI.WrapWithLabel(() => strings.ZoomOnFoVChange_Name, () => strings.ZoomOnFoVChange_Tooltip, contentFlowPanel, width - 16, new Checkbox()
            {
                Height = 20,
                Checked = _zoomOnFoVChange.Value,
                CheckedChangedAction = (b) => _zoomOnFoVChange.Value = b,
            });

            UI.WrapWithLabel(() => strings.AllowManualZoom_Name, () => strings.AllowManualZoom_Tooltip, contentFlowPanel, width - 16, new Checkbox()
            {
                Height = 20,
                Checked = _allowManualZoom.Value,
                CheckedChangedAction = (b) => _allowManualZoom.Value = b,
            });
        }
    }
}
