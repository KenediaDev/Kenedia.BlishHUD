using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
