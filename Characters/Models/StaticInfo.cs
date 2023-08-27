using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.Characters.Models
{
    [DataContract]
    public class StaticInfo
    {
        public static string Url = "https://bhm.blishhud.com/Kenedia.Modules.Characters/static_info.json";

        public StaticInfo(DateTime betaStart, DateTime betaEnd)
        {
            BetaStart = betaStart;
            BetaEnd = betaEnd;
        }

        [DataMember]
        public DateTime BetaStart { get; private set; }

        [DataMember]
        public DateTime BetaEnd { get; private set; }

        public bool IsBeta => DateTime.UtcNow >= BetaStart && DateTime.UtcNow < BetaEnd;

        public async static Task<StaticInfo> GetStaticInfo()
        {
            using var httpClient = new HttpClient();
            string content = await httpClient.GetStringAsync(Url);

            var info = JsonConvert.DeserializeObject<StaticInfo>(content);
            return info;
        }
    }
}
