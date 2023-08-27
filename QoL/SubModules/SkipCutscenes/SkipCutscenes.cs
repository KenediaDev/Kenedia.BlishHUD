using Blish_HUD;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Structs;
using Kenedia.Modules.Core.Utility;
using Blish_HUD.Controls.Intern;
using Mouse = Blish_HUD.Controls.Intern.Mouse;

namespace Kenedia.Modules.QoL.SubModules.SkipCutscenes
{
    public class SkipCutscenes : SubModule
    {
        private Logger _logger = Logger.GetLogger(typeof(SkipCutscenes));
        private CancellationTokenSource _cts;
        private Point _resolution;
        private double _mumbleTick = 0;
        private bool _introCutscene = false;
        private bool _clickAgain = false;
        private bool _sleptBeforeClick;
        private double _ticks;
        private Point _mousePosition;
        private CinematicStateType _cutsceneState;
        private InteractStateType _moduleState;
        private readonly List<int> _introMaps = new()
        {
            573, //Queensdale
            458, //Plains of Ashford
            138, //Wayfarer Foothills
            379, //Caledon Forest
            432 //Metrica Province
        };

        private readonly List<int> _starterMaps = new(){
            15, //Queensdale
            19, //Plains of Ashford
            28, //Wayfarer Foothills
            34, //Caledon Forest
            35 //Metrica Province
        };
        private readonly GameStateDetectionService _gameStateDetectionService;

        public SkipCutscenes(SettingCollection settings, GameStateDetectionService gameStateDetectionService) : base(settings)
        {
            _gameStateDetectionService = gameStateDetectionService;
            GameService.GameIntegration.Gw2Instance.Gw2LostFocus += Gw2Instance_Gw2LostFocus;
        }

        private void Gw2Instance_Gw2LostFocus(object sender, EventArgs e)
        {
            if (Enabled) Cancel();
        }

        public override SubModuleType SubModuleType => SubModuleType.SkipCutscenes;

        public SettingEntry<KeyBinding> Cancel_Key { get; private set; }

        private enum InteractStateType
        {
            Ready,
            MouseMoved,
            Clicked,
            MouseMovedBack,
            Clicked_Again,
            Menu_Opened,
            Menu_Closed,
            Done,
        }

        private enum CinematicStateType
        {
            Ready,
            InitialSleep,
            Clicked_Once,
            Sleeping,
            Clicked_Twice,
            Done,
        }

        public override async void Update(GameTime gameTime)
        {
            var Mumble = GameService.Gw2Mumble;
            var resolution = GameService.Graphics.Resolution;
            var _inGame = GameService.GameIntegration.Gw2Instance.IsInGame;

            if (_introMaps.Contains(Mumble.CurrentMap.Id))
            {
                _introCutscene = true;
            }

            if (GameService.Graphics.Resolution != resolution)
            {
                _resolution = resolution;
                _mumbleTick = Mumble.Tick + 5;
                return;
            }
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

            Cancel_Key = settings.DefineSetting(nameof(Cancel_Key), new KeyBinding(Keys.Escape));

            Cancel_Key.Value.Enabled = true;
            Cancel_Key.Value.Activated += Cancel_Key_Activated;
        }

        private void Cancel_Key_Activated(object sender, EventArgs e)
        {
            if (Enabled)
            {
                _logger.Debug("Escape got pressed manually. Lets cancel!");
                Cancel();
            }
        }

        public override void Load()
        {
            base.Load();

            _gameStateDetectionService.GameStateChanged += On_GameStateChanged;
        }

        private async void On_GameStateChanged(object sender, GameStateChangedEventArgs e)
        {
            _logger.Info($"Gamestate changed to {e.Status}");
            switch (e.Status)
            {
                case GameStatusType.Vista:
                    if (Enabled) await SkipVista();
                    break;

                case GameStatusType.Cutscene:
                    if (Enabled) await SkipCutscene();
                    break;

                default:
                    if (Enabled) Cancel();
                    break;
            }
        }

        private void Cancel()
        {
            _mumbleTick = GameService.Gw2Mumble.Tick + 250;

            if (_mousePosition != Point.Zero)
                Mouse.SetPosition(_mousePosition.X, _mousePosition.Y, true);

            _cts?.Cancel();
            _cts = null;

            _mousePosition = Point.Zero;
        }

        public override void Unload()
        {
            base.Unload();

            _gameStateDetectionService.GameStateChanged -= On_GameStateChanged;
            Cancel_Key.Value.Activated -= Cancel_Key_Activated;
        }

        private void PlayerCharacter_NameChanged(object sender, ValueEventArgs<string> e)
        {
            _introCutscene = false;
        }

        private void CurrentMap_MapChanged(object sender, ValueEventArgs<int> e)
        {
            if (Enabled)
            {
                _mumbleTick = GameService.Gw2Mumble.Tick;

                if (_introCutscene && _starterMaps.Contains(GameService.Gw2Mumble.CurrentMap.Id))
                {
                    _ticks = _ticks + 1250;
                    _cutsceneState = CinematicStateType.InitialSleep;
                    _moduleState = InteractStateType.Ready;
                }
            }
        }

        private async Task SkipCutscene()
        {
            try
            {
                _cts ??= new CancellationTokenSource();

                _logger.Info($"SkipCutscene");
                _logger.Info($"Press Escape and wait a bit");
                await Input.SendKey(Keys.Escape, false);
                await Task.Delay(250, _cts.Token);

                if (_cts is null || _cts.Token.IsCancellationRequested) return;

                _logger.Info($"We are still in the cutscene lets try with mouse.");
                var pos = QoL.ModuleInstance.Services.ClientWindowService.WindowBounds;
                var p = new Point(pos.Right - 50, pos.Bottom - 35);
                var m = GameService.Input.Mouse.Position;
                double factor = GameService.Graphics.UIScaleMultiplier;

                bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
                RectangleDimensions offset = windowed ? QoL.ModuleInstance.Services.SharedSettings.WindowOffset : new(0);

                _mousePosition = new(pos.Left + (int)(m.X * factor) + offset.Left, pos.Top + offset.Top + (int)(m.Y * factor));

                for (int i = 0; i < 3; i++)
                {
                    Mouse.SetPosition(p.X, p.Y, true);
                    if (_cts is null || _cts.Token.IsCancellationRequested) return;

                    await Task.Delay(25, _cts.Token);
                    if (_cts is null || _cts.Token.IsCancellationRequested) return;

                    _logger.Info($"Click with the mouse in the bottom right corner.");
                    Mouse.Click(MouseButton.LEFT, p.X, p.Y, true);
                    if (_cts is null || _cts.Token.IsCancellationRequested) return;

                    await Task.Delay(125, _cts.Token);
                    if (_cts is null || _cts.Token.IsCancellationRequested) return;
                }
            }
            catch (TaskCanceledException)
            {

            }
        }

        private async Task SkipVista()
        {
            try
            {
                _cts ??= new CancellationTokenSource();

                _logger.Info($"Skip Vista with Escape.");
                await Input.SendKey(Keys.Escape, false);
            }
            catch (TaskCanceledException)
            {

            }
        }
    }
}
