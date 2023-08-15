using Blish_HUD;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.Design.WebControls;
using SizingMode = Blish_HUD.Controls.SizingMode;
using ControlFlowDirection = Blish_HUD.Controls.ControlFlowDirection;
using Blish_HUD.Input;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Modules;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.Core.Structs;
using Blish_HUD.Content;
using Microsoft.Xna.Framework.Graphics;
using Keyboard = Blish_HUD.Controls.Intern.Keyboard;
using Key = Blish_HUD.Controls.Extern.VirtualKeyShort;

namespace Kenedia.Modules.QoL.SubModules.ItemDestruction
{
    public class BorderedImage : Image
    {
        public BorderedImage()
        {

        }

        public RectangleDimensions BorderWidth { get; set; } = new(2);

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);

            spriteBatch.DrawFrame(this, bounds, ContentService.Colors.ColonialWhite, 2);
        }
    }

    internal class ItemDestruction : SubModule
    {
        private readonly Image _itemHighlighter;
        private readonly BorderedImage _itemPreview;
        private readonly Label _moduleNameLabel;
        private readonly Label _destroyLabel;
        private readonly Label _instuctionLabel;
        private readonly MouseContainer _mouseContainer;
        private double _lastAction;
        private double _tick;
        private ItemDestructionState _state = ItemDestructionState.None;
        private string _copiedText;

        public ItemDestruction(SettingCollection settings) : base(settings)
        {
            SubModuleType = SubModuleType.ItemDestruction;

            Icon = new()
            {
                Texture = QoL.ModuleInstance.ContentsManager.GetTexture($@"textures\{SubModuleType}.png"),
                HoveredTexture = QoL.ModuleInstance.ContentsManager.GetTexture($@"textures\{SubModuleType}_Hovered.png"),
            };

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
            });

            //var spinner = new LoadingSpinner()
            //{
            //    Parent = _mouseContainer,
            //    Size = new(48),
            //};
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

            _moduleNameLabel = new()
            {
                Parent = flowPanel,
                Font = GameService.Content.DefaultFont18,
                TextColor = ContentService.Colors.Chardonnay,
                AutoSizeWidth = true,
                Height = GameService.Content.DefaultFont18.LineHeight,
                Text = "Item Destruction",
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

            UI_Elements.Add(_itemHighlighter = new()
            {
                Texture = QoL.ModuleInstance.ContentsManager.GetTexture($@"textures\ItemHighlighter.png"),
                Parent = GameService.Graphics.SpriteScreen,
                Visible = false,
                Size = new(36),
            });

            Load();
        }

        private enum ItemDestructionState
        {
            None,
            Selected,
            Dragged,
            Destroyed,
        }

        private ItemDestructionState State { get => _state; set => Common.SetProperty(ref _state, value, OnStateSwitched); }

        private void OnStateSwitched(object sender, Core.Models.ValueChangedEventArgs<ItemDestructionState> e)
        {
            _lastAction = Common.Now();

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
                    _itemHighlighter.Hide();
                    break;

                case ItemDestructionState.Selected:
                    _instuctionLabel.Text = instructions[State];
                    //_itemHighlighter.Show();
                    break;

                case ItemDestructionState.Dragged:
                    _instuctionLabel.Text = instructions[State];
                    break;

                case ItemDestructionState.Destroyed:
                    _instuctionLabel.Text = instructions[State];
                    _itemHighlighter.Hide();
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
        }

        protected override void Disable()
        {
            base.Disable();
            _mouseContainer.Hide();
            State = ItemDestructionState.None;
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
            if (!Enabled) return;

            State = ItemDestructionState.None;
        }

        private async void Mouse_LeftMouseButtonReleased(object sender, MouseEventArgs e)
        {
            if (Common.Now() - _lastAction < 150 || !Enabled) return;

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
            if (Common.Now() - _lastAction < 150 || !Enabled) return;

            if (GameService.Input.Keyboard.KeysDown.Contains(Keys.LeftShift))
            {

                var _clientWindowService = QoL.ModuleInstance.Services.ClientWindowService;
                var _sharedSettings = QoL.ModuleInstance.Services.SharedSettings;

                var wndBounds = _clientWindowService.WindowBounds;

                bool windowed = GameService.GameIntegration.GfxSettings.ScreenMode == Blish_HUD.GameIntegration.GfxSettings.ScreenModeSetting.Windowed;
                Point p = windowed ? new(_sharedSettings.WindowOffset.Left, _sharedSettings.WindowOffset.Top) : Point.Zero;

                double factor = GameService.Graphics.UIScaleMultiplier;

                var point = e.MousePosition.Add(new(-32));

                _itemPreview.Texture = ScreenCapture.CaptureRegion(wndBounds, p, new(point, new(64)), factor, new(64));
                _itemHighlighter.Location = e.MousePosition;

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
            Keyboard.Press(Key.LCONTROL, true);
            Keyboard.Stroke(Key.KEY_V, true);
            await Task.Delay(25);
            Keyboard.Release(Key.LCONTROL, true);
        }

        private async Task<string> CopyItemFromChat()
        {
            _lastAction = Common.Now();
            await Task.Delay(50);

            Keyboard.Release(Key.LSHIFT, true);
            await Task.Delay(5);

            Keyboard.Press(Key.LCONTROL, true);
            Keyboard.Stroke(Key.KEY_A, true);
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
                return string.Empty;
            }

            string[] items = text.Split('[');
            text = items.Last();

            if (text.StartsWith("["))
                text = text.Substring(1);

            if (text.EndsWith("]"))
                text = text.Substring(0, text.Length - 1);

            _ = await ClipboardUtil.WindowsClipboardService.SetTextAsync(text);

            _lastAction = Common.Now();
            return text;
        }

        private async Task OpenWikiForItemFromChat()
        {
            _lastAction = Common.Now();
            await Task.Delay(50);

            bool isReady = false;
            for (int i = 0; i < 500; i++)
            {
                if (GameService.Gw2Mumble.UI.IsTextInputFocused && !GameService.Input.Keyboard.KeysDown.Contains(Keys.RightShift) && !GameService.Input.Keyboard.KeysDown.Contains(Keys.LeftShift))
                {
                    isReady = true;
                    break;
                }

                await Task.Delay(5);
            }

            if (!isReady) return;

            for (int i = 0; i < 5; i++)
            {
                Keyboard.Release(Key.LSHIFT, false);
                await Task.Delay(10);
            }

            int delay = 40;
            Keyboard.Press(Key.LCONTROL, true);
            await Task.Delay(delay);

            Keyboard.Stroke(Key.LEFT, true);
            await Task.Delay(delay);

            bool hasWiki = await ClipboardUtil.WindowsClipboardService.SetTextAsync("/wiki ");
            if (hasWiki)
            {
                Keyboard.Stroke(Key.KEY_V, true);
                await Task.Delay(delay);
            }

            Keyboard.Release(Key.LCONTROL, true);
            if (!hasWiki)
            {
                Keyboard.Stroke(Key.BACK, true);
                Keyboard.Stroke(Key.RETURN, true);
                return;
            }

            await Task.Delay(delay);

            Keyboard.Stroke(Key.RETURN, true);
            await Task.Delay(delay);
            await Task.Delay(300);

            GameService.GameIntegration.Gw2Instance.FocusGw2();
        }

        protected override void SwitchLanguage()
        {
            base.SwitchLanguage();
        }
    }
}
