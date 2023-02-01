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
using System.Diagnostics;
using Kenedia.Modules.Core.Structs;
using static Kenedia.Modules.Core.Utility.WindowsUtil.User32Dll;
using System.Threading.Tasks;
using Kenedia.Modules.Core.Controls;

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

    public class GameState : IDisposable
    {
        private bool _disposed;
        private double _lastTick = 0;
        private double? _cutsceneStart;
        private double _cutsceneDuration = 0;

        private readonly MaskedRegion _spinnerMask = new FramedMaskedRegion()
        {
            Visible = false,
            Parent = GameService.Graphics.SpriteScreen,
            ZIndex = int.MaxValue,
            BorderColor = Color.Transparent,
        };

        private readonly MaskedRegion _logoutMask = new FramedMaskedRegion()
        {
            Visible = false,
            Parent = GameService.Graphics.SpriteScreen,
            ZIndex = int.MaxValue,
            BorderColor = Color.Transparent,
        };

        private readonly List<double> _spinnerResults = new();

        private readonly List<GameStatus> _gameStatuses = new();
        private GameStatus _gameStatus = GameStatus.Unknown;
        public event EventHandler<GameStateChangedEventArgs> GameStateChanged;
        public event EventHandler<GameStateChangedEventArgs> ChangedToIngame;
        public event EventHandler<GameStateChangedEventArgs> ChangedToCharacterSelection;
        public event EventHandler<GameStateChangedEventArgs> ChangedToLoadingScreen;
        public event EventHandler<GameStateChangedEventArgs> ChangedToCutscene;

        public ClientWindowService ClientWindowService { get; set; }

        public SharedSettings SharedSettings { get; set; }

        public GameState(ClientWindowService clientWindowService, SharedSettings sharedSettings)
        {
            ClientWindowService = clientWindowService;
            SharedSettings = sharedSettings;
        }

        public int Count { get; set; } = 0;

        public Stopwatch GameTime { get; set; }

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
                _logoutMask.Hide();
                _spinnerMask.Hide();
            }
            else if (GameService.GameIntegration.Gw2Instance.Gw2HasFocus)
            {
                _spinnerMask.Show();
                _logoutMask.Show();
                // Throttle the check so we don't stress the CPU to badly with the checks
                if (gameTime.TotalGameTime.TotalMilliseconds - _lastTick > 250)
                {
                    _lastTick = gameTime.TotalGameTime.TotalMilliseconds;

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
                        _gameStatuses.Clear();
                    }
                }
            }
        }

        public bool IsButtonVisible()
        {
            RECT wndBounds = ClientWindowService.WindowBounds;
            bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
            RectangleDimensions offset = windowed ? SharedSettings.WindowOffset : new(0);

            Gw2Sharp.Mumble.Models.UiSize uiSize = GameService.Gw2Mumble.UI.UISize;
            Size size = new(50 + ((int)uiSize * 3), 40 + ((int)uiSize * 3));

            var pos = new Point(0, -size.Height);

            double factor = GameService.Graphics.UIScaleMultiplier;

            _logoutMask.Size = new((int)(size.Width * 2 * factor), (int)(size.Height * 2 * factor));
            _logoutMask.Location = new(GameService.Graphics.SpriteScreen.Left, GameService.Graphics.SpriteScreen.Bottom + (-_logoutMask.Size.Y));


            using Bitmap bitmap = new(size.Width, size.Height);
            using var g = Graphics.FromImage(bitmap);
            using MemoryStream s = new();

            g.CopyFromScreen(new(wndBounds.Left + offset.Left, wndBounds.Bottom + offset.Bottom + pos.Y), Point.Empty, size);

            (Bitmap, bool, double) cutFilled = bitmap.IsCutAndCheckFilled(0.4, 0.7f);

            return cutFilled.Item3 is > 0.35;
        }

        public bool IsLoadingSpinnerVisible()
        {
            RECT wndBounds = ClientWindowService.WindowBounds;
            bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
            RectangleDimensions offset = windowed ? SharedSettings.WindowOffset : new(0);

            Gw2Sharp.Mumble.Models.UiSize uiSize = GameService.Gw2Mumble.UI.UISize;
            Size size = new(64 + ((int)uiSize * 2), 64 + ((int)uiSize * 2));

            var pos = new Point(-size.Width, -size.Height);

            double factor = GameService.Graphics.UIScaleMultiplier;

            _spinnerMask.Size = new((int)(size.Width * 2 * factor), (int)(size.Height * 2 * factor));
            _spinnerMask.Location = new(GameService.Graphics.SpriteScreen.Right + (-_spinnerMask.Size.X), GameService.Graphics.SpriteScreen.Bottom + (-_spinnerMask.Size.Y));

            using Bitmap bitmap = new(size.Width, size.Height);
            using var g = Graphics.FromImage(bitmap);
            using MemoryStream s = new();

            g.CopyFromScreen(new(wndBounds.Right + offset.Right + pos.X, wndBounds.Bottom + offset.Bottom + pos.Y - 30), Point.Empty, size);
            (Bitmap, bool, double) isFilled = bitmap.IsNotBlackAndCheckFilled(0.4);

            if (isFilled.Item2) SaveResult(isFilled.Item3, _spinnerResults);

            IEnumerable<double> uniques = _spinnerResults.Distinct();

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

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _spinnerMask?.Dispose();
                _logoutMask?.Dispose();
            }
        }
    }
}
