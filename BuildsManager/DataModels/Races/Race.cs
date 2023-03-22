﻿using Kenedia.Modules.Core.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Kenedia.Modules.Core.DataModels;
using Skill = Kenedia.Modules.BuildsManager.DataModels.Professions.Skill;
using Blish_HUD.Content;

namespace Kenedia.Modules.BuildsManager.DataModels
{
    [DataContract]
    public class Race
    {
        private AsyncTexture2D _icon;
        private AsyncTexture2D _hoveredIcon;

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
        public LocalizedString Names { get; protected set; } = new();

        [DataMember]
        public Dictionary<int, Skill> Skills { get; } = new();

        public AsyncTexture2D Icon
        {
            get
            {
                if (_icon != null) return _icon;

                _icon = BuildsManager.ModuleInstance.ContentsManager.GetTexture($@"textures\races\{Id.ToString().ToLower()}.png");
                return _icon;
            }
        }

        public AsyncTexture2D HoveredIcon
        {
            get
            {
                if (_hoveredIcon != null) return _hoveredIcon;

                _hoveredIcon = BuildsManager.ModuleInstance.ContentsManager.GetTexture($@"textures\races\{Id.ToString().ToLower()}_hovered.png");
                return _hoveredIcon;
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
