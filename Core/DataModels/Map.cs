using Gw2Sharp.Models;
using Kenedia.Modules.Core.Models;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using APIMap = Gw2Sharp.WebApi.V2.Models.Map;

namespace Kenedia.Modules.Core.DataModels
{
    public class Map
    {

        public Map()
        {
        }

        public Map(APIMap map)
        {
            ApplyApiData(map);
        }

        public LocalizedString Names { get; } = [];

        [JsonIgnore]
        public string Name
        {
            get => Names.Text;
            set => Names.Text = value;
        }

        public int Id { get; set; }

        public MapType Type { get; set; }

        public void ApplyApiData(APIMap map)
        {
            Id = map.Id;
            Type = map.Type;
            Names.Text = map.Name;
        }
    }
}
