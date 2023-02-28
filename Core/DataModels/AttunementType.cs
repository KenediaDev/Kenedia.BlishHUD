using System;
using System.Collections.Generic;
using System.Text;

namespace Kenedia.Modules.Core.DataModels
{
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
