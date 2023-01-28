using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Kenedia.Modules.Characters.Models
{
    [DataContract]
    public class AccountSummary
    {
        [DataMember]
        public List<string> CharacterNames { get; set; }

        [DataMember]
        public string AccountName{ get; set; }
    }
}
