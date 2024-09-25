using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.BuildsManager.Utility;
using System;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.BuildsManager.Interfaces;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class AmuletTemplateEntry : TemplateEntry, IDisposable, IEnrichmentTemplateEntry, IStatTemplateEntry
    {
        private bool _isDisposed;
        private Stat _stat;
        private Enrichment _enrichment;

        public AmuletTemplateEntry(TemplateSlotType slot) : base(slot)
        {
        }

        public event EventHandler<ValueChangedEventArgs<Enrichment>> EnrichmentChanged;
        public event EventHandler<ValueChangedEventArgs<Stat>> StatChanged;

        public Trinket Amulet { get; private set; } = BuildsManager.Data?.Trinkets?.TryGetValue(92991, out Trinket accessoire) is true ? accessoire : null;

        public Stat Stat { get => _stat; private set => Common.SetProperty(ref _stat, value); }

        public Enrichment Enrichment { get => _enrichment; private set => Common.SetProperty(ref _enrichment, value); }

        protected override void OnItemChanged(object sender, ValueChangedEventArgs<BaseItem> e)
        {
            base.OnItemChanged(sender, e);

            if (e.NewValue is Trinket trinket)
            {
                Amulet = trinket;
            }
        }

        public override byte[] AddToCodeArray(byte[] array)
        {
            return array.Concat(new byte[]
            {
                Stat?.MappedId ?? 0,
                Enrichment ?.MappedId ?? 0,
            }).ToArray();
        }

        public override byte[] GetFromCodeArray(byte[] array)
        {
            int newStartIndex = 2;

            if (array is not null && array.Length > 0)
            {
                Stat = BuildsManager.Data.Stats.Items.Where(e => e.Value.MappedId == array[0]).FirstOrDefault().Value;
                Enrichment = BuildsManager.Data.Enrichments.Items.Where(e => e.Value.MappedId == array[1]).FirstOrDefault().Value;
            }

            return array is not null && array.Length > 0 ? GearTemplateCode.RemoveFromStart(array, newStartIndex) : array;
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
            else if (subSlot == TemplateSubSlotType.Infusion1)
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
