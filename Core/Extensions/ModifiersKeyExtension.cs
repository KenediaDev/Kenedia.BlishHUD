using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kenedia.Modules.Core.Extensions
{
    public static class ModifiersKeyExtension
    {
        public static Keys GetKey(this ModifierKeys modifier)
        {
            return modifier switch
            {
                ModifierKeys.Alt => Keys.LeftAlt,
                ModifierKeys.Ctrl => Keys.LeftControl,
                ModifierKeys.Shift => Keys.LeftShift,
                _ => Keys.None,
            };
        }
    }
}
