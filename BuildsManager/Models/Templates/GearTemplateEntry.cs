using System.ComponentModel;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using System;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using System.Linq;
using Kenedia.Modules.Core.Extensions;
using Gw2Sharp.WebApi.V2.Models;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class GearTemplateEntry : BaseTemplateEntry
    {
        private Stat _stat;
        private ItemRarity _itemRarity;

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnPropertyChanged); }

        public ItemRarity ItemRarity { get => _itemRarity; set => Common.SetProperty(ref _itemRarity, value, OnRarityChanged); }

        private void OnRarityChanged(object sender, PropertyChangedEventArgs e)
        {
            int? currentStatId = Stat?.Id;

            if (Stat != null && (Item == null || !(Item as EquipmentItem).StatChoices.Contains(Stat.Id)))
            {
                if (BuildsManager.Data.StatMap.TryFind(e => e.Stat == Stat.EquipmentStatType, out var statMap))
                {
                    var choices = (Item as EquipmentItem)?.StatChoices;

                    if (choices != null && choices.TryFind(statMap.Ids.Contains, out int newStatId))
                    {
                        Stat = BuildsManager.Data.Stats[newStatId];
                        return;
                    }
                }

                Stat = null;
            }
            else
            {
                OnPropertyChanged(sender, e);
            }
        }

        public override void OnItemChanged()
        {
            base.OnItemChanged();

            ItemRarity = Item?.Rarity ?? ItemRarity.Unknown;
        }

        public override void Reset()
        {
            ResetStat();
            ResetUpgrades();
        }

        public void ResetStat()
        {
            Stat = null;
        }

        public virtual void ResetUpgrades()
        {

        }
    }
}
