using Newtonsoft.Json;

namespace Kenedia.Modules.Core.Models
{
    public class SerializerSettings
    {
        public static JsonSerializerSettings Default = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };
    }
}
