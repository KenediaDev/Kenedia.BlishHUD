using Blish_HUD.Controls.Extern;
using Blish_HUD.Input;
using Microsoft.Xna.Framework.Input;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kenedia.Modules.Core.Extensions
{
    public static class KeybindExtension
    {

        public static VirtualKeyShort[] ModKeyMapping { get; } =
        {
            (VirtualKeyShort)0,
            VirtualKeyShort.CONTROL,
            VirtualKeyShort.MENU,
            (VirtualKeyShort)0,
            VirtualKeyShort.LSHIFT,
        };

        public static async Task<bool> PerformPress(this KeyBinding keybinding, int keyDelay = 0, bool triggerSystem = true, CancellationToken? cancellationToken = null)
        {
            ModifierKeys mods = keybinding.ModifierKeys;

            foreach (ModifierKeys mod in Enum.GetValues(typeof(ModifierKeys)))
            {
                if (mod != ModifierKeys.None && mods.HasFlag(mod))
                {
                    Blish_HUD.Controls.Intern.Keyboard.Press(ModKeyMapping[(int)mod], false);
                    if (cancellationToken is not null && cancellationToken.Value.IsCancellationRequested) { return false; }
                }
            }

            Blish_HUD.Controls.Intern.Keyboard.Stroke((VirtualKeyShort)keybinding.PrimaryKey, false);

            // Trigger other Modules such as GatherTools
            if (triggerSystem)
            {
                Blish_HUD.Controls.Intern.Keyboard.Stroke((VirtualKeyShort)keybinding.PrimaryKey, true);               
            }

            if (cancellationToken is not null) await Task.Delay(keyDelay, cancellationToken.Value);

            foreach (ModifierKeys mod in Enum.GetValues(typeof(ModifierKeys)))
            {
                if (mod != ModifierKeys.None && mods.HasFlag(mod))
                {
                    Blish_HUD.Controls.Intern.Keyboard.Release(ModKeyMapping[(int)mod], false);
                    if (cancellationToken is not null && cancellationToken.Value.IsCancellationRequested) { return false; }
                }
            }

            return true;
        }
    }
}
