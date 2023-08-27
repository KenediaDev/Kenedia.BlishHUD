using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Kenedia.Modules.Characters.Models
{
    [DataContract]
    public class StaticInfo
    {
        private bool _isBetaUpcoming = true;
        private bool _isBeta = false;

        public static string Url = "https://bhm.blishhud.com/Kenedia.Modules.Characters/static_info.json";
        public static string githubUrl = "https://raw.githubusercontent.com/KenediaDev/Kenedia.BlishHUD/bhud-static/Kenedia.Modules.Characters/static_info.json";

        public StaticInfo(DateTime betaStart, DateTime betaEnd)
        {
            BetaStart = betaStart;
            BetaEnd = betaEnd;
        }

        public event EventHandler<bool> BetaStateChanged;

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

        public void CheckBeta()
        {
            if (!_isBetaUpcoming) return;
            _isBetaUpcoming = DateTime.UtcNow < BetaEnd;

            if (_isBeta != IsBeta)
            {
                _isBeta = IsBeta;

                Characters.Logger.Debug($"Beta has {(IsBeta ? "started." : "ended.")}");
                BetaStateChanged?.Invoke(this, _isBeta);
            }
        }
    }
}
