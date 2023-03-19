using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class GearCollection : Dictionary<GearTemplateSlot, TemplateItem>
    {
        public GearCollection()
        {
            foreach (GearTemplateSlot e in Enum.GetValues(typeof(GearTemplateSlot)))
            {
                Add(e, new());
            }
        }

        public string ToCode(GearTemplateSlot slot)
        {
            return this[slot].ToCode(slot);
        }

        public void FromCode(GearTemplateSlot slot, string code)
        {
            this[slot].FromCode(slot, code);
        }
    }

    public class InfusionCollection : Dictionary<AttributeType, int>
    {
        public InfusionCollection()
        {
            foreach (AttributeType e in Enum.GetValues(typeof(AttributeType)))
            {
                Add(e, 0);
            }
        }

        public void FromCode(string code)
        {
            code = code.Substring(1, code.Length - 2);
            string[] parts = code.Split('|');

            this[AttributeType.Power] = int.TryParse(parts[0], out int power) ? power : 0;
            this[AttributeType.Precision] = int.TryParse(parts[1], out int precision) ? precision : 0;
            this[AttributeType.CritDamage] = int.TryParse(parts[2], out int ferocity) ? ferocity : 0;
            this[AttributeType.Toughness] = int.TryParse(parts[3], out int toughness) ? toughness : 0;
            this[AttributeType.ConditionDamage] = int.TryParse(parts[4], out int condition) ? condition : 0;
            this[AttributeType.ConditionDuration] = int.TryParse(parts[5], out int expertise) ? expertise : 0;
            this[AttributeType.BoonDuration] = int.TryParse(parts[6], out int concentration) ? concentration : 0;
            this[AttributeType.Vitality] = int.TryParse(parts[7], out int vitality) ? vitality : 0;
        }

        public string ToCode()
        {
            return $"[{this[AttributeType.Power]}|{this[AttributeType.Precision]}|{this[AttributeType.CritDamage]}|{this[AttributeType.Toughness]}|{this[AttributeType.ConditionDamage]}|{this[AttributeType.ConditionDuration]}|{this[AttributeType.BoonDuration]}|{this[AttributeType.Vitality]}]";
        }
    }
}
