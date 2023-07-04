using Blish_HUD.Content;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.AdvancedBuildsManager.Services;
using Kenedia.Modules.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using APIStat = Gw2Sharp.WebApi.V2.Models.Itemstat;

namespace Kenedia.Modules.AdvancedBuildsManager.DataModels.Stats
{
    public class StatAttributes : Dictionary<AttributeType, StatAttribute>
    {
        public StatAttributes()
        {
            //foreach(AttributeType attribute in Enum.GetValues(typeof(AttributeType)))
            //{
            //    if (attribute is AttributeType.Unknown or AttributeType.None) continue;

            //    this[attribute] = null;
            //}
        }
    }

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
            Apply(stat);
        }

        [DataMember]
        public StatAttributes Attributes { get; set; } = new();

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public int MappedId { get; set; }

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

                _icon = TryGetTextureId(out int? textureId) ? AdvancedBuildsManager.ModuleInstance.ContentsManager.GetTexture($@"textures\equipment_stats\{textureId}.png") : AsyncTexture2D.FromAssetId(156021);
                return _icon;
            }
        }

        [DataMember]
        public EquipmentStat EquipmentStatType { get; set; }

        private bool TryGetTextureId(out int? id)
        {
            var foundId = AdvancedBuildsManager.Data.StatMap.Find(e => e.Ids.Contains(Id))?.Stat;
            id = foundId != null ? (int) foundId : -1;

            return id != -1;
        }

        public void Apply(APIStat stat)
        {
            Name = stat.Name;
            Id = stat.Id;

            foreach (var att in stat.Attributes)
            {
                Attributes[att.Attribute.ToEnum()] = new(att);
            }

            EquipmentStatType = AdvancedBuildsManager.Data.StatMap.Find(e => e.Ids.Contains(stat.Id)).Stat;
        }
    }
}
