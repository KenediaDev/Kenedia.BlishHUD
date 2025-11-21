using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using System;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.BuildsManager.Interfaces;
using Kenedia.Modules.BuildsManager.Services;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class AccessoireTemplateEntry : TemplateEntry, IDisposable, ISingleInfusionTemplateEntry, IStatTemplateEntry
    {
        private bool _isDisposed;
        private Stat _stat;
        private Infusion _infusion;

        public AccessoireTemplateEntry(TemplateSlotType slot, Data data) : base(slot, data)
        {
        }

        protected override void OnDataLoaded()
        {
            base.OnDataLoaded();

            Accessoire = Data?.Trinkets.TryGetValue(81908, out Trinket accessoire) is true ? accessoire : null;
        }

        public Trinket Accessoire { get; private set; }

        public Stat Stat { get => _stat; private set => Common.SetProperty(ref _stat, value); }

        public Infusion Infusion1 { get => _infusion; private set => Common.SetProperty(ref _infusion, value); }

        protected override void OnItemChanged(object sender, ValueChangedEventArgs<BaseItem> e)
        {
            base.OnItemChanged(sender, e);

            if (e.NewValue is null)
            {
                Accessoire = null;
            }
            else if (e.NewValue is Trinket trinket)
            {
                Accessoire = trinket;
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            Stat = null;
            Infusion1 = null;
            Accessoire = null;
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

            return false;
        }
    }
}
