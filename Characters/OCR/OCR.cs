using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Characters.Res;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework;
using Patagames.Ocr;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Kenedia.Modules.Characters.Utility.WindowsUtil.WindowsUtil;
using Color = Microsoft.Xna.Framework.Color;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using ImageButton = Kenedia.Modules.Core.Controls.ImageButton;
using Label = Kenedia.Modules.Core.Controls.Label;
using Panel = Kenedia.Modules.Core.Controls.Panel;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters
{
    public class OCR : IDisposable
    {
        private readonly System.Drawing.Color _spacingColor = System.Drawing.Color.FromArgb(255, 200, 200, 200);
        private readonly System.Drawing.Color _ignoredColor = System.Drawing.Color.FromArgb(255, 100, 100, 100);

        private readonly FramedContainer _contentContainer;
        private readonly ClientWindowService _clientWindowService;
        private readonly SharedSettings _sharedSettings;
        private readonly SettingsModel  _settings;
        private readonly ObservableCollection<Character_Model> _characterModels;

        private readonly NumberBox _columnBox;
        private readonly NumberBox _thresholdBox;
        private readonly Label _instructions;
        private readonly Label _closestMatch;
        private readonly Label _result;
        private readonly Image _ocrResultImage;
        private readonly Image _ocrResultImageBlackWhite;
        private readonly ImageButton _closeButton;
        private readonly ResizeableContainer _container;
        private readonly OcrApi _ocrApi;
        //private readonly OCR_TrainDisplay _ocrTrainDisplay;

        private bool _disposed = false;

        public OCR(ClientWindowService clientWindowService, SharedSettings sharedSettings, SettingsModel settings, string basePath, ObservableCollection<Character_Model>  characterModels)
        {
            _clientWindowService = clientWindowService;
            _sharedSettings = sharedSettings;
            _settings = settings;
            _characterModels = characterModels;

            OcrApi.PathToEngine = basePath + @"\tesseract.dll";

            _ocrApi = OcrApi.Create();
            _ocrApi.Init(basePath + @"\", "gw2");

            _contentContainer = new FramedContainer()
            {
                Parent = GameService.Graphics.SpriteScreen,
                BorderColor = Color.Black, // new Color(32, 32 , 32),
                BackgroundImage = AsyncTexture2D.FromAssetId(156003),
                TextureRectangle = new Rectangle(50, 50, 500, 500),
                Height = 290,
                Width = 620,
                ZIndex = 999,
                Visible = false,
            };

            FlowPanel contentFlowPanel = new()
            {
                Parent = _contentContainer,
                Width = _contentContainer.Width,
                HeightSizingMode = SizingMode.AutoSize,
                AutoSizePadding = new Point(3, 3),
                OuterControlPadding = new(3, 3),
                ControlPadding = new(3, 3),
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
            };

            var headerPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                Parent = contentFlowPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(5, 5),
            };

            _instructions = new Label()
            {
                Parent = headerPanel,
                AutoSizeHeight = true,
                Width = _contentContainer.Width - 35,
                WrapText = true,
                TextColor = ContentService.Colors.ColonialWhite,
                SetLocalizedText = () => strings.OCR_Instructions,
            };

            _closeButton = new()
            {
                Parent = headerPanel,
                Texture = AsyncTexture2D.FromAssetId(156012),
                HoveredTexture = AsyncTexture2D.FromAssetId(156011),
                Size = new Point(25, 25),
                TextureRectangle = new Rectangle(7, 7, 20, 20),
            };
            _closeButton.Click += CloseButton_Click;

            var fp = new FlowPanel()
            {
                Parent = contentFlowPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new(10, 0),
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
            };
            var p = new FramedContainer()
            {
                Parent = fp,
                Width = 500,
                Height = GameService.Content.DefaultFont32.LineHeight + 8,
                BorderColor = Color.Black * 0.7f,
                BackgroundColor = Color.Black * 0.4f,
            };
            _closestMatch = new Label()
            {
                Location = new(5, 0),
                Parent = p,
                Height = p.Height,
                AutoSizeWidth = true,
                TextColor = ContentService.Colors.ColonialWhite,
                Font = GameService.Content.DefaultFont32,
                VerticalAlignment = VerticalAlignment.Middle,
            };
            _ = new Label()
            {
                Parent = fp,
                VerticalAlignment = VerticalAlignment.Middle,
                Height = p.Height,
                Width = 100,
                TextColor = Color.White,
                Font = GameService.Content.DefaultFont16,
                WrapText = true,
                SetLocalizedText = () => "Best Match",
            };

            fp = new FlowPanel()
            {
                Parent = contentFlowPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new(10, 0),
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
            };
            p = new FramedContainer()
            {
                Parent = fp,
                Width = 500,
                Height = GameService.Content.DefaultFont32.LineHeight + 8,
                BorderColor = Color.Black * 0.7f,
                BackgroundColor = Color.Black * 0.4f,
            };
            _result = new Label()
            {
                Location = new(5, 0),
                Parent = p,
                Height = p.Height,
                AutoSizeWidth = true,
                TextColor = ContentService.Colors.ColonialWhite,
                Font = GameService.Content.DefaultFont32,
                VerticalAlignment = VerticalAlignment.Middle,
            };
            _ = new Label()
            {
                Parent = fp,
                VerticalAlignment = VerticalAlignment.Middle,
                Height = p.Height,
                Width = 100,
                TextColor = Color.White,
                Font = GameService.Content.DefaultFont16,
                WrapText = true,
                SetLocalizedText = () => "OCR Result",
            };

            fp = new FlowPanel()
            {
                Parent = contentFlowPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new(10, 0),
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
            };

            p = new FramedContainer()
            {
                Parent = fp,
                Width = 500,
                Height = 55,
                BorderColor = Color.Black * 0.7f,
                BackgroundColor = Color.Black * 0.4f,
            };

            _ocrResultImageBlackWhite = new Image()
            {
                Location = new(5, 5),
                Parent = p,
            };
            _ = new Label()
            {
                Parent = fp,
                Height = p.Height,
                Width = 100,
                TextColor = Color.White,
                Font = GameService.Content.DefaultFont16,
                WrapText = true,
                SetLocalizedText = () => "Cleaned",
                VerticalAlignment = VerticalAlignment.Middle,
            };
            fp = new FlowPanel()
            {
                Parent = contentFlowPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new(10, 0),
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
            };

            p = new FramedContainer()
            {
                Parent = fp,
                Width = 500,
                Height = 55,
                BorderColor = Color.Black * 0.7f,
                BackgroundColor = Color.Black * 0.4f,
            };

            _ocrResultImage = new Image()
            {
                Location = new(5, 5),
                Parent = p,
            };
            _ = new Label()
            {
                Parent = fp,
                Height = p.Height,
                Width = 100,
                TextColor = Color.White,
                Font = GameService.Content.DefaultFont16,
                WrapText = true,
                SetLocalizedText = () => "Source",
                VerticalAlignment = VerticalAlignment.Middle,
            };

            FlowPanel thresholdPanel = new()
            {
                Parent = contentFlowPanel,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                AutoSizePadding = new Point(5, 5),
                OuterControlPadding = new Vector2(0, 5),
                ControlPadding = new Vector2(5, 5),
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
            };

            _ = new Label()
            {
                Parent = thresholdPanel,
                Height = 25,
                AutoSizeWidth = true,
                TextColor = ContentService.Colors.ColonialWhite,
                SetLocalizedText = () => strings.EmptyColumns,
                SetLocalizedTooltip = () => strings.EmptyColumns_Tooltip,
            };
            _columnBox = new NumberBox()
            {
                Parent = thresholdPanel,
                Size = new Point(100, 25),
                MinValue = 0,
                MaxValue = 100,
                Value = _settings.OCRNoPixelColumns.Value,
                SetLocalizedTooltip = () => strings.EmptyColumnsThreshold_Tooltip,
                ValueChangedAction = (num) => _settings.OCRNoPixelColumns.Value = num,
            };

            _ = new Panel()
            {
                Parent = thresholdPanel,
                BackgroundColor = new Color(_spacingColor.R, _spacingColor.G, _spacingColor.B, _spacingColor.A),
                Size = new Point(25, 25),
            };
            _ = new Label()
            {
                Parent = thresholdPanel,
                Height = 25,
                AutoSizeWidth = true,
                TextColor = ContentService.Colors.ColonialWhite,
                SetLocalizedText = () => strings.EmptyColumn,
                SetLocalizedTooltip = () => strings.EmptyColumn_Tooltip,
            };
            _ = new Panel()
            {
                Parent = thresholdPanel,
                BackgroundColor = new Color(_ignoredColor.R, _ignoredColor.G, _ignoredColor.B, _ignoredColor.A),
                Size = new Point(25, 25),
            };
            _ = new Label()
            {
                Parent = thresholdPanel,
                Height = 25,
                AutoSizeWidth = true,
                TextColor = ContentService.Colors.ColonialWhite,
                SetLocalizedText = () => strings.IgnoredPart,
                SetLocalizedTooltip = () => strings.IgnoredPart_Tooltip,
            };

            _thresholdBox = new NumberBox()
            {
                Parent = thresholdPanel,
                Height = 25,
                Width = 100,
                MinValue = 0,
                MaxValue = 255,
                Value = _settings.OCR_ColorThreshold.Value,
                SetLocalizedTooltip = () => "Threshold of 'white' a pixel has to be to be converted to black to be read (RGB Value: 0 - 255)",
                ValueChangedAction = (num) => _settings.OCR_ColorThreshold.Value = num
            };

            _container = new ResizeableContainer()
            {
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = 999,
                Visible = false,
                Location = _settings.ActiveOCRRegion.Location,
                Size = _settings.ActiveOCRRegion.Size,
                BorderColor = ContentService.Colors.ColonialWhite,
                ShowResizeOnlyOnMouseOver = true,
                MaxSize = new(500, 50),
                BorderWidth = new(1, 1, 1, 1),
            };
            _container.Resized += Container_Changed;
            _container.Moved += Container_Changed;
            _container.LeftMouseButtonReleased += Container_LeftMouseButtonReleased;
            _container.MouseLeft += Container_LeftMouseButtonReleased;

            int height = _settings.ActiveOCRRegion.Size.Y;
            _contentContainer.Location = new Point(_container.Left, _container.Top - _contentContainer.Height - 5);
        }

        private void CloseButton_Click(object sender, MouseEventArgs e)
        {
            ToggleContainer();
        }

        private int CustomThreshold
        {
            get => _settings.OCRNoPixelColumns.Value;
            set => _settings.OCRNoPixelColumns.Value = value;
        }

        public void ToggleContainer()
        {
            _contentContainer?.ToggleVisibility();
            _container?.ToggleVisibility();

            if (_container.Visible)
            {
                _ = Read(true);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _columnBox?.Dispose();
                _thresholdBox?.Dispose();
                _container?.Dispose();
                _instructions?.Dispose();
                _ocrResultImage?.Dispose();
                _ocrResultImageBlackWhite?.Dispose();
                _result?.Dispose();
                _contentContainer?.Dispose();
            }
        }

#nullable enable
        public string? Read(bool show = false)
        {
            string? finalText = null;

            if (_container.Visible && !show)
            {
                ToggleContainer();
                return null;
            }

            var wndBounds = _clientWindowService.WindowBounds;

            bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
            Point p = windowed ? new(_sharedSettings.WindowOffset.Left, _sharedSettings.WindowOffset.Top) : Point.Zero;

            double factor = GameService.Graphics.UIScaleMultiplier;
            Point size = new((int)((_container.Width - _container.BorderWidth.Vertical) * factor), (int)((_container.Height - _container.BorderWidth.Horizontal) * factor));

            using (System.Drawing.Bitmap bitmap = new(size.X, size.Y))
            {
                System.Drawing.Bitmap spacingVisibleBitmap = new(size.X, size.Y);

                using (var g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    int left = wndBounds.Left + p.X + _container.BorderWidth.Left;
                    int top = wndBounds.Top + p.Y + _container.BorderWidth.Top;

                    int x = (int)Math.Ceiling(_container.Left * factor);
                    int y = (int)Math.Ceiling(_container.Top * factor);

                    g.CopyFromScreen(new System.Drawing.Point(left + x, top + y), System.Drawing.Point.Empty, new System.Drawing.Size(size.X, size.Y));

                    if (show)
                    {
                        using MemoryStream s = new();
                        bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Bmp);

                        _ocrResultImage.Size = new Point(bitmap.Size.Width, bitmap.Size.Height);
                        _ocrResultImage.Texture = s.CreateTexture2D();
                    }

                    var black = System.Drawing.Color.FromArgb(255, 0, 0, 0);
                    var white = System.Drawing.Color.FromArgb(255, 255, 255, 255);

                    int emptyPixelRow = 0;
                    for (int i = 0; i < bitmap.Width; i++)
                    {
                        bool containsPixel = false;

                        for (int j = 0; j < bitmap.Height; j++)
                        {
                            System.Drawing.Color oc = bitmap.GetPixel(i, j);
                            int threshold = _settings.OCR_ColorThreshold.Value;

                            if (oc.R >= threshold && oc.G >= threshold && oc.B >= threshold && emptyPixelRow < CustomThreshold)
                            //if (oc.GetBrightness() > 0.5f && emptyPixelRow < CustomThreshold)
                            {
                                bitmap.SetPixel(i, j, black);
                                if (show)
                                {
                                    spacingVisibleBitmap.SetPixel(i, j, black);
                                }

                                containsPixel = true;
                            }
                            else if (emptyPixelRow >= CustomThreshold)
                            {
                                if (show)
                                {
                                    spacingVisibleBitmap.SetPixel(i, j, _ignoredColor);
                                }

                                bitmap.SetPixel(i, j, white);
                            }
                            else
                            {
                                if (show)
                                {
                                    spacingVisibleBitmap.SetPixel(i, j, white);
                                }

                                bitmap.SetPixel(i, j, white);
                            }
                        }

                        if (emptyPixelRow < CustomThreshold && show)
                        {
                            if (!containsPixel)
                            {
                                for (int j = 0; j < bitmap.Height; j++)
                                {
                                    spacingVisibleBitmap.SetPixel(i, j, _spacingColor);
                                }

                                emptyPixelRow++;
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
                            _ocrResultImageBlackWhite.Size = new Point(bitmap.Size.Width, bitmap.Size.Height);
                            _ocrResultImageBlackWhite.Texture = s.CreateTexture2D();
                        }
                    }
                }

                string? plainText = _ocrApi.GetTextFromImage(bitmap);

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

                _closestMatch.Text = GetBestMatch(finalText).Item1;
                _result.Text = finalText;
            }

            return finalText;
        }

        private static (string, int, int, int, bool) GetBestMatch(string name)
        {
            var distances = new List<(string, int, int, int, bool)>();

            foreach (Character_Model c in Characters.ModuleInstance.CharacterModels)
            {
                int distance = name.LevenshteinDistance(c.Name);
                distances.Add((c.Name, distance, 0, 0, true));
            }

            distances.Sort((a, b) => a.Item2.CompareTo(b.Item2));
            (string, int, int, int, bool)? bestMatch = distances?.FirstOrDefault();

            return ((string, int, int, int, bool))(bestMatch != null ? bestMatch : new(string.Empty, 0, 0, 0, false));
        }
#nullable disable

        private async void Container_LeftMouseButtonReleased(object sender, MouseEventArgs e)
        {
            if (!_container.MouseOver)
            {
                await DelayedRead();
            }
        }

        private async Task DelayedRead()
        {
            await Task.Delay(5);
            _ = Read(true);
        }

        private void Container_Changed(object sender, EventArgs e)
        {
            string key = _settings.OCRKey;
            Dictionary<string, Rectangle> regions = _settings.OCRRegions.Value;

            _ = Read(true);

            if (!regions.ContainsKey(key))
            {
                regions.Add(key, _container.LocalBounds);
            }
            else
            {
                regions[key] = _container.LocalBounds;
            }

            _contentContainer.Location = new Point(_container.Left, _container.Top - _contentContainer.Height - 5);
        }
    }
}
