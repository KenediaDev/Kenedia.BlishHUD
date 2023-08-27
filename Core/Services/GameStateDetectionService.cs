using Blish_HUD;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bitmap = System.Drawing.Bitmap;
using Graphics = System.Drawing.Graphics;
using DrawPoint = System.Drawing.Point;
using Point = Microsoft.Xna.Framework.Point;
using Kenedia.Modules.Core.Models;
using System.Diagnostics;
using Kenedia.Modules.Core.Structs;
using static Kenedia.Modules.Core.Utility.WindowsUtil.User32Dll;
using Kenedia.Modules.Core.Controls;

namespace Kenedia.Modules.Core.Services
{
    public enum ScreenRegionType
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Center,
        LoadingSpinner,
    }

    public enum GameStatusType
    {
        None = -1,
        Unknown = 0,
        Ingame,
        CharacterSelection,
        LoadingScreen,
        Cutscene,
        Vista,
        CharacterCreation,
    }

    public class GameStateDetectionService : IDisposable
    {
        private readonly double _startingMumbleTick = GameService.Gw2Mumble.Tick;
        private bool _isDisposed;
        private double _lastTick = 0;

        private readonly FramedMaskedRegion _spinnerMask = new()
        {
            Visible = false,
            Parent = GameService.Graphics.SpriteScreen,
            ZIndex = int.MaxValue,
            BorderColor = Color.Transparent,
        };

        private readonly FramedMaskedRegion _topRightMask = new()
        {
            Visible = false,
            Parent = GameService.Graphics.SpriteScreen,
            ZIndex = int.MaxValue,
            BorderColor = Color.Red,
            BorderWidth = new(2),
        };

        private readonly FramedMaskedRegion _topLeftMask = new()
        {
            Visible = false,
            Parent = GameService.Graphics.SpriteScreen,
            ZIndex = int.MaxValue,
            BorderColor = Color.Red,
            BorderWidth = new(2),
        };

        private readonly FramedMaskedRegion _bottomLeftMask = new()
        {
            Visible = false,
            Parent = GameService.Graphics.SpriteScreen,
            ZIndex = int.MaxValue,
            BorderColor = Color.Red,
            BorderWidth = new(2),
        };

        private readonly FramedMaskedRegion _bottomRightMask = new()
        {
            Visible = false,
            Parent = GameService.Graphics.SpriteScreen,
            ZIndex = int.MaxValue,
            BorderColor = Color.Red,
            BorderWidth = new(2),
        };

        private readonly FramedMaskedRegion _centerMask = new()
        {
            Visible = false,
            Parent = GameService.Graphics.SpriteScreen,
            ZIndex = int.MaxValue,
            BorderColor = Color.Red,
            BorderWidth = new(2),
        };

        private readonly List<GameStatusType> _gameStatuses = new();

        private GameStatusType _gameStatus = GameStatusType.None;

        private (Bitmap lastImage, Bitmap newImage) _bottomLeftImages = new(null, null);
        private (Bitmap lastImage, Bitmap newImage) _bottomRightImages = new(null, null);
        private (Bitmap lastImage, Bitmap newImage) _topRightImages = new(null, null);
        private (Bitmap lastImage, Bitmap newImage) _topLeftImages = new(null, null);
        private (Bitmap lastImage, Bitmap newImage) _centerImages = new(null, null);
        private (Bitmap lastImage, Bitmap newImage) _spinnerImages = new(null, null);

        public GameStateDetectionService(ClientWindowService clientWindowService, SharedSettings sharedSettings)
        {
            ClientWindowService = clientWindowService;
            SharedSettings = sharedSettings;
        }

        public event EventHandler<GameStateChangedEventArgs> GameStateChanged;
        public event EventHandler<GameStateChangedEventArgs> ChangedToIngame;
        public event EventHandler<GameStateChangedEventArgs> ChangedToCharacterSelection;
        public event EventHandler<GameStateChangedEventArgs> ChangedToLoadingScreen;
        public event EventHandler<GameStateChangedEventArgs> ChangedToCutscene;
        public event EventHandler<GameStateChangedEventArgs> ChangedToVista;

        public bool Enabled { get; set; } = true;

        public ClientWindowService ClientWindowService { get; set; }

        public SharedSettings SharedSettings { get; set; }

        private GameStatusType NewStatus { get; set; }

        public GameStatusType OldStatus { get; private set; }

        public GameStatusType GameStatus
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
                        case GameStatusType.Ingame:
                            ChangedToIngame?.Invoke(GameStatus, eventArgs);
                            break;

                        case GameStatusType.CharacterSelection:
                            ChangedToCharacterSelection?.Invoke(GameStatus, eventArgs);
                            break;

                        case GameStatusType.LoadingScreen:
                            ChangedToLoadingScreen?.Invoke(GameStatus, eventArgs);
                            break;

                        case GameStatusType.Cutscene:
                            ChangedToCutscene?.Invoke(GameStatus, eventArgs);
                            break;

                        case GameStatusType.Vista:
                            ChangedToVista?.Invoke(GameStatus, eventArgs);
                            break;
                    }

                    GameStateChanged?.Invoke(GameStatus, eventArgs);
                }
            }
        }

        public bool IsIngame => GameStatus == GameStatusType.Ingame;

        public bool IsCharacterSelection => GameStatus == GameStatusType.CharacterSelection;

        public void Run(GameTime gameTime)
        {
            if (!Enabled) return;

            NewStatus = GameStatusType.Unknown;

            if (GameService.Gw2Mumble.TimeSinceTick.TotalMilliseconds <= 500 && _startingMumbleTick != GameService.Gw2Mumble.Tick)
            {
                NewStatus = GameStatusType.Ingame;

                _topLeftMask.Hide();
                _topRightMask.Hide();
                _bottomLeftMask.Hide();
                _bottomRightMask.Hide();
                _spinnerMask.Hide();
            }

            else if (GameStatus is GameStatusType.None)
            {
                return;
            }

            else if (GameService.GameIntegration.Gw2Instance.Gw2HasFocus)
            {
                // Throttle the check so we don't stress the CPU to badly with the checks
                if (gameTime.TotalGameTime.TotalMilliseconds - _lastTick > 125)
                {
                    var sC = IsScreenChanging();

                    _topLeftMask.Show();
                    _topRightMask.Show();
                    _bottomLeftMask.Show();
                    _bottomRightMask.Show();
                    _spinnerMask.Show();

                    _lastTick = gameTime.TotalGameTime.TotalMilliseconds;

                    //Debug.WriteLine($"screenChanging {screenChanging.IsLeftChanging} | {screenChanging.IsRightChanging}");

                    // All Changing => Vista
                    bool vista = sC.AreAllChanging;

                    // No corner but center => Cutscene
                    bool cutscene = !sC.AreCornersChanging && sC.IsCenterChanging && !sC.IsSpinnerChanging;

                    // No corner but Spinner and Center => Character Selection
                    bool characterSelection = sC.NoneChanging || (!sC.AreCornersChanging && sC.IsCenterChanging && sC.IsSpinnerChanging);

                    // No corner but Spinner => Loading Screen
                    bool loadingScreen = !sC.AreCornersChanging && !sC.IsCenterChanging && sC.IsSpinnerChanging;

                    // After Character Selection we can only get loading screen or character creation
                    bool characterCreation = false;

                    // After Character Creation we can only get cutscene or ingame
                    bool creationCutscene = !sC.AreCornersChanging && sC.IsCenterChanging && sC.IsSpinnerChanging;

                    //After vista we can only get ingame
                    if ((GameStatus is GameStatusType.Ingame && vista) || GameStatus is GameStatusType.Vista)
                    {
                        NewStatus = GameStatusType.Vista;
                    }

                    //After cutscene we can only get ingame or loading screen
                    else if (GameStatus is GameStatusType.Ingame or GameStatusType.LoadingScreen && (cutscene || creationCutscene))
                    {
                        NewStatus = GameStatusType.Cutscene;
                    }

                    //Loading screens can only appear after cutscenes or character selection or while ingame
                    else if (GameStatus is GameStatusType.Cutscene or GameStatusType.Ingame or GameStatusType.CharacterSelection && loadingScreen)
                    {
                        NewStatus = GameStatusType.LoadingScreen;
                    }

                    // Character creation can only appear after Character Selection
                    else if (GameStatus is GameStatusType.CharacterSelection && characterCreation)
                    {
                        NewStatus = GameStatusType.CharacterCreation;
                    }

                    // Character selection can only appear after ingame
                    else if (GameStatus is GameStatusType.Ingame && characterSelection)
                    {
                        NewStatus = GameStatusType.CharacterSelection;
                    }
                }


                Debug.WriteLine($"NewStatus: {NewStatus}");
            }

            if (NewStatus != GameStatusType.Unknown)
            {
                if (StatusConfirmed(NewStatus, _gameStatuses, NewStatus == GameStatusType.LoadingScreen ? 3 : 8))
                {
                    if (GameStatus != NewStatus)
                    {
                        GameStatus = NewStatus;
                        _gameStatuses.Clear();
                    }
                }
            }
        }

        private (Bitmap lastImage, Bitmap newImage) CaptureRegion((Bitmap lastImage, Bitmap newImage) images, FramedMaskedRegion region, ScreenRegionType t)
        {
            images.lastImage?.Dispose();
            images.lastImage = images.newImage;

            var b = GameService.Graphics.SpriteScreen.LocalBounds;
            Point maskSize = t switch
            {
                ScreenRegionType.TopLeft => new(100, 25),
                ScreenRegionType.TopRight => new(100, 25),
                ScreenRegionType.BottomLeft => new(100, 25),
                ScreenRegionType.BottomRight => new(100, 25),
                ScreenRegionType.Center => new(100, 100),
                ScreenRegionType.LoadingSpinner => new(100, 100),
                _ => Point.Zero
            };

            Point maskPos = t switch
            {
                ScreenRegionType.TopLeft => new(0, 0),
                ScreenRegionType.TopRight => new(b.Width - maskSize.X, 0),
                ScreenRegionType.BottomLeft => new(0, b.Height - maskSize.Y),
                ScreenRegionType.BottomRight => new(b.Width - maskSize.X, b.Height - maskSize.Y),
                ScreenRegionType.Center => new(b.Center.X - (maskSize.X / 2), b.Center.Y - (maskSize.Y / 2)),
                ScreenRegionType.LoadingSpinner => new(b.Width - maskSize.X, b.Height - maskSize.Y - 50),
                _ => Point.Zero
            };

            region.BorderColor = Color.Transparent;
            region.Location = maskPos;
            region.Size = maskSize;

            try
            {
                RECT wndBounds = ClientWindowService.WindowBounds;
                bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
                RectangleDimensions offset = windowed ? SharedSettings.WindowOffset : new(0);

                Bitmap bitmap = new(maskSize.X, maskSize.Y);
                using var g = Graphics.FromImage(bitmap);
                using MemoryStream s = new();

                double factor = GameService.Graphics.UIScaleMultiplier;
                g.CopyFromScreen(new(wndBounds.Left + offset.Left + (int)(maskPos.X * factor), wndBounds.Top + offset.Top + (int)(maskPos.Y * factor)), DrawPoint.Empty, new((int)(maskSize.X * factor), (int)(maskSize.Y * factor)));

                images.newImage = bitmap;
            }
            catch
            {

            }

            return images;
        }

        private ScreenChanging IsScreenChanging()
        {
            double threshold = 0.999;
            double spinner_threshold = 1;
            double loadingspinner;
            double center;
            double topLeft;
            double topRight;
            double bottomLeft;
            double bottomRight;

            var changes = new ScreenChanging()
            {
                IsTopLeftChanging = (topLeft = CompareImagesMSE(_topLeftImages = CaptureRegion(_topLeftImages, _topLeftMask, ScreenRegionType.TopLeft))) < threshold,
                IsTopRightChanging = (topRight = CompareImagesMSE(_topRightImages = CaptureRegion(_topRightImages, _topRightMask, ScreenRegionType.TopRight))) < threshold,
                IsBottomLeftChanging = (bottomLeft = CompareImagesMSE(_bottomLeftImages = CaptureRegion(_bottomLeftImages, _bottomLeftMask, ScreenRegionType.BottomLeft))) < threshold,
                IsBottomRightChanging = (bottomRight = CompareImagesMSE(_bottomRightImages = CaptureRegion(_bottomRightImages, _bottomRightMask, ScreenRegionType.BottomRight))) < threshold,
                IsSpinnerChanging = (loadingspinner = CompareImagesMSE(_spinnerImages = CaptureRegion(_spinnerImages, _spinnerMask, ScreenRegionType.LoadingSpinner))) < spinner_threshold,
                IsCenterChanging = (center = CompareImagesMSE(_centerImages = CaptureRegion(_centerImages, _centerMask, ScreenRegionType.Center))) < threshold
            };

            //Debug.WriteLine($"Top Left        : {changes.IsTopLeftChanging} | {topLeft}");
            //Debug.WriteLine($"Top Right       : {changes.IsTopRightChanging} | {topRight}");
            //Debug.WriteLine($"Bottom Left     : {changes.IsBottomLeftChanging} | {bottomLeft}");
            //Debug.WriteLine($"Bottom Right    : {changes.IsBottomRightChanging} | {bottomRight}");
            //Debug.WriteLine($"Loading Spinner : {changes.IsSpinnerChanging} | {loadingspinner}");
            //Debug.WriteLine($"Center          : {changes.IsCenterChanging} | {center}");

            return changes;
        }

        static double CompareImagesMSE((Bitmap lastImage, Bitmap newImage) images)
        {
            var image1 = images.lastImage;
            var image2 = images.newImage;

            if (image1 is null || image2 is null || image1?.Size != image2?.Size)
            {
                return 0;
            }

            double sumSquaredDiff = 0;
            int pixelCount = image1.Width * image1.Height;

            for (int y = 0; y < image1.Height; y++)
            {
                for (int x = 0; x < image1.Width; x++)
                {
                    System.Drawing.Color color1 = image1.GetPixel(x, y);
                    System.Drawing.Color color2 = image2.GetPixel(x, y);

                    int diffR = color1.R - color2.R;
                    int diffG = color1.G - color2.G;
                    int diffB = color1.B - color2.B;

                    sumSquaredDiff += (diffR * diffR) + (diffG * diffG) + (diffB * diffB);
                }
            }

            double mse = sumSquaredDiff / pixelCount;
            double similarity = 1.0 - mse / (255.0 * 255.0 * 3.0); // Normalize to [0, 1]

            return similarity;
        }

        private bool StatusConfirmed(GameStatusType newStatus, List<GameStatusType> resultList, int threshold = 3)
        {
            resultList.Add(newStatus);

            var partial = new List<GameStatusType>();
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
            if (!_isDisposed)
            {
                _isDisposed = true;

                _spinnerMask?.Dispose();
                _spinnerImages.lastImage?.Dispose();
                _spinnerImages.newImage?.Dispose();

                _centerMask?.Dispose();
                _centerImages.lastImage?.Dispose();
                _centerImages.newImage?.Dispose();

                _topLeftMask?.Dispose();
                _topLeftImages.lastImage?.Dispose();
                _topLeftImages.newImage?.Dispose();

                _topRightMask?.Dispose();
                _topRightImages.lastImage?.Dispose();
                _topRightImages.newImage?.Dispose();

                _bottomLeftMask?.Dispose();
                _bottomLeftImages.lastImage?.Dispose();
                _bottomLeftImages.newImage?.Dispose();

                _bottomRightMask?.Dispose();
                _bottomRightImages.lastImage?.Dispose();
                _bottomRightImages.newImage?.Dispose();

            }
        }

        private class ScreenChanging
        {
            public ScreenChanging()
            {

            }

            public bool IsTopLeftChanging { get; set; }

            public bool IsTopRightChanging { get; set; }

            public bool IsBottomLeftChanging { get; set; }

            public bool IsBottomRightChanging { get; set; }

            public bool IsCenterChanging { get; set; }

            public bool IsSpinnerChanging { get; set; }

            public bool AreCornersChanging => IsTopLeftChanging || IsTopRightChanging || IsBottomLeftChanging || IsBottomRightChanging;

            public bool AreAllChanging => AreCornersChanging && IsCenterChanging && IsSpinnerChanging;

            public bool NoneChanging => !IsTopLeftChanging && !IsTopRightChanging && !IsBottomRightChanging && !IsBottomLeftChanging && !IsCenterChanging && !IsSpinnerChanging;

        }
    }
}
