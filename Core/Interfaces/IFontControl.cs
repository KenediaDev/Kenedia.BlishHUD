using MonoGame.Extended.BitmapFonts;

namespace Kenedia.Modules.Core.Interfaces
{
    public interface IFontControl
    {
        BitmapFont Font { get; set; }

        string Text { get; set; }
    }
}
