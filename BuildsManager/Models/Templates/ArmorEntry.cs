using System.ComponentModel;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using System.Linq;
using Kenedia.Modules.Core.Models;
using System.Data;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class ArmorEntry : JuwelleryEntry
    {
        private Rune _rune;
        private ObservableList<int> _runeIds = new() { -1 };

        public ArmorEntry()
        {
            _runeIds.PropertyChanged += RuneIds_Changed;
        }

        public ObservableList<int> RuneIds
        {
            get => _runeIds;
            set
            {
                var temp = _runeIds;
                if (Common.SetProperty(ref _runeIds, value))
                {
                    if (temp != null) temp.PropertyChanged -= RuneIds_Changed;
                    if (_runeIds != null)
                    {
                        _runeIds.PropertyChanged += RuneIds_Changed;
                    }
                }
            }
        }

        private void RuneIds_Changed(object sender, PropertyChangedEventArgs e)
        {
            if (_runeIds != null && _runeIds.Count > 0) _rune = BuildsManager.Data.PveRunes.Values.Where(e => e.MappedId == _runeIds[0]).FirstOrDefault();

            OnPropertyChanged(sender, e);
        }

        public Rune Rune
        {
            get => _rune; set
            {
                if (Common.SetProperty(ref _rune, value))
                {
                    _runeIds[0] = _rune?.MappedId ?? -1;
                }
            }
        }

        public override void OnSlotApply()
        {
            base.OnSlotApply();
        }

        public override string ToCode()
        {
            string infusions = string.Join("|", InfusionIds);
            string runes = string.Join("|", RuneIds);

            return $"[{MappedId}|{Stat?.MappedId ?? -1}|{infusions}|{runes}]";
        }

        public override void FromCode(string code)
        {
            string[] parts = code.Split('|');

            if (parts.Length == 4)
            {
                MappedId = int.TryParse(parts[0], out int mappedId) ? mappedId : -1;
                Stat = int.TryParse(parts[1], out int stat) ? BuildsManager.Data.Stats.Where(e => e.Value.MappedId == stat).FirstOrDefault().Value : null;
                InfusionIds[0] = int.TryParse(parts[2], out int infusionId) ? infusionId : 0;
                RuneIds[0] = int.TryParse(parts[3], out int runeId) ? runeId : 0;

                if (MappedId > -1)
                {
                    Item = BuildsManager.Data.Armors.Values.Where(e => e.MappedId == mappedId).FirstOrDefault();
                }
            }
        }

        public override void ResetUpgrades()
        {
            base.ResetUpgrades();
            ResetRune();
        }

        public void ResetRune()
        {
            Rune = null;
        }
    }
}
