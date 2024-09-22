using Blish_HUD.Content;
using Gw2Sharp;
using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.Interfaces;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using static Kenedia.Modules.BuildsManager.DataModels.Professions.Weapon;
using APISkill = Gw2Sharp.WebApi.V2.Models.Skill;
using SkillSlot = Gw2Sharp.WebApi.V2.Models.SkillSlot;

namespace Kenedia.Modules.BuildsManager.DataModels.Professions
{

    [DataContract]
    public class Skill : IDisposable, IBaseApiData
    {
        private bool _isDisposed;
        private AsyncTexture2D _icon;

        public Skill() { }

        public Skill(APISkill skill)
        {
            Apply(skill);
        }

        public Skill(BaseSkill baseSkill)
        {
            Names = baseSkill.Names;
            Descriptions = baseSkill.Descriptions;
            Id = baseSkill.Id;
            IconAssetId = baseSkill.AssetId ?? 0;
            Slot = baseSkill.Slot;
            foreach (string prof in baseSkill.Professions)
            {
                if (Enum.TryParse(prof, out ProfessionType profType))
                {
                    Professions.Add(profType);
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
                if (_icon is not null) return _icon;

                //TODO: This creates a deadlock on UI creation ... needs a fix!
                if (IconAssetId is not 0)
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
        public WeaponType? Offhand { get; set; }

        [DataMember]
        public SkillFlag Flags { get; set; }

        [DataMember]
        public SkillCategoryType Categories { get; set; } = SkillCategoryType.None;

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

        public List<Gw2Sharp.WebApi.V2.Models.SkillFact> Facts { get; set; }

        public List<Gw2Sharp.WebApi.V2.Models.SkillFact> TraitedFacts { get; set; }

        public void SetLiveAPI(APISkill skill)
        {
            Facts = skill.Facts?.ToList();
            TraitedFacts = skill.TraitedFacts?.ToList();
        }

        internal static Skill FromUShort(ushort id, ProfessionType profession)
        {
            foreach (var race in BuildsManager.Data.Races.Items)
            {
                var skill = race.Value.Skills.Where(e => e.Value.PaletteId == id).FirstOrDefault();
                if (skill.Value is not null)
                {
                    return skill.Value;
                }
            }

            int skillid = 0;
            bool hasSkill = BuildsManager.Data.Professions?[profession]?.SkillsByPalette.TryGetValue((int)id, out skillid) == true;

            return hasSkill ? BuildsManager.Data.Professions?[profession]?.Skills[skillid] : null;
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

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            _icon = null;
        }

        internal void Apply(APISkill skill)
        {
            Id = skill.Id;
            IconAssetId = skill.Icon.GetAssetIdFromRenderUrl();
            Name = skill.Name;
            Description = skill.Description;
            Specialization = skill.Specialization is not null ? (int)skill.Specialization : 0;
            ChatLink = skill.ChatLink;
            Flags = skill.Flags.Count() > 0 ? skill.Flags.Aggregate((x, y) => x |= y.ToEnum()) : SkillFlag.Unknown;
            Slot = skill.Slot?.ToEnum();
            WeaponType = skill.WeaponType is not null ? (WeaponType)skill.WeaponType?.ToEnum() : null;

            var missinCategories = new Dictionary<SkillCategoryType, List<int>>()
            {
                {SkillCategoryType.Preparation, new(){13057, 13026, 13038, 13056} }
            };

            if (Categories == SkillCategoryType.None)
            {
                foreach (var id in missinCategories.Values)
                {
                    if (id.Contains(skill.Id))
                    {
                        Categories |= missinCategories.FirstOrDefault(x => x.Value.Contains(skill.Id)).Key;
                    }
                }

                if (skill.Specialization is not null and not 0)
                {
                    if (!Categories.HasFlag(SkillCategoryType.Specialization)) Categories |= SkillCategoryType.Specialization;
                }
                else if (skill.Professions.Count == 1 && skill.Professions.Contains("Engineer") && skill.BundleSkills is not null)
                {
                    if (!Categories.HasFlag(SkillCategoryType.Kit)) Categories |= SkillCategoryType.Kit;
                }
                else if ((skill.Categories is not null && skill.Categories.Count > 0) || skill.Name.Contains('\"'))
                {
                    if (skill.Name.Contains('\"') && !Categories.HasFlag(SkillCategoryType.Shout)) Categories |= SkillCategoryType.Shout;

                    if (skill.Categories is not null)
                    {
                        foreach (string s in skill.Categories)
                        {
                            if (Enum.TryParse(s, out SkillCategoryType category))
                            {
                                if (!Categories.HasFlag(category)) Categories |= category;
                            }
                        }
                    }
                }
            }

            BundleSkills = skill.BundleSkills is not null && skill.BundleSkills.Count > 0 ? skill.BundleSkills.ToList() : null;
            FlipSkill = skill.FlipSkill is not null ? skill.FlipSkill : null;
            ToolbeltSkill = skill.ToolbeltSkill is not null ? skill.ToolbeltSkill : null;
            PrevChain = skill.PrevChain is not null ? skill.PrevChain : null;
            NextChain = skill.NextChain is not null ? skill.NextChain : null;

            foreach (string profName in skill.Professions)
            {
                if (Enum.TryParse(profName, out ProfessionType profession))
                {
                    Professions.Add(profession);
                }
            }

            Facts = skill.Facts?.ToList();
            TraitedFacts = skill.TraitedFacts?.ToList();
        }

        internal static int GetRevPaletteId(APISkill skill)
        {
            return GetRevPaletteId(skill.Id);
        }
    }
}
