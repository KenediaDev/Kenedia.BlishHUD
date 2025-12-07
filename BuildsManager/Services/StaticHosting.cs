using Kenedia.Modules.BuildsManager.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Services;
using Blish_HUD;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class StaticHosting : Core.Services.StaticHosting
    {
        public StaticHosting(Logger logger) : base(logger)
        {
        }

        public override string BaseUrl { get; } = "https://bhm.blishhud.com/Kenedia.Modules.BuildsManager/";

        public async Task<StaticStats> GetStaticStats()
        {            
            try
            {
                var info = await GetStaticContent<StaticStats>("Stats.json");
                return info;
            }
            catch(Exception ex)
            {
                Logger.Warn($"{ex}");
            }

            return new StaticStats();
        }

        public async Task<StaticVersion> GetStaticVersion()
        {
            try
            {
                var info = await GetStaticContent<StaticVersion>("DataMap.json");
                return info;
            }
            catch(Exception ex)
            {
                Logger.Warn($"{ex}");
            }

            return new StaticVersion();
        }
    }
}
