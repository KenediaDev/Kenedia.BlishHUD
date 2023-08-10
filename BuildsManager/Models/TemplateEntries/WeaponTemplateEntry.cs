using Kenedia.Modules.BuildsManager.Models.Templates;
using System;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Weapon = Kenedia.Modules.BuildsManager.DataModels.Items.Weapon;
using ItemWeaponType = Gw2Sharp.WebApi.V2.Models.ItemWeaponType;
using Kenedia.Modules.BuildsManager.Utility;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class WeaponTemplateEntry : TemplateEntry
    {
        public WeaponTemplateEntry(TemplateSlot slot) : base(slot)
        {
        }

        public Weapon Item { get; set; }

        public Sigil Sigil { get; set; }

        public Sigil PvpSigil { get; set; }

        public Infusion Infusion { get; set; }

        public Stat Stat { get; set; }

        public override void FromCode(string code)
        {
            string[] parts = GetCode(code).Split('|');

            if (parts.Length == 5)
            {
                Item = Enum.TryParse(parts[0], out ItemWeaponType weaponType) ? BuildsManager.Data.Weapons.Values.Where(e => e.WeaponType == weaponType).FirstOrDefault() : null;
                Stat = int.TryParse(parts[1], out int stat) ? BuildsManager.Data.Stats.Where(e => e.Value.MappedId == stat).FirstOrDefault().Value : null;
                Sigil = int.TryParse(parts[2], out int sigil) ? BuildsManager.Data.PveSigils.Where(e => e.Value.MappedId == sigil).FirstOrDefault().Value : null;
                PvpSigil = int.TryParse(parts[3], out int pvpSigil) ? BuildsManager.Data.PvpSigils.Where(e => e.Value.MappedId == pvpSigil).FirstOrDefault().Value : null;
                Infusion = int.TryParse(parts[4], out int infusion) ? BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == infusion).FirstOrDefault().Value : null;
            }
        }

        public override string ToCode()
        {
            return $"[{(int)(Item?.WeaponType ?? ItemWeaponType.Unknown)}|{Stat?.MappedId ?? -1}|{Sigil?.MappedId ?? -1}|{PvpSigil?.MappedId ?? -1}|{Infusion?.MappedId ?? -1}]";
        }

        public override short[] AddToCodeArray(short[] array)
        {
            return array.Concat(new short[]
            {
                (short)(Item?.WeaponType ?? ItemWeaponType.Unknown),
                (short)(Stat?.MappedId ?? -1),
                (short)(Sigil?.MappedId ?? -1),
                (short)(PvpSigil?.MappedId ?? -1),
                (short)(Infusion?.MappedId ?? -1),
            }).ToArray();
        }

        public override short[] GetFromCodeArray(short[] array)
        {
            int newStartIndex = 5;

            Item = Enum.TryParse($"{array[0]}", out ItemWeaponType weaponType) ? BuildsManager.Data.Weapons.Values.Where(e => e.WeaponType == weaponType).FirstOrDefault() : null;
            Stat = int.TryParse($"{array[1]}", out int stat) ? BuildsManager.Data.Stats.Where(e => e.Value.MappedId == stat).FirstOrDefault().Value : null;
            Sigil = int.TryParse($"{array[2]}", out int sigil) ? BuildsManager.Data.PveSigils.Where(e => e.Value.MappedId == sigil).FirstOrDefault().Value : null;
            PvpSigil = int.TryParse($"{array[3]}", out int pvpSigil) ? BuildsManager.Data.PvpSigils.Where(e => e.Value.MappedId == pvpSigil).FirstOrDefault().Value : null;
            Infusion = int.TryParse($"{array[4]}", out int infusion) ? BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == infusion).FirstOrDefault().Value : null;

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }
    }
}
