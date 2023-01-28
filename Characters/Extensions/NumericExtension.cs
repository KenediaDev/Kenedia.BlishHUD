using System;

namespace Kenedia.Modules.Characters.Extensions
{
    public static class NumericExtension
    {
        public static int Difference(this int nr1, int nr2)
        {
            return Math.Abs(nr1 - nr2);
        }
    }
}
