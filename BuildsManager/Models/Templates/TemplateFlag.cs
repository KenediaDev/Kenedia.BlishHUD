using System;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    [Flags]
    public enum TemplateFlag
    {
        None = 0,
        Favorite = 1 << 0,
        Pve = 1 << 1,
        Pvp = 1 << 2,
        Wvw = 1 << 3,
        OpenWorld = 1 << 4,
        Dungeons = 1 << 5,
        Fractals = 1 << 6,
        Raids = 1 << 7,
        Power = 1 << 8,
        Condition = 1 << 9,
        Tank = 1 << 10,
        Support = 1 << 11,
        Heal = 1 << 12,
        Quickness = 1 << 13,
        Alacrity = 1 << 14,
        WorldCompletion = 1 << 15,
        Leveling = 1 << 16,
        Farming = 1 << 17,
    }
}
