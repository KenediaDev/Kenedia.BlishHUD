using Blish_HUD.Controls.Extern;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InputKeyboard = Blish_HUD.Controls.Intern.Keyboard;

namespace Kenedia.Modules.Core.Utility
{
    public static class Input
    {
        public static VirtualKeyShort[] ModKeyMapping { get; } =
        {
            (VirtualKeyShort)0,
            VirtualKeyShort.LCONTROL,
            VirtualKeyShort.LMENU,
            (VirtualKeyShort)0,
            VirtualKeyShort.LSHIFT,
        };

        public static async Task SendKey(Keys key, bool sendToSystem = false)
        {
            InputKeyboard.Stroke((VirtualKeyShort)key, sendToSystem);
        }

        public static async Task SendKey(KeyBinding keybinding, bool sendToSystem = false, int delay = 25)
        {
            await SendKey(keybinding.PrimaryKey, keybinding.ModifierKeys, sendToSystem, delay);
        }

        public static async Task SendKey(ModifierKeys modifier, Keys key, bool sendToSystem = false, int delay = 25)
        {
            await SendKey(key, modifier, sendToSystem, delay);
        }

        public static async Task SendKey(Keys key, ModifierKeys modifier, bool sendToSystem = false, int delay = 25)
        {
            var modifiers = modifier.GetFlags();
            foreach(ModifierKeys mod in modifiers.Select(v => (ModifierKeys)v))
            {
                InputKeyboard.Press(ModKeyMapping[(int)mod], sendToSystem);
            }

            await Task.Delay(delay);

            InputKeyboard.Stroke((VirtualKeyShort)key, sendToSystem);

            await Task.Delay(delay);

            foreach (ModifierKeys mod in modifiers.Select(v => (ModifierKeys)v))
            {
                InputKeyboard.Release(ModKeyMapping[(int)mod], sendToSystem);
            }
        }

        public static async Task SendKey(Keys[] modifiers, Keys key, bool sendToSystem = false, int delay = 25)
        {
            foreach(var mod in modifiers)
            {
                InputKeyboard.Press((VirtualKeyShort)mod, sendToSystem);
            }

            await Task.Delay(delay);

            InputKeyboard.Stroke((VirtualKeyShort)key, sendToSystem);

            await Task.Delay(delay);

            foreach (var mod in modifiers)
            {
                InputKeyboard.Release((VirtualKeyShort)mod, sendToSystem);
            }
        }

        public static async Task Press(this KeyBinding keybinding, bool sendToSystem = false, int delay = 25)
        {
            await SendKey(keybinding, sendToSystem, delay);
        }

        public static async Task Press(this SettingEntry<KeyBinding> keybinding, bool sendToSystem = false, int delay = 25)
        {
            await SendKey(keybinding.Value, sendToSystem, delay);
        }
    }
}
