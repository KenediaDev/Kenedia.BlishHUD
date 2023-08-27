using Gw2Sharp.WebApi.V2.Models;
using System;

namespace Kenedia.Modules.FashionManager.Models
{
    public class FashionTemplateWeaponChanged : EventArgs
    {
        public FashionTemplateWeaponChanged(WeaponSlotType slot, SkinWeapon skin)
        {
            Slot = slot;
            Skin = skin;
        }

        public WeaponSlotType Slot { get; set; }
        public SkinWeapon Skin { get; set; }
    }
}
