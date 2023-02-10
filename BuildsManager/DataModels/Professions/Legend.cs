using Kenedia.Modules.Core.Models;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.Professions
{
    [DataContract]
    public class Legend
    {
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
        public Dictionary<int, Skill> Utilities { get; set; } = new();

        [DataMember]
        public Skill Heal { get; set; }

        [DataMember]
        public Skill Elite { get; set; }

        [DataMember]
        public Skill Swap { get; set; }

        [DataMember]
        public Skill Skill { get; set; }

        [DataMember]
        public int Specialization { get; set; }
    }
}
