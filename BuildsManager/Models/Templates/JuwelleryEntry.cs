using System.ComponentModel;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using System.Linq;
using Kenedia.Modules.Core.Models;
using System.Data;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class JuwelleryEntry : GearTemplateEntry
    {
        private Enrichment _enrichment;
        private Infusion _infusion;
        private Infusion _infusion2;
        private Infusion _infusion3;

        private ObservableList<int> _infusionIds = new();

        public JuwelleryEntry()
        {
            _enrichmentIds.PropertyChanged += EnrichmentIds_Changed;
        }

        public ObservableList<int> InfusionIds
        {
            get => _infusionIds;
            set
            {
                var temp = _infusionIds;
                if (Common.SetProperty(ref _infusionIds, value))
                {
                    if (temp != null) temp.PropertyChanged -= InfusionIds_Changed;
                    if (_infusionIds != null)
                    {
                        _infusionIds.PropertyChanged += InfusionIds_Changed;
                        OnPropertyChanged(this, new(nameof(InfusionIds)));
                    }
                }
            }
        }

        private void InfusionIds_Changed(object sender, PropertyChangedEventArgs e)
        {
            if (_infusionIds != null && _infusionIds.Count > 0) _infusion = BuildsManager.Data.Infusions.Values.Where(e => e.MappedId == _infusionIds[0]).FirstOrDefault();
            if (_infusionIds != null && _infusionIds.Count > 1) _infusion2 = BuildsManager.Data.Infusions.Values.Where(e => e.MappedId == _infusionIds[1]).FirstOrDefault();
            if (_infusionIds != null && _infusionIds.Count > 2) _infusion3 = BuildsManager.Data.Infusions.Values.Where(e => e.MappedId == _infusionIds[2]).FirstOrDefault();

            OnPropertyChanged(sender, e);
        }

        public Infusion Infusion
        {
            get => InfusionIds == null || InfusionIds.Count < 1 ? null : _infusion;
            set
            {
                if (Common.SetProperty(ref _infusion, value) && InfusionIds != null)
                {
                    InfusionIds[0] = _infusion?.MappedId ?? -1;
                }
            }
        }

        public Infusion Infusion2
        {
            get => InfusionIds == null || InfusionIds.Count < 2 ? null : _infusion2;
            set
            {
                if (InfusionIds == null || InfusionIds.Count < 2) return;
                if (Common.SetProperty(ref _infusion2, value) && InfusionIds != null)
                {
                    InfusionIds[1] = _infusion2?.MappedId ?? -1;
                }
            }
        }

        public Infusion Infusion3
        {
            get => InfusionIds == null || InfusionIds.Count < 3 ? null : _infusion3;
            set
            {
                if (InfusionIds == null || InfusionIds.Count < 3) return;
                if (Common.SetProperty(ref _infusion3, value) && InfusionIds != null)
                {
                    InfusionIds[2] = _infusion3?.MappedId ?? -1;
                }
            }
        }

        private ObservableList<int> _enrichmentIds = new();

        public ObservableList<int> EnrichmentIds
        {
            get => _enrichmentIds;
            set
            {
                var temp = _enrichmentIds;
                if (Common.SetProperty(ref _enrichmentIds, value))
                {
                    if (temp != null) temp.PropertyChanged -= EnrichmentIds_Changed;
                    if (_enrichmentIds != null)
                    {
                        _enrichmentIds.PropertyChanged += EnrichmentIds_Changed;
                        OnPropertyChanged(this, new(nameof(EnrichmentIds)));
                    }
                }
            }
        }

        private void EnrichmentIds_Changed(object sender, PropertyChangedEventArgs e)
        {
            if (_enrichmentIds != null && _enrichmentIds.Count > 0)
                _enrichment = BuildsManager.Data.Enrichments.Values.Where(e => e.MappedId == _enrichmentIds[0]).FirstOrDefault();

            OnPropertyChanged(sender, e);
        }

        public Enrichment Enrichment
        {
            get => EnrichmentIds.Count < 1 ? null : _enrichment;
            set
            {
                if (Common.SetProperty(ref _enrichment, value))
                {
                    EnrichmentIds[0] = _enrichment?.MappedId ?? -1;
                }
            }
        }

        public override void OnSlotApply()
        {
            base.OnSlotApply();

            EnrichmentIds = Slot switch
            {
                TemplateSlot.Amulet => new() { -1 },
                _ => null,
            };

            InfusionIds = Slot switch
            {
                TemplateSlot.Ring_1 or TemplateSlot.Ring_2 => new() { -1, -1, -1, },
                TemplateSlot.Back or TemplateSlot.Aquatic or TemplateSlot.AltAquatic => new() { -1, -1 },
                TemplateSlot.Amulet => null,
                _ => new() { -1 },
            };
        }

        public override string ToCode()
        {
            string enrichmentsOrInfusions = string.Join("|", EnrichmentIds ?? InfusionIds);
            return $"[{MappedId}|{Stat?.MappedId ?? -1}|{enrichmentsOrInfusions}]";
        }

        public override void FromCode(string code)
        {
            string[] parts = code.Split('|');

            if (Slot is TemplateSlot.Ring_1 or TemplateSlot.Ring_2)
            {
                if (parts.Length != 5) return;
            }
            else if (Slot is TemplateSlot.Back)
            {
                if (parts.Length != 4) return;
            }
            else
            {
                if (parts.Length != 3) return;
            }

            MappedId = int.TryParse(parts[0], out int mappedId) ? mappedId : -1;
            Stat = int.TryParse(parts[1], out int stat) ? BuildsManager.Data.Stats.Where(e => e.Value.MappedId == stat).FirstOrDefault().Value : null;

            if (Slot == TemplateSlot.Back)
            {
                InfusionIds[0] = int.TryParse(parts[2], out int infusion1) ? infusion1 : -1;
                InfusionIds[1] = int.TryParse(parts[3], out int infusion2) ? infusion2 : -1;
            }
            else if (Slot is TemplateSlot.Ring_1 or TemplateSlot.Ring_2)
            {
                InfusionIds[0] = int.TryParse(parts[2], out int infusion1) ? infusion1 : -1;
                InfusionIds[1] = int.TryParse(parts[3], out int infusion2) ? infusion2 : -1;
                InfusionIds[2] = int.TryParse(parts[4], out int infusion3) ? infusion3 : -1;
            }
            else if (Slot is TemplateSlot.Amulet)
            {
                EnrichmentIds[0] = int.TryParse(parts[2], out int enrichment) ? enrichment : -1;
                Enrichment = BuildsManager.Data.Enrichments.Values.Where(e => e.MappedId == enrichment).FirstOrDefault();
            }
            else
            {
                InfusionIds[0] = int.TryParse(parts[2], out int infusion1) ? infusion1 : -1;
            }

            if (MappedId > -1)
            {
                Item = Slot == TemplateSlot.Back ?
                    BuildsManager.Data.Backs.Values.Where(e => e.MappedId == mappedId).FirstOrDefault() :
                    BuildsManager.Data.Trinkets.Values.Where(e => e.MappedId == mappedId).FirstOrDefault();
            }
        }

        public override void Reset()
        {
            base.Reset();

            ResetInfusion();
            ResetEnrichment();
        }

        public void ResetInfusion(int? index = null)
        {
            switch (index)
            {
                case null:
                    Infusion = null;
                    Infusion2 = null;
                    Infusion3 = null;
                    break;

                case 0:
                    Infusion = null;
                    break;

                case 1:
                    Infusion2 = null;
                    break;

                case 2:
                    Infusion3 = null;
                    break;
            }
        }

        public void ResetEnrichment()
        {
            Enrichment = null;
        }
    }
}
