﻿using Kenedia.Modules.AdvancedBuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Models;
using System;

namespace Kenedia.Modules.AdvancedBuildsManager.Models.Templates
{
    public class SkillCollection : ObservableDictionary<BuildSkillSlot, Skill>
    {
        //public new Skill this[BuildSkillSlot key]
        //{
        //    get => base[key];
        //    //set => OnValueChanged(key, ContainsKey(key) ? base[key] : default, value);
        //}

        public SkillCollection()
        {
            foreach (BuildSkillSlot e in Enum.GetValues(typeof(BuildSkillSlot)))
            {
                Add(e, null);
            }
        }

        public ushort GetPaletteId(BuildSkillSlot slot)
        {
            return (ushort)(TryGetValue(slot, out Skill skill) && skill is not null ? skill.PaletteId : 0);
        }

        public bool HasSkill(Skill skill)
        {
            foreach(var s in this)
            {
                if (s.Value is not null && s.Value == skill) return true;
            }

            return false;
        }

        public bool HasSkill(int skillid)
        {
            foreach(var s in this)
            {
                if (s.Value is not null && s.Value.Id == skillid) return true;
            }

            return false;
        }

        public BuildSkillSlot GetSkillSlot(int skillid)
        {
            foreach(var s in this)
            {
                if (s.Value is not null && s.Value.Id == skillid) return s.Key;
            }

            return BuildSkillSlot.Utility_1;
        }

        public BuildSkillSlot GetSkillSlot(Skill skill)
        {
            foreach(var s in this)
            {
                if (s.Value is not null && s.Value == skill) return s.Key;
            }

            return BuildSkillSlot.Utility_1;
        }
    }
}
