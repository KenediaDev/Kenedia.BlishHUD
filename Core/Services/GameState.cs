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
using System.Diagnostics;
using Kenedia.Modules.Core.Structs;

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

    public static class GameState
    {
        private static double s_lastTick = 0;
        private static double? s_cutsceneStart;
        private static double s_cutsceneDuration = 0;

        private static readonly List<double> s_spinnerResults = new();

        private static readonly List<GameStatus> s_gameStatuses = new();
        private static GameStatus s_gameStatus = GameStatus.Unknown;
        public static event EventHandler<GameStateChangedEventArgs> GameStateChanged;
        public static event EventHandler<GameStateChangedEventArgs> ChangedToIngame;
        public static event EventHandler<GameStateChangedEventArgs> ChangedToCharacterSelection;
        public static event EventHandler<GameStateChangedEventArgs> ChangedToLoadingScreen;
        public static event EventHandler<GameStateChangedEventArgs> ChangedToCutscene;

        public static int Count { get; set; } = 0;

        public static Stopwatch GameTime { get; set; }

        private static GameStatus NewStatus { get; set; }

        public static GameStatus OldStatus { get; private set; }

        public static GameStatus GameStatus
        {
            get => s_gameStatus;
            private set
            {
                if (s_gameStatus != value)
                {
                    OldStatus = s_gameStatus;
                    s_gameStatus = value;

                    var eventArgs = new GameStateChangedEventArgs()
                    {
                        OldStatus = OldStatus,
                        Status = s_gameStatus,
                    };

                    switch (s_gameStatus)
                    {
                        case GameStatus.Ingame:
                            s_spinnerResults.Clear();
                            s_cutsceneDuration = 0;
                            s_cutsceneStart = null;
                            ChangedToIngame?.Invoke(GameStatus, eventArgs);
                            break;

                        case GameStatus.CharacterSelection:
                            s_spinnerResults.Clear();
                            s_cutsceneDuration = 0;
                            s_cutsceneStart = null;
                            ChangedToCharacterSelection?.Invoke(GameStatus, eventArgs);
                            break;

                        case GameStatus.LoadingScreen:
                            s_cutsceneDuration = 0;
                            s_cutsceneStart = null;
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

        public static bool IsIngame => GameStatus == GameStatus.Ingame;

        public static void Run(GameTime gameTime)
        {
            NewStatus = GameStatus.Unknown;

            if (GameService.Gw2Mumble.TimeSinceTick.TotalMilliseconds <= 500)
            {
                NewStatus = GameStatus.Ingame;
            }
            else if (GameService.GameIntegration.Gw2Instance.Gw2HasFocus)
            {
                // Throttle the check so we don't stress the CPU to badly with the checks
                if (gameTime.TotalGameTime.TotalMilliseconds - s_lastTick > 250)
                {
                    s_lastTick = gameTime.TotalGameTime.TotalMilliseconds;

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
                                s_cutsceneStart ??= gameTime.TotalGameTime.TotalMilliseconds;
                                s_cutsceneDuration += gameTime.TotalGameTime.TotalMilliseconds - (double)s_cutsceneStart;
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
                                    NewStatus = s_cutsceneDuration > 1000 ? GameStatus.Cutscene : NewStatus;
                                }
                            }
                            else if (GameStatus is GameStatus.Ingame)
                            {
                                // From ingame we can go to all game states, character selection is reliable from ingame and checked already, so we now test only Loading screen and Cutscene
                                NewStatus = isLoadingSpinnerVisible ? GameStatus.LoadingScreen : s_cutsceneDuration > 1000 ? GameStatus.Cutscene : GameStatus.Unknown;
                            }
                        }
                    }
                }
            }

            if (NewStatus != GameStatus.Unknown)
            {
                if (StatusConfirmed(NewStatus, s_gameStatuses, NewStatus == GameStatus.LoadingScreen ? 6 : 3))
                {
                    if (GameStatus != NewStatus)
                    {
                        Core.Logger.Info($"Game status changed from '{GameStatus}' to '{NewStatus}'");
                        GameStatus = NewStatus;
                        s_gameStatuses.Clear();
                    }
                }
            }
        }

        public static bool IsButtonVisible()
        {
            RECT wndBounds = ClientWindowService.WindowBounds;
            bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
            RectangleDimensions offset = windowed ? SharedSettings.WindowOffset : new(0);

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

        public static bool IsLoadingSpinnerVisible()
        {
            RECT wndBounds = ClientWindowService.WindowBounds;
            bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
            RectangleDimensions offset = windowed ? SharedSettings.WindowOffset : new(0);

            var uiSize = GameService.Gw2Mumble.UI.UISize;
            Size size = new(64 + ((int)uiSize * 2), 64 + ((int)uiSize * 2));

            var pos = new Point(-size.Width, -size.Height);

            using Bitmap bitmap = new(size.Width, size.Height);
            using var g = Graphics.FromImage(bitmap);
            using MemoryStream s = new();

            g.CopyFromScreen(new(wndBounds.Right + offset.Right + pos.X, wndBounds.Bottom + offset.Bottom + pos.Y - 30), Point.Empty, size);
            var isFilled = bitmap.IsNotBlackAndCheckFilled(0.4, 0.3f);

            if (isFilled.Item2) SaveResult(isFilled.Item3, s_spinnerResults);

            var uniques = s_spinnerResults.Distinct();
            return uniques?.Count() >= 3;
        }

        private static void SaveResult(double result, List<double> list)
        {
            list.Add(result);
            if (list.Count > 4) list.RemoveAt(0);
        }

        private static bool StatusConfirmed(GameStatus newStatus, List<GameStatus> resultList, int threshold = 3)
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
