﻿using Blish_HUD;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Kenedia.Modules.QoL.Res;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using Kenedia.Modules.Core.Extensions;

namespace Kenedia.Modules.QoL.SubModules.ZoomOut
{
    public class ZoomOut : SubModule
    {
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

            if (Mumble.UI.IsMapOpen || !Mumble.Info.IsGameFocused || gameTime.TotalGameTime.TotalMilliseconds - _ticks < 25)
            {
                return;
            }

            if (gameTime.TotalGameTime.TotalMilliseconds - _saveDistanceTicks < 0)
            {
                _distance = ComputeCameraDistance();
            }

            _ticks = gameTime.TotalGameTime.TotalMilliseconds;
            double threshold = 1;
            float distance = ComputeCameraDistance();
            float delta = Math.Max(_distance, distance) - Math.Min(_distance, distance);

            if (_zoomOnCameraChange.Value && delta >= threshold)
            {
                ZoomCameraOut(distance);
                _distance = distance;
            }

            if(_zoomOnFoVChange.Value && Mumble.PlayerCamera.FieldOfView != _fieldOfView)
            {
                ZoomCameraOut(distance);
                _distance = distance;
                _fieldOfView = Mumble.PlayerCamera.FieldOfView;
            }
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

            _zoomOutKey = Settings.DefineSetting(nameof(_zoomOutKey), new KeyBinding(Keys.PageDown), () => strings.ZoomOutKey_Name, () => strings.ZoomOutKey_Tooltip);
            _manualMaxZoom = Settings.DefineSetting(nameof(_manualMaxZoom), new KeyBinding(ModifierKeys.Shift, Keys.Left), () => strings.ManualZoom_Name, () => strings.ManualZoom_Tooltip);

            _zoomOnCameraChange = settings.DefineSetting(nameof(_zoomOnCameraChange),
                true,
                () => strings.ZoomOnCameraChange_Name,
                () => strings.ZoomOnCameraChange_Tooltip);

            _allowManualZoom = settings.DefineSetting(nameof(_allowManualZoom),
                true,
                () => strings.AllowManualZoom_Name,
                () => strings.AllowManualZoom_Tooltip);

            _zoomOnFoVChange = settings.DefineSetting(nameof(_zoomOnFoVChange),
                true,
                () => strings.AllowManualZoom_Name,
                () => strings.AllowManualZoom_Tooltip);

            _manualMaxZoom.Value.Enabled = true;
            _manualMaxZoom.Value.Activated += ManualMaxZoomOut;
        }

        private void ManualMaxZoomOut(object sender, EventArgs e)
        {
            float distance = ComputeCameraDistance();

            for (int i = 0; i < ((25 - distance) * 2); i++)
            {
                _zoomOutKey.Press();
            }
        }

        private void ZoomCameraOut(float distance)
        {
            for (int i = 0; i < ((25 - distance) * 2); i++)
            {
                _zoomOutKey.Press();
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

            float distance = ComputeCameraDistance();

            _distance = distance;
            _saveDistanceTicks = Common.Now() + 1000;
        }

        private float ComputeCameraDistance()
        {
            var Mumble = GameService.Gw2Mumble;
            var camera = GameService.Gw2Mumble.PlayerCamera;
            var ppos = Mumble.PlayerCharacter.Position;
            return camera.Position.Distance3D(ppos);
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
    }
}
