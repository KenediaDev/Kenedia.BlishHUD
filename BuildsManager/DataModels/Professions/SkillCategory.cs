using System;

namespace Kenedia.Modules.BuildsManager.DataModels.Professions
{
    [Flags]
    public enum SkillCategory : long
    {
        None = 0,
        Trap = 1L << 0,
        Turret = 1L << 1,
        Elixir = 1L << 2,
        Kit = 1L << 3,
        Gadget = 1L << 4,
        Well = 1L << 5,
        Minion = 1L << 6,
        Venom = 1L << 7,
        Preparation = 1L << 8, //Thief Traps
        Arcane = 1L << 9,
        Glyph = 1L << 10,
        SpiritWeapon = 1L << 11,
        Physical = 1L << 12,
        Signet = 1L << 13,
        Survival = 1L << 14,
        Spirit = 1L << 15,
        Command = 1L << 16, //Ranger Shouts
        Spectral = 1L << 17,
        Corruption = 1L << 18,
        Trick = 1L << 19,
        Deception = 1L << 20,
        Mantra = 1L << 21,
        Manipulation = 1L << 22,
        Clone = 1L << 23,
        Glamour = 1L << 24,
        Cantrip = 1L << 25,
        Conjure = 1L << 26,
        Shout = 1L << 27,
        Consecration = 1L << 28,
        Meditation = 1L << 29,
        Banner = 1L << 30,
        Stance = 1L << 31,
        Specialization = 1L << 32,
        Racial = 1L << 33,

        Transform = 1L << 34,
        Symbol = 1L << 35,
        Virtue = 1L << 36,
        Ward = 1L << 37,
        Phantasm = 1L << 38,
        Mark = 1L << 39,
        StealthAttack = 1L << 40,
        DualWield = 1L << 41,
        Burst = 1L << 42,
        LegendaryDwarf = 1L << 43,
        LegendaryDemon = 1L << 44,
        LegendaryAssassin = 1L << 45,
        Overload = 1L << 46,
        Rage = 1L << 47,
        PrimalBurst = 1L << 48,
        CelestialAvatar = 1L << 49,
        LegendaryDragon = 1L << 50
    }
}
