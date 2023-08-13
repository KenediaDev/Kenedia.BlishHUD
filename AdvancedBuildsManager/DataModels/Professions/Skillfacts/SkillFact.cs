using Blish_HUD.Content;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using System.Runtime.Serialization;

namespace Kenedia.Modules.AdvancedBuildsManager.DataModels.Professions
{
    [DataContract]
    public class SkillFactAttributeAdjust : SkillFact
    {
        public SkillFactAttributeAdjust(Gw2Sharp.WebApi.V2.Models.SkillFactAttributeAdjust fact) : base(fact)
        {
        }
    }

    [DataContract]
    public class SkillFactBuff : SkillFact
    {
        public SkillFactBuff(Gw2Sharp.WebApi.V2.Models.SkillFactBuff fact) : base(fact)
        {
        }
    }

    [DataContract]
    public class SkillFactComboField : SkillFact
    {
        public SkillFactComboField(Gw2Sharp.WebApi.V2.Models.SkillFactComboField fact) : base(fact)
        {
        }
    }

    [DataContract]
    public class SkillFactComboFinisher : SkillFact
    {

    }

    [DataContract]
    public class SkillFactDamage : SkillFact
    {

    }

    [DataContract]
    public class SkillFactDistance : SkillFact
    {

    }

    [DataContract]
    public class SkillFactDuration : SkillFact
    {

    }

    [DataContract]
    public class SkillFactHeal : SkillFact
    {

    }

    [DataContract]
    public class SkillFactHealingAdjust : SkillFact
    {

    }

    [DataContract]
    public class SkillFactNumber : SkillFact
    {

    }

    [DataContract]
    public class SkillFactPercent : SkillFact
    {

    }

    [DataContract]
    public class SkillFactPrefixedBuff : SkillFact
    {

    }

    [DataContract]
    public class SkillFactRadius : SkillFact
    {

    }

    [DataContract]
    public class SkillFactRange : SkillFact
    {

    }

    [DataContract]
    public class SkillFactRecharge : SkillFact
    {

    }

    [DataContract]
    public class SkillFactStunBreak : SkillFact
    {

    }

    [DataContract]
    public class SkillFactTime : SkillFact
    {

    }

    [DataContract]
    public class SkillFactUnblockable : SkillFact
    {

    }

    [DataContract]
    public class SkillFact
    {
        private AsyncTexture2D _icon;

        public SkillFact()
        {

        }

        public SkillFact(Gw2Sharp.WebApi.V2.Models.SkillFact fact)
        {
            Text = fact.Text;
            IconAssetId = fact.Icon.GetAssetIdFromRenderUrl();
            RequiresTrait = fact.RequiresTrait;
            Type = fact.Type.Value;
            Overrides = fact.Overrides;
        }

        [DataMember]
        public int? RequiresTrait { get; set; }

        [DataMember]
        public int? Overrides { get; set; }

        [DataMember]
        public SkillFactType Type { get; }

        [DataMember]
        public LocalizedString Texts { get; protected set; } = new();

        public string Text
        {
            get => Texts.Text;
            set => Texts.Text = value;
        }

        [DataMember]
        public int IconAssetId { get; set; }
        public AsyncTexture2D Icon
        {
            get
            {
                if (_icon is not null) return _icon;

                _icon = AsyncTexture2D.FromAssetId(IconAssetId);
                return _icon;
            }
        }
    }
}
