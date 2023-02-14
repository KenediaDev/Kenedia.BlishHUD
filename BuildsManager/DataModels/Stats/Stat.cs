using Blish_HUD.Content;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Models;
using System.Collections.Generic;
using System.Runtime.Serialization;
using APIStat = Gw2Sharp.WebApi.V2.Models.Itemstat;

namespace Kenedia.Modules.BuildsManager.DataModels.Stats
{

    public class Attribute
    {
        public Attribute() { }
    }

    [DataContract]
    public class StatAttribute
    {
        public StatAttribute() { }

        public StatAttribute(ItemstatAttribute attribute)
        {
            Id = attribute.Attribute.ToEnum();
            Multiplier = attribute.Multiplier;
            Value = attribute.Value;
        }

        [DataMember]
        public double Multiplier { get; set; }

        [DataMember]
        public AttributeType Id { get; set; }

        [DataMember]
        public int Value { get; set; }
    }

    [DataContract]
    public class Stat
    {
        private AsyncTexture2D _icon;

        public Stat()
        {
        }

        public Stat(APIStat stat)
        {
            Name = stat.Name;
            Id = stat.Id;

            foreach (var att in stat.Attributes)
            {
                Attributes.Add(att.Attribute.ToEnum(), new(att));
            }
        }

        [DataMember]
        public Dictionary<AttributeType, StatAttribute> Attributes { get; set; } = new();

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public LocalizedString Names { get; protected set; } = new();
        public string Name
        {
            get => Names.Text;
            set => Names.Text = value;
        }

        [DataMember]
        public int AssetId { get; set; }

        public AsyncTexture2D Icon
        {
            get
            {
                if (_icon != null) return _icon;

                _icon = AsyncTexture2D.FromAssetId(AssetId);
                return _icon;
            }
        }

        public void ApplyLanguage(APIStat stat)
        {
            Name = stat.Name;
        }
    }
}
