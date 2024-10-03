using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.TemplateEntries;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Utility
{
    public class GearChatCode
    {
        private enum TemplateBytePosition : byte
        {
            MainHandWeaponType = 0,
            MainHandStat = 1,
            MainHandSigil1 = 2,
            MainHandPvpSigil = 3,
            MainHandInfusion1 = 4,

            OffHandWeaponType = 5,
            OffHandStat = 6,
            OffHandSigil1 = 7,
            OffHandPvpSigil = 8,
            OffHandInfusion1 = 9,

            AltMainHandWeaponType = 10,
            AltMainHandStat = 11,
            AltMainHandSigil1 = 12,
            AltMainHandPvpSigil = 13,
            AltMainHandInfusion1 = 14,

            AltOffHandWeaponType = 15,
            AltOffHandStat = 16,
            AltOffHandSigil1 = 17,
            AltOffHandPvpSigil = 18,
            AltOffHandInfusion1 = 19,

            HeadStat = 20,
            HeadRune = 21,
            HeadInfusion1 = 22,

            ShoulderStat = 23,
            ShoulderRune = 24,
            ShoulderInfusion1 = 25,

            ChestStat = 26,
            ChestRune = 27,
            ChestInfusion1 = 28,

            HandStat = 29,
            HandRune = 30,
            HandInfusion1 = 31,

            LegStat = 32,
            LegRune = 33,
            LegInfusion1 = 34,

            FootStat = 35,
            FootRune = 36,
            FootInfusion1 = 37,

            BackStat = 38,
            BackInfusion1 = 39,
            BackInfusion2 = 40,

            AmuletStat = 41,
            AmuletEnrichment = 42,

            Accessory1Stat = 43,
            Accessory1Infusion1 = 44,

            Accessory2Stat = 45,
            Accessory2Infusion1 = 46,

            Ring1Stat = 47,
            Ring1Infusion1 = 48,
            Ring1Infusion2 = 49,
            Ring1Infusion3 = 50,

            Ring2Stat = 51,
            Ring2Infusion1 = 52,
            Ring2Infusion2 = 53,
            Ring2Infusion3 = 54,

            AquaBreatherStat = 55,
            AquaBreatherRune = 56,
            AquaBreatherInfusion1 = 57,

            AquaticWeaponType = 58,
            AquaticStat = 59,
            AquaticSigil1 = 60,
            AquaticSigil2 = 61,
            AquaticInfusion1 = 62,
            AquaticInfusion2 = 63,

            AltAquaticWeaponType = 64,
            AltAquaticStat = 65,
            AltAquaticSigil1 = 66,
            AltAquaticSigil2 = 67,
            AltAquaticInfusion1 = 68,
            AltAquaticInfusion2 = 69,

            PvpAmulet = 70,
            PvpAmuletRune = 71,

            Nourishment = 72,

            Enhancement = 73,

            PowerCore = 74,
            
            PveRelic = 75,

            PvpRelic = 76,
        }

        public static string GetGearChatCode(Template template)
        {
            //Create code array with all TemplateBytePosition
            byte[] codeArray = new byte[77];

            // MainHand
            codeArray[(byte)TemplateBytePosition.MainHandWeaponType] = (byte)(template.MainHand.Weapon?.WeaponType ?? ItemWeaponType.Unknown);
            codeArray[(byte)TemplateBytePosition.MainHandStat] = template.MainHand.Stat?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.MainHandSigil1] = template.MainHand.Sigil1?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.MainHandPvpSigil] = template.MainHand.PvpSigil?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.MainHandInfusion1] = template.MainHand.Infusion1?.MappedId ?? 0;

            // OffHand
            codeArray[(byte)TemplateBytePosition.OffHandWeaponType] = (byte)(template.OffHand.Weapon?.WeaponType ?? ItemWeaponType.Unknown);
            codeArray[(byte)TemplateBytePosition.OffHandStat] = template.OffHand.Stat?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.OffHandSigil1] = template.OffHand.Sigil1?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.OffHandPvpSigil] = template.OffHand.PvpSigil?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.OffHandInfusion1] = template.OffHand.Infusion1?.MappedId ?? 0;

            //AltMainHand
            codeArray[(byte)TemplateBytePosition.AltMainHandWeaponType] = (byte)(template.AltMainHand.Weapon?.WeaponType ?? ItemWeaponType.Unknown);
            codeArray[(byte)TemplateBytePosition.AltMainHandStat] = template.AltMainHand.Stat?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.AltMainHandSigil1] = template.AltMainHand.Sigil1?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.AltMainHandPvpSigil] = template.AltMainHand.PvpSigil?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.AltMainHandInfusion1] = template.AltMainHand.Infusion1?.MappedId ?? 0;

            //AltOffHand
            codeArray[(byte)TemplateBytePosition.AltOffHandWeaponType] = (byte)(template.AltOffHand.Weapon?.WeaponType ?? ItemWeaponType.Unknown);
            codeArray[(byte)TemplateBytePosition.AltOffHandStat] = template.AltOffHand.Stat?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.AltOffHandSigil1] = template.AltOffHand.Sigil1?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.AltOffHandPvpSigil] = template.AltOffHand.PvpSigil?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.AltOffHandInfusion1] = template.AltOffHand.Infusion1?.MappedId ?? 0;

            //Head
            codeArray[(byte)TemplateBytePosition.HeadStat] = template.Head.Stat?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.HeadRune] = template.Head.Rune?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.HeadInfusion1] = template.Head.Infusion1?.MappedId ?? 0;

            //Shoulder
            codeArray[(byte)TemplateBytePosition.ShoulderStat] = template.Shoulder.Stat?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.ShoulderRune] = template.Shoulder.Rune?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.ShoulderInfusion1] = template.Shoulder.Infusion1?.MappedId ?? 0;

            //Chest
            codeArray[(byte)TemplateBytePosition.ChestStat] = template.Chest.Stat?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.ChestRune] = template.Chest.Rune?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.ChestInfusion1] = template.Chest.Infusion1?.MappedId ?? 0;

            //Hand
            codeArray[(byte)TemplateBytePosition.HandStat] = template.Hand.Stat?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.HandRune] = template.Hand.Rune?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.HandInfusion1] = template.Hand.Infusion1?.MappedId ?? 0;

            //Leg
            codeArray[(byte)TemplateBytePosition.LegStat] = template.Leg.Stat?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.LegRune] = template.Leg.Rune?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.LegInfusion1] = template.Leg.Infusion1?.MappedId ?? 0;

            //Foot
            codeArray[(byte)TemplateBytePosition.FootStat] = template.Foot.Stat?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.FootRune] = template.Foot.Rune?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.FootInfusion1] = template.Foot.Infusion1?.MappedId ?? 0;

            //Back
            codeArray[(byte)TemplateBytePosition.BackStat] = template.Back.Stat?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.BackInfusion1] = template.Back.Infusion1?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.BackInfusion2] = template.Back.Infusion2?.MappedId ?? 0;

            //Amulet
            codeArray[(byte)TemplateBytePosition.AmuletStat] = template.Amulet.Stat?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.AmuletEnrichment] = template.Amulet.Enrichment?.MappedId ?? 0;

            //Accessory_1
            codeArray[(byte)TemplateBytePosition.Accessory1Stat] = template.Accessory_1.Stat?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.Accessory1Infusion1] = template.Accessory_1.Infusion1?.MappedId ?? 0;

            //Accessory_2   
            codeArray[(byte)TemplateBytePosition.Accessory2Stat] = template.Accessory_2.Stat?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.Accessory2Infusion1] = template.Accessory_2.Infusion1?.MappedId ?? 0;

            //Ring_1
            codeArray[(byte)TemplateBytePosition.Ring1Stat] = template.Ring_1.Stat?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.Ring1Infusion1] = template.Ring_1.Infusion1?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.Ring1Infusion2] = template.Ring_1.Infusion2?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.Ring1Infusion3] = template.Ring_1.Infusion3?.MappedId ?? 0;

            //Ring_2
            codeArray[(byte)TemplateBytePosition.Ring2Stat] = template.Ring_2.Stat?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.Ring2Infusion1] = template.Ring_2.Infusion1?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.Ring2Infusion2] = template.Ring_2.Infusion2?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.Ring2Infusion3] = template.Ring_2.Infusion3?.MappedId ?? 0;

            //AquaBreather
            codeArray[(byte)TemplateBytePosition.AquaBreatherStat] = template.AquaBreather.Stat?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.AquaBreatherRune] = template.AquaBreather.Rune?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.AquaBreatherInfusion1] = template.AquaBreather.Infusion1?.MappedId ?? 0;

            //Aquatic
            codeArray[(byte)TemplateBytePosition.AquaticWeaponType] = (byte)(template.Aquatic.Weapon?.WeaponType ?? ItemWeaponType.Unknown);
            codeArray[(byte)TemplateBytePosition.AquaticStat] = template.Aquatic.Stat?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.AquaticSigil1] = template.Aquatic.Sigil1?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.AquaticSigil2] = template.Aquatic.Sigil2?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.AquaticInfusion1] = template.Aquatic.Infusion1?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.AquaticInfusion2] = template.Aquatic.Infusion2?.MappedId ?? 0;

            //AltAquatic
            codeArray[(byte)TemplateBytePosition.AltAquaticWeaponType] = (byte)(template.AltAquatic.Weapon?.WeaponType ?? ItemWeaponType.Unknown);
            codeArray[(byte)TemplateBytePosition.AltAquaticStat] = template.AltAquatic.Stat?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.AltAquaticSigil1] = template.AltAquatic.Sigil1?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.AltAquaticSigil2] = template.AltAquatic.Sigil2?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.AltAquaticInfusion1] = template.AltAquatic.Infusion1?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.AltAquaticInfusion2] = template.AltAquatic.Infusion2?.MappedId ?? 0;

            //PvpAmulet
            codeArray[(byte)TemplateBytePosition.PvpAmulet] = template.PvpAmulet.PvpAmulet?.MappedId ?? 0;
            codeArray[(byte)TemplateBytePosition.PvpAmuletRune] = template.PvpAmulet.Rune?.MappedId ?? 0;

            //Nourishment
            codeArray[(byte)TemplateBytePosition.Nourishment] = template.Nourishment.Nourishment?.MappedId ?? 0;

            //Enhancement
            codeArray[(byte)TemplateBytePosition.Enhancement] = template.Enhancement.Enhancement?.MappedId ?? 0;

            //PowerCore
            codeArray[(byte)TemplateBytePosition.PowerCore] = template.PowerCore.PowerCore?.MappedId ?? 0;

            //PveRelic
            codeArray[(byte)TemplateBytePosition.PveRelic] = template.PveRelic.Relic?.MappedId ?? 0;

            //PvpRelic
            codeArray[(byte)TemplateBytePosition.PvpRelic] = template.PvpRelic.Relic?.MappedId ?? 0;

            return $"[&{Convert.ToBase64String([.. codeArray])}]";
        }

        public static void LoadTemplateFromChatCode(Template template, string? chatCode)
        {
            if (string.IsNullOrEmpty(chatCode))
            {
                return;
            }

            byte[] array = Convert.FromBase64String(GearTemplateCode.PrepareBase64String(chatCode));

            // MainHand
            template.SetItem(template.MainHand.Slot, TemplateSubSlotType.Item, Enum.TryParse($"{array[(byte)TemplateBytePosition.MainHandWeaponType]}", out ItemWeaponType mainHandWeaponType) ? BuildsManager.Data.Weapons.Values.Where(e => e.WeaponType == mainHandWeaponType).FirstOrDefault() : null);
            template.SetItem(template.MainHand.Slot, TemplateSubSlotType.Stat, BuildsManager.Data.Stats.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.MainHandStat]).FirstOrDefault().Value);
            template.SetItem(template.MainHand.Slot, TemplateSubSlotType.Sigil1, BuildsManager.Data.PveSigils.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.MainHandSigil1]).FirstOrDefault().Value);
            template.SetItem(template.MainHand.Slot, TemplateSubSlotType.Infusion1, BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.MainHandInfusion1]).FirstOrDefault().Value);
            template.SetItem(template.MainHand.Slot, TemplateSubSlotType.PvpSigil, BuildsManager.Data.PvpSigils.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.MainHandPvpSigil]).FirstOrDefault().Value);

            // OffHand
            template.SetItem(template.OffHand.Slot, TemplateSubSlotType.Item, Enum.TryParse($"{array[(byte)TemplateBytePosition.OffHandWeaponType]}", out ItemWeaponType offHandWeaponType) ? BuildsManager.Data.Weapons.Values.Where(e => e.WeaponType == offHandWeaponType).FirstOrDefault() : null);
            template.SetItem(template.OffHand.Slot, TemplateSubSlotType.Stat, BuildsManager.Data.Stats.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.OffHandStat]).FirstOrDefault().Value);
            template.SetItem(template.OffHand.Slot, TemplateSubSlotType.Sigil1, BuildsManager.Data.PveSigils.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.OffHandSigil1]).FirstOrDefault().Value);
            template.SetItem(template.OffHand.Slot, TemplateSubSlotType.Infusion1, BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.OffHandInfusion1]).FirstOrDefault().Value);
            template.SetItem(template.OffHand.Slot, TemplateSubSlotType.PvpSigil, BuildsManager.Data.PvpSigils.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.OffHandPvpSigil]).FirstOrDefault().Value);

            //AltMainHand
            template.SetItem(template.AltMainHand.Slot, TemplateSubSlotType.Item, Enum.TryParse($"{array[(byte)TemplateBytePosition.AltMainHandWeaponType]}", out ItemWeaponType altMainHandWeaponType) ? BuildsManager.Data.Weapons.Values.Where(e => e.WeaponType == altMainHandWeaponType).FirstOrDefault() : null);
            template.SetItem(template.AltMainHand.Slot, TemplateSubSlotType.Stat, BuildsManager.Data.Stats.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.AltMainHandStat]).FirstOrDefault().Value);
            template.SetItem(template.AltMainHand.Slot, TemplateSubSlotType.Sigil1, BuildsManager.Data.PveSigils.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.AltMainHandSigil1]).FirstOrDefault().Value);
            template.SetItem(template.AltMainHand.Slot, TemplateSubSlotType.Infusion1, BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.AltMainHandInfusion1]).FirstOrDefault().Value);
            template.SetItem(template.AltMainHand.Slot, TemplateSubSlotType.PvpSigil, BuildsManager.Data.PvpSigils.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.AltMainHandPvpSigil]).FirstOrDefault().Value);

            //AltOffHand
            template.SetItem(template.AltOffHand.Slot, TemplateSubSlotType.Item, Enum.TryParse($"{array[(byte)TemplateBytePosition.AltOffHandWeaponType]}", out ItemWeaponType altOffHandWeaponType) ? BuildsManager.Data.Weapons.Values.Where(e => e.WeaponType == altOffHandWeaponType).FirstOrDefault() : null);
            template.SetItem(template.AltOffHand.Slot, TemplateSubSlotType.Stat, BuildsManager.Data.Stats.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.AltOffHandStat]).FirstOrDefault().Value);
            template.SetItem(template.AltOffHand.Slot, TemplateSubSlotType.Sigil1, BuildsManager.Data.PveSigils.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.AltOffHandSigil1]).FirstOrDefault().Value);
            template.SetItem(template.AltOffHand.Slot, TemplateSubSlotType.Infusion1, BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.AltOffHandInfusion1]).FirstOrDefault().Value);
            template.SetItem(template.AltOffHand.Slot, TemplateSubSlotType.PvpSigil, BuildsManager.Data.PvpSigils.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.AltOffHandPvpSigil]).FirstOrDefault().Value);

            //Head
            template.SetItem(template.Head.Slot, TemplateSubSlotType.Stat, BuildsManager.Data.Stats.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.HeadStat]).FirstOrDefault().Value);
            template.SetItem(template.Head.Slot, TemplateSubSlotType.Rune, BuildsManager.Data.PveRunes.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.HeadRune]).FirstOrDefault().Value);
            template.SetItem(template.Head.Slot, TemplateSubSlotType.Infusion1, BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.HeadInfusion1]).FirstOrDefault().Value);

            //Shoulder
            template.SetItem(template.Shoulder.Slot, TemplateSubSlotType.Stat, BuildsManager.Data.Stats.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.ShoulderStat]).FirstOrDefault().Value);
            template.SetItem(template.Shoulder.Slot, TemplateSubSlotType.Rune, BuildsManager.Data.PveRunes.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.ShoulderRune]).FirstOrDefault().Value);
            template.SetItem(template.Shoulder.Slot, TemplateSubSlotType.Infusion1, BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.ShoulderInfusion1]).FirstOrDefault().Value);

            //Chest
            template.SetItem(template.Chest.Slot, TemplateSubSlotType.Stat, BuildsManager.Data.Stats.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.ChestStat]).FirstOrDefault().Value);
            template.SetItem(template.Chest.Slot, TemplateSubSlotType.Rune, BuildsManager.Data.PveRunes.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.ChestRune]).FirstOrDefault().Value);
            template.SetItem(template.Chest.Slot, TemplateSubSlotType.Infusion1, BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.ChestInfusion1]).FirstOrDefault().Value);

            //Hand
            template.SetItem(template.Hand.Slot, TemplateSubSlotType.Stat, BuildsManager.Data.Stats.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.HandStat]).FirstOrDefault().Value);
            template.SetItem(template.Hand.Slot, TemplateSubSlotType.Rune, BuildsManager.Data.PveRunes.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.HandRune]).FirstOrDefault().Value);
            template.SetItem(template.Hand.Slot, TemplateSubSlotType.Infusion1, BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.HandInfusion1]).FirstOrDefault().Value);

            //Leg
            template.SetItem(template.Leg.Slot, TemplateSubSlotType.Stat, BuildsManager.Data.Stats.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.LegStat]).FirstOrDefault().Value);
            template.SetItem(template.Leg.Slot, TemplateSubSlotType.Rune, BuildsManager.Data.PveRunes.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.LegRune]).FirstOrDefault().Value);
            template.SetItem(template.Leg.Slot, TemplateSubSlotType.Infusion1, BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.LegInfusion1]).FirstOrDefault().Value);

            //Foot
            template.SetItem(template.Foot.Slot, TemplateSubSlotType.Stat, BuildsManager.Data.Stats.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.FootStat]).FirstOrDefault().Value);
            template.SetItem(template.Foot.Slot, TemplateSubSlotType.Rune, BuildsManager.Data.PveRunes.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.FootRune]).FirstOrDefault().Value);
            template.SetItem(template.Foot.Slot, TemplateSubSlotType.Infusion1, BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.FootInfusion1]).FirstOrDefault().Value);

            //Back
            template.SetItem(template.Back.Slot, TemplateSubSlotType.Stat, BuildsManager.Data.Stats.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.BackStat]).FirstOrDefault().Value);
            template.SetItem(template.Back.Slot, TemplateSubSlotType.Infusion1, BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.BackInfusion1]).FirstOrDefault().Value);
            template.SetItem(template.Back.Slot, TemplateSubSlotType.Infusion2, BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.BackInfusion2]).FirstOrDefault().Value);

            //Amulet
            template.SetItem(template.Amulet.Slot, TemplateSubSlotType.Stat, BuildsManager.Data.Stats.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.AmuletStat]).FirstOrDefault().Value);
            template.SetItem(template.Amulet.Slot, TemplateSubSlotType.Enrichment, BuildsManager.Data.Enrichments.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.AmuletEnrichment]).FirstOrDefault().Value);

            //Accessory_1
            template.SetItem(template.Accessory_1.Slot, TemplateSubSlotType.Stat, BuildsManager.Data.Stats.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.Accessory1Stat]).FirstOrDefault().Value);
            template.SetItem(template.Accessory_1.Slot, TemplateSubSlotType.Infusion1, BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.Accessory1Infusion1]).FirstOrDefault().Value);

            //Accessory_2
            template.SetItem(template.Accessory_2.Slot, TemplateSubSlotType.Stat, BuildsManager.Data.Stats.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.Accessory2Stat]).FirstOrDefault().Value);
            template.SetItem(template.Accessory_2.Slot, TemplateSubSlotType.Infusion1, BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.Accessory2Infusion1]).FirstOrDefault().Value);

            //Ring_1
            template.SetItem(template.Ring_1.Slot, TemplateSubSlotType.Stat, BuildsManager.Data.Stats.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.Ring1Stat]).FirstOrDefault().Value);
            template.SetItem(template.Ring_1.Slot, TemplateSubSlotType.Infusion1, BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.Ring1Infusion1]).FirstOrDefault().Value);
            template.SetItem(template.Ring_1.Slot, TemplateSubSlotType.Infusion2, BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.Ring1Infusion2]).FirstOrDefault().Value);
            template.SetItem(template.Ring_1.Slot, TemplateSubSlotType.Infusion3, BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.Ring1Infusion3]).FirstOrDefault().Value);

            //Ring_2
            template.SetItem(template.Ring_2.Slot, TemplateSubSlotType.Stat, BuildsManager.Data.Stats.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.Ring2Stat]).FirstOrDefault().Value);
            template.SetItem(template.Ring_2.Slot, TemplateSubSlotType.Infusion1, BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.Ring2Infusion1]).FirstOrDefault().Value);
            template.SetItem(template.Ring_2.Slot, TemplateSubSlotType.Infusion2, BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.Ring2Infusion2]).FirstOrDefault().Value);
            template.SetItem(template.Ring_2.Slot, TemplateSubSlotType.Infusion3, BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.Ring2Infusion3]).FirstOrDefault().Value);

            //AquaBreather
            template.SetItem(template.AquaBreather.Slot, TemplateSubSlotType.Stat, BuildsManager.Data.Stats.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.AquaBreatherStat]).FirstOrDefault().Value);
            template.SetItem(template.AquaBreather.Slot, TemplateSubSlotType.Rune, BuildsManager.Data.PveRunes.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.AquaBreatherRune]).FirstOrDefault().Value);
            template.SetItem(template.AquaBreather.Slot, TemplateSubSlotType.Infusion1, BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.AquaBreatherInfusion1]).FirstOrDefault().Value);

            //Aquatic
            template.SetItem(template.Aquatic.Slot, TemplateSubSlotType.Item, Enum.TryParse($"{array[(byte)TemplateBytePosition.AquaticWeaponType]}", out ItemWeaponType aquaticWeaponType) ? BuildsManager.Data.Weapons.Values.Where(e => e.WeaponType == aquaticWeaponType).FirstOrDefault() : null);
            template.SetItem(template.Aquatic.Slot, TemplateSubSlotType.Stat, BuildsManager.Data.Stats.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.AquaticStat]).FirstOrDefault().Value);
            template.SetItem(template.Aquatic.Slot, TemplateSubSlotType.Sigil1, BuildsManager.Data.PveSigils.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.AquaticSigil1]).FirstOrDefault().Value);
            template.SetItem(template.Aquatic.Slot, TemplateSubSlotType.Sigil2, BuildsManager.Data.PveSigils.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.AquaticSigil2]).FirstOrDefault().Value);
            template.SetItem(template.Aquatic.Slot, TemplateSubSlotType.Infusion1, BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.AquaticInfusion1]).FirstOrDefault().Value);
            template.SetItem(template.Aquatic.Slot, TemplateSubSlotType.Infusion2, BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.AquaticInfusion2]).FirstOrDefault().Value);

            //AltAquatic
            template.SetItem(template.AltAquatic.Slot, TemplateSubSlotType.Item, Enum.TryParse($"{array[(byte)TemplateBytePosition.AltAquaticWeaponType]}", out ItemWeaponType altAquaticWeaponType) ? BuildsManager.Data.Weapons.Values.Where(e => e.WeaponType == altAquaticWeaponType).FirstOrDefault() : null);
            template.SetItem(template.AltAquatic.Slot, TemplateSubSlotType.Stat, BuildsManager.Data.Stats.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.AltAquaticStat]).FirstOrDefault().Value);
            template.SetItem(template.AltAquatic.Slot, TemplateSubSlotType.Sigil1, BuildsManager.Data.PveSigils.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.AltAquaticSigil1]).FirstOrDefault().Value);
            template.SetItem(template.AltAquatic.Slot, TemplateSubSlotType.Sigil2, BuildsManager.Data.PveSigils.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.AltAquaticSigil2]).FirstOrDefault().Value);
            template.SetItem(template.AltAquatic.Slot, TemplateSubSlotType.Infusion1, BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.AltAquaticInfusion1]).FirstOrDefault().Value);
            template.SetItem(template.AltAquatic.Slot, TemplateSubSlotType.Infusion2, BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.AltAquaticInfusion2]).FirstOrDefault().Value);

            //PvpAmulet
            template.SetItem(template.PvpAmulet.Slot, TemplateSubSlotType.Item, BuildsManager.Data.PvpAmulets.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.PvpAmulet]).FirstOrDefault().Value);
            template.SetItem(template.PvpAmulet.Slot, TemplateSubSlotType.Rune, BuildsManager.Data.PvpRunes.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.PvpAmuletRune]).FirstOrDefault().Value);

            //Nourishment
            template.SetItem(template.Nourishment.Slot, TemplateSubSlotType.Item, BuildsManager.Data.Nourishments.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.Nourishment]).FirstOrDefault().Value);

            //Enhancement
            template.SetItem(template.Enhancement.Slot, TemplateSubSlotType.Item, BuildsManager.Data.Enhancements.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.Enhancement]).FirstOrDefault().Value);

            //PowerCore
            template.SetItem(template.PowerCore.Slot, TemplateSubSlotType.Item, BuildsManager.Data.PowerCores.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.PowerCore]).FirstOrDefault().Value);

            //PveRelic
            template.SetItem(template.PveRelic.Slot, TemplateSubSlotType.Item, BuildsManager.Data.PveRelics.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.PveRelic]).FirstOrDefault().Value);

            //PvpRelic
            template.SetItem(template.PvpRelic.Slot, TemplateSubSlotType.Item, BuildsManager.Data.PvpRelics.Items.Where(e => e.Value.MappedId == array[(byte)TemplateBytePosition.PvpRelic]).FirstOrDefault().Value);


        }
    }
}
