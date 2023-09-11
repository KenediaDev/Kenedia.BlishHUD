using Kenedia.Modules.BuildsManager.Models;
using NAudio.MediaFoundation;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Kenedia.Modules.Core.Models;
using System.IO;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class StaticHosting
    {
        public static string BaseUrl = "https://bhm.blishhud.com/Kenedia.Modules.BuildsManager/";

        public async static Task<StaticVersion> GetStaticVersion()
        {
            string url = $"{BaseUrl}DataMap.json";
            string content = string.Empty;
            try
            {
                using var httpClient = new HttpClient();
                content = await httpClient.GetStringAsync(url);

                var info = JsonConvert.DeserializeObject<StaticVersion>(content, SerializerSettings.Default);
                return info;
            }
            catch(Exception ex)
            {
                BuildsManager.Logger.Warn($"Failed to get versions from {url}");
                BuildsManager.Logger.Warn($"Fetched content: {content}");
                BuildsManager.Logger.Warn($"{ex}");
            }

            return new StaticVersion();
        }

        public async static Task<ByteIntMap> GetItemMap(string fileName, System.Threading.CancellationToken cancellationToken)
        {
            string url = $"{BaseUrl}{fileName}.json";
            string content = string.Empty;

            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(url, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                content = await response.Content.ReadAsStringAsync();

                var info = JsonConvert.DeserializeObject<ByteIntMap>(content, SerializerSettings.Default);
                return info;
            }
            catch (Exception ex)
            {
                BuildsManager.Logger.Warn($"Failed to get item map from {url}");
                BuildsManager.Logger.Warn($"Fetched content: {content}");
                BuildsManager.Logger.Warn($"{ex}");
            }

            return null;
        }
    }
}
