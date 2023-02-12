using Kenedia.Modules.Core.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using APILegend = Gw2Sharp.WebApi.V2.Models.Legend;

namespace Kenedia.Modules.BuildsManager.DataModels.Professions
{
    [DataContract]
    public class Legend
    {

        public Legend()
        {

        }

        public Legend(APILegend legend, Dictionary<int, Skill> skills)
        {
            if (int.TryParse(legend.Id.Replace("Legend", ""), out int id))
            {
                if (skills.TryGetValue(legend.Swap, out Skill skill))
                {
                    Id = id;
                    Swap = skill;
                    Specialization = skill.Specialization;

                    if (skills.TryGetValue(legend.Heal, out Skill heal))
                    {
                        Heal = heal;
                    }

                    if (skills.TryGetValue(legend.Elite, out Skill elite))
                    {
                        Elite = elite;
                    }

                    foreach (int util in legend.Utilities)
                    {
                        if (skills.TryGetValue(util, out Skill utility))
                        {
                            Utilities.Add(utility.Id, utility);
                        }
                    }
                }
            }
        }

        [DataMember]
        public LocalizedString Names { get; protected set; } = new();

        public string Name
        {
            get => Names.Text;
            set => Names.Text = value;
        }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public Dictionary<int, Skill> Utilities { get; set; } = new();

        [DataMember]
        public Skill Heal { get; set; }

        [DataMember]
        public Skill Elite { get; set; }

        [DataMember]
        public Skill Swap { get; set; }

        [DataMember]
        public int Specialization { get; set; }

        internal void ApplyLanguage(KeyValuePair<int, Legend> leg)
        {
            Name = leg.Value.Name;
            Heal.Name = leg.Value.Heal.Name;
            Heal.Description = leg.Value.Heal.Description;

            Swap.Name = leg.Value.Swap.Name;
            Swap.Description = leg.Value.Swap.Description;

            Elite.Name = leg.Value.Elite.Name;
            Elite.Description = leg.Value.Elite.Description;

            foreach(var ut in Utilities.Values)
            {
                if(leg.Value.Utilities.TryGetValue(ut.Id, out var utility))
                {
                    utility.Name = ut.Name;
                    utility.Description = ut.Description;
                }
            }
        }
    }
}
