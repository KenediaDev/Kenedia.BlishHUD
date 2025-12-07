using Kenedia.Modules.OverflowTradingAssist.DataEntries;
using Kenedia.Modules.OverflowTradingAssist.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Kenedia.Modules.Core.Models;
using Blish_HUD;

namespace Kenedia.Modules.OverflowTradingAssist.Services
{
    public class StaticHosting : Core.Services.StaticHosting
    {
        public override string BaseUrl { get; } = "https://github.com/KenediaDev/Kenedia.BlishHUD/tree/bhud-static/Kenedia.Modules.OverflowTradingAssist/";

        public StaticHosting(Logger logger) : base(logger)
        {
        }

        public async Task<StaticVersion> GetStaticVersion()
        {
            string url = $"{BaseUrl}{"Version"}.json";

            try
            {
                using var httpClient = new HttpClient();
                string content = await httpClient.GetStringAsync(url);

                var info = JsonConvert.DeserializeObject<StaticVersion>(content, SerializerSettings.Default);
                return info;
            }
            catch
            {
                OverflowTradingAssist.Logger.Warn($"Failed to get versions from {url}");
            }

            return new StaticVersion();
        }

        public async Task<HostedItems<int>> GetItemMap(string fileName, System.Threading.CancellationToken cancellationToken)
        {
            string url = $"{BaseUrl}{fileName}.json";

            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(url, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                string content = await response.Content.ReadAsStringAsync();

                var info = JsonConvert.DeserializeObject<HostedItems<int>>(content, SerializerSettings.Default);
                return info;
            }
            catch
            {
                OverflowTradingAssist.Logger.Warn($"Failed to get item map from {url}");
            }

            return null;
        }
    }
}
