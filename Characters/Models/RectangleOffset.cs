namespace Kenedia.Modules.Characters.Models
{
    public class RectangleOffset
    {
        public RectangleOffset(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public int Left { get; set; }

        public int Top { get; set; }

        public int Right { get; set; }

        public int Bottom { get; set; }
    }
}
