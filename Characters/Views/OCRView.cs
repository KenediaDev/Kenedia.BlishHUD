using Blish_HUD.Content;
using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Res;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Controls;
using System;
using System.Collections.Generic;
using Color = Microsoft.Xna.Framework.Color;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using ImageButton = Kenedia.Modules.Core.Controls.ImageButton;
using Label = Kenedia.Modules.Core.Controls.Label;
using Panel = Kenedia.Modules.Core.Controls.Panel;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Kenedia.Modules.Characters;
using Kenedia.Modules.Core.Extensions;
using Image = Kenedia.Modules.Core.Controls.Image;

namespace Characters.Views
{
    public class OCRView : FramedContainer
    {
        private readonly OCR _ocr;
        private readonly Settings _settings;

        private readonly System.Drawing.Color _spacingColor = System.Drawing.Color.FromArgb(255, 200, 200, 200);
        private readonly System.Drawing.Color _ignoredColor = System.Drawing.Color.FromArgb(255, 100, 100, 100);
        private readonly NumberBox _columnBox;
        private readonly NumberBox _thresholdBox;
        private readonly Label _instructions;
        private readonly Label _bestMatchLabel;
        private readonly Label _resultLabel;
        private readonly Image _sourceImage;
        private readonly Image _cleanedImage;
        private readonly Image _scaledImage;
        private readonly ImageButton _closeButton;
        private readonly ResizeableContainer _ocrRegionContainer;
        private readonly MaskedRegion _maskedRegion;

        private bool _sizeSet = false;
        private double _readTick;

        public OCRView(Settings settings, OCR ocr)
        {
            _settings = settings;
            _ocr = ocr;

            BorderColor = Color.Black;
            BackgroundImage = AsyncTexture2D.FromAssetId(156003);
            TextureRectangle = new Rectangle(50, 50, 500, 500);
            Height = 350;
            Width = 620;

            FlowPanel contentFlowPanel = new()
            {
                Parent = this,
                Width = Width,
                HeightSizingMode = SizingMode.AutoSize,
                AutoSizePadding = new Point(3, 3),
                OuterControlPadding = new(3, 3),
                ControlPadding = new(3, 3),
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                BorderColor = Color.Black,
                BorderWidth = new(2),
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
                Width = contentFlowPanel.ContentRegion.Width - 35,
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
                BorderWidth = new(2),
            };
            _bestMatchLabel = new Label()
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
                BorderWidth = new(2),
            };
            _resultLabel = new Label()
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
                BorderWidth = new(2),
            };

            _scaledImage = new Image()
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
                SetLocalizedText = () => "Scaled",
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
                BorderWidth = new(2),
            };

            _cleanedImage = new Image()
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
                BorderWidth = new(2),
            };

            _sourceImage = new Image()
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
                //AutoSizePadding = new Point(5, 5),
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

            _maskedRegion = new MaskedRegion()
            {
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = int.MaxValue,
                Visible = false,
            };

            _ocrRegionContainer = new ResizeableContainer()
            {
                Parent = GameService.Graphics.SpriteScreen,
                Visible = false,
                Location = _settings.ActiveOCRRegion.Location,
                Size = _settings.ActiveOCRRegion.Size,
                BorderColor = ContentService.Colors.ColonialWhite,
                ShowResizeOnlyOnMouseOver = true,
                MaxSize = new(Width, 100),
                BorderWidth = new(2),
                ZIndex = int.MaxValue - 1,
            };
            _ocrRegionContainer.Resized += Container_Changed;
            _ocrRegionContainer.Moved += Container_Changed;

            int height = _settings.ActiveOCRRegion.Size.Y;
            Location = new Point(_ocrRegionContainer.Left, _ocrRegionContainer.Top - Height - 5);

            ForceOnScreen();
        }

        public void EnableMaskedRegion()
        {
            var b = _settings.ActiveOCRRegion;

            _maskedRegion.Size = b.Size;
            _maskedRegion.Location = b.Location;
            _maskedRegion?.Show();
        }

        public void DisableMaskedRegion()
        {
            _maskedRegion?.Hide();
        }

        private void Container_Changed(object sender, EventArgs e)
        {
            if (!_sizeSet)
            {
                string key = _settings.OCRKey;
                Dictionary<string, Rectangle> regions = _settings.OCRRegions.Value;
                var bounds = new Rectangle(_ocrRegionContainer.Left + _ocrRegionContainer.BorderWidth.Left, _ocrRegionContainer.Top + _ocrRegionContainer.BorderWidth.Top, _ocrRegionContainer.Width - _ocrRegionContainer.BorderWidth.Horizontal, _ocrRegionContainer.Height - _ocrRegionContainer.BorderWidth.Vertical);

                if (!regions.ContainsKey(key))
                {
                    regions.Add(key, bounds);
                }
                else
                {
                    regions[key] = bounds;
                }
            }

            _sizeSet = false;
            Location = new Point(_ocrRegionContainer.Left, _ocrRegionContainer.Top - Height - 5);

            var b = _settings.ActiveOCRRegion;
            _maskedRegion.Size = b.Size;
            _maskedRegion.Location = b.Location;
        }

        private void CloseButton_Click(object sender, MouseEventArgs e)
        {
            ToggleContainer();
        }

        public void ToggleContainer()
        {
            bool visible = this.ToggleVisibility();
            ForceOnScreen();

            _ = (_ocrRegionContainer?.ToggleVisibility(visible));
            _ = (_maskedRegion?.ToggleVisibility(visible));

            if (_ocrRegionContainer.Visible)
            {
                _sizeSet = true;

                _ocrRegionContainer.Location = _settings.ActiveOCRRegion.Location.Add(new(-_ocrRegionContainer.BorderWidth.Left, -_ocrRegionContainer.BorderWidth.Top));
                _ocrRegionContainer.Size = _settings.ActiveOCRRegion.Size.Add(new(_ocrRegionContainer.BorderWidth.Horizontal, _ocrRegionContainer.BorderWidth.Vertical));
            }
        }

        public override async void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);
            if (Visible)
            {
                ForceOnScreen();

                _maskedRegion.Visible = !_ocrRegionContainer.AbsoluteBounds.Contains(Input.Mouse.Position);

                if (gameTime.TotalGameTime.TotalMilliseconds - _readTick > 250 && _maskedRegion.Visible)
                {
                    _readTick = gameTime.TotalGameTime.TotalMilliseconds;

                    string result = await _ocr.Read(true);

                    if (result != null)
                    {
                        _sourceImage.Texture = _ocr.SourceTexture;
                        _sourceImage.Size = _ocr.SourceTexture.Bounds.Size;
                        _cleanedImage.Texture = _ocr.CleanedTexture;
                        _cleanedImage.Size = _ocr.CleanedTexture.Bounds.Size;
                        _scaledImage.Texture = _ocr.ScaledTexture;
                        _scaledImage.Size = _ocr.ScaledTexture.Bounds.Size;

                        _resultLabel.Font = Content.DefaultFont32;
                        _resultLabel.WrapText = false;

                        _bestMatchLabel.Text = _ocr.BestMatchResult;
                        _resultLabel.Text = _ocr.ReadResult;
                    }
                    else if(!_ocr.IsLoaded)
                    {
                        _bestMatchLabel.Text = !string.IsNullOrEmpty(result) ? _ocr.BestMatchResult: $"Tesseract Engine Loaded: {_ocr.IsLoaded}";
                        _resultLabel.Text = !string.IsNullOrEmpty(result) ? _ocr.ReadResult : $"{_ocr.PathToEngine}";
                        _resultLabel.Font = Content.DefaultFont14;
                        _resultLabel.WrapText = true;
                    }
                }
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            _columnBox?.Dispose();
            _thresholdBox?.Dispose();
            _ocrRegionContainer?.Dispose();
            _instructions?.Dispose();
            _sourceImage?.Dispose();
            _cleanedImage?.Dispose();
            _scaledImage?.Dispose();
            _resultLabel?.Dispose();
            _bestMatchLabel?.Dispose();
            _maskedRegion?.Dispose();
        }

        private void ForceOnScreen()
        {
            var screen = Graphics.SpriteScreen;
            if (_ocrRegionContainer.Bottom > screen.Bottom)
            {
                _ocrRegionContainer.Bottom = screen.Bottom;
            }

            if (_ocrRegionContainer.Top < screen.Top + Height)
            {
                _ocrRegionContainer.Top = screen.Top + Height;
            }

            if (_ocrRegionContainer.Left < screen.Left)
            {
                _ocrRegionContainer.Left = screen.Left;
            }

            if (Right > screen.Right)
            {
                _ocrRegionContainer.Left = screen.Right - Width;
            }
        }
    }
}
