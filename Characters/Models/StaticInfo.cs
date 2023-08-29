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

        public StaticInfo() { }

        public StaticInfo(DateTime betaStart, DateTime betaEnd)
        {
            BetaStart = betaStart;
            BetaEnd = betaEnd;
        }

        public event EventHandler<bool> BetaStateChanged;

        [DataMember]
        public DateTime BetaStart { get; private set; } = DateTime.MinValue;

        [DataMember]
        public DateTime BetaEnd { get; private set; } = DateTime.MinValue;

        public bool IsBeta => DateTime.UtcNow >= BetaStart && DateTime.UtcNow < BetaEnd;

        public async static Task<StaticInfo> GetStaticInfo()
        {
            try
            {
                using var httpClient = new HttpClient();
                string content = await httpClient.GetStringAsync(Url);

                var info = JsonConvert.DeserializeObject<StaticInfo>(content);
                return info;
            }
            catch 
            { 
                Characters.Logger.Warn($"Failed to get static info from {Url}");
            }

            return new StaticInfo();
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
