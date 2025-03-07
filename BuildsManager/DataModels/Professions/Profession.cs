﻿using Blish_HUD.Content;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using static Kenedia.Modules.BuildsManager.DataModels.Professions.Weapon;
using APIProfession = Gw2Sharp.WebApi.V2.Models.Profession;

namespace Kenedia.Modules.BuildsManager.DataModels.Professions
{
    [DataContract]
    public class Profession : IDisposable, IDataMember
    {
        private bool _isDisposed;
        private AsyncTexture2D _icon;
        private AsyncTexture2D _iconBig;

        [DataMember]
        public ProfessionType Id { get; set; }

        private AsyncTexture2D Icon
        {
            get
            {
                if (_icon is not null) return _icon;

                if (IconAssetId is not 0)
                    _icon = AsyncTexture2D.FromAssetId(IconAssetId);

                return _icon;
            }
        }

        private AsyncTexture2D IconBig
        {
            get
            {
                if (_iconBig is not null) return _iconBig;

                if (IconBigAssetId is not 0)
                    _iconBig = AsyncTexture2D.FromAssetId(IconBigAssetId);

                return _iconBig;
            }
        }

        [DataMember]
        public int IconAssetId { get; set; }

        [DataMember]
        public int IconBigAssetId { get; set; }

        [DataMember]
        public Dictionary<int, Specialization> Specializations { get; set; } = [];

        [DataMember]
        public Dictionary<WeaponType, Weapon> Weapons { get; set; } = [];

        public string Name
        {
            get => Names.Text;
            set => Names.Text = value;
        }

        [DataMember]
        public LocalizedString Names { get; protected set; } = [];

        [DataMember]
        public Dictionary<int, Skill> Skills { get; set; } = [];

        [DataMember]
        public Dictionary<int, Legend> Legends { get; set; }

        [DataMember]
        public Dictionary<int, int> SkillsByPalette { get; set; }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            _icon = null;
            _iconBig = null;

            Skills?.Values.DisposeAll();
            Skills?.Clear();

            Legends?.Values.DisposeAll();
            Legends?.Clear();

            Specializations?.Values.DisposeAll();
            Specializations?.Clear();

            Weapons?.Values.DisposeAll();
            Weapons?.Clear();
        }

        internal void Apply(APIProfession prof, Dictionary<int, Specialization> specializations, Dictionary<int, Trait> traits, Dictionary<int, Skill> skills, Dictionary<int, Legend> legends, Dictionary<Races, Race> races)
        {
            if (Enum.TryParse(prof.Id, out ProfessionType professionType))
            {
                Id = professionType;
                Name = prof.Name;
                IconAssetId = prof.Icon.GetAssetIdFromRenderUrl();
                IconBigAssetId = prof.IconBig.GetAssetIdFromRenderUrl();
                SkillsByPalette = prof.SkillsByPalette.ToDictionary(e => e.Key, e => e.Value);

                foreach (var apiWeapon in prof.Weapons)
                {
                    if (Enum.TryParse(apiWeapon.Key, out WeaponType weaponType))
                    {
                        bool exists = Weapons.TryGetValue(weaponType, out Weapon weapon);
                        weapon ??= new Weapon(apiWeapon, skills);

                        if (professionType == ProfessionType.Guardian && weaponType == WeaponType.Sword)
                        {
                            weapon.Specialization = (int)SpecializationType.Willbender;
                            weapon.SpecializationWielded = WieldingFlag.Offhand;
                        }
                        else if (professionType == ProfessionType.Ranger && weaponType == WeaponType.Dagger)
                        {
                            weapon.Specialization = (int)SpecializationType.Soulbeast;
                            weapon.SpecializationWielded = WieldingFlag.Mainhand;
                        }

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
                var traitedSkillsIds = new List<int>();
                foreach (var spec in specializations.Values.Where(e => e.Profession == professionType))
                {
                    foreach (var traited in spec.MajorTraits.Values.Where(e => e.Skills.Count > 0).Select(e => e.Skills))
                    {
                        traitedSkillsIds.AddRange(traited);
                    }

                    foreach (var traited in spec.MinorTraits.Values.Where(e => e.Skills.Count > 0).Select(e => e.Skills))
                    {
                        traitedSkillsIds.AddRange(traited);
                    }
                }
                profSkillIds.AddRange(traitedSkillsIds);

                var tSkillids = skills.Where(e => e.Value.Professions.Count <= 2 && e.Value.Professions.Contains(professionType)).Select(e => e.Value.Id).Except(profSkillIds).ToList();
                var raceSkills = races.SelectMany(e => e.Value.Skills).Select(e => e.Value.Id).ToList();

                profSkillIds.AddRange(tSkillids);
                _ = profSkillIds.RemoveAll(e => raceSkills.Contains(e));

                var weaponIds = (from pp in prof.Weapons
                                 from pskills in pp.Value.Skills
                                 select pskills.Id).ToList();

                var ids = new List<int>();

                Skill getSkillById(int id, Dictionary<int, Skill> skills)
                {
                    return skills.TryGetValue(id, out var skill) ? skill : null;
                }

                List<int> getIds(Skill skill, List<int> result = null)
                {
                    result ??= [];

                    if (skill == null) return result;

                    result.Add(skill.Id);

                    if (skill.BundleSkills is not null)
                    {
                        result.AddRange(skill.BundleSkills);
                        foreach (int bundleskill in skill.BundleSkills)
                        {
                            result.AddRange(getIds(getSkillById(bundleskill, skills)));
                        }
                    }

                    if (skill.FlipSkill is not null)
                    {
                        result.Add((int)skill.FlipSkill);
                        result.AddRange(getIds(getSkillById((int)skill.FlipSkill, skills)));
                    }

                    if (skill.NextChain is not null)
                    {
                        result.Add((int)skill.NextChain);
                    }

                    if (skill.PrevChain is not null)
                    {
                        result.Add((int)skill.PrevChain);
                    }

                    if (skill.ToolbeltSkill is not null)
                    {
                        result.Add((int)skill.ToolbeltSkill);
                    }

                    return result;
                }

                void AddOrUpdateLocale(int id)
                {
                    var skill = getSkillById(id, skills);
                    var existingSkill = getSkillById(id, Skills);

                    if (skill is not null)
                    {
                        // Update
                        if (existingSkill is not null)
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

                foreach (int id in profSkillIds.Distinct())
                {
                    if (skills.TryGetValue(id, out Skill weaponSkill))
                    {
                        ids.AddRange(getIds(getSkillById(id, skills)));
                    }
                }

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

                                    foreach (int id in trait.Skills)
                                    {
                                        if (skills.TryGetValue(id, out _))
                                        {
                                            ids.AddRange(getIds(getSkillById(id, skills)));
                                        }
                                    }
                                }
                            }

                            foreach (var t in specialization.MinorTraits)
                            {
                                if (traits.TryGetValue(t.Key, out Trait trait))
                                {
                                    t.Value.Name = trait.Name;
                                    t.Value.Description = trait.Description;

                                    foreach (int id in trait.Skills)
                                    {
                                        if (skills.TryGetValue(id, out _))
                                        {
                                            ids.AddRange(getIds(getSkillById(id, skills)));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (int id in ids)
                {
                    AddOrUpdateLocale(id);
                }

                if (professionType == ProfessionType.Revenant)
                {
                    Legends ??= [];

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

        internal void Apply(APIProfession prof, IApiV2ObjectList<Gw2Sharp.WebApi.V2.Models.Specialization> apiSpecializations, IEnumerable<Gw2Sharp.WebApi.V2.Models.Legend> apiLegends, IApiV2ObjectList<Gw2Sharp.WebApi.V2.Models.Trait> apiTraits, IApiV2ObjectList<Gw2Sharp.WebApi.V2.Models.Skill> apiSkills)
        {
            if (Enum.TryParse(prof.Id, out ProfessionType professionType))
            {
                Id = professionType;
                Name = prof.Name;
                IconAssetId = prof.Icon.GetAssetIdFromRenderUrl();
                IconBigAssetId = prof.IconBig.GetAssetIdFromRenderUrl();
                SkillsByPalette = prof.SkillsByPalette.ToDictionary(e => e.Key, e => e.Value);

                // Create Weapons
                foreach (var apiWeapon in prof.Weapons)
                {
                    if (Enum.TryParse(apiWeapon.Key, out WeaponType weaponType))
                    {
                        bool exists = Weapons.TryGetValue(weaponType, out Weapon weapon);
                        weapon ??= new Weapon(apiWeapon);

                        if (professionType == ProfessionType.Guardian && weaponType == WeaponType.Sword)
                        {
                            weapon.Specialization = (int)SpecializationType.Willbender;
                            weapon.SpecializationWielded = WieldingFlag.Offhand;
                        }
                        else if (professionType == ProfessionType.Ranger && weaponType == WeaponType.Dagger)
                        {
                            weapon.Specialization = (int)SpecializationType.Soulbeast;
                            weapon.SpecializationWielded = WieldingFlag.Mainhand;
                        }

                        if (!exists)
                            Weapons.Add(weapon.Type, weapon);
                    }
                }

                //Create Skills
                var weaponSkills = prof.Weapons.SelectMany(weapon => weapon.Value.Skills).ToList();
                var skills = apiSkills.Where(skill => prof.Skills.FirstOrDefault(e => e.Id == skill.Id) is not null || weaponSkills.FirstOrDefault(e => e.Id == skill.Id) is not null);
                skills = skills.Concat(apiSkills.Where(e => e.Professions.Count <= 2 && e.Professions.Contains($"{professionType}")).ToList()).Distinct();

                var traitIds = apiSpecializations.Where(e => e.Profession == $"{professionType}").SelectMany(x => x.MajorTraits);
                if (traitIds.Any())
                {
                    var traitSkills = apiTraits.Where(e => traitIds.Contains(e.Id) && e.Skills is not null).SelectMany(e => e.Skills).Select(x => x.Id).ToList();

                    if (traitSkills.Count > 0)
                    {
                        var traitedSkills = apiSkills.Where(e => traitSkills.Contains(e.Id)).ToList();
                        if (traitedSkills.Count > 0)
                        {
                            skills = skills.Concat(traitedSkills).Distinct();
                        }
                    }
                }

                foreach (var apiSkill in skills)
                {
                    bool exists = Skills.TryGetValue(apiSkill.Id, out Skill skill);
                    skill ??= new Skill();

                    skill.Apply(apiSkill);
                    skill.PaletteId = prof.SkillsByPalette.FirstOrDefault(e => e.Value == apiSkill.Id).Key;

                    if (weaponSkills.FirstOrDefault(e => e.Id == apiSkill.Id) is ProfessionWeaponSkill weaponSkill)
                    {
                        skill.Offhand = weaponSkill.Offhand is not null ? Enum.TryParse(weaponSkill.Offhand, out WeaponType weaponType) ? weaponType : null : null;
                    }

                    if (!exists)
                        Skills.Add(skill.Id, skill);
                }

                //Create Specializations
                foreach (var apiSpecialization in apiSpecializations.Where(e => e.Profession == $"{professionType}"))
                {
                    bool exists = Specializations.TryGetValue(apiSpecialization.Id, out Specialization specialization);
                    specialization ??= new Specialization();

                    specialization.Apply(apiSpecialization, apiTraits);

                    if (!exists)
                        Specializations.Add(specialization.Id, specialization);
                }

                //Create Legends
                if (professionType == ProfessionType.Revenant)
                {
                    Legends ??= [];
                    foreach (var apiLegend in apiLegends)
                    {
                        if (int.TryParse(apiLegend.Id.Replace("Legend", ""), out int id))
                        {
                            bool exists = Legends.TryGetValue(id, out Legend legend);
                            legend ??= new Legend();

                            legend.Apply(apiLegend, apiSkills);

                            if (!exists)
                                Legends.Add(legend.Id, legend);
                        }
                    }
                }
            }
        }
    }
}
