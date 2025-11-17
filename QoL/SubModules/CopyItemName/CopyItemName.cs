using Blish_HUD;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Controls;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Keyboard = Blish_HUD.Controls.Intern.Keyboard;
using Key = Blish_HUD.Controls.Extern.VirtualKeyShort;
using SizingMode = Blish_HUD.Controls.SizingMode;
using ControlFlowDirection = Blish_HUD.Controls.ControlFlowDirection;
using Blish_HUD.Input;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.QoL.Res;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using System.Diagnostics;

namespace Kenedia.Modules.QoL.SubModules.CopyItemName
{
    public class CopyItemName : SubModule
    {
        private readonly BorderedImage _itemPreview;
        private readonly MouseContainer _mouseContainer;
        private readonly Label _destroyLabel;

        private SettingEntry<bool> _disableOnCopy;
        private SettingEntry<bool> _disableOnRightClick;
        private SettingEntry<KeyBinding> _modifierToChat;
        private SettingEntry<ReturnType> _returnType;

        public CopyItemName(SettingCollection settings) : base(settings)
        {
            UI_Elements.Add(_mouseContainer = new()
            {
                Parent = GameService.Graphics.SpriteScreen,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Background = new(156003),
                TexturePadding = new(50, 50, 0, 0),
                BorderColor = Color.Black,
                BorderWidth = new(3),
                Visible = Enabled,
                ContentPadding = new(5),
                MouseOffset = new(25),
                ZIndex = int.MaxValue,
            });

            var p = new Rectangle(0, 0, 0, 0);

            _itemPreview = new()
            {
                Parent = _mouseContainer,
                Size = new(48),
                Location = new(0, p.Bottom),
                BorderWidth = new(2),
                BackgroundColor = Color.Black * 0.5F,
            };
            p = _itemPreview.LocalBounds;

            var flowPanel = new FlowPanel()
            {
                Parent = _mouseContainer,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Location = new(p.Right + 5, p.Top),
                ControlPadding = new(5),
                ContentPadding = new(5, 0, 0, 0)
            };

            _ = new Label()
            {
                Parent = flowPanel,
                Font = GameService.Content.DefaultFont18,
                TextColor = ContentService.Colors.Chardonnay,
                AutoSizeWidth = true,
                Height = GameService.Content.DefaultFont18.LineHeight,
                Text = $"{SubModuleType}".SplitStringOnUppercase(),
            };

            _destroyLabel = new()
            {
                Parent = flowPanel,
                Font = GameService.Content.DefaultFont14,
                TextColor = Color.Lime,
                AutoSizeWidth = true,
                Height = GameService.Content.DefaultFont14.LineHeight,
                Text = "No Item name copied yet.",
            };

            _ = new Label()
            {
                Parent = flowPanel,
                Font = GameService.Content.DefaultFont16,
                TextColor = Color.White,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                Height = GameService.Content.DefaultFont16.LineHeight,
                Text = "SHIFT + Left Click on item to copy its item name!",
            };
        }

        public enum ReturnType
        {
            BracketAmountName,
            AmountName,
            Name,
        }

        public override SubModuleType SubModuleType => SubModuleType.CopyItemName;

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

            _disableOnCopy = settings.DefineSetting(nameof(_disableOnCopy),
                true);

            _disableOnRightClick = settings.DefineSetting(nameof(_disableOnRightClick),
                true);

            _modifierToChat = settings.DefineSetting(nameof(_modifierToChat),
                new KeyBinding(Keys.LeftShift));

            _returnType = settings.DefineSetting(nameof(_returnType),
                ReturnType.Name);
        }

        public override void Update(GameTime gameTime)
        {
            if (Enabled)
                _mouseContainer.Visible = GameService.Input.Keyboard.KeysDown.Contains(Keys.LeftShift) || GameService.Input.Keyboard.KeysDown.Contains(Keys.RightShift);
        }

        protected override void Disable()
        {
            base.Disable();

            _mouseContainer.Visible = false;
        }

        protected override void Enable()
        {
            base.Enable();

            _mouseContainer.Visible = true;
        }

        override public void Load()
        {
            base.Load();

            GameService.Input.Mouse.LeftMouseButtonPressed += Mouse_LeftMouseButtonPressed;
            GameService.Input.Mouse.RightMouseButtonReleased += Mouse_RightMouseButtonPressed;

        }

        public override void Unload()
        {
            base.Unload();

            GameService.Input.Mouse.LeftMouseButtonPressed -= Mouse_LeftMouseButtonPressed;
            GameService.Input.Mouse.RightMouseButtonReleased -= Mouse_RightMouseButtonPressed;
        }

        private void Mouse_RightMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (!Enabled) return;

            if (_disableOnRightClick.Value)
                Disable();
        }

        private async void Mouse_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (!Enabled) return;

            if (GameService.Input.Keyboard.KeysDown.Contains(Keys.LeftShift) || GameService.Input.Keyboard.KeysDown.Contains(Keys.RightShift))
            {
                var _clientWindowService = QoL.ModuleInstance.CoreServices.ClientWindowService;
                var _sharedSettings = QoL.ModuleInstance.CoreServices.SharedSettings;

                var wndBounds = _clientWindowService.WindowBounds;

                bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
                Point p = windowed ? new(_sharedSettings.WindowOffset.Left, _sharedSettings.WindowOffset.Top) : Point.Zero;

                double factor = GameService.Graphics.UIScaleMultiplier;

                var point = e.MousePosition.Add(new(-32));

                _itemPreview.Texture = ScreenCapture.CaptureRegion(wndBounds, p, new(point, new(64)), factor, new(64));

                await GetItemNameFromChat();
            }
        }

        private async Task GetItemNameFromChat()
        {
            try
            {
                var layout = QoL.ModuleInstance.Settings.KeyboardLayout.Value;

                await Task.Delay(50);
                var key = _modifierToChat.Value.PrimaryKey;

                Keyboard.Release((Key)Keys.LeftShift, true);
                Keyboard.Release((Key)Keys.RightShift, true);
                await Task.Delay(5);

                Keyboard.Press(Key.LCONTROL, true);
                Keyboard.Stroke(
                    layout switch
                    {
                        KeyboardLayoutType.AZERTY => Key.KEY_Q,
                        _ => Key.KEY_A,
                    }, true);
                await Task.Delay(25);

                Keyboard.Stroke(Key.KEY_C, true);
                await Task.Delay(50);
                Keyboard.Release(Key.LCONTROL, true);

                Keyboard.Stroke(Key.BACK, true);
                Keyboard.Stroke(Key.RETURN, true);
                await Task.Delay(5);

                string text = await ClipboardUtil.WindowsClipboardService.GetTextAsync();

                if (string.IsNullOrEmpty(text))
                {
                    return;
                }

                string[] items = text.Split('[');
                text = items.Last();

                if (_returnType.Value != ReturnType.BracketAmountName)
                {
                    if (text.StartsWith("["))
                        text = text.Substring(1);

                    if (text.EndsWith("]"))
                        text = text.Substring(0, text.Length - 1);
                }
                else
                {
                    text = "[" + text;
                }

                if(_returnType.Value == ReturnType.Name)
                {
                    text = text.RemoveLeadingNumbers();
                    text = text.TrimStart();
                }

                _ = await ClipboardUtil.WindowsClipboardService.SetTextAsync(text);
                _destroyLabel.Text = text;

                if (_disableOnCopy.Value)
                {
                    Disable(); 
                }
            }
            catch
            {

            }
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

            UI.WrapWithLabel(() => strings.DisableOnCopy_Name, () => strings.DisableOnCopy_Tooltip, contentFlowPanel, width - 16, new Checkbox()
            {
                Height = 20,
                Checked = _disableOnCopy.Value,
                CheckedChangedAction = (b) => _disableOnCopy.Value = b,
            });

            UI.WrapWithLabel(() => strings.DisableOnRightClick_Name, () => strings.DisableOnRightClick_Tooltip, contentFlowPanel, width - 16, new Checkbox()
            {
                Height = 20,
                Checked = _disableOnRightClick.Value,
                CheckedChangedAction = (b) => _disableOnRightClick.Value = b,
            });

            UI.WrapWithLabel(() => strings.ReturnType_Name, () => strings.ReturnType_Tooltip, contentFlowPanel, width - 16, new Dropdown()
            {
                Location = new(250, 0),
                Parent = contentFlowPanel,
                SetLocalizedItems = () =>
                {
                    return
                    [
                        $"{ReturnType.Name}".SplitStringOnUppercase(),
                        $"{ReturnType.AmountName}".SplitStringOnUppercase(),
                        $"{ReturnType.BracketAmountName}".SplitStringOnUppercase(),
                    ];
                },
                SelectedItem = $"{_returnType.Value}",
                ValueChangedAction = (b) => _returnType.Value = Enum.TryParse(b.RemoveSpaces(), out ReturnType returnType) ? returnType : _returnType.Value,
            });
        }
    }
}
