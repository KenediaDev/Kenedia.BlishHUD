using Gw2Sharp.WebApi.V2.Models;

namespace Kenedia.Modules.FashionManager.Models
{
    public class MountFashionSlot
    {
        public MountSkin Mount { get; protected set; }

        public void Set(FashionSubSlot subSlot, MountSkin skin)
        {
            switch (subSlot)
            {
                case FashionSubSlot.Item:
                    Mount = skin;
                    break;
            }
        }
    }
}
