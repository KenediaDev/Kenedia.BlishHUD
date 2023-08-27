﻿using Gw2Sharp.WebApi.V2.Models;
using System;

namespace Kenedia.Modules.FashionManager.Models
{
    public class FashionTemplateArmorChanged : EventArgs
    {
        public FashionTemplateArmorChanged(ArmorSlotType slot, SkinArmor skin)
        {
            Slot = slot;
            Skin = skin;
        }

        public ArmorSlotType Slot { get; set; }
        public SkinArmor Skin { get; set; }
    }
}
