using Kenedia.Modules.OverflowTradingAssist.DataEntries;
using Kenedia.Modules.OverflowTradingAssist.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.OverflowTradingAssist.Services
{
    public class StaticHosting
    {
        public static string BaseUrl = "https://bhm.blishhud.com/Kenedia.Modules.OverflowTradingAssist/";
        public static string VersionUrl = "https://bhm.blishhud.com/Kenedia.Modules.OverflowTradingAssist/Version.json";

        public async static Task<StaticVersion> GetStaticVersion()
        {
            try
            {
                using var httpClient = new HttpClient();
                string content = await httpClient.GetStringAsync(VersionUrl);

                var info = JsonConvert.DeserializeObject<StaticVersion>(content);
                return info;
            }
            catch
            {
                OverflowTradingAssist.Logger.Warn($"Failed to get versions from {VersionUrl}");
            }

            return new StaticVersion();
        }

        public async static Task<HostedItems<int>> GetItemMap(string fileName, System.Threading.CancellationToken cancellationToken)
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

                var info = JsonConvert.DeserializeObject<HostedItems<int>>(content);
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
