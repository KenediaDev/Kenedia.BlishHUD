using Blish_HUD;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SizingMode = Blish_HUD.Controls.SizingMode;
using ControlFlowDirection = Blish_HUD.Controls.ControlFlowDirection;
using Blish_HUD.Input;
using Microsoft.Xna.Framework.Input;
using Keyboard = Blish_HUD.Controls.Intern.Keyboard;
using Key = Blish_HUD.Controls.Extern.VirtualKeyShort;
using Kenedia.Modules.QoL.Res;
using Kenedia.Modules.Core.Models;

namespace Kenedia.Modules.QoL.SubModules.ItemDestruction
{
    internal class ItemDestruction : SubModule
    {
        private readonly BorderedImage _itemPreview;
        private readonly Label _destroyLabel;
        private readonly Label _instuctionLabel;
        private readonly MouseContainer _mouseContainer;

        private double _lastAction;
        private double _tick;
        private ItemDestructionState _state = ItemDestructionState.Disabled;
        private string _copiedText;

        private SettingEntry<bool> _disableOnRightClick;
        private SettingEntry<KeyBinding> _modifierToChat;

        public ItemDestruction(SettingCollection settings) : base(settings)
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
                Text = "No Item selected yet.",
            };

            _instuctionLabel = new()
            {
                Parent = flowPanel,
                Font = GameService.Content.DefaultFont16,
                TextColor = Color.White,
                AutoSizeWidth = true,
                Height = GameService.Content.DefaultFont16.LineHeight,
                Text = "SHIFT + Left Click on item!",
            };
        }

        private enum ItemDestructionState
        {
            Disabled,
            None,
            Selected,
            Dragged,
            Destroyed,
        }

        private ItemDestructionState State { get => _state; set => Common.SetProperty(ref _state, value, OnStateSwitched); }

        public override SubModuleType SubModuleType => SubModuleType.ItemDestruction;

        private void OnStateSwitched(object sender, Core.Models.ValueChangedEventArgs<ItemDestructionState> e)
        {
            _lastAction = Common.Now;

            Dictionary<ItemDestructionState, string> instructions = new()
            {
                { ItemDestructionState.None, "SHIFT + Left Click on item!" },
                { ItemDestructionState.Selected, "Drag the item out of your inventory!" },
                { ItemDestructionState.Dragged, "Press 'Yes'!" },
                { ItemDestructionState.Destroyed, "SHIFT + Left Click on item!" },
            };

            switch (State)
            {
                case ItemDestructionState.None:
                    _instuctionLabel.Text = instructions[State];
                    break;

                case ItemDestructionState.Selected:
                    _instuctionLabel.Text = instructions[State];
                    break;

                case ItemDestructionState.Dragged:
                    _instuctionLabel.Text = instructions[State];
                    break;

                case ItemDestructionState.Destroyed:
                    _instuctionLabel.Text = instructions[State];
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {

            if (gameTime.TotalGameTime.TotalMilliseconds - _tick > 500)
            {
                _tick = gameTime.TotalGameTime.TotalMilliseconds;

            }
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

            _modifierToChat = settings.DefineSetting(nameof(_modifierToChat),
                new KeyBinding(Keys.LeftShift));

            _disableOnRightClick = settings.DefineSetting(nameof(_disableOnRightClick),
                true);
        }

        protected override void Disable()
        {
            base.Disable();
            _mouseContainer.Hide();
            State = ItemDestructionState.Disabled;
        }

        override protected void Enable()
        {
            base.Enable();
            _mouseContainer.Show();
            State = ItemDestructionState.None;
        }

        public override void Unload()
        {
            base.Unload();

            GameService.Input.Mouse.LeftMouseButtonReleased -= Mouse_LeftMouseButtonReleased;
            GameService.Input.Mouse.LeftMouseButtonPressed -= Mouse_LeftMouseButtonPressed;
            GameService.Input.Mouse.RightMouseButtonReleased -= Mouse_RightMouseButtonPressed;
        }

        override public void Load()
        {
            base.Load();

            GameService.Input.Mouse.LeftMouseButtonReleased += Mouse_LeftMouseButtonReleased;
            GameService.Input.Mouse.LeftMouseButtonPressed += Mouse_LeftMouseButtonPressed;
            GameService.Input.Mouse.RightMouseButtonReleased += Mouse_RightMouseButtonPressed;

        }

        private void Mouse_RightMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (!Enabled || !_disableOnRightClick.Value) return;

            Disable();
            State = ItemDestructionState.None;
        }

        private async void Mouse_LeftMouseButtonReleased(object sender, MouseEventArgs e)
        {
            if (Common.Now - _lastAction < 150 || !Enabled) return;

            switch (State)
            {
                case ItemDestructionState.Selected:
                    await Task.Delay(50);

                    if (GameService.Gw2Mumble.UI.IsTextInputFocused)
                    {
                        State = ItemDestructionState.Dragged;
                        await Paste();
                    }
                    break;
            }
        }

        private async void Mouse_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (Common.Now - _lastAction < 150 || !Enabled) return;

            if (GameService.Input.Keyboard.KeysDown.Contains(Keys.LeftShift) || GameService.Input.Keyboard.KeysDown.Contains(Keys.RightShift))
            {

                var _clientWindowService = QoL.ModuleInstance.Services.ClientWindowService;
                var _sharedSettings = QoL.ModuleInstance.Services.SharedSettings;

                var wndBounds = _clientWindowService.WindowBounds;

                bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
                Point p = windowed ? new(_sharedSettings.WindowOffset.Left, _sharedSettings.WindowOffset.Top) : Point.Zero;

                double factor = GameService.Graphics.UIScaleMultiplier;

                var point = e.MousePosition.Add(new(-32));

                _itemPreview.Texture = ScreenCapture.CaptureRegion(wndBounds, p, new(point, new(64)), factor, new(64));

                _copiedText = await CopyItemFromChat();
                //await OpenWikiForItemFromChat();
                _destroyLabel.Text = $"Copied Name: {_copiedText}";
                _destroyLabel.Text = _copiedText;
                State = ItemDestructionState.Selected;
            }
            else
            {
                switch (State)
                {
                    case ItemDestructionState.Dragged:
                        State = ItemDestructionState.Destroyed;

                        break;
                }
            }
        }

        private async Task Paste()
        {
            try
            {
                Keyboard.Press(Key.LCONTROL, true);
                Keyboard.Stroke(Key.KEY_V, true);
                await Task.Delay(25);
                Keyboard.Release(Key.LCONTROL, true);
            }
            catch { }
        }

        private async Task<string> CopyItemFromChat()
        {
            string text = string.Empty;

            try
            {
                var layout = QoL.ModuleInstance.Settings.KeyboardLayout.Value;

                _lastAction = Common.Now;
                await Task.Delay(50);

                Keyboard.Release(Key.LSHIFT, true);
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

                text = await ClipboardUtil.WindowsClipboardService.GetTextAsync();

                if (string.IsNullOrEmpty(text))
                {
                    return string.Empty;
                }

                string[] items = text.Split('[');
                text = items.Last();

                if (text.StartsWith("["))
                    text = text.Substring(1);

                if (text.EndsWith("]"))
                    text = text.Substring(0, text.Length - 1);

                _ = await ClipboardUtil.WindowsClipboardService.SetTextAsync(text);

                _lastAction = Common.Now;
            }
            catch
            {

            }

            return text;
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

            UI.WrapWithLabel(() => strings.DisableOnRightClick_Name, () => strings.DisableOnRightClick_Tooltip, contentFlowPanel, width - 16, new Checkbox()
            {
                Height = 20,
                Checked = _disableOnRightClick.Value,
                CheckedChangedAction = (b) => _disableOnRightClick.Value = b,
            });
        }
    }
}
