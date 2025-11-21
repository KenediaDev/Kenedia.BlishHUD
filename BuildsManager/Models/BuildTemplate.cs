using Gw2Sharp.Models;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gw2BuildTemplates
{
    public enum TemplateWeaponType
    {
        None = 0,
        Axe = 5,
        LongBow = 35,
        Dagger = 47,
        Focus = 49,
        Greatsword = 50,
        Hammer = 51,
        Mace = 53,
        Pistol = 54,
        Rifle = 85,
        Scepter = 86,
        Shield = 87,
        Staff = 89,
        Sword = 90,
        Torch = 102,
        Warhorn = 103,
        ShortBow = 107,
        Harpoon = 265
    }

    public sealed class SpecializationEntry
    {
        public byte SpecializationId { get; set; }

        public byte Trait1 { get; set; } // 1,2,3 or 0

        public byte Trait2 { get; set; }

        public byte Trait3 { get; set; }
    }

    public sealed class RevenantLegendData
    {
        public byte TerrestrialLegend1 { get; set; }

        public byte TerrestrialLegend2 { get; set; }

        public byte AquaticLegend1 { get; set; }

        public byte AquaticLegend2 { get; set; }

        public ushort InactiveTerrestrial1 { get; set; }

        public ushort InactiveTerrestrial2 { get; set; }

        public ushort InactiveTerrestrial3 { get; set; }

        public ushort InactiveAquatic1 { get; set; }

        public ushort InactiveAquatic2 { get; set; }

        public ushort InactiveAquatic3 { get; set; }
    }

    public sealed class RangerPetData
    {
        public byte Terrestrial1 { get; set; }

        public byte Terrestrial2 { get; set; }

        public byte Aquatic1 { get; set; }

        public byte Aquatic2 { get; set; }
    }

    public sealed class BuildTemplate
    {
        public ProfessionType Profession { get; set; }

        public SpecializationEntry[] Specializations { get; set; } = [
            new SpecializationEntry(),
            new SpecializationEntry(),
            new SpecializationEntry()
        ];

        public ushort TerrestrialHeal { get; set; }

        public ushort AquaticHeal { get; set; }

        public ushort TerrestrialUtility1 { get; set; }

        public ushort AquaticUtility1 { get; set; }

        public ushort TerrestrialUtility2 { get; set; }

        public ushort AquaticUtility2 { get; set; }

        public ushort TerrestrialUtility3 { get; set; }

        public ushort AquaticUtility3 { get; set; }

        public ushort TerrestrialElite { get; set; }

        public ushort AquaticElite { get; set; }

        public RangerPetData RangerPets { get; set; }

        public RevenantLegendData RevenantLegends { get; set; }

        public List<TemplateWeaponType> SelectedWeapons { get; set; } = [];

        public List<uint> SkillOverrides { get; set; } = [];
    }

    public static class Gw2BuildCodec
    {
        private static readonly byte BuildHeader = 0x0D;

        public static Gw2Sharp.WebApi.V2.Models.ItemWeaponType GetFromTemplateWeapon(TemplateWeaponType weaponType)
        {
            return weaponType switch
            {
                TemplateWeaponType.Axe => ItemWeaponType.Axe,
                TemplateWeaponType.LongBow => ItemWeaponType.LongBow,
                TemplateWeaponType.Dagger => ItemWeaponType.Dagger,
                TemplateWeaponType.Focus => ItemWeaponType.Focus,
                TemplateWeaponType.Greatsword => ItemWeaponType.Greatsword,
                TemplateWeaponType.Hammer => ItemWeaponType.Hammer,
                TemplateWeaponType.Mace => ItemWeaponType.Mace,
                TemplateWeaponType.Pistol => ItemWeaponType.Pistol,
                TemplateWeaponType.Rifle => ItemWeaponType.Rifle,
                TemplateWeaponType.Scepter => ItemWeaponType.Scepter,
                TemplateWeaponType.Shield => ItemWeaponType.Shield,
                TemplateWeaponType.Staff => ItemWeaponType.Staff,
                TemplateWeaponType.Sword => ItemWeaponType.Sword,
                TemplateWeaponType.Torch => ItemWeaponType.Torch,
                TemplateWeaponType.Warhorn => ItemWeaponType.Warhorn,
                TemplateWeaponType.ShortBow => ItemWeaponType.ShortBow,
                TemplateWeaponType.Harpoon => ItemWeaponType.Harpoon,
                _ => ItemWeaponType.Unknown,
            };
        }

        public static TemplateWeaponType GetToTemplateWeapon(Gw2Sharp.WebApi.V2.Models.ItemWeaponType weaponType)
        {
            return weaponType switch
            {
                ItemWeaponType.Axe => TemplateWeaponType.Axe,
                ItemWeaponType.LongBow => TemplateWeaponType.LongBow,
                ItemWeaponType.Dagger => TemplateWeaponType.Dagger,
                ItemWeaponType.Focus => TemplateWeaponType.Focus,
                ItemWeaponType.Greatsword => TemplateWeaponType.Greatsword,
                ItemWeaponType.Hammer => TemplateWeaponType.Hammer,
                ItemWeaponType.Mace => TemplateWeaponType.Mace,
                ItemWeaponType.Pistol => TemplateWeaponType.Pistol,
                ItemWeaponType.Rifle => TemplateWeaponType.Rifle,
                ItemWeaponType.Scepter => TemplateWeaponType.Scepter,
                ItemWeaponType.Shield => TemplateWeaponType.Shield,
                ItemWeaponType.Staff => TemplateWeaponType.Staff,
                ItemWeaponType.Sword => TemplateWeaponType.Sword,
                ItemWeaponType.Torch => TemplateWeaponType.Torch,
                ItemWeaponType.Warhorn => TemplateWeaponType.Warhorn,
                ItemWeaponType.ShortBow => TemplateWeaponType.ShortBow,
                ItemWeaponType.Harpoon => TemplateWeaponType.Harpoon,
                _ => TemplateWeaponType.None,
            };
        }

        public static bool TryDecode(string chatCode, out BuildTemplate build)
        {
            try
            {
                build = Decode(chatCode);
                return true;
            }
            catch
            {
                build = null;
                return false;
            }
        }

        public static bool TryEncode(BuildTemplate build, out string chatCode)
        {
            try
            {
                chatCode = Encode(build);
                return true;
            }
            catch
            {
                chatCode = null;
                return false;
            }
        }

        public static BuildTemplate Decode(string chatCode)
        {
            if (chatCode.StartsWith("[&"))
                chatCode = chatCode.Substring(2, chatCode.Length - 3);

            byte[] raw = Convert.FromBase64String(chatCode);

            var build = new BuildTemplate();

            if (raw.Length < 1)
                return build;

            if (raw[0] != BuildHeader)
                throw new Exception("Not a valid build template chat code.");

            int p = 1;

            build.Profession = (ProfessionType)raw[p++];

            build.Specializations = new SpecializationEntry[3];
            for (int i = 0; i < 3; i++)
            {
                byte spec = raw[p++];
                byte traitByte = raw[p++];

                byte t1 = (byte)((traitByte >> 0) & 0b11);
                byte t2 = (byte)((traitByte >> 2) & 0b11);
                byte t3 = (byte)((traitByte >> 4) & 0b11);

                build.Specializations[i] = new SpecializationEntry
                {
                    SpecializationId = spec,
                    Trait1 = t1,
                    Trait2 = t2,
                    Trait3 = t3
                };
            }

            build.TerrestrialHeal = ReadU16(raw, ref p);
            build.AquaticHeal = ReadU16(raw, ref p);
            build.TerrestrialUtility1 = ReadU16(raw, ref p);
            build.AquaticUtility1 = ReadU16(raw, ref p);
            build.TerrestrialUtility2 = ReadU16(raw, ref p);
            build.AquaticUtility2 = ReadU16(raw, ref p);
            build.TerrestrialUtility3 = ReadU16(raw, ref p);
            build.AquaticUtility3 = ReadU16(raw, ref p);
            build.TerrestrialElite = ReadU16(raw, ref p);
            build.AquaticElite = ReadU16(raw, ref p);

            byte[] professionBlock = raw.Skip(p).Take(16).ToArray();
            p += 16;

            if (build.Profession == ProfessionType.Ranger)
            {
                build.RangerPets = new RangerPetData
                {
                    Terrestrial1 = professionBlock[0],
                    Terrestrial2 = professionBlock[1],
                    Aquatic1 = professionBlock[2],
                    Aquatic2 = professionBlock[3]
                };
            }
            else if (build.Profession == ProfessionType.Revenant)
            {
                build.RevenantLegends = new RevenantLegendData
                {
                    TerrestrialLegend1 = professionBlock[0],
                    TerrestrialLegend2 = professionBlock[1],
                    AquaticLegend1 = professionBlock[2],
                    AquaticLegend2 = professionBlock[3],
                    InactiveTerrestrial1 = (ushort)(professionBlock[4] | (professionBlock[5] << 8)),
                    InactiveTerrestrial2 = (ushort)(professionBlock[6] | (professionBlock[7] << 8)),
                    InactiveTerrestrial3 = (ushort)(professionBlock[8] | (professionBlock[9] << 8)),
                    InactiveAquatic1 = (ushort)(professionBlock[10] | (professionBlock[11] << 8)),
                    InactiveAquatic2 = (ushort)(professionBlock[12] | (professionBlock[13] << 8)),
                    InactiveAquatic3 = (ushort)(professionBlock[14] | (professionBlock[15] << 8))
                };
            }

            //Check if we have still data to read weapons and overrides else return
            if (p >= raw.Length)
                return build;

            byte weaponCount = raw[p++];
            for (int i = 0; i < weaponCount; i++)
            {
                build.SelectedWeapons.Add((TemplateWeaponType)ReadU16(raw, ref p));
            }

            byte overrideCount = raw[p++];
            for (int i = 0; i < overrideCount; i++)
            {
                uint id =
                    (uint)(raw[p] |
                          (raw[p + 1] << 8) |
                          (raw[p + 2] << 16) |
                          (raw[p + 3] << 24));
                p += 4;
                build.SkillOverrides.Add(id);
            }

            return build;
        }

        public static string Encode(BuildTemplate t)
        {
            List<byte> buf = [BuildHeader, (byte)t.Profession];

            foreach (var s in t.Specializations)
            {
                buf.Add(s.SpecializationId);

                byte traitByte =
                    (byte)(((s.Trait1 & 0b11) << 0) |
                           ((s.Trait2 & 0b11) << 2) |
                           ((s.Trait3 & 0b11) << 4));

                buf.Add(traitByte);
            }

            WriteU16(buf, t.TerrestrialHeal);
            WriteU16(buf, t.AquaticHeal);
            WriteU16(buf, t.TerrestrialUtility1);
            WriteU16(buf, t.AquaticUtility1);
            WriteU16(buf, t.TerrestrialUtility2);
            WriteU16(buf, t.AquaticUtility2);
            WriteU16(buf, t.TerrestrialUtility3);
            WriteU16(buf, t.AquaticUtility3);
            WriteU16(buf, t.TerrestrialElite);
            WriteU16(buf, t.AquaticElite);

            byte[] profData = new byte[16];

            if (t.Profession == ProfessionType.Ranger && t.RangerPets != null)
            {
                profData[0] = t.RangerPets.Terrestrial1;
                profData[1] = t.RangerPets.Terrestrial2;
                profData[2] = t.RangerPets.Aquatic1;
                profData[3] = t.RangerPets.Aquatic2;
            }
            else if (t.Profession == ProfessionType.Revenant && t.RevenantLegends != null)
            {
                profData[0] = t.RevenantLegends.TerrestrialLegend1;
                profData[1] = t.RevenantLegends.TerrestrialLegend2;
                profData[2] = t.RevenantLegends.AquaticLegend1;
                profData[3] = t.RevenantLegends.AquaticLegend2;

                WriteU16Into(profData, 4, t.RevenantLegends.InactiveTerrestrial1);
                WriteU16Into(profData, 6, t.RevenantLegends.InactiveTerrestrial2);
                WriteU16Into(profData, 8, t.RevenantLegends.InactiveTerrestrial3);
                WriteU16Into(profData, 10, t.RevenantLegends.InactiveAquatic1);
                WriteU16Into(profData, 12, t.RevenantLegends.InactiveAquatic2);
                WriteU16Into(profData, 14, t.RevenantLegends.InactiveAquatic3);
            }

            buf.AddRange(profData);

            buf.Add((byte)t.SelectedWeapons.Count);
            foreach (var w in t.SelectedWeapons)
                WriteU16(buf, (ushort)w);

            buf.Add((byte)t.SkillOverrides.Count);
            foreach (uint o in t.SkillOverrides)
            {
                buf.Add((byte)(o & 0xFF));
                buf.Add((byte)((o >> 8) & 0xFF));
                buf.Add((byte)((o >> 16) & 0xFF));
                buf.Add((byte)((o >> 24) & 0xFF));
            }

            string base64 = Convert.ToBase64String(buf.ToArray());
            return "[&" + base64 + "]";
        }

        private static ushort ReadU16(byte[] raw, ref int p)
        {
            ushort v = (ushort)(raw[p] | (raw[p + 1] << 8));
            p += 2;
            return v;
        }

        private static void WriteU16(List<byte> buf, ushort v)
        {
            buf.Add((byte)(v & 0xFF));
            buf.Add((byte)((v >> 8) & 0xFF));
        }

        private static void WriteU16Into(byte[] arr, int offset, ushort v)
        {
            arr[offset] = (byte)(v & 0xFF);
            arr[offset + 1] = (byte)((v >> 8) & 0xFF);
        }
    }
}
