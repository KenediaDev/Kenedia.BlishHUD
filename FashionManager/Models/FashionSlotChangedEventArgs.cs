using Gw2Sharp.WebApi.V2.Models;
using System;

namespace Kenedia.Modules.FashionManager.Models
{
    public delegate void FashionSlotChangedEventHandler(object sender, FashionSlotChangedEventArgs e);

    public class FashionSlotChangedEventArgs : EventArgs
    {
        public FashionSlot Slot { get; }

        public FashionSubSlot SubSlot { get; }

        public Skin Skin { get; }

        public FashionSlotChangedEventArgs(FashionSlot slot, FashionSubSlot subSlot, Skin skin)
        {
            Slot = slot;
            SubSlot = subSlot;
            Skin = skin;
        }
    }
}
