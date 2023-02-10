using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Utility
{
    public class StatUtility
    {
        public static string UniformAttributeName(string statName)
        {
            return statName switch
            {
                "ConditionDamage" => "Condition Damage",
                "BoonDuration" => "Concentration",
                "ConditionDuration" => "Expertise",
                "Healing" => "Healing Power",
                "CritDamage" => "Ferocity",
                _ => statName,
            };
        }
    }
}
