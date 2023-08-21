using Blish_HUD;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.Core.Models;

namespace Kenedia.Modules.QoL.SubModules.SkipCutscenes
{
    public class SkipCutscenes : SubModule
    {
        private CancellationTokenSource _cts;
        private Point _resolution;
        private double _mumbleTick = 0;
        private bool _introCutscene = false;
        private bool _clickAgain = false;
        private bool _sleptBeforeClick;
        private double _ticks;
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

            if(!_inGame && Mumble.Tick > _mumbleTick)
            {
                //Debug.WriteLine($"Mumble.Tick: {Mumble.Tick} | _mumbleTick {_mumbleTick}");
                HandleCutscene();
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
                _mumbleTick = GameService.Gw2Mumble.Tick + 250;
                _cts?.Cancel();
            }
        }

        public override void Load()
        {
            base.Load();

            _gameStateDetectionService.ChangedToCutscene += On_ChangedToCutscene;

            _gameStateDetectionService.ChangedToIngame += CancelSkip_GameStateNoCutscene;
            _gameStateDetectionService.ChangedToLoadingScreen += CancelSkip_GameStateNoCutscene;
            _gameStateDetectionService.ChangedToCharacterSelection += CancelSkip_GameStateNoCutscene;

            GameService.Gw2Mumble.CurrentMap.MapChanged += CurrentMap_MapChanged;
            GameService.Gw2Mumble.PlayerCharacter.NameChanged += PlayerCharacter_NameChanged;
        }

        private void CancelSkip_GameStateNoCutscene(object sender, GameStateChangedEventArgs e)
        {
            Debug.WriteLine($"GAMESTATE CHANGED TO {e.Status}! Cancel Skipping!");
            if (Enabled) Cancel();
        }

        private void On_ChangedToCutscene(object sender, GameStateChangedEventArgs e)
        {
            Debug.WriteLine($"GAMESTATE CHANGED TO {e.Status}! START SKIPPING!");
            if (Enabled) HandleCutscene();
        }

        private void Cancel()
        {
            _mumbleTick = GameService.Gw2Mumble.Tick + 250;
            _cts?.Cancel();
        }

        public override void Unload()
        {
            base.Unload();

            GameService.Gw2Mumble.CurrentMap.MapChanged -= CurrentMap_MapChanged;
            GameService.Gw2Mumble.PlayerCharacter.NameChanged -= PlayerCharacter_NameChanged;
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

        private void HandleCutscene()
        {
            _cts ??= new CancellationTokenSource();

            _ = Task.Run(SkipCutscene, _cts.Token);
        }

        private async Task SkipCutscene()
        {

            Debug.WriteLine($"SkipCutscene");
        }

        private async Task SkipCutsceneWithEscape()
        {

        }

        private async Task SkipCutsceneWithMouse()
        {

        }
    }
}
