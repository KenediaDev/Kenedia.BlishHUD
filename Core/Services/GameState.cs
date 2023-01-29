using Blish_HUD;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bitmap = System.Drawing.Bitmap;
using Graphics = System.Drawing.Graphics;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using static Kenedia.Modules.Core.Utility.WindowsUtil.User32Dll;

namespace Kenedia.Modules.Core.Services
{
    public enum GameStatus
    {
        Unknown = 0,
        Ingame,
        CharacterSelection,
        LoadingScreen,
        Cutscene
    }

    public class GameState
    {
        private double _lastTick = 0;
        private double? _cutsceneStart;
        private double _cutsceneDuration = 0;

        private readonly List<double> _spinnerResults = new();

        private readonly List<GameStatus> _gameStatuses = new();
        private GameStatus _gameStatus = GameStatus.Unknown;
        public event EventHandler<GameStateChangedEventArgs> GameStateChanged;
        public event EventHandler<GameStateChangedEventArgs> ChangedToIngame;
        public event EventHandler<GameStateChangedEventArgs> ChangedToCharacterSelection;
        public event EventHandler<GameStateChangedEventArgs> ChangedToLoadingScreen;
        public event EventHandler<GameStateChangedEventArgs> ChangedToCutscene;

        public GameState()
        {
            //_panel = new()
            //{
            //    Parent = GameService.Graphics.SpriteScreen,
            //    Location = new Microsoft.Xna.Framework.Point(50, 100),
            //    Height = 250,
            //    WidthSizingMode = SizingMode.AutoSize,
            //    FlowDirection = ControlFlowDirection.SingleTopToBottom,
            //};

            //_logoutButton = new()
            //{
            //    Parent = _panel,
            //    Height = 50,
            //    BackgroundColor = Color.Red,
            //    Width = 100,
            //};

            //_spinner = new()
            //{
            //    Parent = _panel,
            //    Height = 50,
            //    BackgroundColor = Color.Red,
            //    Width = 100,
            //};
        }

        private GameStatus NewStatus { get; set; }

        public GameStatus OldStatus { get; private set; }

        public GameStatus GameStatus
        {
            get => _gameStatus;
            private set
            {
                if (_gameStatus != value)
                {
                    OldStatus = _gameStatus;
                    _gameStatus = value;

                    var eventArgs = new GameStateChangedEventArgs()
                    {
                        OldStatus = OldStatus,
                        Status = _gameStatus,
                    };

                    switch (_gameStatus)
                    {
                        case GameStatus.Ingame:
                            _spinnerResults.Clear();
                            _cutsceneDuration = 0;
                            _cutsceneStart = null;
                            ChangedToIngame?.Invoke(GameStatus, eventArgs);
                            break;

                        case GameStatus.CharacterSelection:
                            _spinnerResults.Clear();
                            _cutsceneDuration = 0;
                            _cutsceneStart = null;
                            ChangedToCharacterSelection?.Invoke(GameStatus, eventArgs);
                            break;

                        case GameStatus.LoadingScreen:
                            _cutsceneDuration = 0;
                            _cutsceneStart = null;
                            ChangedToLoadingScreen?.Invoke(GameStatus, eventArgs);
                            break;

                        case GameStatus.Cutscene:
                            ChangedToCutscene?.Invoke(GameStatus, eventArgs);
                            break;
                    }

                    GameStateChanged?.Invoke(GameStatus, eventArgs);
                }
            }
        }

        public bool IsIngame => GameStatus == GameStatus.Ingame;

        public void Run(GameTime gameTime)
        {
            NewStatus = GameStatus.Unknown;

            if (GameService.Gw2Mumble.TimeSinceTick.TotalMilliseconds <= 500)
            {
                NewStatus = GameStatus.Ingame;
            }
            else if (GameService.GameIntegration.Gw2Instance.Gw2HasFocus)
            {
                // Throttle the check so we don't stress the CPU to badly with the checks
                if (gameTime.TotalGameTime.TotalMilliseconds - _lastTick > 250)
                {
                    _lastTick = gameTime.TotalGameTime.TotalMilliseconds;

                    //bool isCogVisible = IsCogVisible();
                    bool isButtonVisible = IsButtonVisible();

                    // there IS the cog wheel present in the top left. --> Ingame
                    if (isButtonVisible && GameStatus is GameStatus.Unknown or GameStatus.Ingame)
                    {
                        NewStatus = GameStatus.CharacterSelection;
                    }
                    else
                    {
                        // there IS NOT the cog wheel present in the top left. --> LoadingScreen
                        bool isLoadingSpinnerVisible = IsLoadingSpinnerVisible();

                        if (GameStatus is GameStatus.CharacterSelection)
                        {
                            if (isLoadingSpinnerVisible) NewStatus = GameStatus.LoadingScreen;
                        }
                        else
                        {
                            if (!isLoadingSpinnerVisible)
                            {
                                _cutsceneStart ??= gameTime.TotalGameTime.TotalMilliseconds;
                                _cutsceneDuration += gameTime.TotalGameTime.TotalMilliseconds - (double)_cutsceneStart;
                            }

                            if (GameStatus is GameStatus.Cutscene)
                            {
                                // We will miss out every loading screen after a cutscene
                            }
                            else if (GameStatus is GameStatus.LoadingScreen)
                            {
                                // After a loading screen we can only get into a cutscene or ingame, ingame is checked already, so if we are here its a cutscene if the cutscene duration is longer than x ms
                                if (!isLoadingSpinnerVisible)
                                {
                                    NewStatus = _cutsceneDuration > 1000 ? GameStatus.Cutscene : NewStatus;
                                }
                            }
                            else if (GameStatus is GameStatus.Ingame)
                            {
                                // From ingame we can go to all game states, character selection is reliable from ingame and checked already, so we now test only Loading screen and Cutscene
                                NewStatus = isLoadingSpinnerVisible ? GameStatus.LoadingScreen : _cutsceneDuration > 1000 ? GameStatus.Cutscene : GameStatus.Unknown;
                            }
                        }
                    }
                }
            }

            if (NewStatus != GameStatus.Unknown)
            {
                if (StatusConfirmed(NewStatus, _gameStatuses, NewStatus == GameStatus.LoadingScreen ? 6 : 3))
                {
                    if (GameStatus != NewStatus)
                    {
                        GameStatus = NewStatus;
                        Core.Logger.Info($"Game status changed from '{NewStatus}' to '{GameStatus}'");
                        _gameStatuses.Clear();
                    }
                }
            }
        }

        public bool IsButtonVisible()
        {
            RECT wndBounds = Core.ClientWindowService.WindowBounds;
            bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
            var offset = windowed ? Core.ModuleInstance.WindowOffset.Value : new(0, 0, 0, 0);

            var uiSize = GameService.Gw2Mumble.UI.UISize;
            Size size = new(50 + ((int)uiSize * 3), 40 + ((int)uiSize * 3));

            var pos = new Point(0, -size.Height);

            using Bitmap bitmap = new(size.Width, size.Height);
            using var g = Graphics.FromImage(bitmap);
            using MemoryStream s = new();

            g.CopyFromScreen(new(wndBounds.Left + offset.Left, wndBounds.Bottom + offset.Bottom + pos.Y), Point.Empty, size);

            var cutFilled = bitmap.IsCutAndCheckFilled(0.4, 0.7f);

            return cutFilled.Item3 is > 0.35;
        }

        public bool IsLoadingSpinnerVisible()
        {
            RECT wndBounds = Core.ClientWindowService.WindowBounds;
            bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
            var offset = windowed ? Core.ModuleInstance.WindowOffset.Value : new(0, 0, 0, 0);

            var uiSize = GameService.Gw2Mumble.UI.UISize;
            Size size = new(64 + ((int)uiSize * 2), 64 + ((int)uiSize * 2));

            var pos = new Point(-size.Width, -size.Height);

            using Bitmap bitmap = new(size.Width, size.Height);
            using var g = Graphics.FromImage(bitmap);
            using MemoryStream s = new();

            g.CopyFromScreen(new(wndBounds.Right + offset.Right + pos.X, wndBounds.Bottom + offset.Bottom + pos.Y - 30), Point.Empty, size);
            var isFilled = bitmap.IsNotBlackAndCheckFilled(0.4, 0.3f);

            if (isFilled.Item2) SaveResult(isFilled.Item3, _spinnerResults);

            var uniques = _spinnerResults.Distinct();
            return uniques?.Count() >= 3;
        }

        private void SaveResult(double result, List<double> list)
        {
            list.Add(result);
            if (list.Count > 4) list.RemoveAt(0);
        }

        private bool StatusConfirmed(GameStatus newStatus, List<GameStatus> resultList, int threshold = 3)
        {
            resultList.Add(newStatus);

            var partial = new List<GameStatus>();
            for (int i = resultList.Count - 1; i >= 0; i--)
            {
                partial.Add(resultList[i]);

                if (partial.Count >= threshold)
                    break;
            }

            return partial.Count >= threshold && partial.Distinct().Count() == 1;
        }
    }
}
