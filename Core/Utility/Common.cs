using Blish_HUD;

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
