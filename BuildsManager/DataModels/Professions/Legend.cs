using Gw2Sharp.Models;
using Kenedia.Modules.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                        heal.PaletteId = Skill.GetRevPaletteId(heal);
                        Heal = heal;
                    }

                    if (skills.TryGetValue(legend.Elite, out Skill elite))
                    {
                        elite.PaletteId = Skill.GetRevPaletteId(elite);
                        Elite = elite;
                    }

                    foreach (int util in legend.Utilities)
                    {
                        if (skills.TryGetValue(util, out Skill utility))
                        {
                            utility.PaletteId = Skill.GetRevPaletteId(utility);
                            Utilities.Add(utility.Id, utility);
                        }
                    }
                }
            }
        }

        public static (int, int, int, int, int, int) LegendaryAllianceLuxonIds{ get; } = new(62891, 62719, 62832, 62962, 62878, 62942);

        public static (int, int, int, int, int, int) LegendaryAllianceKurzickIds{ get; } = new(62749, 62680, 62702, 62941, 62796, 62687);

        public string Name => Swap.Name;

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

        public static Legend FromByte(byte id)
        {
            bool? exists = BuildsManager.Data.Professions[ProfessionType.Revenant].Legends.TryGetValue((int)id, out Legend legend);
            return exists == true ? legend : null;
        }

        internal static Skill SkillFromUShort(ushort paletteId, Legend legend)
        {
            if (legend != null)
            {
                if (legend.Elite.PaletteId == paletteId) return legend.Elite;
                if (legend.Heal.PaletteId == paletteId) return legend.Heal;

                foreach (var s in legend.Utilities)
                {
                    if (paletteId == s.Value.PaletteId) return s.Value;
                }
            }

            return null;
        }
    }
}
