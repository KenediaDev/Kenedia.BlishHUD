using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Services;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    [DataContract]
    public class Template
    {
        private readonly Data _data;
        private string _buildCode;
        private string _gearCode;

        public Template()
        {

        }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string BuildCode
        {
            get => BuildTemplate?.ParseBuildCode();
            set
            {
                if (_buildCode != value)
                {
                    _buildCode = value;
                    BuildTemplate.LoadFromCode(value);
                }
            }
        }
        public BuildTemplate BuildTemplate { get; set; }

        [DataMember]
        public string GearCode
        {
            get => GearTemplate?.ParseGearCode();
            set
            {
                if (_buildCode != value)
                {
                    _buildCode = value;
                    GearTemplate.LoadFromCode(value);
                }
            }
        }
        public GearTemplate GearTemplate { get; set; }    
    }
}
