using Gw2Sharp.WebApi.V2.Models;

namespace Kenedia.Modules.FashionManager.Models
{
    public class GatheringFashionSlot : FashionTemplateSlot
    {
        public override void Set(FashionSubSlot subSlot, Skin skin)
        {
            switch (subSlot)
            {
                case FashionSubSlot.Item:
                    Skin = skin;
                    break;
            }
        }
    }
}
