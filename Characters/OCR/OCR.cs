using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Characters.Res;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Characters.Controls;
using Kenedia.Modules.Characters.Extensions;
using Kenedia.Modules.Characters.Models;
using Microsoft.Xna.Framework;
using Patagames.Ocr;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters
{
    public class OCR : IDisposable
    {
        private readonly BasicFrameContainer _contentContainer;
        private readonly System.Drawing.Color _spacingColor = System.Drawing.Color.FromArgb(255, 200, 200, 200);
        private readonly System.Drawing.Color _ignoredColor = System.Drawing.Color.FromArgb(255, 100, 100, 100);
        private readonly TextBox _leftBox;
        private readonly TextBox _topBox;
        private readonly TextBox _rightBox;
        private readonly TextBox _bottomBox;
        private readonly TextBox _columnBox;
        private readonly TextBox _thresholdBox;
        private readonly Checkbox _windowedCheckBox;
        private readonly Label _instructions;
        private readonly Label _result;
        private readonly Image _ocrResultImage;
        private readonly Image _ocrResultImageBlackWhite;
        private readonly ImageButton _closeButton;
        private readonly SizeablePanel _container;
        private readonly OcrApi _ocrApi;
        private readonly OCR_TrainDisplay _ocr_TrainDisplay;
        private bool _disposed = false;
        private readonly bool _initialized = false;

        public OCR()
        {
            OcrApi.PathToEngine = BasePath + @"\tesseract.dll";
            
            _ocrApi = OcrApi.Create();
            _ocrApi.Init(BasePath + @"\", "gw2");
            //_ocrApi.SetVariable("tessedit_char_whitelist", "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZzÁáÂâÄäÀàÆæÇçÊêÉéËëÈèÏïÍíÎîÑñŒœÔôÖöÓóÚúÜüÛûÙù");
            Services.TextureManager tM = Characters.ModuleInstance.TextureManager;
            
            _contentContainer = new BasicFrameContainer()
            {
                Parent = GameService.Graphics.SpriteScreen,
                FrameColor = Color.Black, // new Color(32, 32 , 32),
                Background = AsyncTexture2D.FromAssetId(156003),
                TextureRectangle = new Rectangle(50, 50, 500, 500),
                Height = 260,
                Width = 530,
                ZIndex = 999,
                Visible = false,
            };

            FlowPanel contentFlowPanel = new()
            {
                Parent = _contentContainer,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                AutoSizePadding = new Point(3, 3),
                OuterControlPadding = new(3, 3),
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
            };

            var headerPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                Parent = contentFlowPanel,
                Width = 525,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(5, 5),
            };

            _instructions = new Label()
            {
                Text = strings.OCR_Instructions,
                Parent = headerPanel,
                AutoSizeHeight = true,
                Width = 495,
                WrapText = true,
                TextColor = ContentService.Colors.ColonialWhite,
            };

            _closeButton = new()
            {
                Parent = headerPanel,
                Texture = AsyncTexture2D.FromAssetId(156012),
                HoveredTexture = AsyncTexture2D.FromAssetId(156011),
                Size = new Point(25, 25),
                TextureRectangle = new Rectangle(7, 7, 20, 20),
            };
            _closeButton.Click += _closeButton_Click;

            FlowPanel offsetPanel = new()
            {
                Parent = contentFlowPanel,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                AutoSizePadding = new Point(5, 5),
                OuterControlPadding = new Vector2(0, 5),
                ControlPadding = new Vector2(5, 5),
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
            };

            Label offsetLabel = new()
            {
                Parent = offsetPanel,
                Text = strings.Offset,
                Height = 25,
                AutoSizeWidth = true,
                TextColor = ContentService.Colors.ColonialWhite,
                BasicTooltipText = strings.Offset_Tooltip,
            };

            _leftBox = new()
            {
                Parent = offsetPanel,
                Size = new Point(50, 25),
                Text = CustomOffset.Left.ToString(),
                BasicTooltipText = strings.LeftOffset,
            };
            _leftBox.TextChanged += OffsetChanged;

            _topBox = new()
            {
                Parent = offsetPanel,
                Size = new Point(50, 25),
                Text = CustomOffset.Top.ToString(),
                BasicTooltipText = strings.TopOffset,
            };
            _topBox.TextChanged += OffsetChanged;

            _rightBox = new()
            {
                Parent = offsetPanel,
                Size = new Point(50, 25),
                Text = CustomOffset.Right.ToString(),
                BasicTooltipText = strings.RightOffset,
            };
            _rightBox.TextChanged += OffsetChanged;

            _bottomBox = new()
            {
                Parent = offsetPanel,
                Size = new Point(50, 25),
                Text = CustomOffset.Bottom.ToString(),
                BasicTooltipText = strings.BottomOffset,
            };
            _bottomBox.TextChanged += OffsetChanged;

            //_windowedCheckBox = new()
            //{
            //    Parent = offsetPanel,
            //    Text = strings.WindowedMode,
            //    BasicTooltipText = strings.WindowedMode_Tooltip,
            //    Checked = Characters.ModuleInstance.Settings.WindowedMode.Value,
            //    Height = 25,
            //    Width = 200,
            //};
            //_windowedCheckBox.Click += WindowedCheckBox_Click;
            //Characters.ModuleInstance.Settings.WindowedMode.SettingChanged += (s, e) => _windowedCheckBox.Checked = Characters.ModuleInstance.Settings.WindowedMode.Value;

            _result = new Label()
            {
                Parent = contentFlowPanel,
                AutoSizeHeight = false,
                Height = 50,
                AutoSizeWidth = true,
                TextColor = ContentService.Colors.ColonialWhite,
                Font = GameService.Content.DefaultFont32,
            };

            var resultImagePanel = new Panel()
            {
                Parent = contentFlowPanel,
                Height = 50,
                WidthSizingMode = SizingMode.Fill,
            };

            _ocrResultImage = new Image()
            {
                Parent = resultImagePanel,
            };

            var blackWhiteResultImagePanel = new Panel()
            {
                Parent = contentFlowPanel,
                Height = 50,
                WidthSizingMode = SizingMode.Fill,
            };

            _ocrResultImageBlackWhite = new Image()
            {
                Parent = blackWhiteResultImagePanel,
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
                Text = strings.EmptyColumns,
                Height = 25,
                AutoSizeWidth = true,
                TextColor = ContentService.Colors.ColonialWhite,
                BasicTooltipText = strings.EmptyColumns_Tooltip,
            };
            _columnBox = new TextBox()
            {
                Parent = thresholdPanel,
                Size = new Point(50, 25),
                Text = CustomThreshold.ToString(),
                BasicTooltipText = strings.EmptyColumnsThreshold_Tooltip,
            };
            _columnBox.TextChanged += ColumnThresholdChanged;

            _ = new Panel()
            {
                Parent = thresholdPanel,
                BackgroundColor = new Color(_spacingColor.R, _spacingColor.G, _spacingColor.B, _spacingColor.A),
                Size = new Point(25, 25),
            };
            _ = new Label()
            {
                Parent = thresholdPanel,
                Text = strings.EmptyColumn,
                Height = 25,
                AutoSizeWidth = true,
                TextColor = ContentService.Colors.ColonialWhite,
                BasicTooltipText = strings.EmptyColumn_Tooltip,
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
                Text = strings.IgnoredPart,
                Height = 25,
                AutoSizeWidth = true,
                TextColor = ContentService.Colors.ColonialWhite,
                BasicTooltipText = strings.IgnoredPart_Tooltip,
            };

            _thresholdBox = new TextBox()
            {
                Parent = thresholdPanel,
                Text = $"{Characters.ModuleInstance.Settings.OCR_ColorThreshold.Value}",
                Height = 25,
                Width = 50,
                BasicTooltipText = "Threshold of 'white' a pixel has to be to be converted to black to be read (RGB Value: 0 - 255)",
            };
            _thresholdBox.TextChanged += ThresholdBox_TextChanged;

            _container = new SizeablePanel()
            {
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = 999,
                Visible = false,
                Location = Characters.ModuleInstance.Settings.ActiveOCRRegion.Location,
                Size = Characters.ModuleInstance.Settings.ActiveOCRRegion.Size,
                TintOnHover = false,
                ShowResizeOnlyOnMouseOver = true,
                MaxSize = new(530, 50)
            };
            _container.Resized += Container_Changed;
            _container.Moved += Container_Changed;
            _container.LeftMouseButtonReleased += Container_LeftMouseButtonReleased;
            _container.MouseLeft += Container_LeftMouseButtonReleased;

            int height = Characters.ModuleInstance.Settings.ActiveOCRRegion.Size.Y;
            _contentContainer.Location = new Point(_container.Left, _container.Top - _contentContainer.Height - 5);
            _initialized = true;
        }

        private void ThresholdBox_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(_thresholdBox.Text, out int threshold))
            {
                Characters.ModuleInstance.Settings.OCR_ColorThreshold.Value = threshold;
            }
            else
            {
                _thresholdBox.Text = $"{Characters.ModuleInstance.Settings.OCR_ColorThreshold.Value}";
            }
        }

        private void _closeButton_Click(object sender, MouseEventArgs e)
        {
            ToggleContainer();
        }

        private RectangleOffset CustomOffset
        {
            get => Characters.ModuleInstance.Settings.OCRCustomOffset.Value;
            set => Characters.ModuleInstance.Settings.OCRCustomOffset.Value = value;
        }

        private int CustomThreshold
        {
            get => Characters.ModuleInstance.Settings.OCRNoPixelColumns.Value;
            set => Characters.ModuleInstance.Settings.OCRNoPixelColumns.Value = value;
        }

        private string BasePath => Characters.ModuleInstance.BasePath;

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
                _ocr_TrainDisplay?.Dispose();
                _disposed = true;
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

            bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
            Utility.WindowsUtil.WindowsUtil.RECT wndBounds = Characters.ModuleInstance.WindowRectangle;
            int titleBarHeight = !windowed ? 0 : Characters.ModuleInstance.TitleBarHeight;
            int sideBarWidth = !windowed ? 0 : Characters.ModuleInstance.SideBarWidth;

            double factor = GameService.Graphics.UIScaleMultiplier;
            Point size = new(Math.Min((int)((_container.Width + 5) * factor), 499), Math.Min((int)((_container.Height + 5) * factor), 499));

            using (System.Drawing.Bitmap bitmap = new(size.X, size.Y))
            {
                System.Drawing.Bitmap spacingVisibleBitmap = new(size.X, size.Y);

                using (var g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    int left = (int)(wndBounds.Left + sideBarWidth);
                    int top = (int)(wndBounds.Top + titleBarHeight);

                    int x = (int)Math.Ceiling((_container.Left - 10 + CustomOffset.Left) * factor);
                    int y = (int)Math.Ceiling((_container.Top - 10 + CustomOffset.Top) * factor);

                    g.CopyFromScreen(new System.Drawing.Point(left + x, top + y), System.Drawing.Point.Empty, new System.Drawing.Size(size.X - CustomOffset.Right, size.Y - CustomOffset.Bottom));

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
                            int threshold = Characters.ModuleInstance.Settings.OCR_ColorThreshold.Value;

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

                _result.Text = finalText;
            }

            return finalText;
        }
#nullable disable

        private void ColumnThresholdChanged(object sender, EventArgs e)
        {
            if (int.TryParse(_columnBox.Text, out int threshold))
            {
                CustomThreshold = threshold;
            }
        }

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

        private void OffsetChanged(object sender, EventArgs e)
        {
            if (_initialized)
            {
                int left = int.TryParse(_leftBox.Text, out int leftParse) ? leftParse : CustomOffset.Left;
                int top = int.TryParse(_topBox.Text, out int topParse) ? topParse : CustomOffset.Top;
                int right = int.TryParse(_rightBox.Text, out int rightParse) ? rightParse : CustomOffset.Right;
                int bottom = int.TryParse(_bottomBox.Text, out int bottomParse) ? bottomParse : CustomOffset.Bottom;

                CustomOffset = new RectangleOffset(left, top, right, bottom);
                if (_container.Visible)
                {
                    _ = Read(true);
                }
            }
        }

        private void Container_Changed(object sender, EventArgs e)
        {
            string key = Characters.ModuleInstance.Settings.OCRKey;
            System.Collections.Generic.Dictionary<string, Rectangle> regions = Characters.ModuleInstance.Settings.OCRRegions.Value;

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

        private void WindowedCheckBox_Click(object sender, MouseEventArgs e)
        {
            //Characters.ModuleInstance.Settings.WindowedMode.Value = _windowedCheckBox.Checked;
        }
    }
}
