using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;

namespace Kenedia.Modules.Core.DataModels
{
    [Flags]
    public enum Enviroment
    {
        Terrestrial,
        Aquatic,
    }

    public enum AdrenalinStage
    {
        None,
        Stage1,
        Stage2,
        Stage3,
    }

    public class BundleSkill
    {
        public int? Equip;
        public int? Stow;
        public int? Weapon1;
        public int? Weapon2;
        public int? Weapon3;
        public int? Weapon4;
        public int? Weapon5;

        public int?[] Values => new[] { Equip, Stow, Weapon1, Weapon2, Weapon3, Weapon4, Weapon5, };

        public void Clear()
        {
            Equip = null;
            Stow = null;
            Weapon1 = null;
            Weapon2 = null;
            Weapon3 = null;
            Weapon4 = null;
            Weapon5 = null;
        }
    }

    public class Transform
    {
        public int? Enter;
        public int? Exit;
        public int? Weapon1;
        public int? Weapon2;
        public int? Weapon3;
        public int? Weapon4;
        public int? Weapon5;

        public int?[] Values => new[] { Enter, Exit, Weapon1, Weapon2, Weapon3, Weapon4, Weapon5, };

        public void Clear()
        {
            Enter = null;
            Exit = null;
            Weapon1 = null;
            Weapon2 = null;
            Weapon3 = null;
            Weapon4 = null;
            Weapon5 = null;
        }
    }

    public class Burst
    {
        public int? Spellbreaker;
        public int? Berserker;
        public int? Stage0;
        public int? Stage1;
        public int? Stage2;
        public int? Stage3;

        public int?[] Values => new[] { Spellbreaker, Berserker, Stage0, Stage1, Stage2, Stage3, };

        public void Clear()
        {
            Spellbreaker = null;
            Berserker = null;
            Stage0 = null;
            Stage1 = null;
            Stage2 = null;
            Stage3 = null;
        }
    }

    public class Stealth
    {
        public int? Default;
        public int? Deadeye;

        public int?[] Values => new[] { Default, Deadeye, };

        public void Clear()
        {
            Default = null;
            Deadeye = null;
        }
    }

    public class FlipSkill
    {
        public int? Default;
        public int? Activated;

        public int?[] Values => new[] { Default, Activated, };

        public void Clear()
        {
            Default = null;
            Activated = null;
        }
    }

    public class DualSkill
    {
        public int? Axe;
        public int? Dagger;
        public int? Mace;
        public int? Pistol;
        public int? Scepter;
        public int? Sword;
        public int? Focus;
        public int? Shield;
        public int? Torch;
        public int? Warhorn;

        public int?[] Values => new[] { Axe, Dagger, Mace, Pistol, Scepter, Sword, Focus, Shield, Torch, Warhorn };

        public void Clear()
        {
            Axe = null;
            Dagger = null;
            Mace = null;
            Pistol = null;
            Scepter = null;
            Sword = null;
            Focus = null;
            Shield = null;
            Torch = null;
            Warhorn = null;
        }
    }

    public class AttunementSkill
    {
        public Attunement? Attunement { get; set; }
        public int? Fire;
        public int? Water;
        public int? Earth;
        public int? Air;

        public int?[] Values => new[] { Fire, Water, Earth, Air, };

        public void Clear()
        {
            Fire = null;
            Water = null;
            Earth = null;
            Air = null;
            Attunement = null;
        }
    }

    public class Chain
    {
        public int? First;
        public int? Second;
        public int? Third;
        public int? Fourth;
        public int? Fifth;

        public int? Stealth;
        public int? StealthDeadeye;

        public int? Ambush;
        public int? Unleashed;

        public void Clear()
        {
            First = null;
            Second = null;
            Third = null;
            Fourth = null;
            Fifth = null;
            Stealth = null;
            StealthDeadeye = null;
            Ambush = null;
            Unleashed = null;
        }
    }

    public class SkillConnection
    {
        /// <summary>
        /// The skills id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The skills default form
        /// </summary>
        public int? Default { get; set; }

        /// <summary>
        /// Id of the skill which replaces the current skill in Water/Land
        /// </summary>
        public int? EnviromentalCounterskill { get; set; }

        /// <summary>
        /// Flag to set a skill to be either used only in water (<see cref="Enviroment.Aquatic"/>), land (<see cref="Enviroment.Terrestrial"/>) or both.
        /// </summary>
        public Enviroment Enviroment { get; set; } = Enviroment.Terrestrial;

        /// <summary>
        /// Weapon the skill is used with
        /// </summary>
        public SkillWeaponType? Weapon { get; set; }

        /// <summary>
        /// <see cref="Specializations"/> the skill is available for
        /// </summary>
        public Specializations? Specialization { get; set; }

        /// <summary>
        /// Dual Skill for Thiefs 3rd Slot 
        /// </summary>
        public DualSkill DualSkill { get; set; }

        /// <summary>
        /// Attunement Skills for Elementalists. 
        /// AttunementSkill.Attunement = Fire 
        /// -> AttunementSkill.Fire => Weaver Fire Fire
        /// -> AttunementSkill.Air => Weaver Fire Air
        /// </summary>
        public AttunementSkill Attunement { get; set; }

        public Burst Burst { get; set; }

        public Stealth Stealth { get; set; }

        public Transform Transform { get; set; }

        public BundleSkill Bundle { get; set; }

        public int? Unleashed { get; set; }

        public int? Toolbelt { get; set; }

        /// <summary>
        /// <see cref="Pet"/> which you can access this skill through as a <see cref="Specializations.Soulbeast"/> or <see cref="Specializations.Untamed"/>
        /// </summary>
        public List<int> Pets { get; set; }

        /// <summary>
        /// Chained skills
        /// </summary>
        public Chain Chain { get; set; }

        public FlipSkill FlipSkills { get; set; }

        public Dictionary<int, List<int>> Traited { get; set; }

        public void Clear()
        {
            Default = null;
            EnviromentalCounterskill = null;
            Enviroment = Enviroment.Terrestrial;
            Weapon = null;
            Specialization = null;
            DualSkill = null;
            Attunement = null;
            Burst = null;
            Stealth = null;
            Transform= null;
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
