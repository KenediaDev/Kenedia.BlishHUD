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

namespace Kenedia.Modules.QoL.SubModules.WaypointPaste
{
    // Discord Suggestion: https://discord.com/channels/531175899588984842/1064906680904724621/1072599804502364182
    public class WaypointPaste : SubModule
    {
        private double _ticks;
        private string _waypoint = "[&BCAJAAA=]";
        private SettingEntry<KeyBinding> _pasteWaypoint;

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

            _pasteWaypoint = settings.DefineSetting(nameof(_pasteWaypoint), new KeyBinding(ModifierKeys.Shift | ModifierKeys.Alt, Keys.B));

            _pasteWaypoint.Value.Enabled = true;
            _pasteWaypoint.Value.Activated += PasteWaypoint_Activated;
        }

        private async void PasteWaypoint()
        {
            if (!GameService.Gw2Mumble.Info.IsGameFocused) return;

            try
            {
                _ticks = Common.Now();
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

                bool hasWaypoint = await ClipboardUtil.WindowsClipboardService.SetTextAsync(_waypoint);
                if (!hasWaypoint) return;

                await Input.SendKey(new Keys[] { Keys.LeftControl }, Keys.V, true);

                await Input.SendKey(Keys.Enter);

                _ticks = Common.Now();
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
    }
}
