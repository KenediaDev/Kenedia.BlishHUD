using System;

namespace Kenedia.Modules.Core.DataModels
{
    public enum AttunementSlotType
    {
        Main,
        Alt,
    }

    [Flags]
    public enum AttunementType
    {
        None,
        Fire,
        Water,
        Air = 4,
        Earth = 8,
    }
}
