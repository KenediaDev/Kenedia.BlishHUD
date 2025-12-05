using Blish_HUD.Content;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework;
using NAudio.MediaFoundation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using APIStat = Gw2Sharp.WebApi.V2.Models.Itemstat;

namespace Kenedia.Modules.BuildsManager.DataModels.Stats
{
    [DataContract]
    public class Stat : IDisposable, IDataMember
    {
        public class StatTextureMapInfo
        {
            private Point _startOffset = new(8, 8);
            private Point _textureSize = new(36, 36);
            private Point _textureShift = new(44, 0);

            public StatTextureMapInfo(string name, List<int> ids, int position)
            {
                Name = name;
                Ids = ids;
                Position = position;
                TextureRectangle = new(_startOffset.X + (_textureShift.X * (position - 1)), _startOffset.Y + (_textureShift.Y * (position - 1)), _textureSize.X, _textureSize.Y);
            }

            public string Name { get; }

            public List<int> Ids { get; }

            public int Position { get; }

            [JsonIgnore]
            public Rectangle TextureRectangle { get; }

            public bool MatchesId(int id)
            {
                return Ids.Contains(id);
            }
        }

        public static List<StatTextureMapInfo> StatTextureMap { get; set; } = [];
        
        private bool _isDisposed;

        public Stat()
        {
        }

        public Stat(APIStat stat)
        {
            Apply(stat);
        }

        public string DisplayAttributes => Attributes.ToString(0);

        [DataMember]
        public StatAttributes Attributes { get; set; } = [];

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public byte MappedId { get; set; }

        [DataMember]
        public LocalizedString Names { get; protected set; } = [];
        public string Name
        {
            get => Names.Text;
            set => Names.Text = value;
        }

        public DetailedTexture Icon
        {
            get
            {
                field ??= new(TexturesService.GetTextureFromDisk(Path.Combine(BuildsManager.Data.Paths.ModuleDataPath, "stat_map.png")))
                    {
                        TextureRegion = TextureInfo.TextureRectangle
                    };

                return field;
            }

            set;
        }

        public StatTextureMapInfo TextureInfo
        {
            get
            {
                field ??= StatTextureMap?.FirstOrDefault(e => e.Ids.Contains(Id));
                return field;
            }
        }

        public void Apply(APIStat stat)
        {
            Name = stat.Name;
            Id = stat.Id;

            foreach (var att in stat.Attributes)
            {
                Attributes[att.Attribute.ToEnum()] = new(att);
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            Icon = null;
        }
    }
}
