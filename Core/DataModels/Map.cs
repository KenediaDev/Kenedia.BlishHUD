using Kenedia.Modules.Core.Models;
using System.Runtime.Serialization;
using APIMap = Gw2Sharp.WebApi.V2.Models.Map;

namespace Kenedia.Modules.Core.DataModels
{
    [DataContract]
    public class Map
    {
        public string Name
        {
            get => Names.Text;
            set => Names.Text = value;
        }
        public Map()
        {
        }

        public Map(APIMap map)
        {
            Id = map.Id;
            Name = map.Name;
        }

        public Map(APIMap map, Gw2Sharp.WebApi.Locale lang)
        {
            Id = map.Id;
            Names[lang] = map.Name;
        }

        [DataMember]
        public LocalizedString Names { get; } = new();

        [DataMember]
        public int Id { get; set; }
    }
}
