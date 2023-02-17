using Blish_HUD.Content;
using Gw2Sharp;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using System;
using System.Collections.Generic;
using System.IO;
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
            WeaponType = skill.WeaponType != null ? (WeaponType)skill.WeaponType?.ToEnum() : null;
            Attunement = skill.Attunement?.ToEnum();

            if ((skill.Categories != null && skill.Categories.Count > 0) || skill.Name.Contains('\"'))
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

                Categories = Categories.Count > 0 ? Categories : null;
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

            foreach (string profName in skill.Professions)
            {
                if (Enum.TryParse(profName, out ProfessionType profession))
                {
                    Professions.Add(profession);
                }
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
        public int? Parent { get; set; }

        [DataMember]
        public Attunement? Attunement { get; set; }

        [DataMember]
        public int Specialization { get; set; }

        [DataMember]
        public List<ProfessionType> Professions { get; set; } = new();

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
            foreach(var race in BuildsManager.Data.Races)
            {
                var skill = race.Value.Skills.Where(e => e.Value.PaletteId == id).FirstOrDefault();
                if (skill.Value != null) return skill.Value;
            }

            return BuildsManager.Data.Professions?[profession]?.SkillsByPalette.TryGetValue((int)id, out int skillid) == true
                ? (BuildsManager.Data.Professions?[profession]?.Skills[skillid])
                : null;
        }

        public static int GetRevPaletteId(int id)
        {
            Dictionary<int, List<int>> palletteDic = new()
            {
                //Heal
                {4572, new(){ 26937, 27220, 27372, 28219, 28427, 29148, 45686, 62719, 62749}},
                //Util1
                {4614, new(){ 26821, 27322, 28379, 28516, 29209, 29310, 42949, 62832, 62702}},
                //Util2
                {4651, new(){ 26679, 27014, 27025, 27505, 28231, 29082, 40485, 62962, 62941}},
                //Util3
                {4564, new(){26557, 26644, 27107, 27715, 27917, 29197, 41220, 62878, 62796 }},
                //Elite
                {4554, new(){27356, 27760, 27975, 28287, 28406, 29114, 45773, 62942, 62687}},
            };

            foreach (var pair in palletteDic)
            {
                if (pair.Value.Contains(id)) return pair.Key;
            }

            return 0;
        }

        public static int GetRevPaletteId(Skill skill)
        {
            return GetRevPaletteId(skill.Id);
        }
    }
}
