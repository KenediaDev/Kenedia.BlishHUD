using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Kenedia.Modules.Core.ContractResolver;

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
