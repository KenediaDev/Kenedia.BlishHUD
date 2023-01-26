using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Characters.Res;
using Kenedia.Modules.Characters.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static Kenedia.Modules.Characters.Services.TextureManager;
using static Kenedia.Modules.Characters.Utility.WindowsUtil.WindowsUtil;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class CharacterPotraitCapture : Container
    {
        private readonly Checkbox _windowedCheckbox;
        private readonly ImageButton _captureButton;
        private readonly ImageButton _addButton;
        private readonly ImageButton _removeButton;
        private readonly Label _disclaimer;
        private readonly BasicFrameContainer _disclaimerBackground;
        private readonly ImageButton _dragButton;
        private readonly TextBox _sizeBox;
        private readonly TextBox _gapBox;
        private readonly FlowPanel _portraitsPanel;

        private bool _dragging;
        private Point _draggingStart;

        private DateTime _capturePotraits;
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
            _addButton = new ImageButton()
            {
                Parent = this,
                Texture = tM.GetControlTexture(ControlTextures.Plus_Button),
                HoveredTexture = tM.GetControlTexture(ControlTextures.Plus_Button_Hovered),
                Size = new Point(32, 32),
                Location = new Point((32 + 5) * 1, 0),
                BasicTooltipText = string.Format(strings.AddItem, strings.PotraitFrame),
            };
            _addButton.Click += AddButton_Click;

            _removeButton = new ImageButton()
            {
                Parent = this,
                Texture = tM.GetControlTexture(ControlTextures.Minus_Button),
                HoveredTexture = tM.GetControlTexture(ControlTextures.Minus_Button_Hovered),
                Size = new Point(32, 32),
                Location = new Point((32 + 5) * 2, 0),
                BasicTooltipText = string.Format(strings.RemoveItem, strings.PotraitFrame),
            };
            _removeButton.Click += RemoveButton_Click;

            _dragButton = new ImageButton()
            {
                Parent = this,
                Texture = tM.GetControlTexture(ControlTextures.Drag_Button),
                HoveredTexture = tM.GetControlTexture(ControlTextures.Drag_Button_Hovered),
                Size = new Point(32, 32),
                Location = new Point((32 + 5) * 0, 0),
                BasicTooltipText = strings.DragOverCharacter_Instructions,
            };
            _dragButton.LeftMouseButtonPressed += DragButton_LeftMouseButtonPressed;
            _dragButton.LeftMouseButtonReleased += DragButton_LeftMouseButtonReleased;

            _captureButton = new ImageButton()
            {
                Parent = this,
                Texture = tM.GetControlTexture(ControlTextures.Potrait_Button),
                HoveredTexture = tM.GetControlTexture(ControlTextures.Potrait_Button_Hovered),
                Size = new Point(32, 32),
                Location = new Point((32 + 5) * 3, 0),
                BasicTooltipText = strings.CapturePotraits,
            };
            _captureButton.Click += CaptureButton_Click;

            _disclaimerBackground = new BasicFrameContainer()
            {
                Parent = this,
                Location = new Point((32 + 5) * 4, 0),
                FrameColor = Color.Black, // new Color(32, 32 , 32),
                Background = AsyncTexture2D.FromAssetId(156003),
                TextureRectangle = new Rectangle(50, 50, 500, 500),
                WidthSizingMode = SizingMode.AutoSize,
                AutoSizePadding = new Point(15, 0),
                Height = 32,
            };

            //_windowedCheckbox = new Checkbox()
            //{
            //    Parent = _disclaimerBackground,
            //    Text = strings.WindowedMode,
            //    BasicTooltipText = strings.WindowedMode_Tooltip,
            //    Checked = Characters.ModuleInstance.Settings.WindowedMode.Value,
            //    Location = new Point(5, 0),
            //    Height = 32,
            //};
            //_windowedCheckbox.CheckedChanged += WindowedCheckbox_CheckedChanged;
            //Characters.ModuleInstance.Settings.WindowedMode.SettingChanged += (s, e) => _windowedCheckbox.Checked = Characters.ModuleInstance.Settings.WindowedMode.Value;

            _disclaimer = new Label()
            {
                Parent = _disclaimerBackground,
                Location = new Point(0, 0),
                TextColor = ContentService.Colors.ColonialWhite,
                AutoSizeWidth = true,
                Height = 32,
                Font = GameService.Content.DefaultFont16,
                Text = strings.BestResultLargerDisclaimer,
                Padding = new Thickness(0f, 0f),
            };
            _disclaimer.Resized += Disclaimer_Resized;

            // _disclaimerBackground.Size = (_disclaimer.Size).Add(new Point(_windowedCheckbox.Width + 15, 0));
            _sizeBox = new TextBox()
            {
                Parent = this,
                Location = new Point(0, 35),
                Size = new Point(50, 25),
                Text = _characterPotraitSize.ToString(),
                BasicTooltipText = strings.PotraitSize,
            };
            _sizeBox.TextChanged += SizeBox_TextChanged;
            _gapBox = new TextBox()
            {
                Parent = this,
                Location = new Point(0, 65),
                Size = new Point(50, 25),
                Text = _gap.ToString(),
                BasicTooltipText = strings.PotraitGap,
            };
            _gapBox.TextChanged += GapBox_TextChanged;

            _portraitsPanel = new FlowPanel()
            {
                Parent = this,
                Location = new Point(55, 35),
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

            if (_capturePotraits > DateTime.MinValue && DateTime.Now.Subtract(_capturePotraits).TotalMilliseconds >= 5)
            {
                foreach (Control c in _portraitsPanel.Children)
                {
                    CapturePotrait(c.AbsoluteBounds);
                }

                foreach (Control c in _portraitsPanel.Children)
                {
                    c.Show();
                }

                _capturePotraits = DateTime.MinValue;
                Characters.ModuleInstance.MainWindow.CharacterEdit.LoadImages(null, null);
            }
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
        }

        //private void WindowedCheckbox_CheckedChanged(object sender, CheckChangedEvent e)
        //{
        //    Characters.ModuleInstance.Settings.WindowedMode.Value = _windowedCheckbox.Checked;
        //}

        private void Disclaimer_Resized(object sender, ResizedEventArgs e)
        {
            _disclaimerBackground.Size = _disclaimer.Size.Add(new Point(10, 0));
        }

        private void CaptureButton_Click(object sender, MouseEventArgs e)
        {
            _capturePotraits = DateTime.Now;

            foreach (Control c in _portraitsPanel.Children)
            {
                c.Hide();
            }
        }

        private void SizeBox_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(_sizeBox.Text, out int s))
            {
                _characterPotraitSize = s;
                Point p = new(s, s);
                foreach (Control c in _portraitsPanel.Children)
                {
                    c.Size = p;
                }
            }
        }

        private void GapBox_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(_gapBox.Text, out int s))
            {
                _gap = s;
                _portraitsPanel.ControlPadding = new Vector2(s, 0);
            }
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
            _ = new BasicFrameContainer()
            {
                Parent = _portraitsPanel,
                Size = new Point(_characterPotraitSize, _characterPotraitSize),
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

        private void CapturePotrait(Rectangle bounds)
        {
            string path = Characters.ModuleInstance.AccountImagesPath;

            Regex regex = new("Image.*[0-9].png");
            var images = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories).Where(path => regex.IsMatch(path)).ToList();

            IntPtr hWnd = GameService.GameIntegration.Gw2Instance.Gw2WindowHandle;

            RECT wndBounds = default;
            _ = GetWindowRect(hWnd, ref wndBounds);
            _ = GetClientRect(hWnd, out RECT clientRectangle);

            bool fullscreen = GameService.GameIntegration.GfxSettings.ScreenMode != Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;

            int titleBarHeight = fullscreen ? 0 : wndBounds.Bottom - wndBounds.Top - (clientRectangle.Bottom - clientRectangle.Top) - 6;
            int sideBarWidth = fullscreen ? 0 : wndBounds.Right - wndBounds.Left - (clientRectangle.Right - clientRectangle.Left) - 7;

            Rectangle cPos = bounds;
            double factor = GameService.Graphics.UIScaleMultiplier;

            using System.Drawing.Bitmap bitmap = new((int)((_characterPotraitSize - 2) * factor), (int)((_characterPotraitSize - 2) * factor));
            using (var g = System.Drawing.Graphics.FromImage(bitmap))
            {
                int x = (int)(bounds.X * factor);
                int y = (int)(bounds.Y * factor);

                g.CopyFromScreen(new System.Drawing.Point(wndBounds.Left + x + sideBarWidth, clientRectangle.Top + y + titleBarHeight), System.Drawing.Point.Empty, new System.Drawing.Size(_characterPotraitSize - 2, _characterPotraitSize));
            }

            bitmap.Save(path + "Image " + (images.Count + 1) + ".png", System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
