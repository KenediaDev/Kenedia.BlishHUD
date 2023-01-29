using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Characters.Res;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Kenedia.Modules.Characters.Services.TextureManager;
using static Kenedia.Modules.Characters.Utility.WindowsUtil.WindowsUtil;
using Color = Microsoft.Xna.Framework.Color;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using Label = Kenedia.Modules.Core.Controls.Label;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class CharacterPotraitCapture : Container
    {
        private readonly ImageButton _captureButton;
        private readonly ImageButton _addButton;
        private readonly ImageButton _removeButton;
        private readonly Label _disclaimer;
        private readonly FramedContainer _disclaimerBackground;
        private readonly ImageButton _dragButton;
        private readonly NumberBox _sizeBox;
        private readonly NumberBox _gapBox;
        private readonly FlowPanel _portraitsPanel;

        private bool _dragging;
        private Point _draggingStart;

        private int _characterPotraitSize = 130;
        private int _gap = 13;

        public CharacterPotraitCapture()
        {
            Point res = GameService.Graphics.Resolution;
            Size = new Point(100, 100);
            WidthSizingMode = SizingMode.AutoSize;
            HeightSizingMode = SizingMode.AutoSize;

            Location = new Point((res.X - Size.X) / 2, res.Y - 125 - Size.Y);
            Services.TextureManager tM = Characters.ModuleInstance.TextureManager;

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
                ClickAction = async (m) => await CapturePotraits(),
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
                    Point p = new(num, num);
                    foreach (Control c in _portraitsPanel.Children)
                    {
                        c.Size = p;
                    }
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
                    _portraitsPanel.ControlPadding = new Vector2(value, 0);
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
                ClickAction = (m) =>
                {
                    if (_portraitsPanel.Children.Count > 1)
                    {
                        _portraitsPanel.Children.RemoveAt(_portraitsPanel.Children.Count - 1);
                    }
                }
            };

            _portraitsPanel = new FlowPanel()
            {
                Parent = this,
                Location = new Point(_captureButton.Left, 35),
                Size = new Point(110, 110),
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(_gap, 0),
            };

            AddPotrait();
            AddPotrait();
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            _dragging = _dragging && MouseOver;

            if (_dragging)
            {
                Location = Input.Mouse.Position.Add(new Point(-_draggingStart.X, -_draggingStart.Y));
            }
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
            _portraitsPanel?.Dispose();
        }

        private void RemoveButton_Click(object sender, MouseEventArgs e)
        {
            if (_portraitsPanel.Children.Count > 1)
            {
                _portraitsPanel.Children.RemoveAt(_portraitsPanel.Children.Count - 1);
            }
        }

        private void AddButton_Click(object sender, MouseEventArgs e)
        {
            AddPotrait();
        }

        private void AddPotrait()
        {
            _ = new FramedContainer()
            {
                Parent = _portraitsPanel,
                Size = new Point(_characterPotraitSize, _characterPotraitSize),
                BorderColor = ContentService.Colors.ColonialWhite,
            };
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

        private async Task CapturePotraits()
        {            
            string path = Characters.ModuleInstance.AccountImagesPath;

            string GetImagePath(List<string> imagePaths)
            {
                for (int i = 0; i < int.MaxValue; i++)
                {
                    string imagePath = $"{path}Image {i}.png";

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

            RECT wndBounds = Characters.ModuleInstance.WindowRectangle;

            bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
            Point p = windowed ? new(Characters.ModuleInstance.Settings.WindowOffset.Value.Left, Characters.ModuleInstance.Settings.WindowOffset.Value.Top) : Point.Zero;

            double factor = GameService.Graphics.UIScaleMultiplier;

            foreach (var c in _portraitsPanel.Children.Cast<FramedContainer>())
            {
                c.Hide();
            }

            await Task.Delay(1);

            var size = new Size(_characterPotraitSize, _characterPotraitSize);

            foreach (var c in _portraitsPanel.Children.Cast<FramedContainer>())
            {
                var bounds = c.AbsoluteBounds;
                using Bitmap bitmap = new((int)((_characterPotraitSize - 2) * factor), (int)((_characterPotraitSize - 2) * factor));

                using (var g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    int x = (int)(bounds.X * factor);
                    int y = (int)(bounds.Y * factor);

                    g.CopyFromScreen(new System.Drawing.Point(wndBounds.Left + p.X + x, wndBounds.Top + p.Y + y), System.Drawing.Point.Empty, size);
                }

                bitmap.Save(GetImagePath(images), System.Drawing.Imaging.ImageFormat.Png);

                c.Show();
            }

            Characters.ModuleInstance.MainWindow.CharacterEdit.LoadImages(null, null);
        }
    }
}
