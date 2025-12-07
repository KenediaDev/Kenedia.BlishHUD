using Blish_HUD;
using Kenedia.Modules.Core.Converter;
using Newtonsoft.Json;
using SemVer;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.Characters.Services
{
    public class StaticHosting : Core.Services.StaticHosting
    {
        public class Versions
        {
            [JsonConverter(typeof(SemverVersionConverter))]
            public Version Professions { get; set; } = new("0.0.0");

            [JsonConverter(typeof(SemverVersionConverter))]
            public Version Specializations { get; set; } = new("0.0.0");

            [JsonConverter(typeof(SemverVersionConverter))]
            public Version Races { get; set; } = new("0.0.0");

            [JsonConverter(typeof(SemverVersionConverter))]
            public Version CraftingProfessions { get; set; } = new("0.0.0");

            [JsonConverter(typeof(SemverVersionConverter))]
            public Version Maps { get; set; } = new("0.0.0");

            [JsonIgnore]
            public Version this[string key]
            {
                get
                {
                    var properties = typeof(Versions).GetProperties();
                    var property = properties.FirstOrDefault(p => p.Name.ToLower() == key.ToLower());

                    return property is not null && property.GetValue(this) is Version version ? version : new Version("0.0.0");
                }
            }
        }

        public override string BaseUrl { get; } = "https://bhm.blishhud.com/Kenedia.Modules.Characters/";

        public StaticHosting(Logger logger) : base(logger)
        {
        }

        public async Task<Versions> GetStaticVersions()
        {
            try
            {
                var info = await GetStaticContent<Versions>("Versions.json");
                return info;
            }
            catch (System.Exception ex)
            {
                Logger.Warn($"{ex}");
            }
            return new Versions();
        }
    }
}
