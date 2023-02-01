using Blish_HUD;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kenedia.Modules.Core.Utility
{
    public class Common
    {
        public static double Now()
        {
            return GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds;
        }
    }
}
