using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Services;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    [DataContract]
    public class Template
    {
        public Template()
        {
            BuildTemplate = new();
            GearTemplate = new();
        }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        public BuildTemplate BuildTemplate { get; set; }

        public GearTemplate GearTemplate { get; set; }    
    }
}
