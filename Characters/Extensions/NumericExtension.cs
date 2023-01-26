using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
