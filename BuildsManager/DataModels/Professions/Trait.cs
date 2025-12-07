using Blish_HUD.Content;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using APITrait = Gw2Sharp.WebApi.V2.Models.Trait;

namespace Kenedia.Modules.BuildsManager.DataModels.Professions
{
    [DataContract]
    public class Trait : IDisposable
    {
        private bool _isDisposed;

        public Trait()
        {
        }

        public Trait(APITrait trait)
        {
            Apply(trait);
        }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public int Index { get; set; }

        [DataMember]
        public int Specialization { get; set; }

        [DataMember]
        public LocalizedString Names { get; protected set; } = [];

        public string Name
        {
            get => Names.Text;
            set => Names.Text = value;
        }

        [DataMember]
        public LocalizedString Descriptions { get; protected set; } = [];

        public string Description
        {
            get => Descriptions.Text;
            set => Descriptions.Text = value;
        }

        [DataMember]
        public string ChatLink { get; set; }

        [DataMember]
        public int IconAssetId { get; set; }
        private AsyncTexture2D Icon
        {
            get
            {
                if (field is not null) return field;

                if (IconAssetId is not 0)
                    field = AsyncTexture2D.FromAssetId(IconAssetId);

                return field;
            }

            set;
        }

        [DataMember]
        public TraitSlot Type { get; set; }

        [DataMember]
        public Models.Templates.TraitTierType Tier { get; set; }

        [DataMember]
        public int Order { get; set; }

        public List<TraitFact> Facts { get; private set; }

        public List<TraitFact> TraitedFacts { get; private set; }

        [DataMember]
        public List<int> Skills = [];

        public void SetLiveAPI(APITrait trait)
        {
            Facts = trait.Facts?.ToList();
            TraitedFacts = trait.TraitedFacts?.ToList();
        }
        internal static Trait FromByte(byte order, Specialization specialization, Models.Templates.TraitTierType tier)
        {
            return order == 0 ? null :
                specialization?.MajorTraits.Where(e => e.Value.Tier == tier)?.ToList()?.Find(e => e.Value.Order == (int)order - 1).Value;
        }

        public void Dispose()
        {
            if(_isDisposed) return;
            _isDisposed = true;

            Icon = null;
        }

        internal void Apply(APITrait trait)
        {
            Id = trait.Id;
            Name = trait.Name;
            Description = trait.Description;
            Specialization = trait.Specialization;
            IconAssetId = trait.Icon.GetAssetIdFromRenderUrl();
            Tier = (Models.Templates.TraitTierType)trait.Tier;
            Order = trait.Order;
            Type = trait.Slot.Value;
            ChatLink = trait.CreateChatLink();

            if (trait.Skills is not null)
            {
                foreach (var s in trait.Skills)
                {
                    Skills.Add(s.Id);
                }
            }
        }
    }
}
