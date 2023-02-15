using Blish_HUD.Content;
using Gw2Sharp;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using static Kenedia.Modules.BuildsManager.DataModels.Professions.Weapon;
using APISkill = Gw2Sharp.WebApi.V2.Models.Skill;

namespace Kenedia.Modules.BuildsManager.DataModels.Professions
{
    [DataContract]
    public class Skill
    {
        private AsyncTexture2D _icon;

        public Skill() { }

        public Skill(APISkill skill, Dictionary<int, int> paletteBySkills)
        {
            Id = skill.Id;
            IconAssetId = skill.Icon.GetAssetIdFromRenderUrl();
            Name = skill.Name;
            Description = skill.Description;
            Specialization = skill.Specialization != null ? (int)skill.Specialization : 0;
            ChatLink = skill.ChatLink;
            Flags = skill.Flags.Count() > 0 ? skill.Flags.Aggregate((x, y) => x |= y.ToEnum()) : SkillFlag.Unknown;
            Slot = skill.Slot?.ToEnum();
            WeaponType = skill.WeaponType != null ? (WeaponType) skill.WeaponType?.ToEnum() : null;

            if((skill.Categories != null && skill.Categories.Count > 0) || skill.Name.Contains('\"'))
            {
                Categories = new();
                if (skill.Name.Contains('\"')) Categories.Add(SkillCategory.Shout);

                if (skill.Categories != null)
                {
                    foreach (string s in skill.Categories)
                    {
                        if (Enum.TryParse(s, out SkillCategory category)) Categories.Add(category);
                    }
                }

                Categories = Categories.Count> 0 ? Categories : null;
            }

            BundleSkills = skill.BundleSkills != null && skill.BundleSkills.Count > 0 ? skill.BundleSkills.ToList() : null;
            FlipSkill = skill.FlipSkill != null ? skill.FlipSkill : null;
            ToolbeltSkill = skill.ToolbeltSkill != null ? skill.ToolbeltSkill : null;
            PrevChain = skill.PrevChain != null ? skill.PrevChain : null;
            NextChain = skill.NextChain != null ? skill.NextChain : null;

            if (paletteBySkills.TryGetValue(skill.Id, out int paletteId))
            {
                PaletteId = paletteId;
            }
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

        [DataMember]
        public SkillSlot? Slot { get; set; }

        [DataMember]
        public WeaponType? WeaponType { get; set; }

        [DataMember]
        public SkillFlag Flags { get; set; }

        [DataMember]
        public List<SkillCategory> Categories { get; set; }

        [DataMember]
        public int? FlipSkill { get; set; }

        [DataMember]
        public int? NextChain { get; set; }

        [DataMember]
        public int? PrevChain { get; set; }

        [DataMember]
        public int? ToolbeltSkill { get; set; }

        [DataMember]
        public List<int> BundleSkills { get; set; }

        internal static Skill FromUShort(ushort id, ProfessionType profession)
        {
            return BuildsManager.Data.Professions?[profession]?.SkillsByPalette.TryGetValue((int)id, out int skillid) == true
                ? (BuildsManager.Data.Professions?[profession]?.Skills[skillid])
                : null;
        }
    }
}
