using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Utility;
using System.ComponentModel;
using System.Runtime.Serialization;

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

        public Specialization EliteSpecialization => BuildTemplate?.Specializations[SpecializationSlot.Line_3]?.Specialization?.Elite == true ? BuildTemplate.Specializations[SpecializationSlot.Line_3].Specialization : null;

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

        private void TemplateChanged(object sender, PropertyChangedEventArgs e)
        {
            Changed?.Invoke(sender, e);
        }
    }
}
