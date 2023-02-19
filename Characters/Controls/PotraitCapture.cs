using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Characters.Res;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static Kenedia.Modules.Characters.Services.TextureManager;
using Color = Microsoft.Xna.Framework.Color;
using Label = Kenedia.Modules.Core.Controls.Label;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class PotraitCapture : Container
    {
        private readonly List<FramedMaskedRegion> _characterPotraitFrames = new();
        private readonly ClientWindowService _clientWindowService;
        private readonly SharedSettings _sharedSettings;

        private readonly ImageButton _captureButton;
        private readonly ImageButton _addButton;
        private readonly ImageButton _removeButton;
        private readonly Dummy _characterPotraitsBackground;
        private readonly Label _disclaimer;
        private readonly FramedContainer _disclaimerBackground;
        private readonly ImageButton _dragButton;
        private readonly NumberBox _sizeBox;
        private readonly NumberBox _gapBox;

        private bool _dragging;
        private Point _draggingStart;

        private int _characterPotraitSize = 130;
        private int _gap = 13;

        public PotraitCapture(ClientWindowService clientWindowService, SharedSettings sharedSettings, TextureManager tM)
        {
            _clientWindowService = clientWindowService;
            _sharedSettings = sharedSettings;

            Point res = GameService.Graphics.Resolution;
            Size = new Point(100, 100);
            WidthSizingMode = SizingMode.AutoSize;
            HeightSizingMode = SizingMode.AutoSize;

            Location = new Point((res.X - Size.X) / 2, res.Y - 125 - Size.Y);

            _dragButton = new ImageButton()
            {
                Parent = this,
                Texture = tM.GetControlTexture(ControlTextures.Drag_Button),
                HoveredTexture = tM.GetControlTexture(ControlTextures.Drag_Button_Hovered),
                Size = new Point(32, 32),
                Location = new Point((32 + 5) * 0, 0),
                SetLocalizedTooltip = () => strings.DragOverCharacter_Instructions,
            };
            _dragButton.LeftMouseButtonPressed += DragButton_LeftMouseButtonPressed;
            _dragButton.LeftMouseButtonReleased += DragButton_LeftMouseButtonReleased;

            _captureButton = new ImageButton()
            {
                Parent = this,
                Texture = tM.GetControlTexture(ControlTextures.Potrait_Button),
                HoveredTexture = tM.GetControlTexture(ControlTextures.Potrait_Button_Hovered),
                Size = new Point(32, 32),
                Location = new Point(_dragButton.Right + 5, 0),
                SetLocalizedTooltip = () => strings.CapturePotraits,
                ClickAction = (m) => CapturePotraits(),
            };

            _disclaimerBackground = new FramedContainer()
            {
                Parent = this,
                Location = new Point(_captureButton.Right + 5, 0),
                BorderColor = Color.Black, // new Color(32, 32 , 32),
                BackgroundImage = AsyncTexture2D.FromAssetId(156003),
                TextureRectangle = new Rectangle(50, 50, 500, 500),
                WidthSizingMode = SizingMode.AutoSize,
                AutoSizePadding = new Point(15, 0),
                Height = 32,
            };

            _sizeBox = new NumberBox()
            {
                Parent = _disclaimerBackground,
                Location = new Point(5, (_disclaimerBackground.Height - 25) / 2),
                Size = new Point(100, 25),
                Value = _characterPotraitSize,
                SetLocalizedTooltip = () => strings.PotraitSize,
                ValueChangedAction = (num) =>
                {
                    _characterPotraitSize = num;
                    RepositionPotraitFrames();
                },
            };

            _gapBox = new NumberBox()
            {
                Parent = _disclaimerBackground,
                Location = new Point(_sizeBox.Right + 5, (_disclaimerBackground.Height - 25) / 2),
                Size = new Point(100, 25),
                Value = _gap,
                SetLocalizedTooltip = () => strings.PotraitGap,
                ValueChangedAction = (value) =>
                {
                    _gap = value;
                    RepositionPotraitFrames();
                },
            };

            _disclaimer = new Label()
            {
                Parent = _disclaimerBackground,
                Location = new Point(_gapBox.Right + 5, 0),
                TextColor = ContentService.Colors.ColonialWhite,
                AutoSizeWidth = true,
                Height = 32,
                Font = GameService.Content.DefaultFont16,
                SetLocalizedText = () => strings.BestResultLargerDisclaimer,
                Padding = new Thickness(0f, 0f),
            };

            _addButton = new ImageButton()
            {
                Parent = this,
                Texture = tM.GetControlTexture(ControlTextures.Plus_Button),
                HoveredTexture = tM.GetControlTexture(ControlTextures.Plus_Button_Hovered),
                Size = new Point(32, 32),
                Location = new Point(0, 35),
                SetLocalizedTooltip = () => string.Format(strings.AddItem, strings.PotraitFrame),
                ClickAction = (m) => AddPotrait(),
            };

            _removeButton = new ImageButton()
            {
                Parent = this,
                Texture = tM.GetControlTexture(ControlTextures.Minus_Button),
                HoveredTexture = tM.GetControlTexture(ControlTextures.Minus_Button_Hovered),
                Size = new Point(32, 32),
                Location = new Point(0, 70),
                SetLocalizedTooltip = () => string.Format(strings.RemoveItem, strings.PotraitFrame),
                ClickAction = (m) => RemovePortrait(),
            };

            _characterPotraitsBackground = new Dummy()
            {
                BackgroundColor = Color.Black * 0.8f,
                Parent = Graphics.SpriteScreen,
                ZIndex = int.MaxValue - 1,
            };

            AddPotrait();
            AddPotrait();
        }

        public Action OnImageCaptured { get; set; }

        public Func<string> AccountImagePath { get; set; }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            _dragging = _dragging && MouseOver;

            if (_dragging)
            {
                Location = Input.Mouse.Position.Add(new Point(-_draggingStart.X, -_draggingStart.Y));
            }

            ForceOnScreen();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            _captureButton?.Dispose();
            _addButton?.Dispose();
            _removeButton?.Dispose();
            _disclaimer?.Dispose();
            _disclaimerBackground?.Dispose();
            _dragButton?.Dispose();
            _sizeBox?.Dispose();
            _gapBox?.Dispose();
            _characterPotraitsBackground?.Dispose();

            foreach (var frame in _characterPotraitFrames)
            {
                frame.Dispose();
            }
        }

        private void RemovePortrait()
        {
            if (_characterPotraitFrames.Count > 1)
            {
                var frame = _characterPotraitFrames.Last();
                frame.Dispose();
                _ = _characterPotraitFrames.Remove(frame);
                RepositionPotraitFrames();
            }
        }

        private void AddPotrait()
        {
            _characterPotraitFrames.Add(new FramedMaskedRegion()
            {
                Parent = Graphics.SpriteScreen,
                ZIndex = int.MaxValue,
                Visible = Visible,
            });

            RepositionPotraitFrames();
        }

        private void RepositionPotraitFrames()
        {
            int index = 0;
            Point pos = new(_captureButton.AbsoluteBounds.X + 5, _captureButton.AbsoluteBounds.Y + 40);
            
            _characterPotraitsBackground.Location = pos.Add(new(-5, -5));

            foreach (var frame in _characterPotraitFrames)
            {
                frame.Width = _characterPotraitSize;
                frame.Height = _characterPotraitSize;
                frame.Location = pos;

                pos.X += _characterPotraitSize + _gap;
                index++;
            }

            _characterPotraitsBackground.Width = pos.X - _characterPotraitsBackground.Location.X - _gap + 5;
            _characterPotraitsBackground.Height = _characterPotraitSize + 10;
        }

        private void DragButton_LeftMouseButtonReleased(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        private void DragButton_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            _dragging = true;
            _draggingStart = _dragging ? RelativeMousePosition : Point.Zero;
        }

        private void CapturePotraits()
        {
            string path = AccountImagePath?.Invoke();

            if (!string.IsNullOrEmpty(path))
            {
                string GetImagePath(List<string> imagePaths)
                {
                    for (int i = 1; i < int.MaxValue; i++)
                    {
                        string imagePath = $"{path}Image {string.Format("{0:00}", i)}.png";

                        if (!imagePaths.Contains(imagePath))
                        {
                            imagePaths.Add(imagePath);
                            return imagePath;
                        }
                    }

                    return $"{path}Last Image.png";
                }

                Regex regex = new("Image.*[0-9].png");
                var images = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories).Where(path => regex.IsMatch(path)).ToList();

                var wndBounds = _clientWindowService.WindowBounds;

                bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
                Point p = windowed ? new(_sharedSettings.WindowOffset.Left, _sharedSettings.WindowOffset.Top) : Point.Zero;

                double factor = GameService.Graphics.UIScaleMultiplier;

                var size = new Size(_characterPotraitSize, _characterPotraitSize);

                foreach (FramedMaskedRegion c in _characterPotraitFrames)
                {
                    Rectangle bounds = c.MaskedRegion;
                    using Bitmap bitmap = new((int)(bounds.Width * factor), (int)(bounds.Height * factor));

                    using (var g = System.Drawing.Graphics.FromImage(bitmap))
                    {
                        int x = (int)(bounds.X * factor);
                        int y = (int)(bounds.Y * factor);

                        g.CopyFromScreen(new System.Drawing.Point(wndBounds.Left + p.X + x, wndBounds.Top + p.Y + y), System.Drawing.Point.Empty, size);
                    }

                    bitmap.Save(GetImagePath(images), System.Drawing.Imaging.ImageFormat.Png);
                }

                OnImageCaptured?.Invoke();
                ScreenNotification.ShowNotification(string.Format("[Characters]: " + strings.CapturedXPotraits, _characterPotraitFrames.Count));
            }
        }

        protected override void OnMoved(MovedEventArgs e)
        {
            base.OnMoved(e);
            if (_characterPotraitFrames.Count > 0) RepositionPotraitFrames();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _characterPotraitsBackground.Show();

            foreach (var frame in _characterPotraitFrames)
            {
                frame.Show();
            }

            ForceOnScreen();
        }

        protected override void OnHidden(EventArgs e)
        {
            base.OnHidden(e);

            _characterPotraitsBackground.Hide();
            foreach (var frame in _characterPotraitFrames)
            {
                frame.Hide();
            }
        }

        private void ForceOnScreen()
        {
            var screen = Graphics.SpriteScreen;
            if (Bottom > screen.Bottom)
            {
                Bottom = screen.Bottom;
            }

            if (Top < screen.Top + Height)
            {
                Top = screen.Top + Height;
            }

            if (Left < screen.Left)
            {
                Left = screen.Left;
            }

            if (Right > screen.Right)
            {
                Left = screen.Right - Width;
            }
        }
    }
}
