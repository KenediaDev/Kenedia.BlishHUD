using Gw2Sharp.WebApi.V2.Models;

namespace Kenedia.Modules.AdvancedBuildsManager.DataModels.Items
{
    public class BonusStat
    {
        /// <summary>
        /// <see cref="BonusType"/> of the Bonus
        /// </summary>
        public BonusType Type { get; set; }

        /// <summary>
        /// Amount of Attribute Points
        /// </summary>
        public int? Amount { get; set; }
        
        /// <summary>
        /// Duration/Amount in %
        /// </summary>
        public double? Factor { get; set; }

        /// <summary>
        /// <see cref="AttributeType"/> to be converted to the <see cref="Type"/>
        /// </summary>
        public AttributeType? ConversionSourceType { get; set; }
    }
}
