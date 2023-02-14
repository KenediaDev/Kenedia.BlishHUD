using Blish_HUD.Content;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using APITrait = Gw2Sharp.WebApi.V2.Models.Trait;

namespace Kenedia.Modules.BuildsManager.DataModels.Professions
{
    [DataContract]
    public class Trait
    {
        private AsyncTexture2D _icon;

        public Trait()
        {
        }

        public Trait(APITrait trait)
        {
            Id = trait.Id;
            Name = trait.Name;
            Description = trait.Description;
            Specialization = trait.Specialization;
            IconAssetId = trait.Icon.GetAssetIdFromRenderUrl();
            Tier = (Models.Templates.TraitTier)trait.Tier;
            Order = trait.Order;
            Type = trait.Slot.Value;
            ChatLink = trait.CreateChatLink();
        }

        public Trait(APITrait trait, Dictionary<int, Skill> skills) : this(trait)
        {
            if(trait.Skills != null)
            {
                foreach(var s in trait.Skills)
                {
                    if(skills.TryGetValue(s.Id, out Skill skill))
                    {
                        // TODO: Add traited skills
                        Skills.Add(skill.Id);
                    }
                }
            }
        }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public int Index { get; set; }

        [DataMember]
        public int Specialization { get; set; }

        [DataMember]
        public LocalizedString Names { get; protected set; } = new();

        public string Name
        {
            get => Names.Text;
            set => Names.Text = value;
        }

        [DataMember]
        public LocalizedString Descriptions { get; protected set; } = new();

        public string Description
        {
            get => Descriptions.Text;
            set => Descriptions.Text = value;
        }

        [DataMember]
        public string ChatLink { get; set; }

        [DataMember]
        public int IconAssetId { get; set; }
        public AsyncTexture2D Icon
        {
            get
            {
                if (_icon != null) return _icon;

                _icon = AsyncTexture2D.FromAssetId(IconAssetId);
                return _icon;
            }
        }

        [DataMember]
        public TraitSlot Type { get; set; }

        [DataMember]
        public Models.Templates.TraitTier Tier { get; set; }

        [DataMember]
        public int Order { get; set; }

        [DataMember]
        public List<int> Skills = new();

        internal static Trait FromByte(byte order, Specialization specialization, Models.Templates.TraitTier tier)
        {
            return order == 0 ? null :
                specialization?.MajorTraits.Where(e => e.Value.Tier == tier)?.ToList()?.Find(e => e.Value.Order == (int)order - 1).Value;
        }
    }
}
