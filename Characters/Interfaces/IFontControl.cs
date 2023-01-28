using MonoGame.Extended.BitmapFonts;

namespace Kenedia.Modules.Characters.Interfaces
{
    public interface IFontControl
    {
        BitmapFont Font { get; set; }

        string Text { get; set; }
    }
}
