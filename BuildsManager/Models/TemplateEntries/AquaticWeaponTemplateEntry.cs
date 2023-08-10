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
    public class AquaticWeaponTemplateEntry : TemplateEntry
    {
        public AquaticWeaponTemplateEntry(TemplateSlot slot) : base(slot)
        {
        }

        public Weapon Item { get; set; }

        public Sigil Sigil_1 { get; set; }

        public Sigil Sigil_2 { get; set; }

        public Infusion Infusion_1 { get; set; }

        public Infusion Infusion_2 { get; set; }

        public Stat Stat { get; set; }

        public override short[] AddToCodeArray(short[] array)
        {
            return array.Concat(new short[]
            {
                (short)(Item?.WeaponType ?? ItemWeaponType.Unknown),
                (short)(Stat?.MappedId ?? -1),
                (short)(Sigil_1?.MappedId ?? -1),
                (short)(Sigil_2?.MappedId ?? -1),
                (short)(Infusion_1?.MappedId ?? -1),
                (short)(Infusion_2?.MappedId ?? -1),
            }).ToArray();
        }

        public override void FromCode(string code)
        {
            string[] parts = GetCode(code).Split('|');

            if (parts.Length == 6)
            {
                Item = Enum.TryParse(parts[0], out ItemWeaponType weaponType) ? BuildsManager.Data.Weapons.Values.Where(e => e.WeaponType == weaponType).FirstOrDefault() : null;
                Stat = int.TryParse(parts[1], out int stat) ? BuildsManager.Data.Stats.Where(e => e.Value.MappedId == stat).FirstOrDefault().Value : null;
                Sigil_1 = int.TryParse(parts[2], out int sigil_1) ? BuildsManager.Data.PveSigils.Where(e => e.Value.MappedId == sigil_1).FirstOrDefault().Value : null;
                Sigil_2 = int.TryParse(parts[3], out int sigil_2) ? BuildsManager.Data.PveSigils.Where(e => e.Value.MappedId == sigil_2).FirstOrDefault().Value : null;
                Infusion_1 = int.TryParse(parts[4], out int infusion_1) ? BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == infusion_1).FirstOrDefault().Value : null;
                Infusion_2 = int.TryParse(parts[5], out int infusion_2) ? BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == infusion_2).FirstOrDefault().Value : null;
            }
        }

        public override short[] GetFromCodeArray(short[] array)
        {
            int newStartIndex = 6;

            Item = Enum.TryParse($"{array[0]}", out ItemWeaponType weaponType) ? BuildsManager.Data.Weapons.Values.Where(e => e.WeaponType == weaponType).FirstOrDefault() : null;
            Stat = int.TryParse($"{array[1]}", out int stat) ? BuildsManager.Data.Stats.Where(e => e.Value.MappedId == stat).FirstOrDefault().Value : null;
            Sigil_1 = int.TryParse($"{array[2]}", out int sigil_1) ? BuildsManager.Data.PveSigils.Where(e => e.Value.MappedId == sigil_1).FirstOrDefault().Value : null;
            Sigil_2 = int.TryParse($"{array[3]}", out int sigil_2) ? BuildsManager.Data.PveSigils.Where(e => e.Value.MappedId == sigil_2).FirstOrDefault().Value : null;
            Infusion_1 = int.TryParse($"{array[4]}", out int infusion_1) ? BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == infusion_1).FirstOrDefault().Value : null;
            Infusion_2 = int.TryParse($"{array[5]}", out int infusion_2) ? BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == infusion_2).FirstOrDefault().Value : null;

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }

        public override string ToCode()
        {
            return $"[{(int)(Item?.WeaponType ?? ItemWeaponType.Unknown)}|{Stat?.MappedId ?? -1}|{Sigil_1?.MappedId ?? -1}|{Sigil_2?.MappedId ?? -1}|{Infusion_1?.MappedId ?? -1}|{Infusion_2?.MappedId ?? -1}]";
        }
    }
}
