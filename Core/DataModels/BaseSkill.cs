using Blish_HUD.Content;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ApiSkill = Gw2Sharp.WebApi.V2.Models.Skill;

namespace Kenedia.Modules.Core.DataModels
{
    [DataContract]
    public class BaseSkill
    {
        private AsyncTexture2D _icon;

        public BaseSkill()
        {

        }

        public BaseSkill(ApiSkill skill)
        {
            Id = skill.Id;
            Name = skill.Name;
            AssetId = skill.Icon?.GetAssetIdFromRenderUrl();
            Professions = skill.Professions.ToList();
            Slot = skill.Slot?.ToEnum();

            //if (skill.Slot == SkillSlot.Pet && Professions.Count == 0) Professions.Add("Ranger"); 
        }

        [DataMember]
        public SkillSlot? Slot { get; set; }

        [DataMember]
        public int Id { get; set; }

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
        public List<string> Professions { get; set; } = new();

        [DataMember]
        public int? AssetId { get; set; }

        public AsyncTexture2D Icon
        {
            get
            {
                if (_icon != null || AssetId == null) return _icon;

                _icon = AsyncTexture2D.FromAssetId((int)AssetId);
                return _icon;
            }
        }
    }
}
