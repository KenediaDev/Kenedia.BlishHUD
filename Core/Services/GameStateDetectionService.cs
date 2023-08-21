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
using Size = System.Drawing.Size;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using System.Diagnostics;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Structs;
using static Kenedia.Modules.Core.Utility.WindowsUtil.User32Dll;
using Kenedia.Modules.Core.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Kenedia.Modules.Core.Services
{
    public enum ScreenRegionType
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public enum GameStatus
    {
        Unknown = 0,
        Ingame,
        CharacterSelection,
        LoadingScreen,
        Cutscene
    }

    public class GameStateDetectionService : IDisposable
    {
        private bool _isDisposed;
        private double _lastTick = 0;
        private double? _cutsceneStart;
        private double _cutsceneDuration = 0;

        private readonly Label _gameStateLabel = new()
        {
            Visible = true,
            Parent = GameService.Graphics.SpriteScreen,
            ZIndex = int.MaxValue,
            Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size36, ContentService.FontStyle.Regular),
            BackgroundColor = Color.Orange,
            Width = 200,
            Height = 50,
        };
        private readonly FramedMaskedRegion _spinnerMask = new()
        {
            Visible = false,
            Parent = GameService.Graphics.SpriteScreen,
            ZIndex = int.MaxValue,
            BorderColor = Color.Transparent,
        };

        private readonly FramedMaskedRegion _logoutMask = new()
        {
            Visible = false,
            Parent = GameService.Graphics.SpriteScreen,
            ZIndex = int.MaxValue,
            BorderColor = Color.Transparent,
        };

        private readonly Image _topRightImage = new()
        {
            Visible = false,
            Parent = GameService.Graphics.SpriteScreen,
            ZIndex = int.MaxValue,
            BackgroundColor = Color.Red,
        };

        private readonly Image _topLeftImage = new()
        {
            Visible = false,
            Parent = GameService.Graphics.SpriteScreen,
            ZIndex = int.MaxValue,
            BackgroundColor = Color.Red,
        };

        private readonly Image _bottomRightImage = new()
        {
            Visible = false,
            Parent = GameService.Graphics.SpriteScreen,
            ZIndex = int.MaxValue,
            BackgroundColor = Color.Red,
        };

        private readonly Image _bottomLeftImage = new()
        {
            Visible = false,
            Parent = GameService.Graphics.SpriteScreen,
            ZIndex = int.MaxValue,
            BackgroundColor = Color.Red,
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

        private readonly List<double> _spinnerResults = new();

        private readonly List<GameStatus> _gameStatuses = new();
        private GameStatus _gameStatus = GameStatus.Unknown;

        private readonly List<(double time, Bitmap image, double difference)> _imageCache = new();
        private (Bitmap lastImage, Bitmap newImage) _bottomLeftImages = new(null, null);
        private (Bitmap lastImage, Bitmap newImage) _bottomRightImages = new(null, null);
        private (Bitmap lastImage, Bitmap newImage) _topRightImages = new(null, null);
        private (Bitmap lastImage, Bitmap newImage) _topLeftImages = new(null, null);

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

        public bool Enabled { get; set; } = true;

        public ClientWindowService ClientWindowService { get; set; }

        public SharedSettings SharedSettings { get; set; }

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

        public bool IsCharacterSelection => GameStatus == GameStatus.CharacterSelection;

        public void Run(GameTime gameTime)
        {
            if (!Enabled) return;

            NewStatus = GameStatus.Unknown;

            _gameStateLabel.Location = GameService.Graphics.SpriteScreen.LocalBounds.Center.Add(new(-_gameStateLabel.Width / 2, -_gameStateLabel.Height / 2));
            _gameStateLabel.Size = new(400, 50);
            _gameStateLabel.HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Center;

            if (GameService.Gw2Mumble.TimeSinceTick.TotalMilliseconds <= 500)
            {
                NewStatus = GameStatus.Ingame;

                _topLeftMask.Hide();
                _topRightMask.Hide();
                _bottomLeftMask.Hide();
                _bottomRightMask.Hide();
            }

            else if (GameService.GameIntegration.Gw2Instance.Gw2HasFocus)
            {
                // Throttle the check so we don't stress the CPU to badly with the checks
                if (gameTime.TotalGameTime.TotalMilliseconds - _lastTick > 250)
                {
                    var sC = IsScreenChanging();

                    _topLeftMask.Show();
                    _topRightMask.Show();
                    _bottomLeftMask.Show();
                    _bottomRightMask.Show();

                    _lastTick = gameTime.TotalGameTime.TotalMilliseconds;

                    //Debug.WriteLine($"screenChanging {screenChanging.IsLeftChanging} | {screenChanging.IsRightChanging}");

                    if (GameStatus is not GameStatus.CharacterSelection && sC.IsTopRightChanging && sC.IsBottomRightChanging && sC.IsTopLeftChanging && sC.IsBottomLeftChanging)
                    {
                        NewStatus = GameStatus.Cutscene;
                    }
                    else if (!sC.IsTopRightChanging && !sC.IsTopLeftChanging && sC.IsBottomRightChanging && !sC.IsBottomLeftChanging)
                    {
                        NewStatus = GameStatus.LoadingScreen;
                    }
                    else if (GameStatus is not GameStatus.LoadingScreen && !sC.IsTopRightChanging && !sC.IsTopLeftChanging && !sC.IsBottomLeftChanging)
                    {
                        NewStatus = GameStatus.CharacterSelection;
                    }

                    Debug.WriteLine($"NewStatus: {NewStatus}");
                }
            }

            if (NewStatus != GameStatus.Unknown)
            {
                if (StatusConfirmed(NewStatus, _gameStatuses, NewStatus == GameStatus.LoadingScreen ? 2 : 2))
                {
                    if (GameStatus != NewStatus)
                    {
                        Debug.WriteLine($"NewStatus {NewStatus}");
                        GameStatus = NewStatus;
                        _gameStatuses.Clear();
                        _gameStateLabel.Text = GameStatus.ToString();
                    }
                }
            }
        }

        private (Bitmap lastImage, Bitmap newImage) CaptureRegion((Bitmap lastImage, Bitmap newImage) images, FramedMaskedRegion region, ScreenRegionType t)
        {
            images.lastImage?.Dispose();
            images.lastImage = images.newImage;

            var b = GameService.Graphics.SpriteScreen.LocalBounds;
            Color maskColor = t switch
            {
                ScreenRegionType.TopLeft => Color.Red,
                ScreenRegionType.TopRight => Color.Green,
                ScreenRegionType.BottomLeft => Color.Blue,
                ScreenRegionType.BottomRight => Color.Yellow,
                _ => Color.Transparent
            };

            Point maskSize = t switch
            {
                ScreenRegionType.TopLeft => new(100, 25),
                ScreenRegionType.TopRight => new(100, 25),
                ScreenRegionType.BottomLeft => new(100, 25),
                ScreenRegionType.BottomRight => new(100, 150),
                _ => Point.Zero
            };

            Point maskPos = t switch
            {
                ScreenRegionType.TopLeft => new(0, 0),
                ScreenRegionType.TopRight => new(b.Width - maskSize.X, 0),
                ScreenRegionType.BottomLeft => new(0, b.Height - maskSize.Y),
                ScreenRegionType.BottomRight => new(b.Width - maskSize.X, b.Height - maskSize.Y),
                _ => Point.Zero
            };

            Point imagePos = t switch
            {
                ScreenRegionType.TopLeft => maskPos.Add(new(0, maskSize.Y)),
                ScreenRegionType.TopRight => maskPos.Add(new(0, maskSize.Y)),
                ScreenRegionType.BottomLeft => maskPos.Add(new(0, -maskSize.Y)),
                ScreenRegionType.BottomRight => maskPos.Add(new(0, -maskSize.Y)),
                _ => Point.Zero
            };

            Image image = t switch
            {
                ScreenRegionType.TopLeft => _topLeftImage,
                ScreenRegionType.TopRight => _topRightImage,
                ScreenRegionType.BottomLeft => _bottomLeftImage,
                ScreenRegionType.BottomRight => _bottomRightImage,
                _ => null
            };

            region.BorderColor = Color.Transparent;
            region.Location = maskPos;
            region.Size = maskSize;

            image.BackgroundColor = maskColor;
            image.Location = imagePos;
            image.Size = maskSize;

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

                //image.Texture = bitmap.CreateTexture2D();
                images.newImage = bitmap;
            }
            catch
            {

            }

            return images;
        }

        /// <summary>
        /// Captures the region for the spinner and checks if the spinner is visible/the region changes.
        /// </summary>
        private void CaptureRightBottom()
        {
            if (_bottomRightImages.lastImage is not null) _bottomRightImages.lastImage.Dispose();
            if (_bottomRightImages.newImage is not null) _bottomRightImages.lastImage = _bottomRightImages.newImage;

            RECT wndBounds = ClientWindowService.WindowBounds;
            bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
            RectangleDimensions offset = windowed ? SharedSettings.WindowOffset : new(0);

            Gw2Sharp.Mumble.Models.UiSize uiSize = GameService.Gw2Mumble.UI.UISize;
            Size size = new(100 + ((int)uiSize * 2), 128 + ((int)uiSize * 2));

            var pos = new DrawPoint(-size.Width, -size.Height);

            double factor = GameService.Graphics.UIScaleMultiplier;

            _spinnerMask.Size = new((int)(size.Width * 2 * factor), (int)(size.Height * 2 * factor));
            _spinnerMask.Location = new(GameService.Graphics.SpriteScreen.Right + (-_spinnerMask.Size.X), GameService.Graphics.SpriteScreen.Bottom + (-_spinnerMask.Size.Y));

            try
            {
                Bitmap bitmap = new(size.Width, size.Height);
                using var g = Graphics.FromImage(bitmap);
                using MemoryStream s = new();

                g.CopyFromScreen(new(wndBounds.Right + offset.Right + pos.X, wndBounds.Bottom + offset.Bottom + pos.Y - 30), DrawPoint.Empty, size);

                _bottomRightImages.newImage = bitmap;
            }
            catch
            {

            }
        }

        private void CaptureRightTop()
        {
            _topRightImages.lastImage?.Dispose();
            if (_topRightImages.newImage is not null) _topRightImages.lastImage = _topRightImages.newImage;

            RECT wndBounds = ClientWindowService.WindowBounds;
            bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
            RectangleDimensions offset = windowed ? SharedSettings.WindowOffset : new(0);

            Gw2Sharp.Mumble.Models.UiSize uiSize = GameService.Gw2Mumble.UI.UISize;
            Size size = new(100 + ((int)uiSize * 2), 128 + ((int)uiSize * 2));

            var pos = new DrawPoint(-size.Width, -size.Height);

            double factor = GameService.Graphics.UIScaleMultiplier;

            _spinnerMask.Size = new((int)(size.Width * 2 * factor), (int)(size.Height * 2 * factor));
            _spinnerMask.Location = new(GameService.Graphics.SpriteScreen.Right + (-_spinnerMask.Size.X), GameService.Graphics.SpriteScreen.Bottom + (-_spinnerMask.Size.Y));

            try
            {
                Bitmap bitmap = new(size.Width, size.Height);
                using var g = Graphics.FromImage(bitmap);
                using MemoryStream s = new();

                g.CopyFromScreen(new(wndBounds.Right + offset.Right + pos.X, wndBounds.Bottom + offset.Bottom + pos.Y - 30), DrawPoint.Empty, size);

                _topRightImages.newImage = bitmap;
            }
            catch
            {

            }
        }

        /// <summary>
        /// Captures the region for the logout button and checks if the button is visible/the region changes.
        /// </summary>
        private void CaptureLeftBottom()
        {
            _bottomLeftImages.lastImage?.Dispose();
            if (_bottomLeftImages.newImage is not null) _bottomLeftImages.lastImage = _bottomLeftImages.newImage;

            RECT wndBounds = ClientWindowService.WindowBounds;
            bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
            RectangleDimensions offset = windowed ? SharedSettings.WindowOffset : new(0);

            Gw2Sharp.Mumble.Models.UiSize uiSize = GameService.Gw2Mumble.UI.UISize;
            Size size = new(50 + ((int)uiSize * 3), 40 + ((int)uiSize * 3));

            var pos = new DrawPoint(0, -size.Height);

            double factor = GameService.Graphics.UIScaleMultiplier;

            _logoutMask.Size = new((int)(size.Width * 2 * factor), (int)(size.Height * 2 * factor));
            _logoutMask.Location = new(GameService.Graphics.SpriteScreen.Left, GameService.Graphics.SpriteScreen.Bottom + (-_logoutMask.Size.Y));

            try
            {
                Bitmap bitmap = new(size.Width, size.Height);
                using var g = Graphics.FromImage(bitmap);
                using MemoryStream s = new();

                g.CopyFromScreen(new(wndBounds.Left + offset.Left, wndBounds.Bottom + offset.Bottom + pos.Y), DrawPoint.Empty, size);

                _bottomLeftImages.newImage = bitmap;
            }
            catch
            {

            }
        }

        private ScreenChanging IsScreenChanging()
        {
            double topLeft = 0;
            double topRight = 0;
            double bottomLeft = 0;
            double bottomRight = 0;

            double threshold = 0.999;
            var changes = new ScreenChanging()
            {
                IsTopLeftChanging = (topLeft = CompareImagesMSE(_topLeftImages = CaptureRegion(_topLeftImages, _topLeftMask, ScreenRegionType.TopLeft))) < threshold,
                IsTopRightChanging = (topRight = CompareImagesMSE(_topRightImages = CaptureRegion(_topRightImages, _topRightMask, ScreenRegionType.TopRight))) < threshold,
                IsBottomLeftChanging = (bottomLeft = CompareImagesMSE(_bottomLeftImages = CaptureRegion(_bottomLeftImages, _bottomLeftMask, ScreenRegionType.BottomLeft))) < threshold,
                IsBottomRightChanging = (bottomRight = CompareImagesMSE(_bottomRightImages = CaptureRegion(_bottomRightImages, _bottomRightMask, ScreenRegionType.BottomRight))) < threshold
            };

            Debug.WriteLine($"Top Left: {topLeft} | Changing {topLeft < threshold}");
            Debug.WriteLine($"Top Right: {topRight} | Changing {topRight < threshold}");
            Debug.WriteLine($"Bottom Left: {bottomLeft} | Changing {bottomLeft < threshold}");
            Debug.WriteLine($"Bottom Right: {bottomRight} | Changing {bottomRight < threshold}");

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

        public bool IsButtonVisible()
        {
            RECT wndBounds = ClientWindowService.WindowBounds;
            bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
            RectangleDimensions offset = windowed ? SharedSettings.WindowOffset : new(0);

            Gw2Sharp.Mumble.Models.UiSize uiSize = GameService.Gw2Mumble.UI.UISize;
            Size size = new(50 + ((int)uiSize * 3), 40 + ((int)uiSize * 3));

            var pos = new DrawPoint(0, -size.Height);

            double factor = GameService.Graphics.UIScaleMultiplier;

            _logoutMask.Size = new((int)(size.Width * 2 * factor), (int)(size.Height * 2 * factor));
            _logoutMask.Location = new(GameService.Graphics.SpriteScreen.Left, GameService.Graphics.SpriteScreen.Bottom + (-_logoutMask.Size.Y));

            using Bitmap bitmap = new(size.Width, size.Height);
            using var g = Graphics.FromImage(bitmap);
            using MemoryStream s = new();

            g.CopyFromScreen(new(wndBounds.Left + offset.Left, wndBounds.Bottom + offset.Bottom + pos.Y), DrawPoint.Empty, size);

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

            var pos = new DrawPoint(-size.Width, -size.Height);

            double factor = GameService.Graphics.UIScaleMultiplier;

            _spinnerMask.Size = new((int)(size.Width * 2 * factor), (int)(size.Height * 2 * factor));
            _spinnerMask.Location = new(GameService.Graphics.SpriteScreen.Right + (-_spinnerMask.Size.X), GameService.Graphics.SpriteScreen.Bottom + (-_spinnerMask.Size.Y));

            try
            {
                using Bitmap bitmap = new(size.Width, size.Height);
                using var g = Graphics.FromImage(bitmap);
                using MemoryStream s = new();

                g.CopyFromScreen(new(wndBounds.Right + offset.Right + pos.X, wndBounds.Bottom + offset.Bottom + pos.Y - 30), DrawPoint.Empty, size);
                (Bitmap, bool, double) isFilled = bitmap.IsNotBlackAndCheckFilled(0.4);

                if (isFilled.Item2) SaveResult(isFilled.Item3, _spinnerResults);
            }
            catch (Exception)
            {

            }

            IEnumerable<double> uniques = _spinnerResults?.Distinct();

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
            if (!_isDisposed)
            {
                _isDisposed = true;
                _spinnerMask?.Dispose();
                _logoutMask?.Dispose();
            }
        }

        private class ScreenChanging
        {
            public ScreenChanging()
            {

            }

            public ScreenChanging(bool isTopLeftChanging, bool isTopRightChanging, bool isBottomLeftChanging, bool isBottomRightChanging)
            {
                IsTopLeftChanging = isTopLeftChanging;
                IsTopRightChanging = isTopRightChanging;
                IsBottomLeftChanging = isBottomLeftChanging;
                IsBottomRightChanging = isBottomRightChanging;
            }

            public bool IsTopLeftChanging { get; set; }

            public bool IsTopRightChanging { get; set; }

            public bool IsBottomLeftChanging { get; set; }

            public bool IsBottomRightChanging { get; set; }
        }
    }
}
