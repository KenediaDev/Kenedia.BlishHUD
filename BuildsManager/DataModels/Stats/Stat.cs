using Blish_HUD.Content;
using Kenedia.Modules.Core.Models;
using System;
using System.Runtime.Serialization;
using APIStat = Gw2Sharp.WebApi.V2.Models.Itemstat;

namespace Kenedia.Modules.BuildsManager.DataModels.Stats
{
    [DataContract]
    public class Stat : IDisposable
    {
        private bool _isDisposed;

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
        public byte MappedId { get; set; }

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
                if (_icon is not null) return _icon;

                _icon = TryGetTextureId(out int? textureId) ? BuildsManager.ModuleInstance.ContentsManager.GetTexture($@"textures\equipment_stats\{textureId}.png") : AsyncTexture2D.FromAssetId(156021);
                return _icon;
            }
        }

        [DataMember]
        public EquipmentStat EquipmentStatType { get; set; }

        private bool TryGetTextureId(out int? id)
        {
            var foundId = BuildsManager.Data.StatMap.Find(e => e.Ids.Contains(Id))?.Stat;
            id = foundId is not null ? (int) foundId : -1;

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

            EquipmentStatType = BuildsManager.Data.StatMap.Find(e => e.Ids.Contains(stat.Id)).Stat;
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            _icon = null;
        }
    }
}
