using System;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    [Flags]
    public enum EncounterFlag : long
    {
        None = 0,
        NormalMode = 1L << 1,
        ChallengeMode = 1L << 2,
        ValeGuardian = 1L << 3,
        Gorseval = 1L << 4,
        Sabetha = 1L << 5,
        Slothasor = 1L << 6,
        BanditTrio = 1L << 7,
        Matthias = 1L << 8,
        Escort = 1L << 9,
        KeepConstruct = 1L << 10,
        Xera = 1L << 11,
        Cairn = 1L << 12,
        MursaatOverseer = 1L << 13,
        Samarog = 1L << 14,
        Deimos = 1L << 15,
        SoullessHorror = 1L << 16,
        River = 1L << 17,
        Statues = 1L << 18,
        Dhuum = 1L << 19,
        ConjuredAmalgamate = 1L << 20,
        TwinLargos = 1L << 21,
        Qadim1 = 1L << 22,
        Sabir = 1L << 23,
        Adina = 1L << 24,
        Qadim2 = 1L << 25,
        Shiverpeaks = 1L << 26,
        KodanTwins = 1L << 27,
        Fraenir = 1L << 28,
        Boneskinner = 1L << 29,
        WhisperOfJormag = 1L << 30,
        ForgingSteel = 1L << 31,
        ColdWar = 1L << 32,
        OldLionsCourt = 1L << 33,
        Aetherblade = 1L << 34,
        Junkyard = 1L << 35,
        KainengOverlook = 1L << 36,
        HarvestTemple = 1L << 37,
    }
}
