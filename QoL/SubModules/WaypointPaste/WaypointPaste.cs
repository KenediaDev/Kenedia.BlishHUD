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

namespace Kenedia.Modules.QoL.SubModules.WaypointPaste
{
    // Discord Suggestion: https://discord.com/channels/531175899588984842/1064906680904724621/1072599804502364182
    public class WaypointPaste : SubModule
    {
        private double _ticks;
        private SettingEntry<KeyBinding> _pasteWaypoint;
        private SettingEntry<string> _waypoint;
        private SettingEntry<bool> _pasteCurrentClipboardWaypointFirst;

        public WaypointPaste(SettingCollection settings) : base(settings)
        {

        }

        public override SubModuleType SubModuleType => SubModuleType.WaypointPaste;

        public override void Update(GameTime gameTime)
        {

        }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

            _pasteCurrentClipboardWaypointFirst = settings.DefineSetting(nameof(_pasteCurrentClipboardWaypointFirst), false);
            _pasteWaypoint = settings.DefineSetting(nameof(_pasteWaypoint), new KeyBinding(Keys.None));
            _waypoint = settings.DefineSetting(nameof(_waypoint), "[&BCAJAAA=]");

            _pasteWaypoint.Value.Enabled = true;
            _pasteWaypoint.Value.Activated += PasteWaypoint_Activated;
        }

        private async void PasteWaypoint()
        {
            if (!GameService.Gw2Mumble.Info.IsGameFocused) return;

            try
            {
                _ticks = Common.Now;

                string waypoint = _waypoint.Value;
                if (_pasteCurrentClipboardWaypointFirst.Value)
                {
                    string currentContent = await ClipboardUtil.WindowsClipboardService.GetTextAsync();

                    if (!string.IsNullOrEmpty(currentContent) && Gw2ChatLink.TryParse(currentContent, out IGw2ChatLink chatLink) && chatLink is not null)
                    {
                        if (chatLink.Type == ChatLinkType.PointOfInterest)
                        {
                            waypoint = currentContent;
                        }
                    }
                }

                await Input.SendKey(Keys.Enter);

                bool isReady = false;

                var modifiers = new List<Keys>()
                {
                    Keys.RightShift,
                    Keys.LeftShift,
                    Keys.RightAlt,
                    Keys.LeftAlt,
                    Keys.RightControl,
                    Keys.LeftControl
                };

                modifiers.ForEach(e => Keyboard.Release((Key)e, true));
                await Task.Delay(5);

                for (int i = 0; i < 500; i++)
                {
                    if (GameService.Gw2Mumble.UI.IsTextInputFocused && !GameService.Input.Keyboard.KeysDown.ContainsAny(modifiers.ToArray()))
                    {
                        isReady = true;
                        break;
                    }

                    await Task.Delay(5);
                }

                if (!isReady) return;

                bool hasGuild = await ClipboardUtil.WindowsClipboardService.SetTextAsync("/g1 ");
                if (!hasGuild) return;

                await Input.SendKey(new Keys[] { Keys.LeftControl }, Keys.V, true);
                await Task.Delay(50);

                bool hasWhisper = await ClipboardUtil.WindowsClipboardService.SetTextAsync("/w ");
                if (!hasWhisper) return;

                await Input.SendKey(new Keys[] { Keys.LeftControl }, Keys.V, true);

                bool hasName = await ClipboardUtil.WindowsClipboardService.SetTextAsync(GameService.Gw2Mumble.PlayerCharacter.Name);
                if (!hasName) return;

                await Input.SendKey(new Keys[] { Keys.LeftControl }, Keys.V, true);
                await Input.SendKey(Keys.Tab, true);

                bool hasWaypoint = await ClipboardUtil.WindowsClipboardService.SetTextAsync(waypoint);
                if (!hasWaypoint) return;

                await Input.SendKey(new Keys[] { Keys.LeftControl }, Keys.V, true);

                await Input.SendKey(Keys.Enter);

                _ticks = Common.Now;
                Enabled = false;
            }
            catch
            {

            }
        }

        private void PasteWaypoint_Activated(object sender, EventArgs e)
        {
            PasteWaypoint();
        }
        protected override void Enable()
        {
            base.Enable();

            PasteWaypoint();
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

            _ = new KeybindingAssigner()
            {
                Parent = contentFlowPanel,
                Width = width - 16,
                KeyBinding = _pasteWaypoint.Value,
                KeybindChangedAction = (kb) =>
                {
                    _pasteWaypoint.Value = new()
                    {
                        ModifierKeys = kb.ModifierKeys,
                        PrimaryKey = kb.PrimaryKey,
                        Enabled = kb.Enabled,
                        IgnoreWhenInTextField = true,
                    };
                },
                SetLocalizedKeyBindingName = () => strings.PasteWaypointHotkey_Name,
                SetLocalizedTooltip = () => strings.PasteWaypointHotkey_Tooltip,
            };

            UI.WrapWithLabel(() => strings.WaypointChatcode_Name, () => strings.WaypointChatcode_Tooltip, contentFlowPanel, width - 16, new TextBox()
            {
                Text = _waypoint.Value,
                TextChangedAction = (txt) => _waypoint.Value = txt,
            });

            UI.WrapWithLabel(() => strings.PasteWaypointFromClipboard_Name, () => strings.PasteWaypointFromClipboard_Tooltip, contentFlowPanel, width - 16, new Checkbox()
            {
                Checked = _pasteCurrentClipboardWaypointFirst.Value,
                CheckedChangedAction = (b) => _pasteCurrentClipboardWaypointFirst.Value = b,
            });
        }
    }
}
