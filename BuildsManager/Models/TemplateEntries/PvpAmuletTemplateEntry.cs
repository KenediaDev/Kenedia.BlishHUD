using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using System;
using Kenedia.Modules.BuildsManager.Interfaces;
using Kenedia.Modules.BuildsManager.Services;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class PvpAmuletTemplateEntry : TemplateEntry, IDisposable, IRuneTemplateEntry
    {
        private bool _isDisposed;
        private PvpAmulet _pvpAmulet;
        private Rune _rune;

        public PvpAmuletTemplateEntry(TemplateSlotType slot, Data data) : base(slot, data)
        {
        }

        public PvpAmulet PvpAmulet { get => _pvpAmulet; private set => Common.SetProperty(ref _pvpAmulet, value); }

        public Rune Rune { get => _rune; private set => Common.SetProperty(ref _rune, value); }

        override protected void OnItemChanged(object sender, ValueChangedEventArgs<BaseItem> e)
        {
            base.OnItemChanged(sender, e);

            if (e.NewValue is null)
            {
                PvpAmulet = null;
            }
            else if (e.NewValue is PvpAmulet pvpAmulet)
            {
                PvpAmulet = pvpAmulet;
            }
        }
       
        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            PvpAmulet = null;
            Rune = null;
        }

        public override bool SetValue(TemplateSlotType slot, TemplateSubSlotType subSlot, object obj)
        {
            if (subSlot == TemplateSubSlotType.Item)
            {
                if (obj?.Equals(Item) is true)
                {
                    return false;
                }

                if (obj is null)
                {
                    Item = null;
                    return true;
                }
                else if (obj is PvpAmulet pvpAmulet)
                {
                    Item = pvpAmulet;
                    return true;
                }
            }
            else if (subSlot == TemplateSubSlotType.Rune)
            {
                if (obj?.Equals(Rune) is true)
                {
                    return false;
                }

                if (obj is null)
                {
                    Rune = null;
                    return true;
                }
                else if (obj is Rune rune)
                {
                    Rune = rune;
                    return true;
                }
            }

            return false;
        }
    }
}
