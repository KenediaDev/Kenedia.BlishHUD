using Blish_HUD;
using System;
using System.Runtime.CompilerServices;

namespace Kenedia.Modules.Core.Utility
{
    public class Common
    {
        public static double Now()
        {
            return GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds;
        }

        public static bool SetProperty<T>(ref T property, T newValue, bool triggerOnUpdate = true, Action OnUpdated = null)
        {
            if (Equals(property, newValue))
            {
                return false;
            }

            property = newValue;
            if(triggerOnUpdate) OnUpdated?.Invoke();

            return true;
        }
    }
}
