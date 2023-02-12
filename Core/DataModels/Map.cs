using Gw2Sharp.Models;
using Kenedia.Modules.Core.Models;
using System.Runtime.Serialization;
using APIMap = Gw2Sharp.WebApi.V2.Models.Map;

namespace Kenedia.Modules.Core.DataModels
{
    [DataContract]
    public class Map
    {

        public Map()
        {
        }

        public Map(APIMap map)
        {
            Id = map.Id;
            Name = map.Name;
            Type = map.Type;
        }

        public Map(APIMap map, Gw2Sharp.WebApi.Locale lang) : this(map)
        {
            Names[lang] = map.Name;
        }

        [DataMember]
        public LocalizedString Names { get; } = new();
        public string Name
        {
            get => Names.Text;
            set => Names.Text = value;
        }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public MapType Type { get; set; }
    }
}
