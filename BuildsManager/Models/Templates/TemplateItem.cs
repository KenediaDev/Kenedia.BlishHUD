using Kenedia.Modules.BuildsManager.DataModels.Stats;
using System;
using static Kenedia.Modules.BuildsManager.DataModels.Professions.Weapon;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class TemplateItem
    {
        public TemplateItem()
        {

        }

        public EquipmentStat Stat { get; set; } = EquipmentStat.None;

        public WeaponType? Weapon { get; set; }

        public UpgradeCollection Upgrades { get; set; } = new();

        public string ToCode(GearTemplateSlot slot)
        {
            return slot switch
            {
                GearTemplateSlot.Head or GearTemplateSlot.Shoulder or GearTemplateSlot.Chest or GearTemplateSlot.Hand or GearTemplateSlot.Leg or GearTemplateSlot.Foot or GearTemplateSlot.PvpAmulet
                    => $"[{(int)Stat}|({Upgrades[UpgradeSlot.Rune]})]",

                GearTemplateSlot.MainHand or GearTemplateSlot.AltMainHand or GearTemplateSlot.OffHand or GearTemplateSlot.AltOffHand
                    => $"[{(int)Stat}|{(int)(Weapon ?? WeaponType.Unknown)}|({Upgrades[UpgradeSlot.Sigil_1]}-{Upgrades[UpgradeSlot.PvpSigil_1]})]",

                GearTemplateSlot.Aquatic or GearTemplateSlot.AltAquatic
                    => $"[{(int)Stat}|{(int)(Weapon ?? WeaponType.Unknown)}|({Upgrades[UpgradeSlot.Sigil_1]}-{Upgrades[UpgradeSlot.Sigil_2]})]",
                _
                    => $"[{(int)Stat}]",
            };
        }

        public void FromCode(GearTemplateSlot slot, string code)
        {
            try
            {
                if (slot is GearTemplateSlot.Head or GearTemplateSlot.Shoulder or GearTemplateSlot.Chest or GearTemplateSlot.Hand or GearTemplateSlot.Leg or GearTemplateSlot.Foot or GearTemplateSlot.PvpAmulet)
                {
                    code = code.Substring(1, code.Length - 2);
                    string[] parts = code.Split('|');
                    string[] upgrades = parts[1].Replace("(", "").Split('-');
                    Upgrades[UpgradeSlot.Rune] = int.TryParse(upgrades[0], out int rune) ? rune : 0;
                    return;
                }
                else if (slot is GearTemplateSlot.MainHand or GearTemplateSlot.AltMainHand or GearTemplateSlot.OffHand or GearTemplateSlot.AltOffHand)
                {
                    code = code.Substring(1, code.Length - 2);
                    string[] parts = code.Split('|');
                    string[] upgrades = parts[2].Replace("(", "").Split('-');
                    Weapon = (WeaponType)(int.TryParse(parts[1], out int weaponType) ? weaponType : 0);
                    Upgrades[UpgradeSlot.Sigil_1] = int.TryParse(upgrades[0], out int sigil) ? sigil : 0;
                    Upgrades[UpgradeSlot.PvpSigil_1] = int.TryParse(upgrades[1], out int pvpsigil) ? pvpsigil : 0;
                    return;
                }
                else if (slot is GearTemplateSlot.Aquatic or GearTemplateSlot.AltAquatic)
                {
                    code = code.Substring(1, code.Length - 2);
                    string[] parts = code.Split('|');
                    string[] upgrades = parts[2].Replace("(", "").Split('-');
                    Weapon = (WeaponType)(int.TryParse(parts[1], out int weaponType) ? weaponType : 0);
                    Upgrades[UpgradeSlot.Sigil_1] = int.TryParse(upgrades[0], out int sigil) ? sigil : 0;
                    Upgrades[UpgradeSlot.Sigil_2] = int.TryParse(upgrades[1], out int sigil2) ? sigil2 : 0;
                    return;
                }

                code = code.Substring(1, code.Length - 1);
                Stat = (EquipmentStat)(int.TryParse(code, out int stat) ? stat : 0);

            }
            catch (Exception ex) 
            {

            }
        }
    }
}
