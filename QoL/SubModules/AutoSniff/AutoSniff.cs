using Blish_HUD;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Kenedia.Modules.QoL.Res;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Kenedia.Modules.Core.Extensions;
using SizingMode = Blish_HUD.Controls.SizingMode;
using ControlFlowDirection = Blish_HUD.Controls.ControlFlowDirection;
using Kenedia.Modules.Core.Services;
using System.Drawing;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Diagnostics;
using Blish_HUD.Controls.Extern;
using InputKeyboard = Blish_HUD.Controls.Intern.Keyboard;

namespace Kenedia.Modules.QoL.SubModules.AutoSniff
{
    public class AutoSniff : SubModule
    {
        private double _ticks;
        private double _threshold = 500;
        private SettingEntry<KeyBinding> _sniffSkillKey;
        private Blish_HUD.Controls.Image _capturedImageControl;

        public AutoSniff(SettingCollection settings, ClientWindowService clientWindowService, SharedSettings sharedSettings) : base(settings)
        {
            ClientWindowService = clientWindowService;
            SharedSettings = sharedSettings;
        }

        public Texture2D SlotTexture { get; private set; }

        public Texture2D MountSlotTexture { get; private set; }

        public override SubModuleType SubModuleType => SubModuleType.AutoSniff;

        public ClientWindowService ClientWindowService { get; }

        public SharedSettings SharedSettings { get; }
        public bool WasMounted { get; private set; }

        public override void Update(GameTime gameTime)
        {
            if (!Enabled) return;
            var Mumble = GameService.Gw2Mumble;

            if (_capturedImageControl is not null)
            {
                //_capturedImageControl.Location = new Point(1950, 1635);
                _capturedImageControl.Location = new Point(1990, 1595);
                _capturedImageControl.Size = new Point(40);

                //Debug.WriteLine($"{_capturedImageControl.Location.ClientToScreenPos()}");
            }

            if (!Mumble.Info.IsGameFocused || Mumble.UI.IsTextInputFocused || !GameService.GameIntegration.Gw2Instance.IsInGame || gameTime.TotalGameTime.TotalMilliseconds - _ticks < _threshold)
            {
                return;
            }

            if (GameService.Gw2Mumble.CurrentMap.Id is 1554 or 1550)
            {
                if (Sniff())
                {
                    _ticks = gameTime.TotalGameTime.TotalMilliseconds;
                    return;
                }
            }

            double delay = _threshold * 0.5;
            _ticks = gameTime.TotalGameTime.TotalMilliseconds + delay;

            //Debug.WriteLine($"{_threshold - delay} Until next tick");
        }

        private bool IsMounted()
        {
            if (GameService.Gw2Mumble.UI.IsMapOpen && WasMounted)
            {
                return true;
            }

            var mountSlotPosition = new Point(1944, 1600);

            var wndBounds = ClientWindowService.WindowBounds;

            bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
            Point p = windowed ? new(SharedSettings.WindowOffset.Left, SharedSettings.WindowOffset.Top) : Point.Zero;

            float factor = GameService.Graphics.UIScaleMultiplier;

            var size = new Size(40, 40);

            var bounds = new Rectangle(mountSlotPosition.X, mountSlotPosition.Y, size.Width, size.Height);
            using Bitmap bitmap = new((int)(bounds.Width * factor), (int)(bounds.Height * factor));

            using (var g = System.Drawing.Graphics.FromImage(bitmap))
            {
                int x = (int)(bounds.X * factor);
                int y = (int)(bounds.Y * factor);

                g.CopyFromScreen(new System.Drawing.Point(wndBounds.Left + p.X + x, wndBounds.Top + p.Y + y), System.Drawing.Point.Empty, size);
            }

            using MemoryStream s = new();
            bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Bmp);

            MountSlotTexture?.Dispose();
            MountSlotTexture = s.CreateTexture2D();
            _capturedImageControl?.SetTexture(MountSlotTexture);

            System.Drawing.Color color = bitmap.GetAverageColor();
            //Debug.WriteLine($"Mount Slot Texture Color {color.R}, {color.G}, {color.B} | {(color.R + color.B + color.G) / 3}");
            int avg = (color.R + color.B + color.G) / 3;

            WasMounted = avg is 26;

            return WasMounted;
        }

        private bool IsSkillReady()
        {
            if (GameService.Gw2Mumble.UI.IsMapOpen && WasMounted)
            {
                return true;
            }

            var slotPosition = new Point(1292, 1600);

            var wndBounds = ClientWindowService.WindowBounds;

            bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
            Point p = windowed ? new(SharedSettings.WindowOffset.Left, SharedSettings.WindowOffset.Top) : Point.Zero;

            float factor = GameService.Graphics.UIScaleMultiplier;

            var size = new Size(40, 40);

            var bounds = new Rectangle(slotPosition.X, slotPosition.Y, size.Width, size.Height);
            using Bitmap bitmap = new((int)(bounds.Width * factor), (int)(bounds.Height * factor));

            using (var g = System.Drawing.Graphics.FromImage(bitmap))
            {
                int x = (int)(bounds.X * factor);
                int y = (int)(bounds.Y * factor);

                g.CopyFromScreen(new System.Drawing.Point(wndBounds.Left + p.X + x, wndBounds.Top + p.Y + y), System.Drawing.Point.Empty, size);
            }

            using MemoryStream s = new();
            bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Bmp);

            SlotTexture?.Dispose();
            SlotTexture = s.CreateTexture2D();
            _capturedImageControl?.SetTexture(SlotTexture);

            System.Drawing.Color color = bitmap.GetAverageColor();
            //Debug.WriteLine($"Skill#2 Slot Texture Color {color.R}, {color.G}, {color.B} | {(color.R + color.B + color.G) / 3}");
            int avg = (color.R + color.B + color.G) / 3;

            return avg is 119 or 172;
        }

        private bool Sniff()
        {
            if (IsMounted())
            {
                //GameService.Gw2Mumble.PlayerCharacter.UseSkill(_sniffSkillKey.Value);
                if(IsSkillReady())
                {
                    Debug.WriteLine($"SNIFF SNIFF");
                    InputKeyboard.Stroke((VirtualKeyShort)_sniffSkillKey.Value.PrimaryKey);
                    return true;
                }
            }

            return false;
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

            _sniffSkillKey = Settings.DefineSetting(nameof(_sniffSkillKey), new KeyBinding(Keys.D2));
        }

        protected override void Enable()
        {
            base.Enable();
        }

        override protected void Disable()
        {
            base.Disable();
        }

        public override void Load()
        {
            base.Load();

            var Mumble = GameService.Gw2Mumble;
            Mumble.CurrentMap.MapChanged += CurrentMap_MapChanged;
            Mumble.PlayerCharacter.NameChanged += PlayerCharacter_NameChanged;

            //BuildUi();
        }

        private void BuildUi()
        {
            UI_Elements.Add(_capturedImageControl = new Blish_HUD.Controls.Image()
            {
                Parent = GameService.Graphics.SpriteScreen,
                Texture = GameService.Content.GetTexture("102339"),
                Location = new Point(1990, 1595),
                Size = new Point(40),
            });
        }

        private void PlayerCharacter_NameChanged(object sender, ValueEventArgs<string> e)
        {
            _ticks = 0;
        }

        private void CurrentMap_MapChanged(object sender, ValueEventArgs<int> e)
        {
            _ticks = 0;
        }

        public override void Unload()
        {
            base.Unload();

            var Mumble = GameService.Gw2Mumble;
            Mumble.CurrentMap.MapChanged -= CurrentMap_MapChanged;
            Mumble.PlayerCharacter.NameChanged -= PlayerCharacter_NameChanged;
        }

        protected override void SwitchLanguage()
        {
            base.SwitchLanguage();
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

            UI.WrapWithLabel(() => string.Format(strings.ShowInHotbar_Name, $"{SubModuleType}"), () => string.Format(strings.ShowInHotbar_Description, $"{SubModuleType}"), contentFlowPanel, width - 16, new Checkbox()
            {
                Height = 20,
                Checked = ShowInHotbar.Value,
                CheckedChangedAction = (b) => ShowInHotbar.Value = b,
            });

            _ = new KeybindingAssigner()
            {
                Parent = contentFlowPanel,
                Width = width - 16,
                KeyBinding = HotKey.Value,
                KeybindChangedAction = (kb) =>
                {
                    HotKey.Value = new()
                    {
                        ModifierKeys = kb.ModifierKeys,
                        PrimaryKey = kb.PrimaryKey,
                        Enabled = kb.Enabled,
                        IgnoreWhenInTextField = true,
                    };
                },
                SetLocalizedKeyBindingName = () => string.Format(strings.HotkeyEntry_Name, $"{SubModuleType}"),
                SetLocalizedTooltip = () => string.Format(strings.HotkeyEntry_Description, $"{SubModuleType}"),
            };

            _ = new KeybindingAssigner()
            {
                Parent = contentFlowPanel,
                Width = width - 16,
                KeyBinding = _sniffSkillKey.Value,
                KeybindChangedAction = (kb) =>
                {
                    _sniffSkillKey.Value = new()
                    {
                        ModifierKeys = kb.ModifierKeys,
                        PrimaryKey = kb.PrimaryKey,
                        Enabled = kb.Enabled,
                        IgnoreWhenInTextField = true,
                    };
                },
                SetLocalizedKeyBindingName = () => "Sniff Skill Keybind",
                SetLocalizedTooltip = () => "Set Sniff Skill Keybind",
            };
        }
    }
}
