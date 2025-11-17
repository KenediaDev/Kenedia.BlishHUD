using Blish_HUD;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Keyboard = Blish_HUD.Controls.Intern.Keyboard;
using Key = Blish_HUD.Controls.Extern.VirtualKeyShort;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Controls;
using SizingMode = Blish_HUD.Controls.SizingMode;
using ControlFlowDirection = Blish_HUD.Controls.ControlFlowDirection;
using Kenedia.Modules.QoL.Res;
using Gw2Sharp.ChatLinks;
using System.Diagnostics;
using Mouse = Blish_HUD.Controls.Intern.Mouse;
using static Kenedia.Modules.Core.Utility.WindowsUtil.User32Dll;
using Blish_HUD.Controls.Intern;
using System.Runtime.InteropServices;
using Gw2Sharp.Mumble.Models;
using Microsoft.Xna.Framework.Graphics;
using static Blish_HUD.ContentService;
using Bitmap = System.Drawing.Bitmap;

namespace Kenedia.Modules.QoL.SubModules.SchemanticProcessing
{
    public class ClickContainer : Panel
    {
        private bool _selected = false;

        public ClickContainer()
        {
            BorderColor = Colors.ColonialWhite * 0.5F;
            BorderWidth = new(1);
        }

        public bool ShowCenter { get; set; } = true;

        public bool Selected { get => _selected; set => Common.SetProperty(ref _selected, value, OnSelectedChanged); }

        public Color CenterColor { get; private set; } = Colors.ColonialWhite * 0.5F;

        public Rectangle MaskedRegion { get; private set; }

        private void OnSelectedChanged(object sender, Core.Models.ValueChangedEventArgs<bool> e)
        {
            BorderColor = CenterColor = Selected ? Color.Lime : Colors.ColonialWhite * 0.5F;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            MaskedRegion = new Rectangle(Location.X + BorderWidth.Left, Location.Y + BorderWidth.Top, Size.X - BorderWidth.Horizontal, Size.Y - BorderWidth.Vertical);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if (ShowCenter)
            {
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle((Width / 2) - 2, (Height / 2) - 2, 4, 4), ContentService.Textures.Pixel.Bounds, CenterColor, 0f, default);
            }
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            CaptureInput = GameService.Input.Keyboard.ActiveModifiers == ModifierKeys.Alt;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            Selected = !Selected;
        }
    }

    // Discord Suggestion: https://discordapp.com/channels/531175899588984842/1168285032293601411/1168285037293215806
    public class SchemanticProcessing : SubModule
    {
        private double _ticks;
        private SettingEntry<Point> _clickContainerLocation;
        private SettingEntry<Point> _clickContainerSize;
        private SettingEntry<KeyBinding> _triggerClick;

        private readonly Dictionary<UiSize, int> _sizes = new()
        {
            { UiSize.Small , 56},
            { UiSize.Normal , 56},
            { UiSize.Large , 58},
            { UiSize.Larger , 56},
        };
        private readonly List<ClickContainer> _slots = [];
        private readonly FlowPanel _slotGrid;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int x, int y);

        public SchemanticProcessing(SettingCollection settings) : base(settings)
        {
            GameService.Gw2Mumble.UI.UISizeChanged += UI_UISizeChanged;

            _slotGrid = new()
            {
                Parent = GameService.Graphics.SpriteScreen,
                Location = _clickContainerLocation.Value,
                BorderColor = Color.Black,
                CanDrag = true,
                CaptureInput = false,
                Visible = Enabled,
                FlowDirection = ControlFlowDirection.LeftToRight,
                HeightSizingMode = SizingMode.AutoSize,
                Width = _sizes[GameService.Gw2Mumble.UI.UISize] * 4,
            };

            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    {
                        _slots.Add(new()
                        {
                            Parent = _slotGrid,
                            CaptureInput = false,
                        });
                    }
                }
            }

            AdjustSizes();

            _slotGrid.Moved += ResizeableContainer_Moved;
            _slotGrid.Resized += ResizeableContainer_Resized;
        }

        private void UI_UISizeChanged(object sender, ValueEventArgs<UiSize> e)
        {
            AdjustSizes();
        }

        private void AdjustSizes()
        {
            _sizes[UiSize.Large] = 56;
            _slotGrid.ControlPadding = new(8);
            _slotGrid.Width = (_sizes[GameService.Gw2Mumble.UI.UISize] + (int)_slotGrid.ControlPadding.X) * 4;

            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    {
                        _slots[(row * 4) + col].Size = new(_sizes[GameService.Gw2Mumble.UI.UISize]);
                        _slots[(row * 4) + col].BorderWidth = new(2);
                    }
                }
            }
        }

        private void ResizeableContainer_Resized(object sender, Blish_HUD.Controls.ResizedEventArgs e)
        {
            _clickContainerSize.Value = _slotGrid.Size;
        }

        private void ResizeableContainer_Moved(object sender, Blish_HUD.Controls.MovedEventArgs e)
        {
            _clickContainerLocation.Value = _slotGrid.Location;
        }

        public override SubModuleType SubModuleType => SubModuleType.SchemanticProcessing;

        public override void Update(GameTime gameTime)
        {
            if (Enabled)
            {
                _slotGrid.CaptureInput = GameService.Input.Keyboard.ActiveModifiers == (ModifierKeys.Alt | ModifierKeys.Shift);

                if (Common.Now - _ticks > 10000 && !_slotGrid.CaptureInput)
                {
                    _ticks = Common.Now;

                    _ = PerformClicks();
                }
            }
        }

        private async Task PerformClicks()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                ClickContainer slot = _slots[i];

                if (slot.Selected)
                {
                    var p = slot.AbsoluteBounds.Center.ScaleToUi();
                    //var okPos = GameService.Graphics.SpriteScreen.AbsoluteBounds.Center.Add(new(0, 35)).ScaleToUi().ClientToScreenPos();
                    var okPos = GameService.Graphics.SpriteScreen.AbsoluteBounds.Center.Add(new(0, 35)).ScaleToUi();

                    bool canInteract = CanInteract(slot, i);
                    bool focused = GameService.GameIntegration.Gw2Instance.Gw2HasFocus;

                    if (canInteract)
                    {
                        Debug.WriteLine($"Clicking at slot #{i}");
                        MouseUtil.DoubleClick(MouseUtil.MouseButton.LEFT, p, false);

                        await Task.Delay(350);

                        MouseUtil.Click(MouseUtil.MouseButton.LEFT, okPos, false);
                        await Task.Delay(250);
                    }

                    //MouseUtil.SetPosition(okPos, true);
                }
            }
        }

        private bool CanInteract(ClickContainer c, int index = 0)
        {
            static bool IsGrayscale(Bitmap image, int threshold = 50)
            {
                int count = 0;
                int highestBlue = 0;

                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        System.Drawing.Color pixelColor = image.GetPixel(x, y);

                        // Check if the red, green, and blue components are different
                        if (pixelColor.R != pixelColor.G || pixelColor.G != pixelColor.B)
                        {
                            count++;
                        }

                        highestBlue = Math.Max(highestBlue, pixelColor.B);

                        if (count > threshold && highestBlue > 100)
                            return false; // It's a color image
                    }
                }

                return true; // It's a grayscale image
            }

            if (GameService.GameIntegration.Gw2Instance.Gw2HasFocus)
            {
                Point size = c.MaskedRegion.Size.Add(new(-c.BorderWidth.Horizontal, -c.BorderWidth.Vertical)).ScaleToUi();

                using Bitmap bitmap = new(size.X, size.Y);

                using (var g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    var p = c.AbsoluteBounds.Location.Add(new(c.BorderWidth.Horizontal, c.BorderWidth.Vertical)).ClientToScreenPos(true);

                    g.CopyFromScreen(new System.Drawing.Point(p.X, p.Y), System.Drawing.Point.Empty, new(size.X, size.Y));
                }

                //bitmap.Save($"F:\\Spam\\container {index}.png", System.Drawing.Imaging.ImageFormat.Png);
                return !IsGrayscale(bitmap, 150);
            }
            else
            {
                return true;
            }
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

            _clickContainerLocation = settings.DefineSetting(nameof(_clickContainerLocation), new Point(50, 50));
            _clickContainerSize = settings.DefineSetting(nameof(_clickContainerSize), new Point(64, 64));

            _triggerClick = settings.DefineSetting(nameof(_triggerClick), new KeyBinding(ModifierKeys.Alt, Keys.D1));
            _triggerClick.Value.Enabled = true;
            _triggerClick.Value.Activated += TriggerClick_Activated;
        }

        private void TriggerClick_Activated(object sender, EventArgs e)
        {
            var p = _slotGrid.AbsoluteBounds.Center.ClientToScreenPos(true);

        }

        protected override void Enable()
        {
            base.Enable();

            _slotGrid.Visible = true;
        }

        protected override void Disable()
        {
            base.Disable();

            _slotGrid.Visible = false;
        }

        public override void CreateSettingsPanel(FlowPanel flowPanel, int width)
        {
            var headerPanel = new Panel()
            {
                Parent = flowPanel,
                Width = width,
                HeightSizingMode = SizingMode.AutoSize,
                ShowBorder = true,
                CanCollapse = true,
                TitleIcon = Icon.Texture,
                Title = SubModuleType.ToString(),
            };

            var contentFlowPanel = new FlowPanel()
            {
                Parent = headerPanel,
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ContentPadding = new(5, 2),
                ControlPadding = new(0, 2),
            };

            UI.WrapWithLabel(() => string.Format(strings.ShowInHotbar_Name, Name), () => string.Format(strings.ShowInHotbar_Description, Name), contentFlowPanel, width - 16, new Checkbox()
            {
                Height = 20,
                Checked = ShowInHotbar.Value,
                CheckedChangedAction = (b) => ShowInHotbar.Value = b,
            });
        }
    }
}
