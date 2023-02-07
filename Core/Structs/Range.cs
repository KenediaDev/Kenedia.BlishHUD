using System;
using System.Collections.Generic;
using System.Text;

namespace Kenedia.Modules.Core.Structs
{
    public struct Range
    {
        public Range(int start, int end)
        {
            Start = start;
            End = end;
        }

        public int Start { get; set; }

        public int End { get; set; }
    }
}
