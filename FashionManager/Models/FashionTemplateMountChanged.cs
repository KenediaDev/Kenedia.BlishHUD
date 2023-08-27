using Gw2Sharp.WebApi.V2.Models;
using System;

namespace Kenedia.Modules.FashionManager.Models
{
    public class FashionTemplateMountChanged : EventArgs
    {
        public FashionTemplateMountChanged(MountSlotType slot, MountSkin skin)
        {
            Slot = slot;
            Skin = skin;
        }

        public MountSlotType Slot { get; set; }
        public MountSkin Skin { get; set; }
    }
}
