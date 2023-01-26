using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.Characters.Interfaces
{
    public interface IFontControl
    {
        BitmapFont Font { get; set; }

        string Text { get; set; }
    }
}
