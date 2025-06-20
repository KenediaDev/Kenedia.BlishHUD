﻿using Newtonsoft.Json;

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

        [JsonIgnore]
        public int Vertical => Top + Bottom;

        [JsonIgnore]
        public int Horizontal => Left + Right;

        public override string ToString()
        {
            return $"Left: {Left}, Top: {Top}, Right: {Right}, Bottom: {Bottom}, Vertical: {Vertical}, Horizontal: {Horizontal}";
        }
    }
}
