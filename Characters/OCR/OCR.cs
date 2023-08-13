using Blish_HUD;
using Characters.Views;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Characters.Views;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework.Graphics;
using Patagames.Ocr;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Color = System.Drawing.Color;
using Point = Microsoft.Xna.Framework.Point;

namespace Kenedia.Modules.Characters
{
    public class OCR : IDisposable
    {
        private readonly OCRView _view;
        private readonly OcrApi _ocrApi;

        private readonly ClientWindowService _clientWindowService;
        private readonly SharedSettings _sharedSettings;
        private readonly Settings _settings;
        private readonly ObservableCollection<Character_Model> _characterModels;

        private readonly Color _spacingColor = Color.FromArgb(255, 200, 200, 200);
        private readonly Color _ignoredColor = Color.FromArgb(255, 100, 100, 100);

        private bool _isDisposed = false;
        private MainWindow _mainWindow;

        public MainWindow MainWindow { get => _mainWindow; set => Common.SetProperty(ref _mainWindow, value, MainWindowChanged); }

        public Texture2D SourceTexture { get; private set; }

        public Texture2D CleanedTexture { get; private set; }

        public Texture2D ScaledTexture { get; private set; }

        public string ReadResult { get; private set; }

        public string BestMatchResult { get; private set; }

        public bool IsLoaded { get; private set; }

        public string PathToEngine => OcrApi.PathToEngine;

        public OCR(ClientWindowService clientWindowService, SharedSettings sharedSettings, Settings settings, string basePath, ObservableCollection<Character_Model> characterModels)
        {
            _clientWindowService = clientWindowService;
            _sharedSettings = sharedSettings;
            _settings = settings;
            _characterModels = characterModels;

            try
            {
                string path = basePath + @"tesseract.dll";
                OcrApi.PathToEngine = path;
                Characters.Logger.Info($"Set Path to Tesseract Engine: {OcrApi.PathToEngine}. File exists: {File.Exists(path)}");
                _ocrApi = OcrApi.Create();
                _ocrApi.Init(basePath, "gw2");
                IsLoaded = true;
            }
            catch (Exception ex)
            {
                Characters.Logger.Warn($"Creating the OcrApi Instance failed. OCR will not be useable. Character names can not be confirmed.");
                Characters.Logger.Warn($"{ex}");

                MainWindow?.SendTesseractFailedNotification(PathToEngine);
            }

            _view = new(_settings, this)
            {
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = (int.MaxValue / 2) - 1,
                Visible = false,
            };
        }

        private int CustomThreshold
        {
            get => _settings.OCRNoPixelColumns.Value;
            set => _settings.OCRNoPixelColumns.Value = value;
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;
            _view?.Dispose();
            CleanedTexture?.Dispose();
            SourceTexture?.Dispose();
        }

#nullable enable
        public async Task<string?> Read(bool show = false)
        {
            string? finalText = null;

            if (IsLoaded)
            {
                try
                {
                    if (!show) _view.EnableMaskedRegion();
                    await Task.Delay(5);

                    var wndBounds = _clientWindowService.WindowBounds;

                    bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
                    Point p = windowed ? new(_sharedSettings.WindowOffset.Left, _sharedSettings.WindowOffset.Top) : Point.Zero;

                    double factor = GameService.Graphics.UIScaleMultiplier;
                    Point size = new((int)(_settings.ActiveOCRRegion.Width * factor), (int)(_settings.ActiveOCRRegion.Height * factor));

                    using (System.Drawing.Bitmap bitmap = new(size.X, size.Y))
                    {
                        System.Drawing.Bitmap spacingVisibleBitmap = new(size.X, size.Y);

                        using (var g = System.Drawing.Graphics.FromImage(bitmap))
                        {
                            int left = wndBounds.Left + p.X;
                            int top = wndBounds.Top + p.Y;

                            int x = (int)Math.Ceiling(_settings.ActiveOCRRegion.Left * factor);
                            int y = (int)Math.Ceiling(_settings.ActiveOCRRegion.Top * factor);

                            g.CopyFromScreen(new System.Drawing.Point(left + x, top + y), System.Drawing.Point.Empty, new System.Drawing.Size(size.X, size.Y));

                            if (show)
                            {
                                using MemoryStream s = new();
                                bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Bmp);

                                SourceTexture?.Dispose();
                                SourceTexture = s.CreateTexture2D();
                            }

                            int emptyPixelRow = 0;
                            bool stringStarted = false;
                            for (int i = 0; i < bitmap.Width; i++)
                            {
                                bool containsPixel = false;

                                for (int j = 0; j < bitmap.Height; j++)
                                {
                                    Color oc = bitmap.GetPixel(i, j);
                                    int threshold = _settings.OCR_ColorThreshold.Value;

                                    if (oc.R >= threshold && oc.G >= threshold && oc.B >= threshold && emptyPixelRow < CustomThreshold)
                                    {
                                        bitmap.SetPixel(i, j, Color.Black);
                                        if (show)
                                        {
                                            spacingVisibleBitmap.SetPixel(i, j, Color.Black);
                                        }

                                        containsPixel = true;
                                        stringStarted = true;
                                    }
                                    else if (emptyPixelRow >= CustomThreshold)
                                    {
                                        if (show)
                                        {
                                            spacingVisibleBitmap.SetPixel(i, j, _ignoredColor);
                                        }

                                        bitmap.SetPixel(i, j, Color.White);
                                    }
                                    else
                                    {
                                        if (show)
                                        {
                                            spacingVisibleBitmap.SetPixel(i, j, Color.White);
                                        }

                                        bitmap.SetPixel(i, j, Color.White);
                                    }
                                }

                                if (emptyPixelRow < CustomThreshold)
                                {
                                    if (!containsPixel)
                                    {
                                        if (show)
                                        {
                                            for (int j = 0; j < bitmap.Height; j++)
                                            {
                                                spacingVisibleBitmap.SetPixel(i, j, _spacingColor);
                                            }
                                        }

                                        if (stringStarted) emptyPixelRow++;
                                    }
                                    else
                                    {
                                        emptyPixelRow = 0;
                                    }
                                }
                            }

                            using (MemoryStream s = new())
                            {
                                spacingVisibleBitmap.Save(s, System.Drawing.Imaging.ImageFormat.Bmp);

                                if (show)
                                {
                                    CleanedTexture?.Dispose();
                                    CleanedTexture = s.CreateTexture2D();
                                }
                            }
                        }

                        var ocr_bitmap = bitmap;
                        double scale = 1;
                        // Patagames free license
                        if (bitmap.Width >= 500 || bitmap.Height >= 500)
                        {
                            scale = 499 / (double)Math.Max(bitmap.Width, bitmap.Height);

                            ocr_bitmap = new(bitmap, (int)(bitmap.Width * scale), (int)(bitmap.Height * scale));
                        }

                        if (show)
                        {
                            using MemoryStream s = new();
                            ocr_bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Bmp);
                            ScaledTexture?.Dispose();
                            ScaledTexture = s.CreateTexture2D();
                        }

                        string? plainText = _ocrApi?.GetTextFromImage(ocr_bitmap);

                        foreach (string word in plainText.Split(' '))
                        {
                            string wordText = word.Trim();

                            if (wordText.StartsWith("l"))
                            {
                                wordText = 'I' + wordText.Remove(0, 1);
                            }

                            finalText = finalText == null ? wordText : finalText + " " + wordText;
                        }

                        finalText = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(finalText?.ToLower());

                        BestMatchResult = GetBestMatch(finalText).Item1;
                        ReadResult = finalText;
                    }

                    if (!show) _view.DisableMaskedRegion();

                    return finalText;
                }
                catch
                {

                }
            }
            else
            {
                return null;
            }

            return "No OCR Result!";
        }

        private (string, int, int, int, bool) GetBestMatch(string name)
        {
            var distances = new List<(string, int, int, int, bool)>();

            foreach (Character_Model c in _characterModels)
            {
                int distance = name.LevenshteinDistance(c.Name);
                distances.Add((c.Name, distance, 0, 0, true));
            }

            distances.Sort((a, b) => a.Item2.CompareTo(b.Item2));
            (string, int, int, int, bool)? bestMatch = distances?.FirstOrDefault();

            return ((string, int, int, int, bool))(bestMatch is not null ? bestMatch : new(string.Empty, 0, 0, 0, false));
        }
#nullable disable

        public void ToggleContainer()
        {
            _view?.ToggleContainer();
        }

        private void MainWindowChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!IsLoaded) MainWindow?.SendTesseractFailedNotification(PathToEngine);
        }
    }
}
