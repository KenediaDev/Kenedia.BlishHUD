using Gw2Sharp.WebApi.V2.Models;
using System;

namespace Kenedia.Modules.FashionManager.Models
{
    public class FashionTemplateGatheringToolChanged : EventArgs
    {
        public FashionTemplateGatheringToolChanged(GatheringSlotType slot, SkinGathering skin)
        {
            Slot = slot;
            Skin = skin;
        }

        public GatheringSlotType Slot { get; set; }
        public SkinGathering Skin { get; set; }
    }
}
