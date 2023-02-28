using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Utility;
using System.ComponentModel;
using System.Runtime.Serialization;
using Attunement = Gw2Sharp.WebApi.V2.Models.Attunement;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    [DataContract]
    public class Template
    {
        private BuildTemplate _buildTemplate;
        private GearTemplate _gearTemplate;
        private ProfessionType _profession;
        private string _description;
        private string _name;
        private string _id;
        private bool _terrestrial = true;
        private AttunementType _mainAttunement = AttunementType.Fire;
        private AttunementType _altAttunement = AttunementType.Earth;
        private LegendSlot _legendSlot = LegendSlot.TerrestrialActive;

        public Template()
        {
            BuildTemplate = new();
            GearTemplate = new();
        }

        public event PropertyChangedEventHandler Changed;

        [DataMember]
        public string Id { get => _id; set => Common.SetProperty(ref _id, value, Changed); }

        [DataMember]
        public string Name { get => _name; set => Common.SetProperty(ref _name, value, Changed); }

        [DataMember]
        public string Description { get => _description; set => Common.SetProperty(ref _description, value, Changed); }

        [DataMember]
        public ProfessionType Profession
        {
            get => BuildTemplate.Profession;
            set
            {
                if (Common.SetProperty(ref _profession, value, Changed))
                {
                    if (BuildTemplate != null)
                    {
                        BuildTemplate.Profession = value;
                        GearTemplate.Profession = value;
                    }
                }
            }
        }

        [DataMember]
        public Races Race = Races.None;

        public Specialization EliteSpecialization => BuildTemplate?.Specializations[SpecializationSlot.Line_3]?.Specialization?.Elite == true ? BuildTemplate.Specializations[SpecializationSlot.Line_3].Specialization : null;

        public AttunementType MainAttunement { get => _mainAttunement; set => Common.SetProperty(ref _mainAttunement, value, Changed); }

        public AttunementType AltAttunement { get => _altAttunement; set => Common.SetProperty(ref _altAttunement, value, Changed); }

        public bool Terrestrial
        {
            get => LegendSlot is LegendSlot.TerrestrialActive or LegendSlot.TerrestrialInactive;
            set
            {
                switch (LegendSlot)
                {
                    case LegendSlot.AquaticActive:
                    case LegendSlot.AquaticInactive:
                        LegendSlot newTerrestialSlot = LegendSlot is LegendSlot.AquaticActive ? LegendSlot.TerrestrialActive : LegendSlot.TerrestrialInactive;
                        _ = Common.SetProperty(ref _legendSlot, newTerrestialSlot, Changed);

                        break;

                    case LegendSlot.TerrestrialActive:
                    case LegendSlot.TerrestrialInactive:
                        LegendSlot newAquaticSlot = LegendSlot is LegendSlot.TerrestrialActive ? LegendSlot.AquaticActive : LegendSlot.AquaticInactive;
                        _ = Common.SetProperty(ref _legendSlot, newAquaticSlot, Changed);
                        break;
                }
            }
        }

        public LegendSlot LegendSlot
        {
            get => _legendSlot;
            set => Common.SetProperty(ref _legendSlot, value, Changed);
        }

        /// <summary>
        /// Active Transform Skill which sets weapon skills to its childs and disables all others
        /// </summary>
        public Skill ActiveTransform { get; set; }

        /// <summary>
        /// Active Bundle Skill which sets weapon skills to its childs
        /// </summary>
        public Skill ActiveBundle { get; set; }

        public BuildTemplate BuildTemplate
        {
            get => _buildTemplate; set
            {
                var prev = _buildTemplate;

                if (Common.SetProperty(ref _buildTemplate, value, Changed))
                {
                    if (prev != null) prev.Changed -= TemplateChanged;

                    _buildTemplate ??= new();
                    _buildTemplate.Changed += TemplateChanged;
                }
            }
        }

        public GearTemplate GearTemplate
        {
            get => _gearTemplate; set
            {
                var prev = _gearTemplate;
                if (Common.SetProperty(ref _gearTemplate, value, Changed))
                {
                    if (prev != null) prev.Changed -= TemplateChanged;

                    _gearTemplate ??= new();
                    _gearTemplate.Changed += TemplateChanged;
                }
            }
        }

        public SkillCollection GetActiveSkills()
        {
            return LegendSlot switch
            {
                LegendSlot.AquaticInactive => BuildTemplate.InactiveAquaticSkills,
                LegendSlot.TerrestrialInactive => BuildTemplate.InactiveTerrestrialSkills,
                LegendSlot.AquaticActive => BuildTemplate.AquaticSkills,
                LegendSlot.TerrestrialActive => BuildTemplate.TerrestrialSkills,
                _ => null,
            };
        }

        private void TemplateChanged(object sender, PropertyChangedEventArgs e)
        {
            if(MainAttunement != AltAttunement && EliteSpecialization?.Id != (int) SpecializationType.Weaver)
            {
                AltAttunement = MainAttunement;
            }

            Changed?.Invoke(sender, e);
        }
    }
}
