using Kenedia.Modules.Core.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Kenedia.Modules.Core.DataModels;
using Skill = Kenedia.Modules.AdvancedBuildsManager.DataModels.Professions.Skill;
using Blish_HUD.Content;
using System.Diagnostics;

namespace Kenedia.Modules.AdvancedBuildsManager.DataModels
{
    [DataContract]
    public class Race
    {
        public Race()
        {

        }

        public Race(Gw2Sharp.WebApi.V2.Models.Race race)
        {
            if (Enum.TryParse(race.Id, out Races racetype))
            {
                Name = race.Name;
                Id = racetype;
            }
        }

        public Race(Gw2Sharp.WebApi.V2.Models.Race race, Dictionary<int, Skill> skills) : this(race)
        {
            if (Enum.TryParse(race.Id, out Races _))
            {
                foreach (int id in race.Skills)
                {
                    if (skills.TryGetValue(id, out Skill skill))
                    {
                        skill.Categories = Professions.SkillCategory.Racial;
                        Debug.WriteLine($"CTOR SET RACIAL FLAG: {Professions.SkillCategory.Racial} -  skill.Categories {skill.Categories}");

                        Skills.Add(id, skill);
                    }
                }
            }
        }

        [DataMember]
        public Races Id { get; set; }

        public string Name
        {
            get => Names.Text;
            set => Names.Text = value;
        }

        [DataMember]
        public LocalizedString Names { get; protected set; } = [];

        [DataMember]
        public Dictionary<int, Skill> Skills { get; } = [];

        public AsyncTexture2D Icon
        {
            get
            {
                if (field is not null) return field;

                field = AdvancedBuildsManager.ModuleInstance.ContentsManager.GetTexture($@"textures\races\{Id.ToString().ToLower()}.png");
                return field;
            }
        }

        public AsyncTexture2D HoveredIcon
        {
            get
            {
                if (field is not null) return field;

                field = AdvancedBuildsManager.ModuleInstance.ContentsManager.GetTexture($@"textures\races\{Id.ToString().ToLower()}_hovered.png");
                return field;
            }
        }

        internal void UpdateLanguage(Gw2Sharp.WebApi.V2.Models.Race race, Dictionary<int, Skill> skills)
        {
            if (Enum.TryParse(race.Id, out Races _))
            {
                Name = race.Name;

                foreach (int id in race.Skills)
                {
                    bool exists = Skills.TryGetValue(id, out Skill skill);

                    if (skills.TryGetValue(id, out Skill allSkillsSkill))
                    {
                        skill ??= allSkillsSkill;
                        skill.Name = allSkillsSkill.Name;
                        skill.Description= allSkillsSkill.Description;
                        allSkillsSkill.Categories = Professions.SkillCategory.Racial;
                        skill.Categories = Professions.SkillCategory.Racial;


                        Debug.WriteLine($"SET RACIAL FLAG: {Professions.SkillCategory.Racial} -  skill.Categories {skill.Categories}");

                        if (!exists)
                        {
                            Skills.Add(id, skill);
                        }
                    }
                }
            }
        }
    }
}
