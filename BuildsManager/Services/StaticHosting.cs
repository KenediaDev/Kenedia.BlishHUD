using Kenedia.Modules.BuildsManager.Models;
using NAudio.MediaFoundation;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Kenedia.Modules.Core.Models;

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

                var info = JsonConvert.DeserializeObject<StaticVersion>(content, SerializerSettings.SemverSerializer);
                return info;
            }
            catch(Exception ex)
            {
                BuildsManager.Logger.Warn($"Failed to get versions from {Url}");
                BuildsManager.Logger.Warn($"{ex}");
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

                var info = JsonConvert.DeserializeObject<ByteIntMap>(content, SerializerSettings.SemverSerializer);
                return info;
            }
            catch (Exception ex)
            {
                BuildsManager.Logger.Warn($"Failed to get item map from {url}");
                BuildsManager.Logger.Warn($"{ex}");
            }

            return null;
        }
    }
}
