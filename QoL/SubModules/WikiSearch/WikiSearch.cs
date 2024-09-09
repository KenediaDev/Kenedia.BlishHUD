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
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.QoL.Res;
using System.Collections.Generic;
using System.Diagnostics;
using static Kenedia.Modules.QoL.SubModules.WikiSearch.WikiLocale;

namespace Kenedia.Modules.QoL.SubModules.WikiSearch
{
    public class WikiSearch : SubModule
    {
        private readonly MouseContainer _mouseContainer;

        private SettingEntry<bool> _disableOnRightClick;
        private SettingEntry<bool> _disableOnSearch;
        private SettingEntry<KeyBinding> _modifierToChat;
        private SettingEntry<Locale> _language;

        public WikiSearch(SettingCollection settings) : base(settings)
        {

            _mouseContainer = new()
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
            };

            var p = new Rectangle(0, 0, 0, 0);

            var image = new BorderedImage()
            {
                Parent = _mouseContainer,
                Size = new(48),
                Location = new(0, p.Bottom),
                BorderWidth = new(1),
                BackgroundColor = Color.Black * 0.5F,
                BorderColor = Color.Black * 0.8F,
                Texture = Icon.Texture,
            };

            p = image.LocalBounds;

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

            _ = new Label()
            {
                Parent = flowPanel,
                Font = GameService.Content.DefaultFont16,
                TextColor = Color.White,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                Height = GameService.Content.DefaultFont16.LineHeight,
                Text = "SHIFT + Left Click on item to open the wiki page!\nYou must release the SHIFT button after the click!",
            };
        }

        public override SubModuleType SubModuleType => SubModuleType.WikiSearch;

        public override void Update(GameTime gameTime)
        {
            if (Enabled)
                _mouseContainer.Visible = GameService.Input.Keyboard.KeysDown.Contains(Keys.LeftShift) || GameService.Input.Keyboard.KeysDown.Contains(Keys.RightShift);
        }

        protected override void Enable()
        {
            base.Enable();
        }

        protected override void Disable()
        {
            base.Disable();

            _mouseContainer.Visible = false;
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

            _disableOnSearch = settings.DefineSetting(nameof(_disableOnSearch),
                true);

            _disableOnRightClick = settings.DefineSetting(nameof(_disableOnRightClick),
                true);

            _modifierToChat = settings.DefineSetting(nameof(_modifierToChat),
                new KeyBinding(Keys.LeftShift));

            _language = settings.DefineSetting(nameof(_language),
                Locale.Default);
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
                await OpenWikiForItemFromChat();
            }
        }

        private async Task OpenWikiForItemFromChat()
        {
            try
            {
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

                bool hasWiki = await ClipboardUtil.WindowsClipboardService.SetTextAsync(GetWikiCommand());
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

                if (_disableOnSearch.Value)
                {
                    Disable();
                }
            }
            catch { }
        }

        private string GetWikiCommand()
        {
            return _language.Value switch
            {
                Locale.English => "/wiki en:",
                Locale.German => "/wiki de: ",
                Locale.French => "/wiki fr: ",
                Locale.Spanish => "/wiki es:",
                _ => "/wiki "
            };
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

            UI.WrapWithLabel(() => strings.DisableOnSearch_Name, () => strings.DisableOnSearch_Tooltip, contentFlowPanel, width - 16, new Checkbox()
            {
                Height = 20,
                Checked = _disableOnSearch.Value,
                CheckedChangedAction = (b) => _disableOnSearch.Value = b,
            });

            UI.WrapWithLabel(() => strings.DisableOnRightClick_Name, () => strings.DisableOnRightClick_Tooltip, contentFlowPanel, width - 16, new Checkbox()
            {
                Height = 20,
                Checked = _disableOnRightClick.Value,
                CheckedChangedAction = (b) => _disableOnRightClick.Value = b,
            });

            Array d = Enum.GetValues(typeof(Locale));

            UI.WrapWithLabel(() => strings.Language_Name, () => strings.Language_Tooltip, contentFlowPanel, width - 16, new Dropdown()
            {
                Height = 20,
                SelectedItem = ToDisplayString(_language.Value),
                SetLocalizedItems = () => Locales.Values.ToList(),
                ValueChangedAction = (s) => _language.Value = FromDisplayString(s),
            });
        }
    }
}
