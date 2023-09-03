using Gw2Sharp.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using APILegend = Gw2Sharp.WebApi.V2.Models.Legend;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.BuildsManager.Interfaces;
using Blish_HUD.Content;

namespace Kenedia.Modules.BuildsManager.DataModels.Professions
{
    [DataContract]
    public class Legend : IDisposable, IBaseApiData
    {
        private bool _isDisposed;

        public Legend()
        {

        }

        public Legend(APILegend legend, Dictionary<int, Skill> skills)
        {
            Apply(legend, skills);
        }

        public static (int, int, int, int, int, int) LegendaryAllianceLuxonIds { get; } = new(62891, 62719, 62832, 62962, 62878, 62942);

        public static (int, int, int, int, int, int) LegendaryAllianceKurzickIds { get; } = new(62749, 62680, 62702, 62941, 62796, 62687);

        public string Name { get => Swap?.Name; set { if (Swap is not null) Swap.Name = value; } }

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

        public string Description { get => Swap?.Description; set { if (Swap is not null) Swap.Description = value; } }

        public AsyncTexture2D Icon => Swap?.Icon;

        internal void ApplyLanguage(KeyValuePair<int, Legend> leg)
        {
            Heal.Name = leg.Value.Heal.Name;
            Heal.Description = leg.Value.Heal.Description;

            Swap.Name = leg.Value.Swap.Name;
            Swap.Description = leg.Value.Swap.Description;

            Elite.Name = leg.Value.Elite.Name;
            Elite.Description = leg.Value.Elite.Description;

            foreach (var ut in Utilities.Values)
            {
                if (leg.Value.Utilities.TryGetValue(ut.Id, out var utility))
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
            if (legend is not null)
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

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            Utilities?.Values?.DisposeAll();
            Utilities.Clear();

            Heal?.Dispose();
            Heal = null;

            Elite?.Dispose();
            Elite = null;

            Swap?.Dispose();
            Swap = null;
        }

        internal void Apply(APILegend legend, Dictionary<int, Skill> skills)
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
    }
}
