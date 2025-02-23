using Gw2Sharp.WebApi.V2.Models;

namespace Kenedia.Modules.FashionManager.Models
{
    public abstract class FashionTemplateSlot
    {
        public Skin Skin { get; protected set; }

        public virtual void Set(FashionSubSlot subSlot, Skin skin)
        {

        }
    }
}
