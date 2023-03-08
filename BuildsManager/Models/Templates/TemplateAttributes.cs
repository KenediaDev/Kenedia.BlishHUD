namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class TemplateAttributes
    {
        public int Power { get; set; } = 1000;

        public int Toughness { get; set; } = 1000;

        public int Vitality { get; set; } = 1000;

        public int Precision { get; set; } = 1000;

        public int Ferocity { get; set; }

        public int ConditionDamage { get; set; }

        public int Expertise { get; set; }

        public int Concentration { get; set; }

        public int HealingPower { get; set; }

        public int AgonyResistance { get; set; }

        public int MagicFind { get; set; }

        public int BoonDuration { get; set; }

        public int ConditionDuration { get; set; }

        public int CritDamage { get; set; }

        public int CritChance { get; set; }

        public int Health { get; set; }

        public int Armor { get; set; } = 1000;

        public int ProfessionSpecific { get; set; }
    }
}