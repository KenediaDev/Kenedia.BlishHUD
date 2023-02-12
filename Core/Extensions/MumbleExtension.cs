using Blish_HUD;
using Blish_HUD.Gw2Mumble;
using Gw2Sharp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kenedia.Modules.Core.Extensions
{
    public static class MumbleExtension
    {

        public static bool IsCommonMap(this CurrentMap map)
        {
            return map.Type is MapType.Public or MapType.PublicMini or MapType.Tutorial;
        }

        public static bool IsPvpMap(this CurrentMap map)
        {
            List<int> pvpMaps = new()
            {
                549,
                1305,
                1171,
                554,
                795,
                1163,
                900,
                894,
                875,
                984,
                1011,
                1201,
                1328,
                1275,
                1200,
            };

            return pvpMaps.Contains(map.Id);
        }

        public static bool IsWvWMap(this CurrentMap map)
        {
            List<int> wvwpMaps = new()
            {
                38,
                95,
                96,
                1099,
                899,
                968,
            };

            return wvwpMaps.Contains(map.Id);
        }

        public static bool IsCompetitiveMap(this CurrentMap map)
        {
            return IsPvpMap(map) || IsWvWMap(map);
        }
    }
}
