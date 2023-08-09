using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using APIWeapon = Gw2Sharp.WebApi.V2.Models.ProfessionWeapon;

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

                foreach(var flag in weapon.Value.Flags)
                {
                    if(Enum.TryParse(flag.Value.ToString(), out WieldingFlag wielded))
                    {
                        Wielded |= wielded;
                    }
                }

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
                        Skills.Add(skill.Id);
                    }
                }
            }
        }

        [Flags]
        public enum WieldingFlag
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            Unknown = 0,

            /// <summary>
            /// Weapon is used as main-hand weapon.
            /// </summary>
            Mainhand = 1,

            /// <summary>
            /// Weapon is used as two-handed weapon.
            /// </summary>
            TwoHand = 2,

            /// <summary>
            /// Weapon is used as off-hand weapon.
            /// </summary>
            Offhand = 4,

            /// <summary>
            /// Weapon is used as underwater weapon.
            /// </summary>
            Aquatic = 8,
        }

        public enum WeaponType
        {
            Unknown,
            None,
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

            LongBow = 14,
            ShortBow = 16,
            Spear = 18,
        }

        [DataMember]
        public WieldingFlag Wielded { get; set; }

        [DataMember]
        public WeaponType Type { get; set; }

        [DataMember]
        public int Specialization { get; set; }

        [DataMember]
        public WieldingFlag? SpecializationWielded { get; set; }

        [DataMember]
        public List<int> Skills { get; set; } = new();

        public void ApplyLanguage(Dictionary<int, Skill> skills)
        {
        }
    }
}
