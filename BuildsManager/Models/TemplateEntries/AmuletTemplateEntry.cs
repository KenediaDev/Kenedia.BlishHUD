using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.BuildsManager.Utility;
using System;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.BuildsManager.Interfaces;
using Kenedia.Modules.BuildsManager.Services;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class AmuletTemplateEntry : TemplateEntry, IDisposable, IEnrichmentTemplateEntry, IStatTemplateEntry
    {
        private bool _isDisposed;
        private Stat _stat;
        private Enrichment _enrichment;

        public AmuletTemplateEntry(TemplateSlotType slot, Data data) : base(slot, data)
        {
        }

        public event EventHandler<ValueChangedEventArgs<Enrichment>> EnrichmentChanged;
        public event EventHandler<ValueChangedEventArgs<Stat>> StatChanged;

        protected override void OnDataLoaded()
        {
            base.OnDataLoaded();

            Amulet = Data?.Trinkets?.TryGetValue(92991, out Trinket accessoire) is true ? accessoire : null;
        }

        public Trinket Amulet { get; private set; } 
        
        public Stat Stat { get => _stat; private set => Common.SetProperty(ref _stat, value); }

        public Enrichment Enrichment { get => _enrichment; private set => Common.SetProperty(ref _enrichment, value); }

        protected override void OnItemChanged(object sender, ValueChangedEventArgs<BaseItem> e)
        {
            base.OnItemChanged(sender, e);

            if (e.NewValue is null)
            {
                Amulet = null;
            }
            else if (e.NewValue is Trinket trinket)
            {
                Amulet = trinket;
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;

            Stat = null;
            Enrichment = null;
            Amulet = null;
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
            else if (subSlot == TemplateSubSlotType.Enrichment)
            {
                if (obj?.Equals(Enrichment) is true)
                {
                    return false;
                }

                if (obj is null)
                {
                    Enrichment = null;
                    return true;
                }
                else if (obj is Enrichment enrichment)
                {
                    Enrichment = enrichment;
                    return true;
                }
            }

            return false;
        }
    }
}
