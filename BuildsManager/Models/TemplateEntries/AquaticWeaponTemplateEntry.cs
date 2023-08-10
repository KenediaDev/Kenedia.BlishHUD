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

        public override byte[] AddToCodeArray(byte[] array)
        {
            return array.Concat(new byte[]
            {
                (byte)(Item?.WeaponType ?? ItemWeaponType.Unknown),
                Stat ?.MappedId ?? 0,
                Sigil_1 ?.MappedId ?? 0,
                Sigil_2 ?.MappedId ?? 0,
                Infusion_1 ?.MappedId ?? 0,
                Infusion_2 ?.MappedId ?? 0,
            }).ToArray();
        }

        public override byte[] GetFromCodeArray(byte[] array)
        {
            int newStartIndex = 6;

            Item = Enum.TryParse($"{array[0]}", out ItemWeaponType weaponType) ? BuildsManager.Data.Weapons.Values.Where(e => e.WeaponType == weaponType).FirstOrDefault() : null;
            Stat = BuildsManager.Data.Stats.Where(e => e.Value.MappedId == array[1]).FirstOrDefault().Value;
            Sigil_1 = BuildsManager.Data.PveSigils.Where(e => e.Value.MappedId == array[2]).FirstOrDefault().Value;
            Sigil_2 = BuildsManager.Data.PveSigils.Where(e => e.Value.MappedId == array[3]).FirstOrDefault().Value;
            Infusion_1 = BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == array[4]).FirstOrDefault().Value;
            Infusion_2 = BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == array[5]).FirstOrDefault().Value;

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }
    }
}
