using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using System;
using Kenedia.Modules.BuildsManager.Interfaces;
using Kenedia.Modules.BuildsManager.Services;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class 
        RingTemplateEntry : TemplateEntry, IDisposable, ITripleInfusionTemplateEntry, IStatTemplateEntry
    {
        private bool _isDisposed;
        private Infusion _infusion1;
        private Infusion _infusion2;
        private Infusion _infusion3;
        private Stat _stat;

        public RingTemplateEntry(TemplateSlotType slot, Data data) : base(slot, data)
        {
        }

        override protected void OnDataLoaded()
        {
            base.OnDataLoaded();

            Ring = Data?.Trinkets.TryGetValue(91234, out Trinket ring) is true ? ring : null;
        }

        public Trinket Ring { get; private set; }

        public Infusion Infusion1 { get => _infusion1; private set => Common.SetProperty(ref _infusion1, value); }

        public Infusion Infusion2 { get=> _infusion2; private set => Common.SetProperty(ref _infusion2, value); }

        public Infusion Infusion3 { get => _infusion3; private set => Common.SetProperty(ref _infusion3, value); }

        public Stat Stat { get => _stat; private set => Common.SetProperty(ref _stat, value); }

        protected override void OnItemChanged(object sender, ValueChangedEventArgs<BaseItem> e)
        {
            base.OnItemChanged(sender, e);

            if (e.NewValue is null)
            {
                Ring = null;
            }
            else if (e.NewValue is Trinket trinket)
            {
                Ring = trinket;
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            
            Ring = null;
            Infusion1 = null;
            Infusion2 = null;
            Infusion3 = null;
            Stat = null;
        }

        public override bool SetValue(TemplateSlotType slot, TemplateSubSlotType subSlot, object obj)
        {
            if (subSlot == TemplateSubSlotType.Item)
            {
                //Do nothing
            }
            else if (subSlot == TemplateSubSlotType.Stat)
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
            else if (subSlot == TemplateSubSlotType.Infusion1)
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
            else if (subSlot == TemplateSubSlotType.Infusion2)
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
            else if (subSlot == TemplateSubSlotType.Infusion3)
            {
                if (obj?.Equals(Infusion3) is true)
                {
                    return false;
                }

                if (obj is null)
                {
                    Infusion3 = null;
                    return true;
                }
                else if (obj is Infusion infusion)
                {
                    Infusion3 = infusion;
                    return true;
                }
            }

            return false;
        }
    }
}
