using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using System;
using Kenedia.Modules.BuildsManager.Interfaces;
using Kenedia.Modules.BuildsManager.Services;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class BackTemplateEntry : TemplateEntry, IDisposable, IStatTemplateEntry, IDoubleInfusionTemplateEntry
    {
        private bool _isDisposed;

        public BackTemplateEntry(TemplateSlotType slot, Data data) : base(slot, data)
        {
        }

        protected override void OnDataLoaded()
        {
            base.OnDataLoaded();

            Back = Data?.Backs?.TryGetValue(74155, out Trinket back) is true ? back : null;
        }

        public Stat Stat { get; private set => Common.SetProperty(ref field, value); }

        public Trinket Back { get; private set; }

        public Infusion Infusion1 { get; private set => Common.SetProperty(ref field, value); }

        public Infusion Infusion2 { get; private set => Common.SetProperty(ref field, value); }

        protected override void OnItemChanged(object sender, ValueChangedEventArgs<BaseItem> e)
        {
            base.OnItemChanged(sender, e);

            if (e.NewValue is null)
            {
                Back = null;
            }
            else if (e.NewValue is Trinket trinket)
            {
                Back = trinket;
            }
        }

        public void Dispose()
        {
            if(_isDisposed) return;
            _isDisposed = true;

            Stat = null;
            Infusion1 = null;
            Infusion2 = null;
            Back = null;
        }

        public override bool SetValue(TemplateSlotType slot, TemplateSubSlotType subSlot, object obj)
        {
            if (subSlot is TemplateSubSlotType.Item)
            {
                //Do nothing
            }
            else if (subSlot is TemplateSubSlotType.Stat)
            {
                if (obj?.Equals(Stat) is true)
                {
                    return false;
                }

                if (obj is null)
                {
                    Stat = null;
                    return true;
                }
                else if (obj is Stat stat)
                {
                    Stat = stat;
                    return true;
                }
            }
            else if (subSlot is TemplateSubSlotType.Infusion1)
            {
                if (obj?.Equals(Infusion1) is true)
                {
                    return false;
                }

                if (obj is null)
                {
                    Infusion1 = null;
                    return true;
                }
                else if (obj is Infusion infusion)
                {
                    Infusion1 = infusion;
                    return true;
                }
            }
            else if (subSlot is TemplateSubSlotType.Infusion2)
            {
                if (obj?.Equals(Infusion2) is true)
                {
                    return false;
                }

                if (obj is null)
                {
                    Infusion2 = null;
                    return true;
                }
                else if (obj is Infusion infusion)
                {
                    Infusion2 = infusion;
                    return true;
                }
            }

            return false;
        }
    }
}
