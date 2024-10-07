using Blish_HUD;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using Kenedia.Modules.Core.Models;

namespace Kenedia.Modules.Core.Services
{
    public class StaticHosting
    {
        public virtual string BaseUrl { get; } = "https://bhm.blishhud.com/Kenedia.Modules.BuildsManager/";

        public Logger Logger { get; }

        public StaticHosting(Logger logger)
        {
            Logger = logger;
        }

        public async Task<T> GetStaticContent<T>(string fileName)
        {
            string url = $"{BaseUrl}{fileName}";
            string content = string.Empty;

            try
            {
                using var httpClient = new HttpClient();
                content = await httpClient.GetStringAsync(url);

                var info = JsonConvert.DeserializeObject<T>(content, SerializerSettings.Default);
                return info;
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to get {fileName} from {url}");
                Logger.Warn($"Fetched content: {content}");
                Logger.Warn($"{ex}");
            }

            return default;
        }
    }
}