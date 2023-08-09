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

        public string ToCode(TemplateSlot slot)
        {
            return slot switch
            {
                TemplateSlot.Head or TemplateSlot.Shoulder or TemplateSlot.Chest or TemplateSlot.Hand or TemplateSlot.Leg or TemplateSlot.Foot or TemplateSlot.PvpAmulet
                    => $"[{(int)Stat}|({Upgrades[UpgradeSlot.Rune]})]",

                TemplateSlot.MainHand or TemplateSlot.AltMainHand or TemplateSlot.OffHand or TemplateSlot.AltOffHand
                    => $"[{(int)Stat}|{(int)(Weapon ?? WeaponType.Unknown)}|({Upgrades[UpgradeSlot.Sigil_1]}-{Upgrades[UpgradeSlot.PvpSigil_1]})]",

                TemplateSlot.Aquatic or TemplateSlot.AltAquatic
                    => $"[{(int)Stat}|{(int)(Weapon ?? WeaponType.Unknown)}|({Upgrades[UpgradeSlot.Sigil_1]}-{Upgrades[UpgradeSlot.Sigil_2]})]",
                _
                    => $"[{(int)Stat}]",
            };
        }

        public void FromCode(TemplateSlot slot, string code)
        {
            try
            {
                if (slot is TemplateSlot.Head or TemplateSlot.Shoulder or TemplateSlot.Chest or TemplateSlot.Hand or TemplateSlot.Leg or TemplateSlot.Foot or TemplateSlot.PvpAmulet)
                {
                    code = code.Substring(1, code.Length - 2);
                    string[] parts = code.Split('|');
                    string[] upgrades = parts[1].Replace("(", "").Split('-');
                    Upgrades[UpgradeSlot.Rune] = int.TryParse(upgrades[0], out int rune) ? rune : 0;
                    return;
                }
                else if (slot is TemplateSlot.MainHand or TemplateSlot.AltMainHand or TemplateSlot.OffHand or TemplateSlot.AltOffHand)
                {
                    code = code.Substring(1, code.Length - 2);
                    string[] parts = code.Split('|');
                    string[] upgrades = parts[2].Replace("(", "").Split('-');
                    Weapon = (WeaponType)(int.TryParse(parts[1], out int weaponType) ? weaponType : 0);
                    Upgrades[UpgradeSlot.Sigil_1] = int.TryParse(upgrades[0], out int sigil) ? sigil : 0;
                    Upgrades[UpgradeSlot.PvpSigil_1] = int.TryParse(upgrades[1], out int pvpsigil) ? pvpsigil : 0;
                    return;
                }
                else if (slot is TemplateSlot.Aquatic or TemplateSlot.AltAquatic)
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
