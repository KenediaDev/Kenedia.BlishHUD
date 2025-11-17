using Blish_HUD.Content;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using System.Collections.Generic;
using System.Runtime.Serialization;
using APIPet = Gw2Sharp.WebApi.V2.Models.Pet;

namespace Kenedia.Modules.AdvancedBuildsManager.DataModels.Professions
{
    [DataContract]
    public class Pet
    {
        private AsyncTexture2D _icon;

        public Pet()
        {

        }

        public Pet(APIPet pet)
        {
            Id = pet.Id;
            IconAssetId = pet.Icon.GetAssetIdFromRenderUrl();

            ApplyLanguage(pet);
        }

        public Pet(APIPet pet, List<Skill> skills) : this(pet)
        {
            foreach (PetSkill petSkill in pet.Skills)
            {
                var skill = skills.Find(e => e.Id == petSkill.Id);

                if (skill is not null)
                {
                    Skills.Add(petSkill.Id, skill);
                }
            }

            ApplyLanguage(pet, skills);
        }

        [DataMember]
        public int Id { get; set; }

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

        [DataMember]
        public Dictionary<int, Skill> Skills { get; set; } = [];

        public static Pet FromByte(byte id)
        {
            return AdvancedBuildsManager.Data.Pets?.TryGetValue((int)id, out Pet pet) == true ? pet : null;
        }

        public void ApplyLanguage(APIPet pet)
        {
            Name = pet.Name;
            Description = pet.Description;
        }

        public void ApplyLanguage(APIPet pet, List<Skill> skills)
        {
            ApplyLanguage(pet);

            foreach (var petSkill in Skills)
            {
                var skill = skills.Find(e => e.Id == petSkill.Value.Id);

                if (skill is not null)
                {
                    petSkill.Value.Name = skill.Name;
                    petSkill.Value.Description = skill.Description;
                }
            }
        }
    }
}
