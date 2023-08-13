using Blish_HUD.Content;
using Gw2Sharp;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.AdvancedBuildsManager.Models.Templates;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using static Kenedia.Modules.AdvancedBuildsManager.DataModels.Professions.Weapon;
using APISkill = Gw2Sharp.WebApi.V2.Models.Skill;

namespace Kenedia.Modules.AdvancedBuildsManager.DataModels.Professions
{

    [DataContract]
    public class Skill
    {
        private AsyncTexture2D _icon;

        public Skill() { }

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

        public Skill(APISkill skill, Dictionary<int, int> paletteBySkills)
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

            _ = AdvancedBuildsManager.Data.SkillConnections.TryGetValue(skill.Id, out var connection);
            SkillConnection = connection ?? null;
            Attunement = connection?.Attunement;

            if (skill.Specialization is not null and not 0)
            {
                if (!Categories.HasFlag(SkillCategory.Specialization)) Categories |= SkillCategory.Specialization;
            }
            else if (skill.Professions.Count == 1 && skill.Professions.Contains("Engineer") && skill.BundleSkills is not null)
            {
                if (!Categories.HasFlag(SkillCategory.Kit)) Categories |= SkillCategory.Kit;
            }
            else if ((skill.Categories is not null && skill.Categories.Count > 0) || skill.Name.Contains('\"'))
            {
                if (skill.Name.Contains('\"') && !Categories.HasFlag(SkillCategory.Shout)) Categories |= SkillCategory.Shout;

                if (skill.Categories is not null)
                {
                    foreach (string s in skill.Categories)
                    {
                        if (Enum.TryParse(s, out SkillCategory category))
                        {
                            if (!Categories.HasFlag(category)) Categories |= category;
                        }
                    }
                }
            }

            BundleSkills = skill.BundleSkills is not null && skill.BundleSkills.Count > 0 ? skill.BundleSkills.ToList() : null;
            FlipSkill = skill.FlipSkill is not null ? skill.FlipSkill : null;
            ToolbeltSkill = skill.ToolbeltSkill is not null ? skill.ToolbeltSkill : null;
            PrevChain = skill.PrevChain is not null ? skill.PrevChain : null;
            NextChain = skill.NextChain is not null ? skill.NextChain : null;

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

            Facts = skill.Facts?.ToList();
            TraitedFacts = skill.TraitedFacts?.ToList();
        }

        [DataMember]
        public SkillConnection SkillConnection { get; protected set; }

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
        public AttunementType? Attunement { get; set; }

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
        public SkillCategory Categories { get; set; }

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
            foreach (var race in AdvancedBuildsManager.Data.Races)
            {
                var skill = race.Value.Skills.Where(e => e.Value.PaletteId == id).FirstOrDefault();
                if (skill.Value is not null)
                {
                    return skill.Value;
                }
            }

            int skillid = 0;
            bool hasSkill = AdvancedBuildsManager.Data.Professions?[profession]?.SkillsByPalette.TryGetValue((int)id, out skillid) == true;

            return hasSkill ? AdvancedBuildsManager.Data.Professions?[profession]?.Skills[skillid] : null;
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

        public Skill GetEffectiveSkill(Template template)
        {
            Skill skill = this;

            if (template is not null)
            {
                List<int> traitIds = new();

                foreach (var spec in template.BuildTemplate.Specializations)
                {
                    if (spec.Value is not null && spec.Value.Specialization is not null)
                    {
                        traitIds.AddRange(spec.Value.Traits.Where(e => e.Value is not null).Select(e => e.Value.Id));
                        traitIds.AddRange(spec.Value.Specialization.MinorTraits.Where(e => e.Value is not null).Select(e => e.Value.Id));
                    }
                }

                if (SkillConnection is not null)
                {
                    if (SkillConnection.Traited is not null)
                    {
                        foreach (int traitid in traitIds)
                        {
                            if (SkillConnection.Traited.ContainsValue(traitid))
                            {
                                foreach (var tPair in SkillConnection.Traited)
                                {
                                    if (tPair.Value == traitid)
                                        _ = AdvancedBuildsManager.Data.Professions[template.Profession].Skills.TryGetValue(tPair.Key, out skill);
                                }
                            }
                        }
                    }

                    if (skill?.SkillConnection is not null && skill.SkillConnection.EnvCounter is not null)
                    {
                        bool useCounterSkill = (template.Terrestrial && !skill.SkillConnection.Enviroment.HasFlag(Enviroment.Terrestrial)) || (!template.Terrestrial && !skill.SkillConnection.Enviroment.HasFlag(Enviroment.Aquatic));
                        if (useCounterSkill) _ = AdvancedBuildsManager.Data.Professions[template.Profession].Skills.TryGetValue((int)skill.SkillConnection.EnvCounter, out skill);
                    }
                }
            }

            return skill;
        }
    }
}
