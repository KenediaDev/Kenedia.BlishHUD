using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Kenedia.Modules.Characters.Models
{
    [DataContract]
    public class Map
    {
        public string Name
        {
            get => Blish_HUD.GameService.Overlay.UserLocale.Value switch
            {
                Gw2Sharp.WebApi.Locale.German => Names.De,
                Gw2Sharp.WebApi.Locale.French => Names.Fr,
                Gw2Sharp.WebApi.Locale.Spanish => Names.Es,
                _ => Names.En,
            };

            set
            {
                switch (Blish_HUD.GameService.Overlay.UserLocale.Value)
                {
                    case Gw2Sharp.WebApi.Locale.German:
                        Names.De = value;
                        break;

                    case Gw2Sharp.WebApi.Locale.French:
                        Names.Fr = value;
                        break;

                    case Gw2Sharp.WebApi.Locale.Spanish:
                        Names.Es = value;
                        break;

                    default:
                        Names.En = value;
                        break;
                }
            }
        }

        [DataMember]
        public Names Names { get; set; } = new Names();

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public int APIId { get; set; }

        [DataMember]
        public IReadOnlyList<int> Floors { get; set; }

        [DataMember]
        public int DefaultFloor { get; set; }

        [DataMember]
        public int ContinentId { get; set; }
    }
}
