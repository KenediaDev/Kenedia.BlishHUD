﻿using Blish_HUD.Content;
using Gw2Sharp.Models;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using static Kenedia.Modules.BuildsManager.DataModels.Professions.Weapon;
using APIProfession = Gw2Sharp.WebApi.V2.Models.Profession;

namespace Kenedia.Modules.BuildsManager.DataModels.Professions
{
    [DataContract]
    public class Profession
    {
        private AsyncTexture2D _icon;
        private AsyncTexture2D _iconBig;

        [DataMember]
        public ProfessionType Id { get; set; }

        public AsyncTexture2D Icon
        {
            get
            {
                if (_icon != null) return _icon;

                _icon = AsyncTexture2D.FromAssetId(IconAssetId);
                return _icon;
            }
        }

        public AsyncTexture2D IconBig
        {
            get
            {
                if (_iconBig != null) return _iconBig;

                _iconBig = AsyncTexture2D.FromAssetId(IconBigAssetId);
                return _iconBig;
            }
        }

        [DataMember]
        public int IconAssetId { get; set; }

        [DataMember]
        public int IconBigAssetId { get; set; }

        [DataMember]
        public Dictionary<int, Specialization> Specializations { get; set; } = new();

        [DataMember]
        public Dictionary<WeaponType, Weapon> Weapons { get; set; } = new();

        public string Name
        {
            get => Names.Text;
            set => Names.Text = value;
        }

        [DataMember]
        public LocalizedString Names { get; protected set; } = new();

        [DataMember]
        public Dictionary<int, Skill> Skills { get; set; } = new();

        [DataMember]
        public Dictionary<int, Legend> Legends { get; set; }

        internal void Apply(APIProfession prof, Dictionary<int, Specialization> specializations, Dictionary<int, Trait> traits, Dictionary<int, Skill> skills, Dictionary<int, Legend> legends)
        {
            if (Enum.TryParse(prof.Id, out ProfessionType professionType))
            {
                Id = professionType;
                Name = prof.Name;
                IconAssetId = prof.Icon.GetAssetIdFromRenderUrl();
                IconBigAssetId = prof.IconBig.GetAssetIdFromRenderUrl();

                foreach (var apiWeapon in prof.Weapons)
                {
                    if (Enum.TryParse(apiWeapon.Key, out WeaponType weaponType))
                    {
                        bool exists = Weapons.TryGetValue(weaponType, out Weapon weapon);
                        weapon ??= new Weapon(apiWeapon, skills);

                        if (!exists)
                        {
                            Weapons.Add(weapon.Type, weapon);
                        }
                        else
                        {
                            weapon.ApplyLanguage(skills);
                        }
                    }
                }

                // Add Flip & Bundle Skills
                var profSkillIds = prof.Skills.Select(e => e.Id).ToList();

                var weaponIds = (from pp in prof.Weapons
                                 from pskills in pp.Value.Skills
                                 select pskills.Id).ToList();

                var ids = new List<int>();

                Skill getSkillById(int id, Dictionary<int, Skill> skills)
                {
                    return skills.TryGetValue(id, out var skill) ? skill : null;
                }

                List<int> getIds(Skill weaponSkill, List<int> result = null)
                {
                    result ??= new List<int>();

                    if (weaponSkill == null) return result;

                    result.Add(weaponSkill.Id);

                    if (weaponSkill.BundleSkills != null)
                    {
                        result.AddRange(weaponSkill.BundleSkills);
                        foreach (int bundleskill in weaponSkill.BundleSkills)
                        {
                            result.AddRange(getIds(getSkillById(bundleskill, skills)));
                        }
                    }

                    if (weaponSkill.FlipSkill != null)
                    {
                        result.Add((int)weaponSkill.FlipSkill);
                        result.AddRange(getIds(getSkillById((int)weaponSkill.FlipSkill, skills)));
                    }

                    if (weaponSkill.NextChain != null)
                    {
                        result.Add((int)weaponSkill.NextChain);
                    }

                    if (weaponSkill.PrevChain != null)
                    {
                        result.Add((int)weaponSkill.PrevChain);
                    }

                    if (weaponSkill.ToolbeltSkill != null)
                    {
                        result.Add((int)weaponSkill.ToolbeltSkill);
                    }

                    return result;
                }

                void AddOrUpdateLocale(int id)
                {
                    var skill = getSkillById(id, skills);
                    var existingSkill = getSkillById(id, Skills);

                    if (skill != null)
                    {
                        // Update
                        if (existingSkill != null)
                        {
                            existingSkill.Name = skill.Name;
                            existingSkill.Description = skill.Description;
                        }
                        // Add
                        else
                        {
                            Skills.Add(id, skill);
                        }
                    }
                }

                foreach (int id in weaponIds)
                {
                    if (skills.TryGetValue(id, out Skill weaponSkill))
                    {
                        ids.AddRange(getIds(getSkillById(id, skills)));
                    }
                }

                foreach (int id in profSkillIds)
                {
                    if (skills.TryGetValue(id, out Skill weaponSkill))
                    {
                        ids.AddRange(getIds(getSkillById(id, skills)));
                    }
                }

                foreach (int id in ids)
                {
                    AddOrUpdateLocale(id);
                }

                //foreach (var s in prof.Skills)
                //{
                //    if (s != null && skills.TryGetValue(s.Id, out Skill skill))
                //    {
                //        bool exists = Skills.TryGetValue(skill.Id, out var existingSkill);

                //        if (SkillsByPalette.TryGetValue(s.Id, out int paletteId))
                //        {
                //            skill.PaletteId = paletteId;
                //        }

                //        if (!exists)
                //        {
                //            Skills.Add(skill.Id, skill);
                //        }
                //        else
                //        {
                //            existingSkill.Name = skill.Name;
                //            existingSkill.Description = skill.Description;
                //        }
                //    }
                //}

                foreach (int s in prof.Specializations)
                {
                    if (specializations.TryGetValue(s, out Specialization spec))
                    {
                        bool exists = Specializations.TryGetValue(spec.Id, out var specialization);

                        if (!exists)
                        {
                            Specializations.Add(spec.Id, spec);
                        }
                        else
                        {
                            specialization.Name = spec.Name;
                            foreach (var t in specialization.MajorTraits)
                            {
                                if (traits.TryGetValue(t.Key, out Trait trait))
                                {
                                    t.Value.Name = trait.Name;
                                    t.Value.Description = trait.Description;
                                }
                            }

                            foreach (var t in specialization.MinorTraits)
                            {
                                if (traits.TryGetValue(t.Key, out Trait trait))
                                {
                                    t.Value.Name = trait.Name;
                                    t.Value.Description = trait.Description;
                                }
                            }
                        }
                    }
                }

                if (professionType == ProfessionType.Revenant)
                {
                    Legends ??= new();

                    foreach (var leg in legends)
                    {
                        bool exists = Legends.TryGetValue(leg.Key, out Legend legend);

                        if (!exists)
                        {
                            Legends.Add(leg.Key, leg.Value);
                        }
                        else
                        {
                            legend.ApplyLanguage(leg);
                        }
                    }
                }
            }
        }
    }
}
