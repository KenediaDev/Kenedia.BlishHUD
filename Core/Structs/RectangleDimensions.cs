namespace Kenedia.Modules.Core.Structs
{
    public struct RectangleDimensions
    {
        public RectangleDimensions(int num)
        {
            Left = num;
            Top = num;
            Right = num;
            Bottom = num;
        }

        public RectangleDimensions(int horizontal, int vertical)
        {
            Left = horizontal;
            Top = vertical;
            Right = horizontal;
            Bottom = vertical;
        }

        public RectangleDimensions(int left, int top, int right, int bottom)
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

        public int Vertical => Top + Bottom;

        public int Horizontal => Left + Right;
    }
}
