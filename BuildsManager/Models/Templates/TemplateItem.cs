using Kenedia.Modules.BuildsManager.DataModels.Stats;
using static Kenedia.Modules.BuildsManager.DataModels.Professions.Weapon;
using ItemWeaponType = Gw2Sharp.WebApi.V2.Models.ItemWeaponType;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class TemplateItem
    {
        public TemplateItem()
        {

        }

        public EquipmentStat Stat { get; set; } = EquipmentStat.None;

        public WeaponType WeaponType { get; set; } = WeaponType.Unknown;

        public UpgradeCollection Upgrades { get; set; } = new();

        public string ToCode(GearSlot slot)
        {
            string upgradesCode = "(0)";
            switch (slot)
            {
                case GearSlot.Head:
                case GearSlot.Shoulder:
                case GearSlot.Hand:
                case GearSlot.Leg:
                case GearSlot.Foot:
                    upgradesCode = $"({Upgrades[UpgradeSlot.Rune]}-{Upgrades[UpgradeSlot.Infusion_1]})";
                    break;

                case GearSlot.Amulet:
                    upgradesCode = $"({Upgrades[UpgradeSlot.Enrichment]})";
                    break;

                case GearSlot.Back:
                    upgradesCode = $"({Upgrades[UpgradeSlot.Infusion_1]}-{Upgrades[UpgradeSlot.Infusion_2]})";
                    break;

                case GearSlot.Accessory_1:
                case GearSlot.Accessory_2:
                    upgradesCode = $"({Upgrades[UpgradeSlot.Infusion_1]})";
                    break;

                case GearSlot.Ring_1:
                case GearSlot.Ring_2:
                    upgradesCode = $"({Upgrades[UpgradeSlot.Infusion_1]}-{Upgrades[UpgradeSlot.Infusion_2]}-{Upgrades[UpgradeSlot.Infusion_3]})";
                    break;

                case GearSlot.MainHand:
                case GearSlot.AltMainHand:
                case GearSlot.OffHand:
                case GearSlot.AltOffHand:
                    upgradesCode = $"({Upgrades[UpgradeSlot.Sigil_1]}-{Upgrades[UpgradeSlot.Infusion_1]})";
                    break;

                case GearSlot.Aquatic:
                case GearSlot.AltAquatic:
                    upgradesCode = $"({Upgrades[UpgradeSlot.Sigil_1]}-{Upgrades[UpgradeSlot.Sigil_2]}-{Upgrades[UpgradeSlot.Infusion_1]}-{Upgrades[UpgradeSlot.Infusion_2]})";
                    break;
            }

            return $"[{(int)Stat}|{(int)WeaponType}|{upgradesCode}]";
        }

        public void FromCode(GearSlot slot, string code)
        {
            code = code.Substring(1, code.Length - 2);

            string[] parts = code.Split('|');
            string[] upgrades = parts[2].Replace("(", "").Split('-');

            if (slot is GearSlot.Head or GearSlot.Shoulder or GearSlot.Hand or GearSlot.Leg or GearSlot.Foot)
            {
                Upgrades[UpgradeSlot.Rune] = int.TryParse(upgrades[0], out int rune) ? rune : 0;
                Upgrades[UpgradeSlot.Infusion_1] = int.TryParse(upgrades[1], out int infusion1) ? infusion1 : 0;
            }
            else if (slot is GearSlot.Amulet)
            {
                Upgrades[UpgradeSlot.Enrichment] = int.TryParse(upgrades[0], out int enrichment) ? enrichment : 0;
            }
            else if (slot is GearSlot.Back)
            {
                Upgrades[UpgradeSlot.Infusion_1] = int.TryParse(upgrades[0], out int infusion1) ? infusion1 : 0;
                Upgrades[UpgradeSlot.Infusion_2] = int.TryParse(upgrades[1], out int infusion2) ? infusion2 : 0;
            }
            else if (slot is GearSlot.Accessory_1 or GearSlot.Accessory_2)
            {
                Upgrades[UpgradeSlot.Infusion_1] = int.TryParse(upgrades[0], out int infusion1) ? infusion1 : 0;
            }
            else if (slot is GearSlot.Ring_1 or GearSlot.Ring_2)
            {
                Upgrades[UpgradeSlot.Infusion_1] = int.TryParse(upgrades[0], out int infusion1) ? infusion1 : 0;
                Upgrades[UpgradeSlot.Infusion_2] = int.TryParse(upgrades[1], out int infusion2) ? infusion2 : 0;
                Upgrades[UpgradeSlot.Infusion_3] = int.TryParse(upgrades[2], out int infusion3) ? infusion3 : 0;
            }
            else if (slot is GearSlot.MainHand or GearSlot.AltMainHand or GearSlot.OffHand or GearSlot.AltOffHand)
            {
                Upgrades[UpgradeSlot.Sigil_1] = int.TryParse(upgrades[0], out int sigil) ? sigil : 0;
                Upgrades[UpgradeSlot.Infusion_1] = int.TryParse(upgrades[1], out int infusion1) ? infusion1 : 0;
            }
            else if (slot is GearSlot.Aquatic or GearSlot.AltAquatic)
            {
                Upgrades[UpgradeSlot.Sigil_1] = int.TryParse(upgrades[0], out int sigil) ? sigil : 0;
                Upgrades[UpgradeSlot.Sigil_2] = int.TryParse(upgrades[0], out int sigil2) ? sigil2 : 0;
                Upgrades[UpgradeSlot.Infusion_1] = int.TryParse(upgrades[1], out int infusion1) ? infusion1 : 0;
                Upgrades[UpgradeSlot.Infusion_2] = int.TryParse(upgrades[2], out int infusion2) ? infusion2 : 0;
            }

            Stat = (EquipmentStat)(int.TryParse(parts[0], out int stat) ? stat : 0);
            WeaponType = (WeaponType)(int.TryParse(parts[1], out int weaponType) ? weaponType : 0);
        }
    }
}
