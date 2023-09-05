using Kenedia.Modules.BuildsManager.Models;
using NAudio.MediaFoundation;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class StaticHosting
    {
        public static string BaseUrl = "https://bhm.blishhud.com/Kenedia.Modules.BuildsManager/";
        public static string Url = "https://bhm.blishhud.com/Kenedia.Modules.BuildsManager/Version.json";

        public async static Task<StaticVersion> GetStaticVersion()
        {
            try
            {
                using var httpClient = new HttpClient();
                string content = await httpClient.GetStringAsync(Url);

                var info = JsonConvert.DeserializeObject<StaticVersion>(content);
                return info;
            }
            catch
            {
                BuildsManager.Logger.Warn($"Failed to get versions from {Url}");
            }

            return new StaticVersion();
        }

        public async static Task<ByteIntMap> GetItemMap(string fileName, System.Threading.CancellationToken cancellationToken)
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

                var info = JsonConvert.DeserializeObject<ByteIntMap>(content);
                return info;
            }
            catch
            {
                BuildsManager.Logger.Warn($"Failed to get item map from {url}");
            }

            return null;
        }
    }
}
