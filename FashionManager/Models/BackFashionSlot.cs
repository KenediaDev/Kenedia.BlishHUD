using Gw2Sharp.WebApi.V2.Models;

namespace Kenedia.Modules.FashionManager.Models
{
    public class BackFashionSlot : FashionTemplateSlot
    {
        public InfusionSkin Infusion { get; protected set; }

        public override void Set(FashionSubSlot subSlot, Skin skin)
        {
            switch (subSlot)
            {
                case FashionSubSlot.Item:
                    Skin = skin;
                    break;

                case FashionSubSlot.Infusion:
                    Infusion = skin as InfusionSkin;
                    break;

            }
        }
    }
}
