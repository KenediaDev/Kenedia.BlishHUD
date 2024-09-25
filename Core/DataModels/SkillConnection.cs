using Gw2Sharp.Models;
using Gw2Sharp.WebApi.V2.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kenedia.Modules.Core.DataModels
{
    public class SkillConnection
    {
        public SkillConnection()
        {

        }

        public SkillConnection(OldSkillConnection c)
        {
            Id = c.Id;
            Default = c.Default != c.Id ? c.Default : null;
            Enviroment = c.Enviroment;
            Attunement = c.Attunement;
            PvP = c.Pvp;
            AssetId = c.AssetId;
            Weapon = c.Weapon;
            Specialization = c.Specialization;
            EnvCounter = c.EnviromentalCounterskill;
            Stealth = c.Stealth?.Default is not null ? c.Stealth?.Default : c.Chain?.Stealth;
            Malicious = c.Stealth?.Malicious is not null ? c.Stealth?.Malicious : c.Chain?.Malicious;
            Ambush = c.Chain?.Ambush;
            Unleashed = c.Chain?.Unleashed;
            
            if(c.Chain is not null)
            {
                Chain =
                [
                    c.Chain.First,
                    c.Chain.Second,
                    c.Chain.Third,
                    c.Chain.Fourth,
                    c.Chain.Fifth,
                ];

                _ = Chain.RemoveAll(e => e == null);
            }

            if(c.FlipSkills is not null)
            {
                FlipSkills =
                [
                    c.FlipSkills.Default,
                    c.FlipSkills.State1,
                    c.FlipSkills.State2,
                    c.FlipSkills.State3,
                    c.FlipSkills.State4,
                    c.FlipSkills.State5,
                ];

                _ = FlipSkills.RemoveAll(e => e == null);
            }

            Transform = c.Transform;

            Burst = c.Burst;
            Bundle = c.Bundle;
            Pets = c.Pets;
            Traited = c.Traited;
        }

        public Enviroment Enviroment { get; set; } = Enviroment.Terrestrial;

        public AttunementType? Attunement { get; set; }

        public SkillWeaponType? Weapon { get; set; }

        public SpecializationType? Specialization { get; set; }

        public SkillSlot? Slot { get; set; }

        public int? AssetId { get; set; }

        public int Id { get; set; }
        public int? Default { get; set; }
        public int? EnvCounter { get; set; }
        public int? Stealth { get; set; }
        public int? Malicious { get; set; }
        public int? Ambush { get; set; }
        public int? Unleashed { get; set; }
        public int? Toolbelt { get; set; }
        public int? PvP { get; set; }
        public int? WvW { get; set; }

        public List<int?> Chain { get; set; }
        public List<int?> FlipSkills { get; set; }
        public Burst Burst { get; set; }
        public Bundle Bundle { get; set; }
        public Pets Pets { get; set; }
        public Traited Traited { get; set; }
        public Transform Transform { get; set; }
        public List<ProfessionType> Professions { get; set; } = [];
    }

    public class BaseConnectionProperty
    {
        [JsonIgnore]
        public List<int?> Values
        {
            get
            {
                var list = new List<int?>();
                foreach (var propInfo in GetType().GetProperties())
                {
                    if (propInfo.PropertyType == typeof(int?) && propInfo.GetIndexParameters().Length == 0)
                    {
                        list.Add((int?)propInfo.GetValue(this));
                    }
                }

                return list;
            }
        }

        [JsonIgnore]
        public int? this[string key]
        {
            get => this[key];
            set => this[key] = value;
        }

        public void Clear()
        {
            foreach (var propInfo in GetType().GetProperties())
            {
                if (propInfo.PropertyType == typeof(int?) && propInfo.GetIndexParameters().Length == 0)
                {
                    propInfo.SetValue(this, null);
                }
            }
        }

        public bool Contains(int? id)
        {
            return Values.Contains(id);
        }

        public bool HasValues()
        {
            return Values.Any(e => e is not null);
        }
    }

    [Flags]
    public enum Enviroment
    {
        Terrestrial = 1,
        Aquatic = 2,
    }

    public class Bundle : BaseConnectionProperty
    {
        public int? Equip { get; set; }
        public int? Stow { get; set; }
        public int? Weapon_1 { get; set; }
        public int? Weapon_2 { get; set; }
        public int? Weapon_3 { get; set; }
        public int? Weapon_4 { get; set; }
        public int? Weapon_5 { get; set; }
    }

    public class Transform : BaseConnectionProperty
    {
        public int? Enter { get; set; }
        public int? Exit { get; set; }
        public int? Weapon_1 { get; set; }
        public int? Weapon_2 { get; set; }
        public int? Weapon_3 { get; set; }
        public int? Weapon_4 { get; set; }
        public int? Weapon_5 { get; set; }
    }

    public class Burst : BaseConnectionProperty
    {
        public int? Spellbreaker { get; set; }
        public int? Berserker { get; set; }
        public int? Stage0 { get; set; }
        public int? Stage1 { get; set; }
        public int? Stage2 { get; set; }
        public int? Stage3 { get; set; }
    }

    public class Stealth : BaseConnectionProperty
    {
        public int? Default { get; set; }
        public int? Malicious { get; set; }
    }

    public class FlipSkills : BaseConnectionProperty
    {
        public int? Default { get; set; }
        public int? State1 { get; set; }
        public int? State2 { get; set; }
        public int? State3 { get; set; }
        public int? State4 { get; set; }
        public int? State5 { get; set; }
    }

    public class DualSkill : BaseConnectionProperty
    {
        public int? NoOffhand { get; set; }
        //public int? Axe { get; set; }
        public int? Dagger { get; set; }
        //public int? Mace { get; set; }
        public int? Pistol { get; set; }
        //public int? Scepter { get; set; }
        //public int? Sword { get; set; }
        //public int? Focus { get; set; }
        //public int? Shield { get; set; }
        //public int? Torch { get; set; }
        //public int? Warhorn { get; set; }
    }

    public class AttunementSkill : BaseConnectionProperty
    {
        public int? Fire { get; set; }
        public int? Water { get; set; }
        public int? Earth { get; set; }
        public int? Air { get; set; }
        public int? FireWater { get; set; }
        public int? FireAir { get; set; }
        public int? FireEarth { get; set; }
        public int? WaterAir { get; set; }
        public int? WaterEarth { get; set; }
        public int? AirEarth { get; set; }
    }

    public class Chain : BaseConnectionProperty
    {
        public int? First { get; set; }
        public int? Second { get; set; }
        public int? Third { get; set; }
        public int? Fourth { get; set; }
        public int? Fifth { get; set; }

        public int? Stealth { get; set; }
        public int? Malicious { get; set; }

        public int? Ambush { get; set; }
        public int? Unleashed { get; set; }
    }

    /// <summary>
    /// Key = Skill | Value = Trait
    /// </summary>
    public class Traited : Dictionary<int, int>
    {
        public bool HasValues()
        {
            return Count > 0;
        }
    }

    public class Pets : List<int?>
    {
        [JsonIgnore]
        public List<int?> Values => this;

        public bool HasValues()
        {
            return this.Any(e => e is not null);
        }
    }

    public class OldSkillConnection
    {
        /// <summary>
        /// The skills id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The skills default form
        /// </summary>
        public int? Default { get; set; }

        public int? AssetId { get; set; }

        /// <summary>
        /// The corresponding Pvp variant of the skill
        /// </summary>
        public int? Pvp { get; set; }

        /// <summary>
        /// Id of the skill which replaces the current skill in Water/Land
        /// </summary>
        public int? EnviromentalCounterskill { get; set; }

        /// <summary>
        /// Flag to set a skill to be either used only in water (<see cref="Enviroment.Aquatic"/>), land (<see cref="Enviroment.Terrestrial"/>) or both.
        /// </summary>
        public Enviroment Enviroment { get; set; } = Enviroment.Terrestrial;

        public AttunementType Attunement { get; set; }

        /// <summary>
        /// Weapon the skill is used with
        /// </summary>
        public SkillWeaponType? Weapon { get; set; }

        /// <summary>
        /// <see cref="SpecializationType"/> the skill is available for
        /// </summary>
        public SpecializationType? Specialization { get; set; }

        /// <summary>
        /// Dual Skill for Thiefs 3rd Slot 
        /// </summary>
        public DualSkill DualSkill { get; set; }

        public Burst Burst { get; set; }

        public Stealth Stealth { get; set; }

        public Transform Transform { get; set; }

        public Bundle Bundle { get; set; }

        public int? Unleashed { get; set; }

        public int? Toolbelt { get; set; }

        /// <summary>
        /// <see cref="Pet"/> which you can access this skill through as a <see cref="SpecializationType.Soulbeast"/> or <see cref="SpecializationType.Untamed"/>
        /// </summary>
        public Pets Pets { get; set; }

        /// <summary>
        /// Chained skills
        /// </summary>
        public Chain Chain { get; set; }

        public FlipSkills FlipSkills { get; set; }

        public Traited Traited { get; set; }

        public void Clear()
        {
            Default = null;
            EnviromentalCounterskill = null;
            Enviroment = Enviroment.Terrestrial;
            Weapon = null;
            Specialization = null;
            DualSkill = null;
            Burst = null;
            Stealth = null;
            Transform = null;
            Bundle = null;
            Unleashed = null;
            Toolbelt = null;
            Pets = null;
            Chain = null;
            FlipSkills = null;
            Traited = null;
        }
    }
}
