using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework;
using System.IO;
using static Kenedia.Modules.Core.Utility.WindowsUtil.User32Dll;
using Bitmap = System.Drawing.Bitmap;
using Color = Microsoft.Xna.Framework.Color;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using Image = Blish_HUD.Controls.Image;
using Label = Kenedia.Modules.Core.Controls.Label;
using Panel = Kenedia.Modules.Core.Controls.Panel;

namespace Kenedia.Modules.Core.Views
{
    public partial class SettingsWindow
    {
        public class SharedSettingsView : BaseTab
        {            
            private NumberBox _topOffsetBox;
            private NumberBox _leftOffsetBox;
            private NumberBox _rightOffsetBox;
            private NumberBox _bottomOffsetBox;

            private Image _topLeftImage;
            private Image _topRightImage;
            private Image _bottomLeftImage;
            private Image _bottomRightImage;

            public SharedSettingsView()
            {
                Icon = AsyncTexture2D.FromAssetId(156736);
                Name = "General Settings";
                Priority = 0;
                CreateLayout();
            }

            private void CreateLayout()
            {
                #region Layout
                Panel p;
                ContentContainer = p = new Panel()
                {
                    Width = 500,
                    WidthSizingMode = SizingMode.Fill,
                    HeightSizingMode = SizingMode.AutoSize,
                    ShowBorder = true,
                };

                _ = new Image()
                {
                    Texture = AsyncTexture2D.FromAssetId(156736),
                    Parent = p,
                    Size = new(30, 30),
                };

                _ = new Label()
                {
                    Parent = p,
                    AutoSizeWidth = true,
                    Location = new(35, 0),
                    Height = 30,
                    SetLocalizedText = () => "Layout Settings",
                };

                var cFP = new FlowPanel()
                {
                    Parent = p,
                    Location = new(5, 35),
                    HeightSizingMode = SizingMode.AutoSize,
                    WidthSizingMode = SizingMode.Fill,
                    FlowDirection = ControlFlowDirection.SingleLeftToRight,
                    ControlPadding = new(3, 3),
                };

                var cP = new FlowPanel()
                {
                    Parent = cFP,
                    HeightSizingMode = SizingMode.AutoSize,
                    Width = (ContentContainer.Width - 20) / 2,
                    FlowDirection = ControlFlowDirection.SingleTopToBottom,
                    ControlPadding = new(3, 3),
                };

                var pp = new FlowPanel()
                {
                    Parent = cP,
                    WidthSizingMode = SizingMode.Fill,
                    HeightSizingMode = SizingMode.AutoSize,
                    FlowDirection = ControlFlowDirection.SingleLeftToRight,
                };

                _ = new Label()
                {
                    Parent = pp,
                    Width = 150,
                    Location = new(35, 0),
                    Height = 20,
                    SetLocalizedText = () => "Top Offset",
                };
                _topOffsetBox = new()
                {
                    Parent = pp,
                    MinValue = -50,
                    MaxValue = 50,
                    Value = SharedSettings.WindowOffset.Top,
                    SetLocalizedTooltip = () => "Top",
                    ValueChangedAction = (num) => UpdateOffset(null, num),
                };

                pp = new FlowPanel()
                {
                    Parent = cP,
                    WidthSizingMode = SizingMode.Fill,
                    HeightSizingMode = SizingMode.AutoSize,
                    FlowDirection = ControlFlowDirection.SingleLeftToRight,
                };
                _ = new Label()
                {
                    Parent = pp,
                    Width = 150,
                    Location = new(35, 0),
                    Height = 20,
                    SetLocalizedText = () => "Left Offset",
                };
                _leftOffsetBox = new()
                {
                    Parent = pp,
                    MinValue = -50,
                    MaxValue = 50,
                    Value = SharedSettings.WindowOffset.Left,
                    SetLocalizedTooltip = () => "Left",
                    ValueChangedAction = (num) => UpdateOffset(null, num),
                };

                pp = new FlowPanel()
                {
                    Parent = cP,
                    WidthSizingMode = SizingMode.Fill,
                    HeightSizingMode = SizingMode.AutoSize,
                    FlowDirection = ControlFlowDirection.SingleLeftToRight,
                };
                _ = new Label()
                {
                    Parent = pp,
                    Width = 150,
                    Location = new(35, 0),
                    Height = 20,
                    SetLocalizedText = () => "Bottom Offset",
                };
                _bottomOffsetBox = new()
                {
                    Parent = pp,
                    MinValue = -50,
                    MaxValue = 50,
                    Value = SharedSettings.WindowOffset.Bottom,
                    SetLocalizedTooltip = () => "Bottom",
                    ValueChangedAction = (num) => UpdateOffset(null, num),
                };

                pp = new FlowPanel()
                {
                    Parent = cP,
                    WidthSizingMode = SizingMode.Fill,
                    HeightSizingMode = SizingMode.AutoSize,
                    FlowDirection = ControlFlowDirection.SingleLeftToRight,
                };
                _ = new Label()
                {
                    Parent = pp,
                    Width = 150,
                    Location = new(35, 0),
                    Height = 20,
                    SetLocalizedText = () => "Right Offset",
                };
                _rightOffsetBox = new()
                {
                    Parent = pp,
                    MinValue = -50,
                    MaxValue = 50,
                    Value = SharedSettings.WindowOffset.Right,
                    SetLocalizedTooltip = () => "Right",
                    ValueChangedAction = (num) => UpdateOffset(null, num),
                };

                var subCP = new FlowPanel()
                {
                    Parent = cFP,
                    HeightSizingMode = SizingMode.AutoSize,
                    WidthSizingMode = SizingMode.Fill,
                    FlowDirection = ControlFlowDirection.SingleLeftToRight,
                    ControlPadding = new(5, 5),
                };

                cP = new FlowPanel()
                {
                    Parent = subCP,
                    HeightSizingMode = SizingMode.AutoSize,
                    Width = 125,
                    FlowDirection = ControlFlowDirection.SingleTopToBottom,
                    ControlPadding = new(5, 5),
                };
                _ = new Label()
                {
                    Parent = cP,
                    SetLocalizedText = () => "Left Top",
                    AutoSizeWidth = true,
                    Visible = false,
                };
                _topLeftImage = new()
                {
                    Parent = cP,
                    BackgroundColor = Color.White,
                    Size = new(100, _rightOffsetBox.Height * 2),
                };
                _ = new Label()
                {
                    Parent = cP,
                    SetLocalizedText = () => "Left Bottom",
                    AutoSizeWidth = true,
                    Visible = false,
                };
                _bottomLeftImage = new()
                {
                    Parent = cP,
                    BackgroundColor = Color.White,
                    Size = new(100, _rightOffsetBox.Height * 2),
                };

                cP = new FlowPanel()
                {
                    Parent = subCP,
                    HeightSizingMode = SizingMode.AutoSize,
                    Width = 125,
                    FlowDirection = ControlFlowDirection.SingleTopToBottom,
                    ControlPadding = new(5, 5),
                };
                _ = new Label()
                {
                    Parent = cP,
                    SetLocalizedText = () => "Right Top",
                    AutoSizeWidth = true,
                    Visible = false,
                };
                _topRightImage = new()
                {
                    Parent = cP,
                    BackgroundColor = Color.White,
                    Size = new(100, _rightOffsetBox.Height * 2),
                };
                _ = new Label()
                {
                    Parent = cP,
                    SetLocalizedText = () => "Right Bottom",
                    AutoSizeWidth = true,
                    Visible = false,
                };
                _bottomRightImage = new()
                {
                    Parent = cP,
                    BackgroundColor = Color.White,
                    Size = new(100, _rightOffsetBox.Height * 2),
                };

                #endregion
            }

            private void UpdateOffset(object sender, int e)
            {
                SharedSettings.WindowOffset = new(_leftOffsetBox.Value, _topOffsetBox.Value, _rightOffsetBox.Value, _bottomOffsetBox.Value);

                SetTopLeftImage();
                SetTopRightImage();

                SetBottomLeftImage();
                SetBottomRightImage();
            }

            private void SetTopLeftImage()
            {
                RECT wndBounds = ClientWindowService.WindowBounds;

                bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
                Point p = windowed ? new(SharedSettings.WindowOffset.Left, SharedSettings.WindowOffset.Top) : Point.Zero;

                using Bitmap bitmap = new(_topLeftImage.Width, _topLeftImage.Height);
                using var g = System.Drawing.Graphics.FromImage(bitmap);
                using MemoryStream s = new();
                g.CopyFromScreen(new System.Drawing.Point(wndBounds.Left + p.X, wndBounds.Top + p.Y), System.Drawing.Point.Empty, new(_topLeftImage.Width, _topLeftImage.Height));
                bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Bmp);
                _topLeftImage.Texture = s.CreateTexture2D();
            }

            private void SetBottomLeftImage()
            {
                RECT wndBounds = ClientWindowService.WindowBounds;

                bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
                Point p = windowed ? new(SharedSettings.WindowOffset.Left, SharedSettings.WindowOffset.Bottom) : Point.Zero;

                using Bitmap bitmap = new(_bottomLeftImage.Width, _bottomLeftImage.Height);
                using var g = System.Drawing.Graphics.FromImage(bitmap);
                using MemoryStream s = new();
                g.CopyFromScreen(new System.Drawing.Point(wndBounds.Left + p.X, wndBounds.Bottom - _bottomLeftImage.Height + p.Y), System.Drawing.Point.Empty, new(_bottomLeftImage.Width, _bottomLeftImage.Height));
                bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Bmp);
                _bottomLeftImage.Texture = s.CreateTexture2D();
            }

            private void SetTopRightImage()
            {
                RECT wndBounds = ClientWindowService.WindowBounds;

                bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
                Point p = windowed ? new(SharedSettings.WindowOffset.Right, SharedSettings.WindowOffset.Top) : Point.Zero;

                using Bitmap bitmap = new(_topRightImage.Width, _topRightImage.Height);
                using var g = System.Drawing.Graphics.FromImage(bitmap);
                using MemoryStream s = new();
                g.CopyFromScreen(new System.Drawing.Point(wndBounds.Right - _topRightImage.Width + p.X, wndBounds.Top + p.Y), System.Drawing.Point.Empty, new(_topRightImage.Width, _topRightImage.Height));
                bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Bmp);
                _topRightImage.Texture = s.CreateTexture2D();

            }

            private void SetBottomRightImage()
            {
                RECT wndBounds = ClientWindowService.WindowBounds;

                bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
                Point p = windowed ? new(SharedSettings.WindowOffset.Right, SharedSettings.WindowOffset.Bottom) : Point.Zero;

                using Bitmap bitmap = new(_bottomLeftImage.Width, _bottomLeftImage.Height);
                using var g = System.Drawing.Graphics.FromImage(bitmap);
                using MemoryStream s = new();
                g.CopyFromScreen(new System.Drawing.Point(wndBounds.Right - _bottomRightImage.Width + p.X, wndBounds.Bottom - _bottomRightImage.Height + p.Y), System.Drawing.Point.Empty, new(_bottomRightImage.Width, _bottomRightImage.Height));
                bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Bmp);
                _bottomRightImage.Texture = s.CreateTexture2D();

            }
        }
    }
}
