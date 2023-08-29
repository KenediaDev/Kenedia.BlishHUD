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
using Kenedia.Modules.Core.Controls;
using SizingMode = Blish_HUD.Controls.SizingMode;
using ControlFlowDirection = Blish_HUD.Controls.ControlFlowDirection;
using Kenedia.Modules.QoL.Res;

namespace Kenedia.Modules.QoL.SubModules.SkipCutscenes
{
    public class SkipCutscenes : SubModule
    {
        private readonly Logger _logger = Logger.GetLogger(typeof(SkipCutscenes));
        private CancellationTokenSource _cts;
        private Point _mousePosition;

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

        public override void Update(GameTime gameTime)
        {
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

        protected override void Enable()
        {
            base.Enable();
            _gameStateDetectionService.Enabled = true;
        }

        protected override void Disable()
        {
            base.Disable();
            _gameStateDetectionService.Enabled = false;

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
        }
    }
}
