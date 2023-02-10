using Blish_HUD.Content;
using Gw2Sharp;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using APISkill = Gw2Sharp.WebApi.V2.Models.Skill;

namespace Kenedia.Modules.BuildsManager.DataModels.Professions
{
    [DataContract]
    public class Skill
    {
        private AsyncTexture2D _icon;

        public Skill(APISkill skill)
        {
            Id = skill.Id;
            IconAssetId = skill.Icon.GetAssetIdFromRenderUrl();
            Name = skill.Name;
            Description = skill.Description;
            Specialization = skill.Specialization != null ? (int) skill.Specialization : 0;
            ChatLink = skill.ChatLink;
            Flags = skill.Flags.Count() > 0 ? skill.Flags.Aggregate((x, y) => x |= y.Value) : SkillFlag.Unknown;
        }

        public enum SkillSlot
        {
            Weapon_1 = 1,
            Weapon_2 = 2,
            Weapon_3 = 3,
            Weapon_4 = 4,
            Weapon_5 = 5,
            Profession_1 = 6,
            Profession_2 = 7,
            Profession_3 = 8,
            Profession_4 = 9,
            Profession_5 = 10,
            Heal = 11,
            Utility = 12,
            Elite = 13,
        }

        [DataMember]
        public LocalizedString Names { get; protected set; } = new();

        public string Name
        {
            get => Names.Text;
            set => Names.Text = value;
        }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public int Specialization { get; set; }

        [DataMember]
        public int PaletteId { get; set; }

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
        public string ChatLink { get; set; }

        [DataMember]
        public LocalizedString Descriptions { get; protected set; } = new();

        public string Description
        {
            get => Descriptions.Text;
            set => Descriptions.Text = value;
        }

        public SkillSlot Slot;
        public SkillFlag Flags;
        public List<string> Categories;
    }
}
