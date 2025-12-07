using Kenedia.Modules.Core.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Kenedia.Modules.Core.DataModels;
using Skill = Kenedia.Modules.BuildsManager.DataModels.Professions.Skill;
using Blish_HUD.Content;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Gw2Sharp.WebApi.V2;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.DataModels
{
    [DataContract]
    public class Race : IDisposable, IDataMember
    {
        private bool _isDisposed;

        public Race()
        {

        }

        public Race(Gw2Sharp.WebApi.V2.Models.Race race)
        {
            Apply(race);
        }

        public Race(Gw2Sharp.WebApi.V2.Models.Race race, Dictionary<int, Skill> skills) : this(race)
        {
            if (Enum.TryParse(race.Id, out Races _))
            {
                foreach (int id in race.Skills)
                {
                    if (skills.TryGetValue(id, out Skill skill))
                    {
                        skill.Categories = Professions.SkillCategoryType.Racial;

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

        public string IconPath => $@"textures\races\{Id.ToString().ToLower()}.png";

        private AsyncTexture2D Icon
        {
            get
            {
                if (field is not null) return field;

                field = BuildsManager.ModuleInstance.ContentsManager.GetTexture($@"textures\races\{Id.ToString().ToLower()}.png");
                return field;
            }

            set;
        }

        public AsyncTexture2D HoveredIcon
        {
            get
            {
                if (field is not null) return field;

                field = BuildsManager.ModuleInstance.ContentsManager.GetTexture($@"textures\races\{Id.ToString().ToLower()}_hovered.png");
                return field;
            }

            private set;
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
                        skill.Description = allSkillsSkill.Description;
                        allSkillsSkill.Categories = Professions.SkillCategoryType.Racial;
                        skill.Categories = Professions.SkillCategoryType.Racial;

                        if (!exists)
                        {
                            Skills.Add(id, skill);
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            Icon = null;
            HoveredIcon = null;
        }

        internal void Apply(Gw2Sharp.WebApi.V2.Models.Race race)
        {
            if (Enum.TryParse(race.Id, out Races racetype))
            {
                Name = race.Name;
                Id = racetype;
            }
        }

        public void Apply(Gw2Sharp.WebApi.V2.Models.Race race, IApiV2ObjectList<Gw2Sharp.WebApi.V2.Models.Skill> skills, IReadOnlyDictionary<int, int> skillsByPalette)
        {
            Apply(race);

            if (Enum.TryParse(race.Id, out Races racetype))
            {
                foreach (int id in race.Skills)
                {
                    bool exists = Skills.TryGetValue(id, out Skill skill);
                    skill ??= new();

                    skill.PaletteId = skillsByPalette.FirstOrDefault(e => e.Value == id).Key;

                    Gw2Sharp.WebApi.V2.Models.Skill apiSkill = skills.Where(x => x.Id == id).FirstOrDefault();

                    if (apiSkill is not null)
                        skill.Apply(apiSkill);

                    skill.Categories = Professions.SkillCategoryType.Racial;

                    if (!exists)
                        Skills.Add(id, skill);
                }
            }
        }
    }
}
