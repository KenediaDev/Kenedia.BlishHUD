using Gw2Sharp;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using APIWeapon = Gw2Sharp.WebApi.V2.Models.ProfessionWeapon;
using APISkill = Gw2Sharp.WebApi.V2.Models.Skill;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.DataModels.Professions
{
    [DataContract]
    public class Weapon
    {
        public Weapon()
        {

        }

        public Weapon(KeyValuePair<string, APIWeapon> weapon)
        {
            if (Enum.TryParse(weapon.Key, out WeaponType weaponType))
            {
                Type = weaponType;
                Wielded = weapon.Value.Flags.Count() > 0 ? weapon.Value.Flags.Aggregate((x, y) => x |= y.Value) : ProfessionWeaponFlag.Unknown;
                Specialization = weapon.Value.Specialization;
            }
        }

        public Weapon(KeyValuePair<string, APIWeapon> weapon, Dictionary<int, Skill> skills) : this(weapon)
        {
            if (Enum.TryParse(weapon.Key, out WeaponType _))
            {
                foreach (var s in weapon.Value.Skills)
                {
                    if (skills.TryGetValue(s.Id, out Skill skill))
                    {
                        Skills.Add(s.Id, skill);
                    }
                }
            }
        }

        public enum WeaponType
        {
            Unkown = -1,
            Axe,
            Dagger,
            Mace,
            Pistol,
            Scepter,
            Sword,
            Focus,
            Shield,
            Torch,
            Warhorn,
            Greatsword,
            Hammer,
            Longbow,
            Rifle,
            Shortbow,
            Staff,
            Harpoon,
            Speargun,
            Trident,

            Spear = 16,
            ShortBow = 14,
            LongBow = 12,
        }

        [DataMember]
        public ProfessionWeaponFlag Wielded { get; set; }

        [DataMember]
        public WeaponType Type { get; set; }

        [DataMember]
        public int Specialization { get; set; }

        [DataMember]
        public Dictionary<int, Skill> Skills { get; set; } = new();

        public void ApplyLanguage(Dictionary<int, Skill> skills)
        {
            foreach (var s in Skills)
            {
                if (skills.TryGetValue(s.Key, out Skill skill))
                {
                    s.Value.Name = skill.Name; 
                    s.Value.Description = skill.Description; 
                }
            }
        }
    }
}
