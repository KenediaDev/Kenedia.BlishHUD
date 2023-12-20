using Blish_HUD.Controls.Extern;
using Blish_HUD.Controls.Intern;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using System.Threading.Tasks;
using InputKeyboard = Blish_HUD.Controls.Intern.Keyboard;
using InputMouse = Blish_HUD.Controls.Intern.Mouse;

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

        public static async Task ClickMouse(MouseButton mouseButton, Point pos, int clicks = 1, bool sendToSystem = false, bool moveMouse = false)
        {
            await ClickMouse(mouseButton, pos.X, pos.Y, clicks, sendToSystem, moveMouse);
        }

        public static async Task ClickMouse(MouseButton mouseButton = MouseButton.LEFT, int xPos = -1, int yPos = -1, int clicks = 1, bool sendToSystem = false, bool moveMouse = false)
        {
            if (moveMouse)
            {
                InputMouse.SetPosition(xPos, yPos, sendToSystem);
                await Task.Delay(25);
            }

            for (int i = 0; i < clicks; i++)
            {
                InputMouse.Press(mouseButton, xPos, yPos, sendToSystem);
                await Task.Delay(25);
            }
        }

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
            foreach (ModifierKeys mod in modifiers.Select(v => (ModifierKeys)v))
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
            foreach (var mod in modifiers)
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
