using Kenedia.Modules.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Kenedia.Modules.AdvancedBuildsManager.DataModels.Items
{
    [DataContract]
    public class RuneBonuses
    {
        [DataMember]
        public Dictionary<int, LocalizedString> LocalizedBonuses { get; private set; } = [];

        public string this[int key]
        {
            get => LocalizedBonuses[key].Text;
            set 
            {
                if (!LocalizedBonuses.ContainsKey(key))
                {
                    LocalizedBonuses[key] = [];
                }

                LocalizedBonuses[key].Text = value;
            }
        }

        public List<string> Bonuses => LocalizedBonuses.Select(e => e.Value.Text).ToList();

        internal void AddBonuses(IReadOnlyList<string> bonuses)
        {
            for (int i = 0; i < bonuses.Count; i++)
            {
                this[i] = bonuses[i];
            }
        }
    }
}
